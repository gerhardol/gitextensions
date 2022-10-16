using System.Diagnostics;
using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.BranchTreePanel
{
    [DebuggerDisplay("(Folder) FullPath = {FullPath}")]
    internal sealed class RemoteRepoFolderNode : BaseBranchNode
    {
        public RemoteRepoFolderNode(Tree tree, ObjectId objectId, string name) : base(tree, objectId, name, true)
        {
        }

        protected override void ApplyStyle()
        {
            base.ApplyStyle();
            TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey = nameof(Images.EyeClosed);
        }

        protected override string DisplayText()
        {
            return Name;
        }
    }
}
