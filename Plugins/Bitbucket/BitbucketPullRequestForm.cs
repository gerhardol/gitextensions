using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using GitUIPluginInterfaces;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Atlassian.Stash;
using ResourceManager;
using GitCommands;
using Atlassian.Stash.Api;
using Atlassian.Stash.Entities;
using Atlassian.Stash.Helpers;
using Atlassian.Stash.Api.Entities;
using Atlassian.Stash.Api.Exceptions;

namespace Bitbucket
{
    public partial class BitbucketPullRequestForm : GitExtensionsFormBase
    {
        private readonly TranslationString _yourRepositoryIsNotInBitbucket = new TranslationString("Your repository is not hosted in Bitbucket.");
        private readonly TranslationString _commited = new TranslationString("{0} committed\n{1}");
        private readonly TranslationString _success = new TranslationString("Success");
        private readonly TranslationString _error = new TranslationString("Error");

        private Settings _settings;
        private readonly BitbucketPlugin _plugin;
        private readonly GitUIBaseEventArgs _gitUiCommands;
        private readonly ISettingsSource _settingsContainer;
        private readonly AuthorWrapper[] _reviewers = new AuthorWrapper[]{};
        private readonly List<string> _bitbucketUsers = new List<string>();
        private readonly StashClient _stashClient;


        public BitbucketPullRequestForm(BitbucketPlugin plugin, ISettingsSource settings, GitUIBaseEventArgs gitUiCommands)
        {
            InitializeComponent();
            Translate();

            _plugin = plugin;
            _settingsContainer = settings;
            _gitUiCommands = gitUiCommands;
            //TODO Retrieve all users and set default reviewers
            ReviewersDataGrid.Visible = false;
            _settings = Settings.Parse(_gitUiCommands.GitModule, _settingsContainer, _plugin);
            if (_settings == null)
            {
                MessageBox.Show(_yourRepositoryIsNotInBitbucket.Text);
                Close();
                return;
            }
            _stashClient = new StashClient(_settings.BitbucketUrl, _settings.Username, _settings.Password);
            this.Load += BitbucketPullRequestFormLoad;
            this.Load += BitbucketViewPullRequestFormLoad;
            createPullLinkLabel.Text = _settings + "pull-requests?create";
            viewPullLinkLabel.Text = _settings + "pull-requests";
        }

        private void BitbucketPullRequestFormLoad(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                var repositories = GetRepositories();
                try
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ddlRepositorySource.DataSource = repositories;
                        ddlRepositoryTarget.DataSource = repositories;
                        ddlRepositorySource.Enabled = true;
                        ddlRepositoryTarget.Enabled = true;
                    });
                }
                catch (System.InvalidOperationException)
                {
                }
            });
        }

        private void BitbucketViewPullRequestFormLoad(object sender, EventArgs e)
        {
            if (_settings == null)
                return;
            ThreadPool.QueueUserWorkItem(state =>
            {
                var pullReqs = GetPullRequests();
                try
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        lbxPullRequests.DataSource = pullReqs;
                        lbxPullRequests.DisplayMember = "DisplayName";
                    });
                }
                catch(System.InvalidOperationException){
                    return;
                }
            });
        }

        private IList<Repository> GetRepositories()
        {
            var list = new List<Repository>();
            var defaultRepo = _stashClient.Repositories.GetById(_settings.ProjectKey, _settings.RepoSlug);
            if (defaultRepo.Result!=null)
                list.Add(defaultRepo.Result);
            return list;
        }

        private IEnumerable<PullRequest> GetPullRequests()
        {
            var response = _stashClient.PullRequests.Get(_settings.ProjectKey, _settings.RepoSlug, state: PullRequestState.OPEN);
            var pullRequests = response.Result.Values;
            return pullRequests;
        }

        private void BtnCreateClick(object sender, EventArgs e)
        {
            if (ddlBranchSource.SelectedValue == null ||
                ddlBranchTarget.SelectedValue == null ||
                ddlRepositorySource.SelectedValue == null ||
                ddlRepositoryTarget.SelectedValue == null)
            {
                return;
            }

            //var response = _stashClient.Repositories.GetPullRequestSettings(_settings.ProjectKey, _settings.RepoSlug);

            var pullRequest = new PullRequest
            {
                Title = txtTitle.Text,
                Description = txtDescription.Text,
                FromRef = new Ref
                {
                    Repository = (Repository)ddlRepositorySource.SelectedValue,
                    Id = ddlBranchSource.SelectedValue.ToString()
                },
                ToRef = new Ref
                {
                Repository = (Repository)ddlRepositoryTarget.SelectedValue,
                Id = ddlBranchTarget.SelectedValue.ToString()
            },
                 Reviewers = _reviewers
            };
            var response2 = _stashClient.PullRequests.Create(_settings.ProjectKey, _settings.RepoSlug, pullRequest);
            //var pullRequest = new CreatePullRequestRequest(_settings, info);
            if (response2.IsCompleted)
            {
                MessageBox.Show(_success.Text);
                BitbucketViewPullRequestFormLoad(null, null);
            }
            else
                MessageBox.Show(string.Join(Environment.NewLine, response2.Status),
                    _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        /*
        private IEnumerable<BBBitbucketUser> GetBitbucketUsers()
        {
            var list = new List<BBBitbucketUser>();
            var getUser = new GetUserRequest(_settings);
            var result = getUser.Send();
            if (result.Success)
            {
                foreach (var value in result.Result["values"])
                {
                    list.Add(new BBBitbucketUser { Slug = value["slug"].ToString() });
                }
            }
            return list;
        }
        */
        readonly Dictionary<Repository, IEnumerable<Branch>> _branches = new Dictionary<Repository,IEnumerable<Branch>>();
        private IEnumerable<Branch> GetBitbucketBranches(Repository selectedRepo)
        {
            if (_branches.ContainsKey(selectedRepo))
            {
                return _branches[selectedRepo];
            }
            var response = _stashClient.Branches.Get(_settings.ProjectKey, _settings.RepoSlug);
            var list = response.Result.Values.ToList();
            _branches.Add(selectedRepo, list);
            return list;
        }

        private void ReviewersDataGridEditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            var cellEdit = e.Control as DataGridViewTextBoxEditingControl;
            if (cellEdit != null)
            {
                cellEdit.AutoCompleteCustomSource = new AutoCompleteStringCollection();
                cellEdit.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                cellEdit.AutoCompleteSource = AutoCompleteSource.CustomSource;
                cellEdit.AutoCompleteCustomSource.AddRange(_bitbucketUsers.ToArray());
            }
        }

        private void DdlRepositorySourceSelectedValueChanged(object sender, EventArgs e)
        {
            RefreshDDLBranch(ddlBranchSource, ((ComboBox)sender).SelectedValue);
        }

        private void DdlRepositoryTargetSelectedValueChanged(object sender, EventArgs e)
        {
            RefreshDDLBranch(ddlBranchTarget, ((ComboBox)sender).SelectedValue);
        }

        private void RefreshDDLBranch(ComboBox branchComboBox, object selectedValue)
        {
            List<string> branchNames = GetBitbucketBranches((Repository)selectedValue).Select(i => i.Name).ToList();
            if (AppSettings.BranchOrderingCriteria == BranchOrdering.Alphabetically)
            {
                branchNames.Sort();
            }
            branchNames.Insert(0, "");
            branchComboBox.DataSource = branchNames;
        }

        private void DdlBranchSourceSelectedValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlBranchSource.SelectedValue.ToString())) return;
            var commit = GetCommitInfo((Repository)ddlRepositorySource.SelectedValue,
                                                ddlBranchSource.SelectedValue.ToString());

            ddlBranchSource.Tag = commit;
            UpdateCommitInfo(lblCommitInfoSource, commit);
            txtTitle.Text = ddlBranchSource.SelectedValue.ToString().Replace("-"," ");
            UpdatePullRequestDescription();
        }

        private void DdlBranchTargetSelectedValueChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlBranchTarget.SelectedValue.ToString())) return;
            var commit = GetCommitInfo((Repository)ddlRepositoryTarget.SelectedValue,
                                                ddlBranchTarget.SelectedValue.ToString());

            ddlBranchTarget.Tag = commit;
            UpdateCommitInfo(lblCommitInfoTarget, commit);
            UpdatePullRequestDescription();
        }

        private Commit GetCommitInfo(Repository repo, string branch)
        {
            //if (repo == null || string.IsNullOrWhiteSpace(branch))
                return null;
            //TODO
            /*var getCommit = new GetHeadCommitRequest(repo, branch, _settings); //TODO Not accepted refs/hesd
            var result = getCommit.Send();
            return result.Success ? result.Result : null;
            */
        }

        private void UpdateCommitInfo(Label label, Commit commit)
        {
            if (commit == null)
                label.Text = string.Empty;
            else
                label.Text = string.Format(_commited.Text,
                    commit.Author, commit.Message);
        }

        private void UpdatePullRequestDescription()
        {
            if (ddlRepositorySource.SelectedValue == null
                || ddlRepositoryTarget.SelectedValue == null
                || ddlBranchSource.Tag == null
                || ddlBranchTarget.Tag == null)
                return;
            /*
            var getCommitsInBetween = new GetInBetweenCommitsRequest(
                (Repository)ddlRepositorySource.SelectedValue,
                (Repository)ddlRepositoryTarget.SelectedValue,
                (Commit)ddlBranchSource.Tag,
                (Commit)ddlBranchTarget.Tag,
                _settings);

            var result = getCommitsInBetween.Send();
            if (result.Success)
            {
                var sb = new StringBuilder();
                sb.AppendLine();
                foreach (var commit in result.Result)
                {
                    if (!commit.IsMerge)
                        sb.Append("* ").AppendLine(commit.Message);
                }
                txtDescription.Text = sb.ToString();
            }
            */
        }
        private void PullRequestChanged(object sender, EventArgs e)
        {
            var curItem = lbxPullRequests.SelectedItem as BBPullRequest;

            txtPRTitle.Text = curItem.Title;
            txtPRDescription.Text = curItem.Description;
            lblPRAuthor.Text = curItem.Author;
            lblPRState.Text = curItem.State;
            txtPRReviewers.Text = curItem.Reviewers;
            lblPRSourceRepo.Text = curItem.SrcDisplayName;
            lblPRSourceBranch.Text = curItem.SrcBranch;
            lblPRDestRepo.Text = curItem.DestDisplayName;
            lblPRDestBranch.Text = curItem.DestBranch;
        }

        private void BtnMergeClick(object sender, EventArgs e)
        {
          /* var curItem = lbxPullRequests.SelectedItem as PullRequest;
            if (curItem == null) return;

            var mergeInfo = new MergeRequestInfo
            {
                Id = curItem.Id,
                Version = curItem.Version,
                ProjectKey = curItem.DestProjectKey,
                TargetRepo = curItem.DestRepo,
            };

            //Merge
            var mergeRequest = new MergePullRequest(_settings, mergeInfo);
            var response = mergeRequest.Send();
            if (response.Success)
            {
                MessageBox.Show(_success.Text);
                BitbucketViewPullRequestFormLoad(null, null);
            }
            else
                MessageBox.Show(string.Join(Environment.NewLine, response.Messages),
                    _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    */
        }

        private void BtnApproveClick(object sender, EventArgs e)
        {
            /*   var curItem = lbxPullRequests.SelectedItem as PullRequest;
               if (curItem == null) return;
   
               var mergeInfo = new MergeRequestInfo
               {
                   Id = curItem.Id,
                   Version = curItem.Version,
                   ProjectKey = curItem.DestProjectKey,
                   TargetRepo = curItem.DestRepo,
               };
   
               //Approve
               var approveRequest = new ApprovePullRequest(_settings, mergeInfo);
               var response = approveRequest.Send();
               if (response.Success)
               {
                   MessageBox.Show(_success.Text);
                       BitbucketViewPullRequestFormLoad(null, null);
               }
               else
                   MessageBox.Show(string.Join(Environment.NewLine, response.Messages),
                       _error.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
           */
        }

        private void viewPullLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void createPullLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }
    }
}
