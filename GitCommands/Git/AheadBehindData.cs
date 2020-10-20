namespace GitCommands.Git
{
    public struct AheadBehindData
    {
        public static readonly string Gone = "gone";
        public string Branch { get; set; }
        public string AheadCount { get; set; }
        public string BehindCount { get; set; }

        public string ToDisplay()
        {
            return AheadCount == Gone
                ? "-"
                : AheadCount == "0" && string.IsNullOrEmpty(BehindCount)
                ? "0↑↓"
                : (!string.IsNullOrEmpty(AheadCount) && AheadCount != "0"
                    ? AheadCount + "↑" + (!string.IsNullOrEmpty(BehindCount) ? " " : string.Empty)
                    : string.Empty)
                + (!string.IsNullOrEmpty(BehindCount) ? BehindCount + "↓" : string.Empty);
        }
    }
}
