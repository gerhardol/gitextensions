using GitCommands;
using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BranchTreePanel
{
    internal sealed class StashTree : Tree
    {
        private readonly ICheckRefs _refsSource;

        public StashTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource)
            : base(treeNode, uiCommands)
        {
            _refsSource = refsSource;
        }

        internal void Refresh(Lazy<IReadOnlyCollection<GitRevision>> getStashRevs)
        {
            if (!IsAttached)
            {
                return;
            }

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ReloadNodesAsync((token, _) =>
                {
                    Task<Nodes>? loadNodesTask = LoadNodesAsync(token, getStashRevs);
                    return loadNodesTask;
                }, null).ConfigureAwait(false);
            });
        }

        private async Task<Nodes> LoadNodesAsync(CancellationToken token, Lazy<IReadOnlyCollection<GitRevision>> getStashRevs)
        {
            await TaskScheduler.Default;
            token.ThrowIfCancellationRequested();

            return FillStashTree(getStashRevs.Value.ToList(), token);
        }

        private Nodes FillStashTree(IReadOnlyList<GitRevision> stashes, CancellationToken token)
        {
            Nodes nodes = new(this);
            Dictionary<string, BaseBranchNode> pathToNodes = new();

            foreach (GitRevision stash in stashes)
            {
                token.ThrowIfCancellationRequested();

                // Stashes does not supprt filtering, but stashes may not be visible
                bool isVisible = stash.ObjectId is not null && _refsSource.Contains(stash.ObjectId);
                StashNode node = new(this, stash.ObjectId, stash.ReflogSelector, stash.Subject, isVisible);
                var parent = node.CreateRootNode(pathToNodes, (tree, parentPath) => new BasePathNode(tree, parentPath));

                if (parent is not null)
                {
                    nodes.AddNode(parent);
                }
            }

            return nodes;
        }

        protected override void PostFillTreeViewNode(bool firstTime)
        {
            if (firstTime)
            {
                TreeViewNode.Collapse();
            }
        }

        public void OpenStash(IWin32Window owner, StashNode node)
        {
            UICommands.StartStashDialog(owner, manageStashes: true, node.ReflogSelector);
        }

        public void ApplyStash(IWin32Window owner, StashNode node)
        {
            UICommands.StashApply(owner, node.ReflogSelector);
        }

        public void PopStash(IWin32Window owner, StashNode node)
        {
            UICommands.StashPop(owner, node.ReflogSelector);
        }

        public void DropStash(IWin32Window owner, StashNode node)
        {
            UICommands.StashDrop(owner, node.ReflogSelector);
        }
    }
}
