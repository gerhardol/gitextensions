using GitUIPluginInterfaces;

namespace GitUI.UserControls.RevisionGrid
{
    public class RevisionLoadEventArgs : GitUIEventArgs
    {
        public RevisionLoadEventArgs(IWin32Window? ownerForm, IGitUICommands gitUICommands, Lazy<IReadOnlyList<IGitRef>> getRefs, bool forceRefresh, Lazy<IReadOnlyCollection<GitRevision>> getStashRevs)
            : base(ownerForm, gitUICommands, getRefs)
        {
            ForceRefresh = forceRefresh;
            GetStashRevs = getStashRevs;
        }

        public bool ForceRefresh { get; init; }
        public Lazy<IReadOnlyCollection<GitRevision>> GetStashRevs { get; init; }
    }
}
