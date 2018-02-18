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

        bool ShouldShowMenuAB(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuALocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuBLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuAParentLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuBParentLocal(ContextMenuDiffToolInfo selectionInfo);

        bool LocalExists(IEnumerable<GitItemStatusWithParent> selectedItemsWithParent, IFullPathResolver fullPathResolver);
        bool AisParent(IEnumerable<string> parentRevs, string firstParent, string selectedParent);
    }

    public sealed class ContextMenuSelectionInfo
    {
        public ContextMenuSelectionInfo(GitRevision selectedRevision, GitItemStatus selectedDiff, bool aIsParent, bool isAnyCombinedDiff, bool isSingleGitItemSelected, bool isAnyItemSelected, bool isBareRepository, bool singleFileExists, bool isAnyTracked, bool isAnySubmodule)
        {
            SelectedRevision = selectedRevision;
            //xxx SelectedDiff = selectedDiff;
            AIsParent = aIsParent;
            IsAnyCombinedDiff = isAnyCombinedDiff;
            IsSingleGitItemSelected = isSingleGitItemSelected;
            IsAnyItemSelected = isAnyItemSelected;
            IsBareRepository = isBareRepository;
            SingleFileExists = singleFileExists;
            IsAnyTracked = isAnyTracked;
            IsAnySubmodule = isAnySubmodule;
        }
        public GitRevision SelectedRevision { get; }
        public bool AIsParent { get; }
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
        public ContextMenuDiffToolInfo(GitRevision selectedRevision, IEnumerable<GitItemStatusWithParent> selectedItemsWithParent, bool aIsParent, bool localExists)
        {
            SelectedRevision = selectedRevision;
            SelectedItemsWithParent = selectedItemsWithParent;
            AIsParent = aIsParent;
            LocalExists = localExists;
        }
        public GitRevision SelectedRevision { get; }
        public IEnumerable<GitItemStatusWithParent> SelectedItemsWithParent { get; }
        public bool AIsParent { get; }
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
            return selectionInfo.IsAnyItemSelected && !selectionInfo.IsBareRepository
                && (!selectionInfo.IsSingleGitItemSelected || (!selectionInfo.IsAnySubmodule && selectionInfo.IsAnyTracked));//xxx
        }
        #endregion

        #region Main menu items
        public bool ShouldShowMenuSaveAs(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.IsAnySubmodule
                && !selectionInfo.SelectedRevision.IsArtificial();
        }

        public bool ShouldShowMenuCherryPick(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.IsAnySubmodule
                && !selectionInfo.IsAnyCombinedDiff && !selectionInfo.IsBareRepository
                && !selectionInfo.SelectedRevision.IsArtificial();
        }

        //Stage/unstage must limit the selected items, IsStaged is not reflecting Staged status
        public bool ShouldShowMenuStage(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.AIsParent && selectionInfo.SelectedRevision.Guid == GitRevision.UnstagedGuid;
        }

        public bool ShouldShowMenuUnstage(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.AIsParent && selectionInfo.SelectedRevision.Guid == GitRevision.IndexGuid;
        }

        public bool ShouldShowSubmoduleMenus(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnySubmodule && /*selectionInfo.AIsParent &&*/ selectionInfo.SelectedRevision.Guid == GitRevision.UnstagedGuid;
        }

        public bool ShouldShowMenuEditFile(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.IsAnySubmodule && selectionInfo.SingleFileExists;
        }

        public bool ShouldShowMenuCopyFileName(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemSelected;
        }

        public bool ShouldShowMenuShowInFileTree(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.SelectedRevision.IsArtificial();
        }

        public bool ShouldShowMenuFileHistory(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && selectionInfo.IsAnyTracked;
        }

        public bool ShouldShowMenuBlame(ContextMenuSelectionInfo selectionInfo)
        {
            return ShouldShowMenuFileHistory(selectionInfo) && !selectionInfo.IsAnySubmodule;
        }
        #endregion

        #region difftool submenu
        public bool ShouldShowMenuAB(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null;
        }

        public bool ShouldShowMenuALocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.LocalExists && selectionInfo.SelectedRevision != null
                //A exists (Can only determine that A does not exist if A is parent (one selected) and B is new)
                && (!selectionInfo.AIsParent
                  || selectionInfo.SelectedItemsWithParent.Any(i => !i.Item.IsNew))
                //A is not local
                && selectionInfo.SelectedItemsWithParent.Any(i => i.ParentGuid != GitRevision.UnstagedGuid);
        }

        public bool ShouldShowMenuBLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.LocalExists && selectionInfo.SelectedRevision != null
                //B exists
                && selectionInfo.SelectedItemsWithParent.Any(i => !i.Item.IsDeleted)
                //B is not local
                && selectionInfo.SelectedRevision.Guid != GitRevision.UnstagedGuid;
        }

        public bool ShouldShowMenuAParentLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.LocalExists && selectionInfo.SelectedRevision != null;
        }

        public bool ShouldShowMenuBParentLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.LocalExists && selectionInfo.SelectedRevision != null
                //B parent exists
                && (selectionInfo.SelectedItemsWithParent.Count() > 1
                  || selectionInfo.SelectedItemsWithParent.Any(i => !i.Item.IsNew));
        }
        #endregion

        #region helpers
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

        public bool AisParent(IEnumerable<string> parentRevs, string firstParent, string selectedParent)
        {
            return parentRevs.Count() == 1 && firstParent == selectedParent;
        }
        #endregion
    }
}