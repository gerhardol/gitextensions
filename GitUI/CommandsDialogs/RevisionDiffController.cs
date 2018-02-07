using System.Collections.Generic;
using System.IO;
using System.Linq;
using GitCommands;

namespace GitUI.CommandsDialogs
{
    public interface IRevisionDiffController
    {
        bool ShouldShowMenuBlame(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuCherryPick(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuEditFile(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowResetFileMenus(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuFileHistory(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuSaveAs(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuCopyFileName(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuShowInFileTree(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuStage(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuUnstage(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowSubmoduleMenus(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowDifftoolMenus(ContextMenuSelectionInfo selectionInfo);

        bool LocalExists(IEnumerable<GitItemStatusWithParent> selectedItemsWithParent, IFullPathResolver fullPathResolver);

        bool ShouldShowMenuAB(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuALocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuBLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuAParentLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuBParentLocal(ContextMenuDiffToolInfo selectionInfo);
    }

    public sealed class ContextMenuSelectionInfo
    {
        public ContextMenuSelectionInfo(IList<GitRevision> selectedRevisions, GitItemStatus selectedDiff, bool isAnyCombinedDiff, bool isSingleGitItemSelected, bool isAnyItemSelected, bool isBareRepository, bool singleFileExists, bool isAnyTracked, bool isAnySubmodule)
        {
            SelectedRevisions = selectedRevisions;
            SelectedDiff = selectedDiff;
            IsAnyCombinedDiff  = isAnyCombinedDiff;
            IsSingleGitItemSelected = isSingleGitItemSelected;
            IsAnyItemSelected = isAnyItemSelected;
            IsBareRepository = isBareRepository;
            SingleFileExists = singleFileExists;
            IsAnyTracked = isAnyTracked;
            IsAnySubmodule = isAnySubmodule;
        }
        public IEnumerable<GitRevision> SelectedRevisions { get; }
        public GitItemStatus SelectedDiff { get; }
        public bool IsAnyCombinedDiff { get; }
        public bool IsSingleGitItemSelected { get; }
        public bool IsAnyItemSelected { get; }
        public bool IsBareRepository { get; }
        public bool SingleFileExists { get; }
        public bool IsAnyTracked { get; }
        public bool IsAnySubmodule { get; }
    }

    public sealed class ContextMenuDiffToolInfo
    {
        public ContextMenuDiffToolInfo(IList<GitRevision> selectedRevisions, IEnumerable<GitItemStatusWithParent> selectedItemsWithParent, bool localExists)
        {
            SelectedRevisions = selectedRevisions;
            SelectedItemsWithParent = selectedItemsWithParent;
            LocalExists = localExists;
        }
        public IEnumerable<GitRevision> SelectedRevisions { get; }
        public IEnumerable<GitItemStatusWithParent> SelectedItemsWithParent { get; }
        public bool LocalExists { get; }
    }

    public sealed class RevisionDiffController : IRevisionDiffController
    {
        //The enabling of menu items is related to how the actions have been implemented

        #region Menu dropdowns
        public bool ShouldShowDifftoolMenus(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemSelected && !selectionInfo.IsAnyCombinedDiff && selectionInfo.IsAnyTracked;
        }

        public bool ShouldShowResetFileMenus(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemSelected && !selectionInfo.IsAnyCombinedDiff && !selectionInfo.IsBareRepository
                && (!selectionInfo.IsSingleGitItemSelected || (!selectionInfo.SelectedDiff.IsSubmodule && selectionInfo.SelectedDiff.IsTracked));
        }
        #endregion

        #region Main menu items
        public bool ShouldShowMenuSaveAs(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.SelectedDiff.IsSubmodule
                && selectionInfo.SelectedRevisions.Count() == 1 && !selectionInfo.SelectedRevisions.First().IsArtificial();
        }

        public bool ShouldShowMenuCherryPick(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.SelectedDiff.IsSubmodule
                && !selectionInfo.IsAnyCombinedDiff && !selectionInfo.IsBareRepository
                && !selectionInfo.SelectedRevisions.First().IsArtificial();
        }

        //Stage/unstage must limit the selected items, IsStaged is not reflecting Staged status
        public bool ShouldShowMenuStage(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedRevisions.Count() == 1 && selectionInfo.SelectedRevisions.First().Guid == GitRevision.UnstagedGuid;
        }

        public bool ShouldShowMenuUnstage(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.SelectedRevisions.Count() == 1 && selectionInfo.SelectedRevisions.First().Guid == GitRevision.IndexGuid;
        }

        public bool ShouldShowSubmoduleMenus(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnySubmodule && selectionInfo.SelectedRevisions.Any(i => i.Guid == GitRevision.UnstagedGuid);
        }

        public bool ShouldShowMenuEditFile(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.SelectedDiff.IsSubmodule && selectionInfo.SingleFileExists;
        }

        public bool ShouldShowMenuCopyFileName(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemSelected;
        }

        public bool ShouldShowMenuShowInFileTree(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.SelectedRevisions.First().IsArtificial();
        }

        public bool ShouldShowMenuFileHistory(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && selectionInfo.SelectedDiff.IsTracked;
        }

        public bool ShouldShowMenuBlame(ContextMenuSelectionInfo selectionInfo)
        {
            return ShouldShowMenuFileHistory(selectionInfo) && !selectionInfo.SelectedDiff.IsSubmodule;
        }
        #endregion

        public bool LocalExists(IEnumerable<GitItemStatusWithParent> selectedItemsWithParent, IFullPathResolver fullPathResolver)
        {
            bool localExists = selectedItemsWithParent.Any(item => !item.Item.IsTracked);
            if (!localExists)
            {
                //enable *<->Local items only when (any) local file exists
                foreach (var item in selectedItemsWithParent)
                {
                    string filePath = fullPathResolver.Resolve(item.Item.Name);
                    if (File.Exists(filePath))
                    {
                        localExists = true;
                        break;
                    }
                }
            }

            return localExists;
        }

        #region difftool submenu
        public bool ShouldShowMenuAB(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevisions.Count() > 0;
        }

        public bool ShouldShowMenuALocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.LocalExists && selectionInfo.SelectedRevisions.Count() > 0
                //A exists (Can only determine that A does not exist if A is parent (one selected) and B is new (compare to bParentExists))
                && (selectionInfo.SelectedRevisions.Count() > 1
                  || selectionInfo.SelectedItemsWithParent.Any(i => !i.Item.IsNew))
                //A is not local
                && selectionInfo.SelectedItemsWithParent.Any(i => i.ParentGuid != GitRevision.UnstagedGuid);
        }

        public bool ShouldShowMenuBLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.LocalExists && selectionInfo.SelectedRevisions.Count() > 0
                //B exists
                && selectionInfo.SelectedItemsWithParent.Any(i => !i.Item.IsDeleted)
                //B is not local
                && selectionInfo.SelectedRevisions.First().Guid != GitRevision.UnstagedGuid;
        }

        public bool ShouldShowMenuAParentLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.LocalExists && selectionInfo.SelectedRevisions.Count() > 0;
        }

        public bool ShouldShowMenuBParentLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.LocalExists && selectionInfo.SelectedRevisions.Count() > 0
                //B parent exists
                && (selectionInfo.SelectedItemsWithParent.Count() > 1
                  || selectionInfo.SelectedItemsWithParent.Any(i => !i.Item.IsNew));
        }
        #endregion
    }
}