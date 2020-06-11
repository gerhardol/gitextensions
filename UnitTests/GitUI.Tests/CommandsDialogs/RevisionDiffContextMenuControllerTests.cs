using FluentAssertions;
using GitCommands;
using GitUI.CommandsDialogs;
using GitUI.UserControls;
using GitUIPluginInterfaces;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [TestFixture]
    public class RevisionDiffContextMenuControllerTests
    {
        private FileStatusListContextMenuController _revisionDiffContextMenuController;

        /// <summary>
        /// Mock of GitModule.GetFileBlobHash
        /// </summary>
        /// <param name="name">The git blob name</param>
        /// <param name="id">The commit id</param>
        /// <returns>The Git blob for the item</returns>
        private static ObjectId GetFileBlobHash(string name, ObjectId id)
        {
            return id;
        }

        [SetUp]
        public void Setup()
        {
            _revisionDiffContextMenuController = new FileStatusListContextMenuController();
        }

        [Test]
        public void BrowseDiff_SuppressDiffToLocalWhenNoSelectedRevision()
        {
            var selectionInfo = new ContextMenuDiffToolInfo();
            _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_SuppressDiffToLocalWhenNoLocalExists()
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = new ContextMenuDiffToolInfo(selectedRevision: rev, localExists: false);
            _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_ShowContextDiffToolForWorkTree()
        {
            var rev = new GitRevision(ObjectId.WorkTreeId);
            var selectionInfo = new ContextMenuDiffToolInfo(selectedRevision: rev);
            _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().BeTrue();
        }

        [Test]
        public void BrowseDiff_ShowContextDiffToolForWorkTreeParent()
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = new ContextMenuDiffToolInfo(selectedRevision: rev, selectedItemParentRevs: new[] { ObjectId.WorkTreeId });
            _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeFalse();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().BeTrue();
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void BrowseDiff_ShowContextDiffToolForDeletedAndNew(bool d, bool n)
        {
            var rev = new GitRevision(ObjectId.Random());
            var selectionInfo = new ContextMenuDiffToolInfo(selectedRevision: rev, allAreDeleted: d, allAreNew: n);
            _revisionDiffContextMenuController.ShouldShowMenuFirstToSelected(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuFirstToLocal(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedToLocal(selectionInfo).Should().Be(!d);
            _revisionDiffContextMenuController.ShouldShowMenuFirstParentToLocal(selectionInfo).Should().BeTrue();
            _revisionDiffContextMenuController.ShouldShowMenuSelectedParentToLocal(selectionInfo).Should().Be(!n);
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void BrowseDiff_ShouldEnableFirstSpecialCompare_WorkTree(bool isSubmodule, bool isDeleted)
        {
            var rev = new GitRevision(ObjectId.Random());
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: new GitRevision(ObjectId.WorkTreeId),
                item: new GitItemStatus
                {
                    IsSubmodule = isSubmodule,
                    IsDeleted = isDeleted
                });
            _revisionDiffContextMenuController.ShouldEnableFirstSpecialCompare(item).Should().BeFalse();
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void BrowseDiff_ShouldEnableFirstSpecialCompare_Commit(bool isSubmodule, bool isDeleted)
        {
            var rev = new GitRevision(ObjectId.Random());
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: rev,
                item: new GitItemStatus
                {
                    IsSubmodule = isSubmodule,
                    IsDeleted = isDeleted
                });
            _revisionDiffContextMenuController.ShouldEnableFirstSpecialCompare(item).Should().Be(!isSubmodule);
        }

        [TestCase(true, true, false)]
        [TestCase(true, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, true)]
        public void BrowseDiff_ShouldEnableSecondSpecialCompare_WorkTree(bool isSubmodule, bool isDeleted, bool result)
        {
            var rev = new GitRevision(ObjectId.Random());
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: new GitRevision(ObjectId.WorkTreeId),
                item: new GitItemStatus
                {
                    IsSubmodule = isSubmodule,
                    IsDeleted = isDeleted
                });
            _revisionDiffContextMenuController.ShouldEnableSecondSpecialCompare(item).Should().Be(result);
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void BrowseDiff_ShouldEnableSecondSpecialCompare_Commit(bool isSubmodule, bool isDeleted)
        {
            var rev = new GitRevision(ObjectId.Random());
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: rev,
                item: new GitItemStatus
                {
                    IsSubmodule = isSubmodule,
                    IsDeleted = isDeleted
                });
            _revisionDiffContextMenuController.ShouldEnableSecondSpecialCompare(item).Should().Be(!isSubmodule);
        }

        [Test]
        public void BrowseDiff_GetGitCommit_FirstDisabled()
        {
            var rev = new GitRevision(ObjectId.Random());
            var workTree = new GitRevision(ObjectId.WorkTreeId);
            var item = new FileStatusItem(
                firstRev: workTree,
                secondRev: rev,
                item: new GitItemStatus());
            _revisionDiffContextMenuController.GetGitCommit(null, item, true).Should().BeNull();
        }

        [Test]
        public void BrowseDiff_GetGitCommit_SecondDisabled()
        {
            var rev = new GitRevision(ObjectId.Random());
            var workTree = new GitRevision(ObjectId.WorkTreeId);
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: workTree,
                item: new GitItemStatus { IsSubmodule = true });
            _revisionDiffContextMenuController.GetGitCommit(null, item, false).Should().BeNull();
        }

        [Test]
        public void BrowseDiff_GetGitCommit_SecondWorkTree()
        {
            var rev = new GitRevision(ObjectId.Random());
            var workTree = new GitRevision(ObjectId.WorkTreeId);
            var name = "WorkTreeFile";
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: workTree,
                item: new GitItemStatus { Name = name });
            _revisionDiffContextMenuController.GetGitCommit(null, item, false).Should().Be(name);
        }

        [Test]
        public void BrowseDiff_GetGitCommit_Index_Tree()
        {
            var rev = new GitRevision(ObjectId.Random());
            var workTree = new GitRevision(ObjectId.IndexId);
            const string name = "File";
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: workTree,
                item: new GitItemStatus { Name = name, TreeGuid = ObjectId.Random() });
            _revisionDiffContextMenuController.GetGitCommit(null, item, false).Should().Be(item.Item.TreeGuid?.ToString());
        }

        [Test]
        public void BrowseDiff_GetGitCommit_Index_GetBlob()
        {
            var rev = new GitRevision(ObjectId.Random());
            var workTree = new GitRevision(ObjectId.IndexId);
            const string name = "File";
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: workTree,
                item: new GitItemStatus { Name = name, TreeGuid = null });
            _revisionDiffContextMenuController.GetGitCommit(GetFileBlobHash, item, false).Should().Be(ObjectId.IndexId.ToString());
        }

        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void BrowseDiff_GetGitCommit_Commit(bool isFirst, bool isDeleted)
        {
            var id = ObjectId.Random();
            var rev = new GitRevision(id);
            const string newName = "newName";
            const string oldName = "oldName";
            var item = new FileStatusItem(
                firstRev: rev,
                secondRev: rev,
                item: new GitItemStatus
                {
                    Name = newName,
                    OldName = oldName,
                    IsDeleted = isDeleted
                });
            var expected = $"{id}:{(isDeleted ? oldName : newName)}";
            _revisionDiffContextMenuController.GetGitCommit(null, item, isFirst).Should().Be(expected);
        }
    }
}
