using System;
using System.Collections.Generic;
using System.Threading;
using GitCommands;
using GitCommands.Submodules;
using GitUI;

namespace CommonTestUtils
{
    public class SubmoduleTestHelpers
    {
        public static SubmoduleInfoResult UpdateSubmoduleStructureAndWaitForResult(ISubmoduleStatusProvider provider, GitModule module, bool updateStatus = false)
        {
            SubmoduleInfoResult result = null;
            provider.StatusUpdated += Provider_StatusUpdated;

            provider.UpdateSubmodulesStructure(
                workingDirectory: module.WorkingDir,
                noBranchText: string.Empty,
                updateStatus: updateStatus);

            AsyncTestHelper.WaitForPendingOperations();

            provider.StatusUpdated -= Provider_StatusUpdated;

            return result;

            void Provider_StatusUpdated(object sender, SubmoduleStatusEventArgs e)
            {
                result = e.Info;
            }
        }

        public static void UpdateSubmoduleStatusAndWaitForResult(ISubmoduleStatusProvider provider, GitModule module, IReadOnlyList<GitItemStatus> gitStatus)
        {
            CancellationTokenSequence submodulesStatusSequence = new CancellationTokenSequence();

            // await status to be updated in result struct returned for the structure, update not running
            AsyncTestHelper.RunAndWaitForPendingOperations(() => provider.GetTestAccessor().UpdateSubmodulesStatusAsync(
                module: new GitModule(module.WorkingDir),
                gitStatus: gitStatus,
                submodulesStatusSequence.Next()));

            return;
        }
    }
}
