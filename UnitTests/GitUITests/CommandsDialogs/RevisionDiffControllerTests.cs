using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using FluentAssertions;
using GitCommands;
using GitCommands.Git;
using GitUI.CommandsDialogs;
using GitUI.Properties;
using GitUIPluginInterfaces;
using NSubstitute;
using NUnit.Framework;

namespace GitUITests.CommandsDialogs
{
    [TestFixture]
    public class RevisionDiffControllerTests
    {
        private RevisionDiffController _controller;

        [SetUp]
        public void Setup()
        {
            _controller = new RevisionDiffController();
        }

        [Test]
        public void BrowseDiff_ShowContextMenus()
        {
            var rev = new GitRevision(null, null);
            var selectionInfo = new ContextMenuSelectionInfo(rev, false, false, true, false, true, true, true, false);
            _controller.ShouldShowDifftoolMenus(selectionInfo).Should().BeFalse();
            _controller.ShouldShowResetFileMenus(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_SuppressDiffToLocalWhenNoSelectedRevision()
        {
            var selectionInfo = new ContextMenuDiffToolInfo(null, null, false, false, true, true, true);
            _controller.ShouldShowMenuAB(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuALocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuBLocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuAParentLocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuBParentLocal(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_SuppressDiffToLocalWhenNoLocalExists()
        {
            var rev = new GitRevision(null, "1234567890");
            var selectionInfo = new ContextMenuDiffToolInfo(rev, null, false, false, true, false, false);
            _controller.ShouldShowMenuAB(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuALocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuBLocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuAParentLocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuBParentLocal(selectionInfo).Should().BeFalse();
        }

        [Test]
        public void BrowseDiff_ShowContextDiffToolForUnstaged()
        {
            var rev = new GitRevision(null, GitRevision.UnstagedGuid);
            var selectionInfo = new ContextMenuDiffToolInfo(rev, new string[]{ GitRevision.UnstagedGuid }, false, false, true, true, true);
            _controller.ShouldShowMenuAB(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuALocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuBLocal(selectionInfo).Should().BeFalse();
            _controller.ShouldShowMenuAParentLocal(selectionInfo).Should().BeTrue();
            _controller.ShouldShowMenuBParentLocal(selectionInfo).Should().BeFalse();
        }
    }
}
