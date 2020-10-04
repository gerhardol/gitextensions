using System.Collections.Generic;
using GitCommands;
using GitUIPluginInterfaces;

namespace GitUI
{
    public sealed partial class FileStatusList
    {
        private class FileStatusWithDescription
        {
            public GitRevision FirstRev;
            public GitRevision SecondRev;
            public ObjectId BaseA;
            public ObjectId BaseB;
            public string Summary;
            public IReadOnlyList<GitItemStatus> Statuses;
        }
    }
}
