using System.Diagnostics;
using GitCommands;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.BranchTreePanel
{
    [DebuggerDisplay("(Tag) FullPath = {FullPath}, Hash = {ObjectId}, Visible: {Visible}")]
    internal sealed class StashNode : BaseBranchNode
    {
        public StashNode(Tree tree, in ObjectId? objectId, string reflogSelector, string subject, bool visible)
            : base(tree, objectId, reflogSelector.RemovePrefix("refs/"), visible)
        {
            DisplayName = $"{reflogSelector.RemovePrefix(GitRefName.RefsStashPrefix)}: {subject}";
            ReflogSelector = reflogSelector;
        }

        public string DisplayName { get; }
        public string ReflogSelector { get; }

        internal override void OnSelected()
        {
            if (Tree.IgnoreSelectionChangedEvent)
            {
                return;
            }

            base.OnSelected();
            SelectRevision();
        }

        internal override void OnDoubleClick()
        {
            OpenStash();
        }

        internal bool OpenStash()
        {
            return UICommands.StartStashDialog(TreeViewNode.TreeView, manageStashes: true, ReflogSelector);
        }

        protected override void ApplyStyle()
        {
            base.ApplyStyle();

            TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey =
                Visible
                    ? nameof(Images.Stash)
                    : nameof(Images.EyeClosed);
        }

        protected override string DisplayText()
        {
            return DisplayName;
        }

        protected override void SelectRevision()
        {
            TreeViewNode.TreeView?.BeginInvoke(new Action(() =>
            {
                UICommands.BrowseGoToRef(ObjectId.ToString(), showNoRevisionMsg: true, toggleSelection: RepoObjectsTree.ModifierKeys.HasFlag(Keys.Control));
                TreeViewNode.TreeView?.Focus();
            }));
        }
    }
}
