﻿using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;
using Microsoft.VisualStudio.Threading;

namespace GitUI.BranchTreePanel
{
    internal sealed class TagTree : BaseRefTree
    {
        // Retains the list of currently loaded tags.
        // This is needed to apply filtering without reloading the data.
        // Whether or not force the reload of data is controlled by <see cref="_isFiltering"/> flag.
        private IReadOnlyList<IGitRef>? _loadedTags;

        public TagTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource)
            : base(treeNode, uiCommands, refsSource)
        {
        }

        /// <inheritdoc/>
        protected internal override void Refresh(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
        {
            // Break the local cache to ensure the data is requeried to reflect the required sort order.
            _loadedTags = null;

            base.Refresh(getRefs);
        }

        protected override async Task<Nodes> LoadNodesAsync(CancellationToken token, Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
        {
            await TaskScheduler.Default;
            token.ThrowIfCancellationRequested();

            if (!IsFiltering.Value || _loadedTags is null)
            {
                _loadedTags = getRefs(RefsFilter.Tags);
                token.ThrowIfCancellationRequested();
            }

            return FillTagTree(_loadedTags, token);
        }

        private Nodes FillTagTree(IReadOnlyList<IGitRef> tags, CancellationToken token)
        {
            Nodes nodes = new(this);
            Dictionary<string, BaseRevisionNode> pathToNodes = new();

            foreach (IGitRef tag in tags)
            {
                token.ThrowIfCancellationRequested();

                TagNode tagNode = new(this, tag.ObjectId, tag.Name, visible: true);
                var parent = tagNode.CreateRootNode(pathToNodes, (tree, parentPath) => new BasePathNode(tree, parentPath));

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
    }
}
