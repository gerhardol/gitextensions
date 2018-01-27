using System.Collections.Generic;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI.CommandsDialogs
{
    public interface IRevisionDiffController
    {
        bool ShouldShowMenuBlame(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuCherryPick(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuEditFile(ContextMenuSelectionInfo selectionInfo);
        bool ShouldShowMenuResetFile(ContextMenuSelectionInfo selectionInfo);
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
        bool ShouldShowMenuAParent(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuBParent(ContextMenuDiffToolInfo selectionInfo);
    }

    public sealed class ContextMenuSelectionInfo
    {
        public ContextMenuSelectionInfo(IList<GitRevision> selectedRevisions, GitItemStatus selectedDiff, bool isAnyCombinedDiff, bool isSingleGitItemSelected, bool isAnyItemSelected, bool isBareRepository, bool singleFileExists, bool isAnyTracked)
        {
            SelectedRevisions = selectedRevisions;
            SelectedDiff = selectedDiff;
            IsAnyCombinedDiff  = isAnyCombinedDiff;
            IsSingleGitItemSelected = isSingleGitItemSelected;
            IsAnyItemSelected = isAnyItemSelected;
            IsBareRepository = isBareRepository;
            SingleFileExists = singleFileExists;
            IsAnyTracked = isAnyTracked;
        }
        public IList<GitRevision> SelectedRevisions { get; }
        public GitItemStatus SelectedDiff { get; }
        public bool IsAnyCombinedDiff { get; }
        public bool IsSingleGitItemSelected { get; }
        public bool IsAnyItemSelected { get; }
        public bool IsBareRepository { get; }
        public bool SingleFileExists { get; }
        public bool IsAnyTracked { get; }
    }

    public sealed class ContextMenuDiffToolInfo
    {
        public ContextMenuDiffToolInfo(bool aIsLocal, bool bIsLocal, bool localExists)
        {
            AIsLocal = aIsLocal;
            BIsLocal = bIsLocal;
            LocalExists = localExists;
        }
        public bool AIsLocal { get; }
        public bool BIsLocal { get; }
        public bool LocalExists { get; }
    }

    public sealed class RevisionDiffController : IRevisionDiffController
    {
        public bool ShouldShowDifftoolMenus(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemSelected && !selectionInfo.IsAnyCombinedDiff && selectionInfo.IsAnyTracked;
        }

        public bool ShouldShowMenuBlame(ContextMenuSelectionInfo selectionInfo)
        {
            return ShouldShowMenuFileHistory(selectionInfo) && !selectionInfo.SelectedDiff.IsSubmodule;
        }

        public bool ShouldShowMenuCherryPick(ContextMenuSelectionInfo selectionInfo)
        {
            return !selectionInfo.IsAnyCombinedDiff && selectionInfo.IsSingleGitItemSelected && !selectionInfo.IsBareRepository &&
                   !selectionInfo.SelectedDiff.IsSubmodule && !selectionInfo.SelectedRevisions[0].IsArtificial();
        }

        public bool ShouldShowMenuEditFile(ContextMenuSelectionInfo selectionInfo)
        {
            return !selectionInfo.SelectedDiff.IsSubmodule && selectionInfo.SingleFileExists;
        }

        public bool ShouldShowMenuResetFile(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemSelected && !selectionInfo.IsAnyCombinedDiff && !selectionInfo.IsBareRepository &&
                !(selectionInfo.IsSingleGitItemSelected && (selectionInfo.SelectedDiff.IsSubmodule || selectionInfo.SelectedDiff.IsNew) && selectionInfo.SelectedRevisions[0].Guid == GitRevision.UnstagedGuid);
        }

        public bool ShouldShowMenuFileHistory(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && selectionInfo.SelectedDiff.IsTracked;
        }

        public bool ShouldShowMenuSaveAs(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemSelected && !selectionInfo.IsAnyCombinedDiff && selectionInfo.IsSingleGitItemSelected && !selectionInfo.SelectedDiff.IsSubmodule;
        }

        public bool ShouldShowMenuCopyFileName(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemSelected;
        }

        public bool ShouldShowMenuShowInFileTree(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && !selectionInfo.SelectedRevisions[0].IsArtificial();
        }

        public bool ShouldShowMenuStage(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemSelected &&
                   selectionInfo.SelectedRevisions.Count >= 1 && selectionInfo.SelectedRevisions[0].Guid == GitRevision.UnstagedGuid ||
                   selectionInfo.SelectedRevisions.Count >= 2 && selectionInfo.SelectedRevisions[1].Guid == GitRevision.UnstagedGuid;
        }

        public bool ShouldShowMenuUnstage(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsAnyItemSelected &&
                   selectionInfo.SelectedRevisions.Count >= 1 && selectionInfo.SelectedRevisions[0].Guid == GitRevision.IndexGuid ||
                   selectionInfo.SelectedRevisions.Count >= 2 && selectionInfo.SelectedRevisions[1].Guid == GitRevision.IndexGuid;
        }

        public bool ShouldShowSubmoduleMenus(ContextMenuSelectionInfo selectionInfo)
        {
            return selectionInfo.IsSingleGitItemSelected && selectionInfo.SelectedDiff.IsSubmodule && selectionInfo.SelectedRevisions[0].Guid == GitRevision.UnstagedGuid;
        }

        public bool ShouldShowMenuAB(ContextMenuDiffToolInfo selectionInfo)
        {
            return true;
        }

        public bool ShouldShowMenuALocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.LocalExists && !selectionInfo.AIsLocal;
        }

        public bool ShouldShowMenuBLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.LocalExists && !selectionInfo.BIsLocal;
        }

        public bool ShouldShowMenuAParentLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.LocalExists;
        }

        public bool ShouldShowMenuBParentLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.LocalExists;
        }

        public bool ShouldShowMenuAParent(ContextMenuDiffToolInfo selectionInfo)
        {
            return true;
        }

        public bool ShouldShowMenuBParent(ContextMenuDiffToolInfo selectionInfo)
        {
            return true;
        }
    }
}