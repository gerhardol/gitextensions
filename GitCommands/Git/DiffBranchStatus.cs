using System;
using System.Text;
using System.Threading.Tasks;
using GitUIPluginInterfaces;
using Microsoft;
using Microsoft.VisualStudio.Threading;

namespace GitCommands
{
     public enum DiffBranchStatus
    {
        Unknown = 0,
        OnlyAChange,
        OnlyBChange,
        SameChange,
        UniqueChange
    }
}
