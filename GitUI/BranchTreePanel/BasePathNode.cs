using GitUI.Properties;
using GitUIPluginInterfaces;

namespace GitUI.BranchTreePanel
{
    internal class BasePathNode : BaseBranchNode
    {
        public BasePathNode(Tree tree, ObjectId objectId, string fullPath) : base(tree, objectId, fullPath, visible: true)
        {
        }

        protected override void ApplyStyle()
        {
            base.ApplyStyle();

            TreeViewNode.ImageKey = TreeViewNode.SelectedImageKey =
                FullPath == TranslatedStrings.Inactive ? nameof(Images.EyeClosed) : nameof(Images.BranchFolder);
        }
    }
}
