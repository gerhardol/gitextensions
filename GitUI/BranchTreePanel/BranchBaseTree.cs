using GitUI.UserControls.RevisionGrid;
using GitUIPluginInterfaces;

namespace GitUI.BranchTreePanel
{
    internal abstract class BranchBaseTree : Tree
    {
        // A flag to indicate whether the data is being filtered (e.g. Show Current Branch Only).
        private protected AsyncLocal<bool> IsFiltering = new();
        protected bool SupportsFiltering { get; } = true;

        protected BranchBaseTree(TreeNode treeNode, IGitUICommandsSource uiCommands, ICheckRefs refsSource) : base(treeNode, uiCommands, refsSource)
        {
        }

        protected override void OnAttached()
        {
            IsFiltering.Value = false;
            base.OnAttached();
        }

        /// <summary>
        /// Requests (from FormBrowse) to refresh the data tree and to apply filtering, if necessary.
        /// </summary>
        /// <param name="isFiltering">
        ///  <see langword="true"/>, if the data is being filtered; otherwise <see langword="false"/>.
        /// </param>
        /// <param name="forceRefresh">Refresh may be required as references may have been changed.</param>
        /// <param name="getRefs">Function to get refs.</param>
        internal void Refresh(bool isFiltering, bool forceRefresh, Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
        {
            if (!IsAttached)
            {
                return;
            }

            // If we're not currently filtering and no need to filter now -> exit.
            // Else we need to iterate over the list and rebind the tree - whilst there
            // could be a situation whether a user just refreshed the grid, there could
            // also be a situation where the user applied a different filter, or checked
            // out a different ref (e.g. a branch or commit), and we have a different
            // set of branches to show/hide.
            if (!forceRefresh && (!SupportsFiltering || (!isFiltering && !IsFiltering.Value)))
            {
                return;
            }

            IsFiltering.Value = isFiltering && SupportsFiltering;
            Refresh(getRefs);
        }

        /// <summary>
        /// Requests to refresh the data tree and to apply filtering, if necessary.
        /// </summary>
        protected internal virtual void Refresh(Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs)
        {
            // NOTE: descendants may need to break their local caches to ensure the latest data is loaded.

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await ReloadNodesAsync(LoadNodesAsync, getRefs);
            });
        }

        protected abstract Task<Nodes> LoadNodesAsync(CancellationToken token, Func<RefsFilter, IReadOnlyList<IGitRef>> getRefs);
    }
}
