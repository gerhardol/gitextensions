using System;
using System.Collections.Generic;
using System.Linq;
using GitCommands;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using JetBrains.Annotations;

namespace GitUI.CommandsDialogs
{
    public interface IFileStatusListContextMenuController
    {
        bool ShouldShowMenuFirstToSelected(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuFirstToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuSelectedToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuFirstParentToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldShowMenuSelectedParentToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldDisplayMenuFirstParentToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldDisplayMenuSelectedParentToLocal(ContextMenuDiffToolInfo selectionInfo);
        bool ShouldEnableFirstSpecialCompare([CanBeNull] FileStatusItem item);
        bool ShouldEnableSecondSpecialCompare([CanBeNull] FileStatusItem item);

        /// <summary>
        /// A Git commitish representation of an object
        /// https://git-scm.com/docs/gitrevisions#_specifying_revisions
        /// </summary>
        /// <param name="getFileBlobHash">the Git module function to get the blob</param>
        /// <param name="item">the item</param>
        /// <returns>A Git commitish</returns>
        string GetGitCommit([CanBeNull] Func<string, ObjectId, ObjectId> getFileBlobHash, [CanBeNull] FileStatusItem item, bool isFirst);
    }

    public sealed class ContextMenuDiffToolInfo
    {
        public ContextMenuDiffToolInfo(
            GitRevision selectedRevision = null,
            IReadOnlyList<ObjectId> selectedItemParentRevs = null,
            bool allAreNew = false,
            bool allAreDeleted = false,
            bool firstIsParent = false,
            bool firstParentsValid = true,
            bool localExists = true)
        {
            SelectedRevision = selectedRevision;
            SelectedItemParentRevs = selectedItemParentRevs;
            AllAreNew = allAreNew;
            AllAreDeleted = allAreDeleted;
            FirstIsParent = firstIsParent;
            FirstParentsValid = firstParentsValid;
            LocalExists = localExists;
        }

        [CanBeNull]
        public GitRevision SelectedRevision { get; }
        [CanBeNull]
        public IEnumerable<ObjectId> SelectedItemParentRevs { get; }
        public bool AllAreNew { get; }
        public bool AllAreDeleted { get; }
        public bool FirstIsParent { get; }
        public bool FirstParentsValid { get; }
        public bool LocalExists { get; }
    }

    public class FileStatusListContextMenuController : IFileStatusListContextMenuController
    {
        public bool ShouldShowMenuFirstToSelected(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null;
        }

        public bool ShouldShowMenuFirstToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null && selectionInfo.LocalExists

                // First (A) exists (Can only determine that A does not exist if A is parent and B is new)
                && (!selectionInfo.FirstIsParent || !selectionInfo.AllAreNew)

                // First (A) is not local
                && (selectionInfo.SelectedItemParentRevs == null || !selectionInfo.SelectedItemParentRevs.Contains(ObjectId.WorkTreeId));
        }

        public bool ShouldShowMenuSelectedToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null && selectionInfo.LocalExists

                // Selected (B) exists
                && !selectionInfo.AllAreDeleted

                // Selected (B) is not local
                && selectionInfo.SelectedRevision.ObjectId != ObjectId.WorkTreeId;
        }

        public bool ShouldShowMenuFirstParentToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null && selectionInfo.LocalExists
                && ShouldDisplayMenuFirstParentToLocal(selectionInfo);
        }

        public bool ShouldShowMenuSelectedParentToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            return selectionInfo.SelectedRevision != null && selectionInfo.LocalExists
                && ShouldDisplayMenuSelectedParentToLocal(selectionInfo)

                // Selected (B) parent exists
                && !selectionInfo.AllAreNew;
        }

        public bool ShouldDisplayMenuFirstParentToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            // First (A) parents may not be known, then hide this option
            return selectionInfo.FirstParentsValid;
        }

        public bool ShouldDisplayMenuSelectedParentToLocal(ContextMenuDiffToolInfo selectionInfo)
        {
            // Not visible if same revision as ShouldShowMenuFirstToLocal()
            return !selectionInfo.FirstIsParent;
        }

        public bool ShouldEnableFirstSpecialCompare(FileStatusItem item)
        {
            // First item must be a git reference, i.e. other than work tree
            return item != null && !item.Item.IsSubmodule && item.SecondRevision.ObjectId != ObjectId.WorkTreeId;
        }

        public bool ShouldEnableSecondSpecialCompare(FileStatusItem item)
        {
            // Work tree file must exist on file system
            return item != null && !item.Item.IsSubmodule && (item.SecondRevision.ObjectId != ObjectId.WorkTreeId || !item.Item.IsDeleted);
        }

        /// <inheritdoc/>>
        public string GetGitCommit([CanBeNull] Func<string, ObjectId, ObjectId> getFileBlobHash, [CanBeNull] FileStatusItem item, bool isFirst)
        {
            if (isFirst ? !ShouldEnableFirstSpecialCompare(item) : !ShouldEnableSecondSpecialCompare(item))
            {
                return null;
            }

            var name = item.Item.IsDeleted && !string.IsNullOrWhiteSpace(item.Item.OldName)
                ? item.Item.OldName
                : item.Item.Name;

            var id = (item.Item.IsDeleted ? item.FirstRevision : item.SecondRevision)?.ObjectId;
            if (string.IsNullOrWhiteSpace(name) || id == null)
            {
                return null;
            }

            if (id == ObjectId.WorkTreeId)
            {
                // A file system file
                return name;
            }

            if (id == ObjectId.IndexId)
            {
                // Must be referenced by blob - no commit. File name presented in tool will be blob or the other file
                return item.Item.TreeGuid != null
                    ? item.Item.TreeGuid.ToString()
                    : getFileBlobHash != null
                        ? getFileBlobHash(name, id)?.ToString()
                        : null;
            }

            // revision:path
            return $"{id}:{name}";
        }
    }
}
