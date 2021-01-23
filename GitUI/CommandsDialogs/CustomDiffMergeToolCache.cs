using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GitCommands;

namespace GitUI
{
    public class CustomDiffMergeToolCache
    {
        private SemaphoreSlim _mutex = new(1);
        private IEnumerable<string> _tools = null;

        /// <summary>
        /// Clear the existing caches
        /// </summary>
        public void Clear()
        {
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                await _mutex.WaitAsync().ConfigureAwait(false);

                try
                {
                    _tools = null;
                }
                finally
                {
                    _mutex.Release();
                }
            }).FileAndForget();
        }

        /// <summary>
        /// Load the availble DiffMerge tools and apply to the menus
        /// </summary>
        /// <param name="module">The Git module</param>
        /// <param name="isDiff">True if diff, false if merge</param>
        /// <param name="delay">The delay before starting the operation</param>
        public async Task<IEnumerable<string>> GetToolsAsync(GitModule module, bool isDiff, int delay)
        {
            if (_tools != null)
            {
                return _tools;
            }

            // The command will compete with other resources, avoid delaying startup
            // Do not bother with cancel tokens
            await Task.Delay(delay);
            if (_tools != null)
            {
                return _tools;
            }

            await _mutex.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_tools == null)
                {
                    _tools = await module.GetCustomDiffMergeTools(isDiff: isDiff);
                }
            }
            finally
            {
                _mutex.Release();
            }

            return _tools;
        }
    }
}
