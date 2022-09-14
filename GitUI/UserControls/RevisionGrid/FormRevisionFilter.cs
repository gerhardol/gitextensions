using System;

namespace GitUI.UserControls.RevisionGrid
{
    public partial class FormRevisionFilter : GitExtensionsDialog
    {
        private readonly FilterInfo _filterInfo;

        [Obsolete("For VS designer and translation test only. Do not remove.")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private FormRevisionFilter()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
        }

        public FormRevisionFilter(GitUICommands commands, FilterInfo filterInfo)
            : base(commands, enablePositionRestore: false)
        {
            InitializeComponent();
            InitializeComplete();
            _NO_TRANSLATE_lblSince.Text = TranslatedStrings.Since;
            _NO_TRANSLATE_lblUntil.Text = TranslatedStrings.Until;
            _NO_TRANSLATE_lblAuthor.Text = TranslatedStrings.Author;
            _NO_TRANSLATE_lblCommitter.Text = TranslatedStrings.Committer;
            _NO_TRANSLATE_lblMessage.Text = TranslatedStrings.Message;
            _NO_TRANSLATE_lblDiffContent.Text = "DiffContentxxx";
            _NO_TRANSLATE_lblRawLog.Text = "RawLogxxx";
            _NO_TRANSLATE_lblIgnoreCase.Text = TranslatedStrings.IgnoreCase;
            _NO_TRANSLATE_lblLimit.Text = TranslatedStrings.Limit;
            _NO_TRANSLATE_lblPathFilter.Text = TranslatedStrings.PathFilter;
            _NO_TRANSLATE_lblBranches.Text = TranslatedStrings.Branches;
            _NO_TRANSLATE_lblCurrentBranchOnlyCheck.Text = TranslatedStrings.ShowCurrentBranchOnly;
            _NO_TRANSLATE_lblReflogCheck.Text = "Reflogxxx";
            _NO_TRANSLATE_lblShowOnlyFirstParent.Text = "ShowOnlyFirstParentxxx";
            _NO_TRANSLATE_lblShowMergeCommits.Text = "ShowMergeCommitsxxx";
            _NO_TRANSLATE_lblSimplifyByDecoration.Text = TranslatedStrings.SimplifyByDecoration;
            _NO_TRANSLATE_lblFullHistory.Text = "FullHistoryxxx";
            _NO_TRANSLATE_lblSimplifyMerges.Text = "SimplifyMergesxx";

            _filterInfo = filterInfo;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            FilterInfo rawFilterInfo = _filterInfo with { IsRaw = true };

            SinceCheck.Checked = rawFilterInfo.ByDateFrom;
            Since.Value = rawFilterInfo.DateFrom == DateTime.MinValue ? DateTime.Today : rawFilterInfo.DateFrom;
            CheckUntil.Checked = rawFilterInfo.ByDateTo;
            Until.Value = rawFilterInfo.DateTo == DateTime.MinValue ? DateTime.Today : rawFilterInfo.DateTo;
            AuthorCheck.Checked = rawFilterInfo.ByAuthor;
            Author.Text = rawFilterInfo.Author;
            CommitterCheck.Checked = rawFilterInfo.ByCommitter;
            Committer.Text = rawFilterInfo.Committer;
            MessageCheck.Checked = rawFilterInfo.ByMessage;
            Message.Text = rawFilterInfo.Message;
            DiffContentCheck.Checked = rawFilterInfo.ByDiffContent;
            DiffContent.Text = rawFilterInfo.DiffContent;
            RawLogCheck.Checked = false; //// rawFilterInfo.ByMessage;
            RawLog.Text = ""; //// rawFilterInfo.Message;
            IgnoreCase.Checked = rawFilterInfo.IgnoreCase;
            IgnoreCase.Enabled = Author.Enabled || Committer.Enabled || MessageCheck.Checked || DiffContentCheck.Checked;
            CommitsLimitCheck.Checked = rawFilterInfo.ByCommitsLimit;
            _NO_TRANSLATE_CommitsLimit.Value = rawFilterInfo.CommitsLimit;
            PathFilterCheck.Checked = rawFilterInfo.ByPathFilter;
            PathFilter.Text = rawFilterInfo.PathFilter;
            BranchFilterCheck.Checked = rawFilterInfo.IsShowFilteredBranchesChecked;
            BranchFilter.Text = rawFilterInfo.BranchFilter;
            CurrentBranchOnlyCheck.Checked = rawFilterInfo.ShowCurrentBranchOnly;
            ReflogCheck.Checked = rawFilterInfo.ShowCurrentBranchOnly;
            ShowOnlyFirstParentCheck.Checked = rawFilterInfo.ShowSimplifyByDecoration; //// xxx change below
            ShowMergeCommitsCheck.Checked = rawFilterInfo.ShowSimplifyByDecoration;
            SimplifyByDecorationCheck.Checked = rawFilterInfo.ShowSimplifyByDecoration;
            FullHistoryCheck.Checked = rawFilterInfo.ShowSimplifyByDecoration;
            SimplifyMergesCheck.Checked = rawFilterInfo.ShowSimplifyByDecoration;

            UpdateFilters();
        }

        private void option_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFilters();

            // If CommitsLimitCheck was changed, the displayed value may need to be updated too
            if (sender == CommitsLimitCheck && !CommitsLimitCheck.Checked)
            {
                _NO_TRANSLATE_CommitsLimit.Value = _filterInfo.CommitsLimitDefault;
            }
        }

        private void UpdateFilters()
        {
            Since.Enabled = SinceCheck.Checked;
            Until.Enabled = CheckUntil.Checked;
            Author.Enabled = AuthorCheck.Checked;
            Committer.Enabled = CommitterCheck.Checked;
            Message.Enabled = MessageCheck.Checked;
            DiffContent.Enabled = DiffContentCheck.Checked;
            RawLog.Enabled = RawLogCheck.Checked;
            IgnoreCase.Enabled = Author.Enabled || Committer.Enabled || MessageCheck.Checked || DiffContentCheck.Checked;
            _NO_TRANSLATE_CommitsLimit.Enabled = CommitsLimitCheck.Checked;
            PathFilter.Enabled = PathFilterCheck.Checked;

            CurrentBranchOnlyCheck.Enabled = !ReflogCheck.Checked;
            BranchFilterCheck.Enabled = !CurrentBranchOnlyCheck.Checked && !ReflogCheck.Checked;
            BranchFilter.Enabled = BranchFilterCheck.Checked;
        }

        private void OkClick(object sender, EventArgs e)
        {
            _filterInfo.ByDateFrom = SinceCheck.Checked;
            _filterInfo.DateFrom = Since.Value;
            _filterInfo.ByDateTo = CheckUntil.Checked;
            _filterInfo.DateTo = Until.Value;
            _filterInfo.ByAuthor = AuthorCheck.Checked;
            _filterInfo.Author = Author.Text;
            _filterInfo.ByCommitter = CommitterCheck.Checked;
            _filterInfo.Committer = Committer.Text;
            _filterInfo.ByMessage = MessageCheck.Checked;
            _filterInfo.Message = Message.Text;
            _filterInfo.ByDiffContent = DiffContentCheck.Checked;
            _filterInfo.DiffContent = DiffContent.Text;
            ////_filterInfo.ByMessage = RawLogCheck.Checked;
            ////_filterInfo.Message = RawLog.Text;
            _filterInfo.IgnoreCase = IgnoreCase.Checked;
            _filterInfo.ByCommitsLimit = CommitsLimitCheck.Checked;
            _filterInfo.CommitsLimit = (int)_NO_TRANSLATE_CommitsLimit.Value;
            _filterInfo.ByPathFilter = PathFilterCheck.Checked;
            _filterInfo.PathFilter = PathFilter.Text;
            _filterInfo.ByBranchFilter = BranchFilterCheck.Checked;
            _filterInfo.BranchFilter = BranchFilter.Text;
            ////xxx_filterInfo.ShowCurrentBranchOnly = ReflogCheck.Checked;
            _filterInfo.ShowCurrentBranchOnly = CurrentBranchOnlyCheck.Checked;
            ////xxx_filterInfo.ShowSimplifyByDecoration = ShowOnlyFirstParentCheck.Checked;
            ////xxx_filterInfo.ShowSimplifyByDecoration = ShowMergeCommitsCheck.Checked;
            _filterInfo.ShowSimplifyByDecoration = SimplifyByDecorationCheck.Checked;
            ////xxx_filterInfo.ShowSimplifyByDecoration = FullHistoryCheck.Checked;
            ////xxx_filterInfo.ShowSimplifyByDecoration = SimplifyMergesCheck.Checked;
        }
    }
}
