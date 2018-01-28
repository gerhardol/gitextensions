using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitUI.CommandsDialogs.BrowseDialog;
using GitUI.HelperDialogs;
using ResourceManager;
using GitUI.Hotkey;
using GitUI.UserControls.RevisionGridClasses;

namespace GitUI.CommandsDialogs
{
    public partial class RevisionDiff : GitModuleControl
    {
        private readonly TranslationString _saveFileFilterCurrentFormat = new TranslationString("Current format");
        private readonly TranslationString _saveFileFilterAllFiles = new TranslationString("All files");
        private readonly TranslationString _deleteSelectedFilesCaption = new TranslationString("Delete");
        private readonly TranslationString _deleteSelectedFiles =
            new TranslationString("Are you sure you want delete the selected file(s)?");
        private readonly TranslationString _deleteFailed = new TranslationString("Delete file failed");
        private readonly TranslationString _multipleDescription = new TranslationString("<multiple>");

        private RevisionGrid _revisionGrid;
        private RevisionFileTree _revisionFileTree;
        private string _oldRevision;
        private GitItemStatus _oldDiffItem;
        private IRevisionDiffController _revisionDiffController;
        private readonly IFullPathResolver _fullPathResolver;

        public RevisionDiff()
        {
            InitializeComponent();
            DiffFiles.AlwaysRevisionGroups = true;
            Translate();
            this.HotkeysEnabled = true;
            _fullPathResolver = new FullPathResolver(() => Module.WorkingDir);
        }

        public void ForceRefreshRevisions()
        {
            var revisions = _revisionGrid.GetSelectedRevisions();
            if (revisions.Count != 0)
            {
                _oldRevision = revisions[0].Guid;
                _oldDiffItem = DiffFiles.SelectedItem;
            }
            else
            {
                _oldRevision = null;
                _oldDiffItem = null;
            }
            RefreshArtificial();
        }

        public void RefreshArtificial()
        {
            if (this.Visible)
            {
                var revisions = _revisionGrid.GetSelectedRevisions();

                if (revisions.Count > 0 && revisions[0].IsArtificial())
                {
                    DiffFiles.SetDiffs(revisions);
                }
            }
        }

        #region Hotkey commands

        public static readonly string HotkeySettingsName = "BrowseDiff";

        internal enum Commands
        {
            DeleteSelectedFiles,
        }

        protected override bool ExecuteCommand(int cmd)
        {
            Commands command = (Commands)cmd;

            switch (command)
            {
                case Commands.DeleteSelectedFiles: return DeleteSelectedFiles();
                default: return base.ExecuteCommand(cmd);
            }
        }

        internal Keys GetShortcutKeys(Commands cmd)
        {
            return GetShortcutKeys((int)cmd);
        }

        #endregion

        public void GetTabText()
        {
            var revisions = _revisionGrid.GetSelectedRevisions();

            DiffText.SaveCurrentScrollPos();
            DiffFiles.SetDiffs(revisions);
            if (_oldDiffItem != null && revisions.Count > 0 && revisions[0].Guid == _oldRevision)
            {
                DiffFiles.SelectedItem = _oldDiffItem;
                _oldDiffItem = null;
                _oldRevision = null;
            }
        }

        public void Bind(RevisionGrid revisionGrid, RevisionFileTree revisionFileTree)
        {
            _revisionGrid = revisionGrid;
            _revisionFileTree = revisionFileTree;
        }

        public void InitSplitterManager(SplitterManager splitterManager)
        {
            splitterManager.AddSplitter(DiffSplitContainer, "DiffSplitContainer");
        }

        public void ReloadHotkeys()
        {
            this.Hotkeys = HotkeySettingsManager.LoadHotkeys(HotkeySettingsName);
            this.diffDeleteFileToolStripMenuItem.ShortcutKeyDisplayString = GetShortcutKeys(Commands.DeleteSelectedFiles).ToShortcutKeyDisplayString();
            DiffText.ReloadHotkeys();
        }


        protected override void OnRuntimeLoad(EventArgs e)
        {
            _revisionDiffController = new RevisionDiffController();

            DiffFiles.FilterVisible = true;
            DiffFiles.DescribeRevision = DescribeRevision;
            DiffText.SetFileLoader(GetNextPatchFile);
            DiffText.Font = AppSettings.DiffFont;
            ReloadHotkeys();

            GotFocus += (s, e1) => DiffFiles.Focus();

            base.OnRuntimeLoad(e);
        }


        private string DescribeRevision(string sha1)
        {
            return DescribeRevision(sha1, 0);
        }

        private string DescribeRevision(string sha1, int maxLength)
        {
            if (sha1.IsNullOrEmpty())
            {
                //No parent at all, present as working directory
                return Strings.GetCurrentUnstagedChanges();
            }
            var revision = _revisionGrid.GetRevision(sha1);
            if (revision == null)
            {
                return sha1.ShortenTo(8);
            }

            return _revisionGrid.DescribeRevision(revision, maxLength);
        }

        /// <summary>
        /// Provide a description for the first selected or parent to the "primary" selected last 
        /// </summary>
        /// <returns></returns>
        private string DescribeSelectedParentRevision(bool showUnstaged)
        {
            var parents = DiffFiles.SelectedItemsWithParent
                .Where(i => showUnstaged || !(i.ParentGuid == GitRevision.UnstagedGuid || i.ParentGuid.IsNullOrWhiteSpace()))
                .Select(i => i.ParentGuid)
                .Distinct()
                .Count();
            if (parents == 0)
            {
                return null;
            }
            else if (parents == 1)
            {
                return DescribeRevision(DiffFiles.SelectedItemsWithParent.First().ParentGuid, 50);
            }
            else
            {
                return _multipleDescription.Text;
            }
        }

        private static int GetNextIdx(int curIdx, int maxIdx, bool searchBackward)
        {
            if (searchBackward)
            {
                if (curIdx == 0)
                {
                    curIdx = maxIdx;
                }
                else
                {
                    curIdx--;
                }
            }
            else
            {
                if (curIdx == maxIdx)
                {
                    curIdx = 0;
                }
                else
                {
                    curIdx++;
                }
            }
            return curIdx;
        }

        private Tuple<int, string> GetNextPatchFile(bool searchBackward)
        {
            var revisions = _revisionGrid.GetSelectedRevisions();
            if (revisions.Count == 0)
                return null;
            int idx = DiffFiles.SelectedIndex;
            if (idx == -1)
                return new Tuple<int, string>(idx, null);

            idx = GetNextIdx(idx, DiffFiles.GitItemStatuses.Count() - 1, searchBackward);
            DiffFiles.SetSelectedIndex(idx, notify: false);

            return new Tuple<int, string>(idx, DiffText.GetSelectedPatch(DiffFiles.SelectedItemParent, revisions[0].Guid, DiffFiles.SelectedItem));
        }

        private ContextMenuSelectionInfo GetSelectionInfo()
        {
            IList<GitRevision> selectedRevisions = _revisionGrid.GetSelectedRevisions();

            var selectedItemStatus = DiffFiles.SelectedItem;
            bool isAnyCombinedDiff = DiffFiles.SelectedItemParents.Any(item => item == DiffFiles.CombinedDiff.Text);
            bool isExactlyOneItemSelected = DiffFiles.SelectedItems.Count() == 1;
            bool isAnyItemSelected = DiffFiles.SelectedItems.Count() > 0;
            bool isBareRepository = Module.IsBareRepository();
            bool singleFileExists = isExactlyOneItemSelected && File.Exists(_fullPathResolver.Resolve(DiffFiles.SelectedItem.Name));
            bool isAnyTracked = DiffFiles.SelectedItems.Any(item => item.IsTracked);

            var selectionInfo = new ContextMenuSelectionInfo(selectedRevisions, selectedItemStatus, isAnyCombinedDiff, isExactlyOneItemSelected, isAnyItemSelected, isBareRepository, singleFileExists, isAnyTracked);
            return selectionInfo;
        }

        private void ResetSelectedItemsTo(bool actsAsChild)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();

            if (!DiffFiles.SelectedItems.Any())
            {
                return;
            }

            var selectedItems = DiffFiles.SelectedItems;
            if (actsAsChild)
            {
                //selected, all are the same
                string revision = revisions[0].Guid;
                var deletedItems = selectedItems.Where(item => item.IsDeleted);
                Module.RemoveFiles(deletedItems.Select(item => item.Name), false);

                var itemsToCheckout = selectedItems.Where(item => !item.IsDeleted);
                Module.CheckoutFiles(itemsToCheckout.Select(item => item.Name), revision, false);
            }
            else //acts as parent
            {
                //if file is new to the parent, it has to be removed
                var addedItems = selectedItems.Where(item => item.IsNew);
                Module.RemoveFiles(addedItems.Select(item => item.Name), false);

                foreach (var parent in DiffFiles.SelectedItemsWithParent.Select(item => item.ParentGuid).Distinct())
                {
                    var itemsToCheckout = DiffFiles.SelectedItemsWithParent.Where(item => !item.Item.IsNew && item.ParentGuid==parent);
                    Module.CheckoutFiles(itemsToCheckout.Select(item => item.Item.Name), parent, false);
                }
            }
            RefreshArtificial();
        }

        private void ShowSelectedFileDiff()
        {
            var revisions = _revisionGrid.GetSelectedRevisions();
            if (DiffFiles.SelectedItem == null || revisions.Count() == 0)
            {
                DiffText.ViewPatch("");
                return;
            }

            if (revisions.Count() == 1 && DiffFiles.SelectedItemParent != null)
            {
                if (!string.IsNullOrWhiteSpace(DiffFiles.SelectedItemParent)
                    && DiffFiles.SelectedItemParent == DiffFiles.CombinedDiff.Text)
                {
                    var diffOfConflict = Module.GetCombinedDiffContent(revisions[0], DiffFiles.SelectedItem.Name,
                        DiffText.GetExtraDiffArguments(), DiffText.Encoding);

                    if (string.IsNullOrWhiteSpace(diffOfConflict))
                    {
                        diffOfConflict = Strings.GetUninterestingDiffOmitted();
                    }

                    DiffText.ViewPatch(diffOfConflict);
                    return;
                }
            }
            DiffText.ViewChanges(DiffFiles.SelectedItemParent, revisions[0].Guid, DiffFiles.SelectedItem, String.Empty);
        }


        private void DiffFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowSelectedFileDiff();
        }

        private void DiffFiles_DoubleClick(object sender, EventArgs e)
        {
            if (DiffFiles.SelectedItem == null)
                return;

            if (AppSettings.OpenSubmoduleDiffInSeparateWindow && DiffFiles.SelectedItem.IsSubmodule)
            {
                var submoduleName = DiffFiles.SelectedItem.Name;
                DiffFiles.SelectedItem.SubmoduleStatus.ContinueWith(
                    (t) =>
                    {
                        Process process = new Process();
                        process.StartInfo.FileName = Application.ExecutablePath;
                        process.StartInfo.Arguments = "browse -commit=" + t.Result.Commit;
                        process.StartInfo.WorkingDirectory = _fullPathResolver.Resolve(submoduleName.EnsureTrailingPathSeparator());
                        process.Start();
                    });
            }
            else
            {

                UICommands.StartFileHistoryDialog(this, (DiffFiles.SelectedItem).Name);
            }
        }

        private void DiffFiles_DataSourceChanged(object sender, EventArgs e)
        {
            if (DiffFiles.GitItemStatuses == null || !DiffFiles.GitItemStatuses.Any())
                DiffText.ViewPatch(String.Empty);
        }

        private void DiffText_ExtraDiffArgumentsChanged(object sender, EventArgs e)
        {
            ShowSelectedFileDiff();
        }

        private void diffShowInFileTreeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // switch to view (and fills the first level of file tree data model if not already done)
            (FindForm() as FormBrowse)?.ExecuteCommand(FormBrowse.Commands.FocusFileTree);
            _revisionFileTree.ExpandToFile(DiffFiles.SelectedItems.First().Name);
        }

        private void DiffContextMenu_Opening(object sender, CancelEventArgs e)
        {
            var selectionInfo = GetSelectionInfo();

            //Many options have no meaning for artificial commits or submodules
            //Hide the obviously no action options when single selected, handle them in actions if multi select

            openWithDifftoolToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowDifftoolMenus(selectionInfo);
            saveAsToolStripMenuItem1.Visible = _revisionDiffController.ShouldShowMenuSaveAs(selectionInfo);
            copyFilenameToClipboardToolStripMenuItem1.Enabled = _revisionDiffController.ShouldShowMenuCopyFileName(selectionInfo);

            stageFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuStage(selectionInfo);
            unstageFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuUnstage(selectionInfo);

            cherryPickSelectedDiffFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuCherryPick(selectionInfo);
            //Visibility of FileTree is not known, assume (CommitInfoTabControl.Contains(TreeTabPage);)
            diffShowInFileTreeToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuShowInFileTree(selectionInfo);
            fileHistoryDiffToolstripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuFileHistory(selectionInfo);
            blameToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuBlame(selectionInfo);
            resetFileToToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuResetFile(selectionInfo);

            diffEditFileToolStripMenuItem.Visible =
               diffDeleteFileToolStripMenuItem.Visible = _revisionDiffController.ShouldShowMenuEditFile(selectionInfo);

            diffCommitSubmoduleChanges.Visible =
                diffResetSubmoduleChanges.Visible =
                diffStashSubmoduleChangesToolStripMenuItem.Visible =
                diffUpdateSubmoduleMenuItem.Visible =
                diffSubmoduleSummaryMenuItem.Visible =
                diffUpdateSubmoduleMenuItem.Visible = _revisionDiffController.ShouldShowSubmoduleMenus(selectionInfo);

            diffToolStripSeparator13.Visible = _revisionDiffController.ShouldShowMenuEditFile(selectionInfo) || _revisionDiffController.ShouldShowSubmoduleMenus(selectionInfo);

            // openContainingFolderToolStripMenuItem.Enabled or not
            {
                openContainingFolderToolStripMenuItem.Enabled = false;

                foreach (var item in DiffFiles.SelectedItems)
                {
                    string filePath = _fullPathResolver.Resolve(item.Name);
                    if (FormBrowseUtil.FileOrParentDirectoryExists(filePath))
                    {
                        openContainingFolderToolStripMenuItem.Enabled = true;
                        break;
                    }
                }
            }
        }

        private void blameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
            {
                GitRevision revision = _revisionGrid.GetSelectedRevisions().FirstOrDefault();
                UICommands.StartFileHistoryDialog(this, item.Name, revision, true, true);
            }
        }

        private void StageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            var files = new List<GitItemStatus>();
            foreach (var item in DiffFiles.SelectedItems)
            {
                files.Add(item);
            }
            bool err;
            Module.StageFiles(files, out err);
            RefreshArtificial();
        }

        private void UnstageFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            var files = new List<GitItemStatus>();
            foreach (var item in DiffFiles.SelectedItems.Where(i => i.IsStaged))
            {
                if (!item.IsNew)
                {
                    Module.UnstageFileToRemove(item.Name);

                    if (item.IsRenamed)
                        Module.UnstageFileToRemove(item.OldName);
                }
                else
                {
                    files.Add(item);
                }
            }

            Module.UnstageFiles(files);
            RefreshArtificial();
        }

        private void cherryPickSelectedDiffFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DiffText.CherryPickAllChanges();
        }

        private void copyFilenameToClipboardToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FormBrowse.CopyFullPathToClipboard(DiffFiles, Module);
        }

        private void findInDiffToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var candidates = DiffFiles.GitItemStatuses;

            Func<string, IList<GitItemStatus>> FindDiffFilesMatches = (string name) =>
            {

                string nameAsLower = name.ToLower();

                return candidates.Where(item =>
                {
                    return item.Name != null && item.Name.ToLower().Contains(nameAsLower)
                        || item.OldName != null && item.OldName.ToLower().Contains(nameAsLower);
                }
                    ).ToList();
            };

            GitItemStatus selectedItem;
            using (var searchWindow = new SearchWindow<GitItemStatus>(FindDiffFilesMatches)
            {
                Owner = FindForm()
            })
            {
                searchWindow.ShowDialog(this);
                selectedItem = searchWindow.SelectedItem;
            }
            if (selectedItem != null)
            {
                DiffFiles.SelectedItem = selectedItem;
            }
        }

        private void fileHistoryDiffToolstripMenuItem_Click(object sender, EventArgs e)
        {
            GitItemStatus item = DiffFiles.SelectedItem;

            if (item.IsTracked)
            {
                GitRevision revision = _revisionGrid.GetSelectedRevisions().FirstOrDefault();
                UICommands.StartFileHistoryDialog(this, item.Name, revision, false);
            }
        }

        private void openContainingFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormBrowse.OpenContainingFolder(DiffFiles, Module);
        }

        private void openWithDifftoolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();
            if (DiffFiles.SelectedItem == null || revisions == null || revisions.Count == 0)
                return;

            GitUI.RevisionDiffKind diffKind;

            if (sender == aLocalToolStripMenuItem)
                diffKind = GitUI.RevisionDiffKind.DiffALocal;
            else if (sender == bLocalToolStripMenuItem)
                diffKind = GitUI.RevisionDiffKind.DiffBLocal;
            else if (sender == parentOfALocalToolStripMenuItem)
                diffKind = GitUI.RevisionDiffKind.DiffAParentLocal;
            else if (sender == parentOfBLocalToolStripMenuItem)
                diffKind = GitUI.RevisionDiffKind.DiffBParentLocal;
            else
            {
                diffKind = GitUI.RevisionDiffKind.DiffAB;
            }

            foreach (var itemWithParent in DiffFiles.SelectedItemsWithParent)
            {
                IList<GitRevision> revs = new List<GitRevision> { revisions[0], new GitRevision(Module, itemWithParent.ParentGuid) };
                _revisionGrid.OpenWithDifftool(revs, itemWithParent.Item.Name, itemWithParent.Item.OldName, diffKind, itemWithParent.Item.IsTracked);
            }
        }

        private ContextMenuDiffToolInfo GetContextMenuDiffToolInfo()
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();
            if (revisions.Count == 0)
            {
                //Should be blocked in the GUI but not an error to show to the user
                return null;
            }

            bool aIsLocal = DiffFiles.SelectedItemsWithParent.Any(i => i.ParentGuid == GitRevision.UnstagedGuid);
            bool bIsLocal = revisions[0].Guid == GitRevision.UnstagedGuid;

            bool localExists = DiffFiles.SelectedItems.Any(item => !item.IsTracked);
            if (!localExists)
            {
                //enable *<->Local items only when (any) local file exists
                foreach (var item in DiffFiles.SelectedItems)
                {
                    string filePath = FormBrowseUtil.GetFullPathFromGitItemStatus(Module, item);
                    if (File.Exists(filePath))
                    {
                        localExists = true;
                        break;
                    }
                }
            }

            var selectionInfo = new ContextMenuDiffToolInfo(aIsLocal, bIsLocal, localExists);
            return selectionInfo;
        }

        private void openWithDifftoolToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();
            ContextMenuDiffToolInfo selectionInfo = GetContextMenuDiffToolInfo();

            if (DiffFiles.SelectedItemsWithParent.Count() > 0 )
            {
                bDiffCaptionMenuItem.Text = "B: (" + _revisionGrid.DescribeRevision(revisions[0], 50) + ")";
                bDiffCaptionMenuItem.Tag = "caption";
                bDiffCaptionMenuItem.Visible = true;
                MenuUtil.SetAsCaptionMenuItem(bDiffCaptionMenuItem, DiffContextMenu);

                aDiffCaptionMenuItem.Text = "A:";
                var parentDesc = DescribeSelectedParentRevision(true);
                if (parentDesc.IsNotNullOrWhitespace())
                {
                    aDiffCaptionMenuItem.Text += " (" + parentDesc + ")";
                }
                aDiffCaptionMenuItem.Tag = "caption";
                aDiffCaptionMenuItem.Visible = true;
                MenuUtil.SetAsCaptionMenuItem(aDiffCaptionMenuItem, DiffContextMenu);
            }
            else
            {
                aDiffCaptionMenuItem.Visible = false;
                bDiffCaptionMenuItem.Visible = false;
            }

            aBToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuAB(selectionInfo);
            aLocalToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuALocal(selectionInfo);
            bLocalToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuBLocal(selectionInfo);
            parentOfALocalToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuAParentLocal(selectionInfo);
            parentOfBLocalToolStripMenuItem.Enabled = _revisionDiffController.ShouldShowMenuBParentLocal(selectionInfo);
            parentOfALocalToolStripMenuItem.Visible = parentOfALocalToolStripMenuItem.Enabled || _revisionDiffController.ShouldShowMenuAParent(selectionInfo);
            parentOfBLocalToolStripMenuItem.Visible = parentOfBLocalToolStripMenuItem.Enabled || _revisionDiffController.ShouldShowMenuBParent(selectionInfo);
        }

        private void resetFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetSelectedItemsTo(sender == resetFileToSelectedToolStripMenuItem);
        }

        private void resetFileToToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();

            if (revisions.Count == 0)
            {
                resetFileToSelectedToolStripMenuItem.Visible = false;
                resetFileToParentToolStripMenuItem.Visible = false;
                return;
            }

            if (revisions[0].Guid == GitRevision.UnstagedGuid)
            {
                resetFileToSelectedToolStripMenuItem.Visible = false;
            }
            else
            {
                resetFileToSelectedToolStripMenuItem.Visible = true;
                TranslateItem(resetFileToSelectedToolStripMenuItem.Name, resetFileToSelectedToolStripMenuItem);
                resetFileToSelectedToolStripMenuItem.Text += " (" + _revisionGrid.DescribeRevision(revisions[0], 50) + ")";
            }

            var parentDesc = DescribeSelectedParentRevision(false);
            if (parentDesc.IsNotNullOrWhitespace())
            {
                resetFileToParentToolStripMenuItem.Visible = true;
                TranslateItem(resetFileToParentToolStripMenuItem.Name, resetFileToParentToolStripMenuItem);
                resetFileToParentToolStripMenuItem.Text += " (" + parentDesc + ")";
            }
            else
            {
                resetFileToParentToolStripMenuItem.Visible = false;
            }
        }

        private void saveAsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            IList<GitRevision> revisions = _revisionGrid.GetSelectedRevisions();

            if (revisions.Count == 0)
                return;

            if (DiffFiles.SelectedItem == null)
                return;

            GitItemStatus item = DiffFiles.SelectedItem;

            var fullName = _fullPathResolver.Resolve(item.Name);
            using (var fileDialog =
                new SaveFileDialog
                {
                    InitialDirectory = Path.GetDirectoryName(fullName),
                    FileName = Path.GetFileName(fullName),
                    DefaultExt = GitCommandHelpers.GetFileExtension(fullName),
                    AddExtension = true
                })
            {
                fileDialog.Filter =
                    _saveFileFilterCurrentFormat.Text + " (*." +
                    fileDialog.DefaultExt + ")|*." +
                    fileDialog.DefaultExt +
                    "|" + _saveFileFilterAllFiles.Text + " (*.*)|*.*";

                if (fileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    Module.SaveBlobAs(fileDialog.FileName, string.Format("{0}:\"{1}\"", revisions[0].Guid, item.Name));
                }
            }
        }

        private bool DeleteSelectedFiles()
        {
            try
            {
                if (DiffFiles.SelectedItem == null || !DiffFiles.Revision.IsArtificial() ||
                    MessageBox.Show(this, _deleteSelectedFiles.Text, _deleteSelectedFilesCaption.Text, MessageBoxButtons.YesNo) !=
                    DialogResult.Yes)
                {
                    return false;
                }

                var selectedItems = DiffFiles.SelectedItems;
                if (DiffFiles.Revision.Guid == GitRevision.IndexGuid)
                {
                    var files = new List<GitItemStatus>();
                    var stagedItems = selectedItems.Where(item => item.IsStaged);
                    foreach (var item in stagedItems)
                    {
                        if (!item.IsNew)
                        {
                            Module.UnstageFileToRemove(item.Name);

                            if (item.IsRenamed)
                                Module.UnstageFileToRemove(item.OldName);
                        }
                        else
                        {
                            files.Add(item);
                        }
                    }
                    Module.UnstageFiles(files);
                }
                DiffFiles.StoreNextIndexToSelect();
                var items = DiffFiles.SelectedItems.Where(item => !item.IsSubmodule);
                foreach (var item in items)
                {
                    File.Delete(_fullPathResolver.Resolve(item.Name));
                }
                RefreshArtificial();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, _deleteFailed.Text + Environment.NewLine + ex.Message);
                return false;
            }
            return true;
        }

        private void diffDeleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedFiles();
        }

        private void diffEditFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = DiffFiles.SelectedItem;
            var fileName = _fullPathResolver.Resolve(item.Name);

            UICommands.StartFileEditorDialog(fileName);
            RefreshArtificial();
        }

        private void diffCommitSubmoduleChanges_Click(object sender, EventArgs e)
        {
            GitUICommands submodulCommands = new GitUICommands(_fullPathResolver.Resolve(DiffFiles.SelectedItem.Name.EnsureTrailingPathSeparator()));
            submodulCommands.StartCommitDialog(this, false);
            RefreshArtificial();
        }

        private void diffResetSubmoduleChanges_Click(object sender, EventArgs e)
        {
            var unStagedFiles = DiffFiles.SelectedItems.ToList();
            if (unStagedFiles.Count == 0)
                return;

            // Show a form asking the user if they want to reset the changes.
            FormResetChanges.ActionEnum resetType = FormResetChanges.ShowResetDialog(this, true, true);
            if (resetType == FormResetChanges.ActionEnum.Cancel)
                return;

            foreach (var item in unStagedFiles.Where(it => it.IsSubmodule))
            {
                GitModule module = Module.GetSubmodule(item.Name);

                // Reset all changes.
                module.ResetHard("");

                // Also delete new files, if requested.
                if (resetType == FormResetChanges.ActionEnum.ResetAndDelete)
                {
                    var unstagedFiles = module.GetUnstagedFiles();
                    foreach (var file in unstagedFiles.Where(file => file.IsNew))
                    {
                        try
                        {
                            string path = _fullPathResolver.Resolve(file.Name);
                            if (File.Exists(path))
                                File.Delete(path);
                            else
                                Directory.Delete(path, true);
                        }
                        catch (IOException) { }
                        catch (UnauthorizedAccessException) { }
                    }
                }
            }
            RefreshArtificial();
        }
        
        private void diffUpdateSubmoduleMenuItem_Click(object sender, EventArgs e)
        {
            var unStagedFiles = DiffFiles.SelectedItems.ToList();
            if (unStagedFiles.Count == 0)
                return;

            foreach (var item in unStagedFiles.Where(it => it.IsSubmodule))
            {
                FormProcess.ShowDialog((FindForm() as FormBrowse), GitCommandHelpers.SubmoduleUpdateCmd(item.Name));
            }
            RefreshArtificial();
        }

        private void diffStashSubmoduleChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var unStagedFiles = DiffFiles.SelectedItems.ToList();
            if (unStagedFiles.Count == 0)
                return;

            foreach (var item in unStagedFiles.Where(it => it.IsSubmodule))
            {
                GitUICommands uiCmds = new GitUICommands(Module.GetSubmodule(item.Name));
                uiCmds.StashSave(this, AppSettings.IncludeUntrackedFilesInManualStash);
            }
            RefreshArtificial();
        }

        private void diffSubmoduleSummaryMenuItem_Click(object sender, EventArgs e)
        {
            string summary = Module.GetSubmoduleSummary(DiffFiles.SelectedItem.Name);
            using (var frm = new FormEdit(summary)) frm.ShowDialog(this);
        }
    }
}
