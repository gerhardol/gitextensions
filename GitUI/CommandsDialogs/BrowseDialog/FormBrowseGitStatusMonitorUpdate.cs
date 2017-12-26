using System;
using System.Collections.Generic;
using System.Windows.Forms;
using GitCommands;
using GitUI.UserControls.ToolStripClasses;

namespace GitUI.CommandsDialogs
{
    //Handle objects that are updated at Git status changes
    public interface IGitStatusMonitorUpdate
    {
        void Update(IList<GitItemStatus> status);
        void VisibilityChanged(bool value);
    }

    public sealed class FormBrowseGitStatusMonitorUpdate: IGitStatusMonitorUpdate
    {
        public FormBrowseGitStatusMonitorUpdate(ToolStripMenuItem toolStripGitStatus, RevisionGrid revisionGrid, RevisionDiff revisionDiff, string commitTranslatedString)
        {
            _toolStripGitStatus = toolStripGitStatus;
            _revisionGrid = revisionGrid;
            //_revisionDiff = revisionDiff;
            _commitTranslatedString = commitTranslatedString;
            _commitIconProvider = new CommitIconProvider();
        }

        public void Update(IList<GitItemStatus> status)
        {
            _toolStripGitStatus.Image = _commitIconProvider.GetCommitIcon(status);

            if (status.Count == 0)
                _toolStripGitStatus.Text = _commitTranslatedString;
            else
                _toolStripGitStatus.Text = string.Format(_commitTranslatedString + " ({0})", status.Count.ToString());

            _revisionGrid.UpdateArtificialCommitCount(status);
            //The diff filelist is not updated, as the selected diff is unset
            //_revisionDiff.RefreshArtificial();
        }

        public void VisibilityChanged(bool value)
        {
            _toolStripGitStatus.Visible = value;
            if (!value)
            {
                _toolStripGitStatus.Text = String.Empty;
            }
        }

        private readonly ToolStripMenuItem _toolStripGitStatus;
        private readonly RevisionGrid _revisionGrid;
        //private readonly RevisionDiff _revisionDiff;
        private readonly ICommitIconProvider _commitIconProvider;
        private readonly string _commitTranslatedString;
    }
}