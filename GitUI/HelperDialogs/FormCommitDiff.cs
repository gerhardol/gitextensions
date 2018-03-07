using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using GitCommands;

namespace GitUI.HelperDialogs
{
    public sealed partial class FormCommitDiff : GitModuleForm
    {
        private FormCommitDiff(GitUICommands aCommands)
            : base(aCommands)
        {
            InitializeComponent();
            Translate();
            DiffText.ExtraDiffArgumentsChanged += DiffText_ExtraDiffArgumentsChanged;
            DiffFiles.Focus();
            DiffFiles.SetDiffs();
        }

        private FormCommitDiff()
            : this(null)
        {
        }

        public FormCommitDiff(GitUICommands aCommands, string revisionGuid)
            : this(aCommands)
        {
            // We cannot use the GitRevision from revision grid. When a filtered commit list
            // is shown (file history/normal filter) the parent guids are not the 'real' parents,
            // but the parents in the filtered list.
            GitRevision revision = Module.GetRevision(revisionGuid);

            if (revision != null)
            {
                commitInfo.Revision = revision;

                DiffFiles.SetDiffs(new[] { revision });

                Text = "Diff - " + GitRevision.ToShortSha(revision.Guid) + " - " + revision.AuthorDate + " - " + revision.Author + " - " + Module.WorkingDir; ;
            }
        }

        private async void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                await ViewSelectedDiff();
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task ViewSelectedDiff()
        {
            if (DiffFiles.SelectedItem != null && DiffFiles.Revision != null)
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    await DiffText.ViewChanges(DiffFiles.SelectedItemParent?.Guid, DiffFiles.Revision?.Guid, DiffFiles.SelectedItem, String.Empty);
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
            }
        }

        async void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            try
            {
                await ViewSelectedDiff();
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
