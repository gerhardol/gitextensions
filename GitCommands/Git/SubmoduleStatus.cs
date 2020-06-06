namespace GitCommands.Git
{
    public enum SubmoduleStatus
    {
        Unknown = 0,
        SameCommit,
        NewSubmodule,
        FastForward,
        Rewind,
        NewerTime,
        OlderTime,
        SameTime
    }
}
