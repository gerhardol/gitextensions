using System.Drawing;
using System.Text;
using GitCommands;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

/// <summary>
/// Common class for highlighting of diff style files.
/// </summary>
public abstract class DiffHighlightService : TextHighlightService
{
    protected readonly bool _useGitColoring;
    protected readonly List<TextMarker> _textMarkers = [];
    protected DiffLinesInfo _diffLinesInfo;

    public DiffHighlightService(ref string text, bool useGitColoring)
    {
        _useGitColoring = useGitColoring;
        SetText(ref text);
    }

    public static IGitCommandConfiguration GetGitCommandConfiguration(IGitModule module, bool useGitColoring, string command)
    {
        if (!useGitColoring)
        {
            // Use default
            return null;
        }

        GitCommandConfiguration commandConfiguration = new();
        IReadOnlyList<GitConfigItem> items = GitCommandConfiguration.Default.Get(command);
        foreach (GitConfigItem cfg in items)
        {
            commandConfiguration.Add(cfg, command);
        }

        // https://git-scm.com/docs/git-diff#Documentation/git-diff.txt---color-moved-wsltmodesgt
        // Disable by default, document that this can be enabled.
        SetIfUnsetInGit(key: "diff.colorMovedWS", value: "no");

        // https://git-scm.com/docs/git-diff#Documentation/git-diff.txt-diffwordRegex
        // Set to "minimal" diff unless configured.
        SetIfUnsetInGit(key: "diff.wordRegex", value: "\"[a-z0-9_]+|.\"");

        // dimmed-zebra highlights borders better than the default "zebra"
        SetIfUnsetInGit(key: "diff.colorMoved", value: "dimmed-zebra");

        // Use reverse color to follow GE theme
        string reverse = AppSettings.ReverseGitColoring.Value ? "reverse" : "";

        SetIfUnsetInGit(key: "color.diff.old", value: $"red {reverse}");
        SetIfUnsetInGit(key: "color.diff.new", value: $"green {reverse}");

        if (AppSettings.ReverseGitColoring.Value)
        {
            // Fix: Force black foreground to avoid that foreground is calculated to white
            SetIfUnsetInGit(key: "color.diff.oldMoved", value: "black brightmagenta");
            SetIfUnsetInGit(key: "color.diff.newMoved", value: "black brightblue");
            SetIfUnsetInGit(key: "color.diff.oldMovedAlternative", value: "black brightcyan");
            SetIfUnsetInGit(key: "color.diff.newMovedAlternative", value: "black brightyellow");
        }

        // Set dimmed colors, default is gray dimmed/italic
        SetIfUnsetInGit(key: "color.diff.oldMovedDimmed", value: $"magenta dim {reverse}");
        SetIfUnsetInGit(key: "color.diff.newMovedDimmed", value: $"blue dim {reverse}");
        SetIfUnsetInGit(key: "color.diff.oldMovedAlternativeDimmed", value: $"cyan dim {reverse}");
        SetIfUnsetInGit(key: "color.diff.newMovedAlternativeDimmed", value: $"yellow dim {reverse}");

        // range-diff
        if (command == "range-diff")
        {
            SetIfUnsetInGit(key: "color.diff.contextBold", value: $"normal bold {reverse}");
            SetIfUnsetInGit(key: "color.diff.oldBold", value: $"brightred {reverse}");
            SetIfUnsetInGit(key: "color.diff.newBold", value: $"brightgreen  {reverse}");
        }

        return commandConfiguration;

        void SetIfUnsetInGit(string key, string value)
        {
            // Note: Only check Windows, not WSL settings
            if (string.IsNullOrEmpty(module.GetEffectiveSetting(key)))
            {
                commandConfiguration.Add(new GitConfigItem(key, value), command);
            }
        }
    }

    public override void AddTextHighlighting(IDocument document)
    {
        if (_useGitColoring)
        {
            // Apply GE word highlighting for Patch display (may apply to Difftastic setting, if not available for a repo)
            if (AppSettings.DiffDisplayAppearance.Value != GitCommands.Settings.DiffDisplayAppearance.GitWordDiff)
            {
                MarkInlineDifferences(document);
            }

            foreach (TextMarker tm in _textMarkers)
            {
                document.MarkerStrategy.AddMarker(tm);
            }

            return;
        }

        MarkInlineDifferences(document);
        HighlightAddedAndDeletedLines(document);

        foreach (ISegment segment in GetAllLines(document, DiffLineType.Header))
        {
            document.MarkerStrategy.AddMarker(CreateTextMarker(segment, AppColor.DiffSection.GetThemeColor()));
        }

        return;

        static TextMarker CreateTextMarker(ISegment segment, Color color)
            => new(segment.Offset, segment.Length, TextMarkerType.SolidBlock, color, ColorHelper.GetForeColorForBackColor(color));
    }

    public override bool IsSearchMatch(DiffViewerLineNumberControl lineNumbersControl, int indexInText)
        => lineNumbersControl.GetLineInfo(indexInText)?.LineType is (DiffLineType.Minus or DiffLineType.Plus or DiffLineType.MinusPlus or DiffLineType.Grep);

    public abstract string[] GetFullDiffPrefixes();

    private void SetText(ref string text)
    {
        if (!_useGitColoring)
        {
            return;
        }

        StringBuilder sb = new(text.Length);
        AnsiEscapeUtilities.ParseEscape(text, sb, _textMarkers);

        text = sb.ToString();
    }

    /// <summary>
    /// Highlight added and removed lines.
    /// Overridden in the HighlightServices where GE coloring is used (AddTextHighlighting() for Patch and CombinedDiff).
    /// </summary>
    /// <param name="document">The document to analyze.</param>
    private void HighlightAddedAndDeletedLines(IDocument document)
    {
        foreach (ISegment segment in GetAllLines(document, DiffLineType.Minus))
        {
            document.MarkerStrategy.AddMarker(CreateTextMarker(segment, AppColor.AnsiTerminalRedBackNormal.GetThemeColor()));
        }

        foreach (ISegment segment in GetAllLines(document, DiffLineType.Plus))
        {
            document.MarkerStrategy.AddMarker(CreateTextMarker(segment, AppColor.AnsiTerminalGreenBackNormal.GetThemeColor()));
        }

        return;

        static TextMarker CreateTextMarker(ISegment segment, Color color)
            => new(segment.Offset, segment.Length, TextMarkerType.SolidBlock, color, ColorHelper.GetForeColorForBackColor(color));
    }

    /// <summary>
    ///  Matches related removed and added lines in a consecutive block of a patch document and marks identical parts dimmed.
    /// </summary>
    private void MarkInlineDifferences(IDocument document)
    {
        int line = 0;
        bool found = false;

        const int diffContentOffset = 1; // in order to skip the prefixes '-' / '+'

        // Process the next blocks of removed / added lines and mark in-line differences
        while (line < document.TotalNumberOfLines)
        {
            found = false;

            List<ISegment> linesRemoved = GetBlockOfLines(document, DiffLineType.Minus, ref line, ref found);
            List<ISegment> linesAdded = GetBlockOfLines(document, DiffLineType.Plus, ref line, ref found);
            // git-diff presents the removed lines followed by added lines in a "block"

            MarkInlineDifferences(document, linesRemoved, linesAdded, diffContentOffset);
        }
    }

    /// <summary>
    ///  Matches related removed and added lines in a consecutive block and marks identical parts dimmed.
    /// </summary>
    private static void MarkInlineDifferences(IDocument document, IReadOnlyList<ISegment> linesRemoved, IReadOnlyList<ISegment> linesAdded, int beginOffset)
    {
        MarkerStrategy markerStrategy = document.MarkerStrategy;

        Func<ISegment, string> getText = line => document.GetText(line.Offset + beginOffset, line.Length - beginOffset);
        foreach (TextMarker marker in GetDifferenceMarkers(getText, linesRemoved, linesAdded, beginOffset))
        {
            markerStrategy.AddMarker(marker);
        }
    }

    private List<ISegment> GetAllLines(IDocument document, DiffLineType diffLineType)
    => _diffLinesInfo?.DiffLines.Where(i => i.Value.LineType == diffLineType && i.Value.Segment is not null)
            .Select(i => i.Value.Segment)
            .ToList()
            ?? [];

    /// <summary>
    /// Get next block of lines following beginline
    /// </summary>
    /*
    /// <param name="document"></param>
    /// <param name="diffLineType"></param>
    /// <param name="beginIndex"></param>
    /// <param name="found"></param>
    /// <returns></returns>
    */
    private List<ISegment> GetBlockOfLines(IDocument document, DiffLineType diffLineType, ref int beginIndex, ref bool found)
    {
        List<ISegment> result = [];

        while (beginIndex < document.TotalNumberOfLines)
        {
            beginIndex++;
            if (!_diffLinesInfo.DiffLines.TryGetValue(beginIndex, out DiffLineInfo diffLine) || diffLine.Segment is null || diffLine.LineType != diffLineType)
            {
                if (found)
                {
                    // Block ended, no more to add.
                    break;
                }

                // Start of block not found yet.
                continue;
            }

            // In block, continue to add
            found = true;
            result.Add(diffLine.Segment);
        }

        return result;
    }

    private static IEnumerable<TextMarker> GetDifferenceMarkers(Func<ISegment, string> getText, IReadOnlyList<ISegment> linesRemoved, IReadOnlyList<ISegment> linesAdded, int beginOffset)
    {
        foreach ((ISegment lineRemoved, ISegment lineAdded) in LinesMatcher.FindLinePairs(getText, linesRemoved, linesAdded))
        {
            foreach (TextMarker marker in GetDifferenceMarkers(getText, lineRemoved, lineAdded, beginOffset))
            {
                yield return marker;
            }
        }
    }

    private static IEnumerable<TextMarker> GetDifferenceMarkers(Func<ISegment, string> getText, ISegment lineRemoved, ISegment lineAdded, int beginOffset)
    {
        string textRemoved = getText(lineRemoved);
        string textAdded = getText(lineAdded);
        int endRemoved = textRemoved.Length;
        int endAdded = textAdded.Length;
        int startIndexIdenticalRemoved = 0;
        int startIndexIdenticalAdded = 0;

        while (startIndexIdenticalRemoved < endRemoved || startIndexIdenticalAdded < endAdded)
        {
            // find end of identical part (but exclude start of next word in order to match common words entirely)
            int endIndexIdenticalRemoved = startIndexIdenticalRemoved;
            int endIndexIdenticalAdded = startIndexIdenticalAdded;
            while (endIndexIdenticalRemoved < endRemoved && endIndexIdenticalAdded < endAdded
                && textRemoved[endIndexIdenticalRemoved] == textAdded[endIndexIdenticalAdded]
                && !LinesMatcher.IsWordChar(textRemoved[endIndexIdenticalRemoved]))
            {
                ++endIndexIdenticalRemoved;
                ++endIndexIdenticalAdded;
            }

            // find start of identical part at end of line
            int startIndexIdenticalAtEolRemoved = endRemoved;
            int startIndexIdenticalAtEolAdded = endAdded;
            while (startIndexIdenticalAtEolRemoved > endIndexIdenticalRemoved && startIndexIdenticalAtEolAdded > endIndexIdenticalAdded
                && textRemoved[startIndexIdenticalAtEolRemoved - 1] == textAdded[startIndexIdenticalAtEolAdded - 1])
            {
                --startIndexIdenticalAtEolRemoved;
                --startIndexIdenticalAtEolAdded;
            }

            int lengthIdenticalAtEol = endRemoved - startIndexIdenticalAtEolRemoved;
            if (lengthIdenticalAtEol > 0)
            {
                yield return CreateDimmedMarker(lineRemoved, startIndexIdenticalAtEolRemoved, lengthIdenticalAtEol, GetRemovedBackColor());
                yield return CreateDimmedMarker(lineAdded, startIndexIdenticalAtEolAdded, lengthIdenticalAtEol, GetAddedBackColor());
                endRemoved = startIndexIdenticalAtEolRemoved;
                endAdded = startIndexIdenticalAtEolAdded;
            }

            // match on next word
            int endIndexDifferentRemoved;
            int endIndexDifferentAdded;

            (string Word, int Offset)[] wordsRemoved = LinesMatcher.GetWords(textRemoved[endIndexIdenticalRemoved..endRemoved]).ToArray();
            (string? commonWord, int offsetOfWordAdded) = LinesMatcher.GetWords(textAdded[endIndexIdenticalAdded..endAdded])
                .IntersectBy(wordsRemoved.Select(LinesMatcher.SelectWord), LinesMatcher.SelectWord)
                .FirstOrDefault();
            if (commonWord is not null)
            {
                endIndexDifferentRemoved = endIndexIdenticalRemoved + wordsRemoved.First(pair => pair.Word == commonWord).Offset;
                endIndexDifferentAdded = endIndexIdenticalAdded + offsetOfWordAdded;
            }
            else
            {
                endIndexDifferentRemoved = endRemoved;
                endIndexDifferentAdded = endAdded;
            }

            // find end of different part
            while (endIndexDifferentRemoved > endIndexIdenticalRemoved && endIndexDifferentAdded > endIndexIdenticalAdded
                && textRemoved[endIndexDifferentRemoved - 1] == textAdded[endIndexDifferentAdded - 1])
            {
                --endIndexDifferentRemoved;
                --endIndexDifferentAdded;
            }

            // find end of identical part (including partial word)
            while (endIndexIdenticalRemoved < endRemoved && endIndexIdenticalAdded < endAdded
                && textRemoved[endIndexIdenticalRemoved] == textAdded[endIndexIdenticalAdded])
            {
                ++endIndexIdenticalRemoved;
                ++endIndexIdenticalAdded;
            }

            int lengthIdentical = endIndexIdenticalRemoved - startIndexIdenticalRemoved;
            if (lengthIdentical > 0)
            {
                yield return CreateDimmedMarker(lineRemoved, startIndexIdenticalRemoved, lengthIdentical, GetRemovedBackColor());
                yield return CreateDimmedMarker(lineAdded, startIndexIdenticalAdded, lengthIdentical, GetAddedBackColor());
                endIndexDifferentRemoved = Math.Max(endIndexDifferentRemoved, endIndexIdenticalRemoved);
                endIndexDifferentAdded = Math.Max(endIndexDifferentAdded, endIndexIdenticalAdded);
            }

            int lengthRemoved = endIndexDifferentRemoved - endIndexIdenticalRemoved;
            int lengthAdded = endIndexDifferentAdded - endIndexIdenticalAdded;
            if (lengthRemoved == 0 && lengthAdded > 0)
            {
                yield return CreateAnchorMarker(lineRemoved, endIndexIdenticalRemoved, GetAddedForeColor());
            }
            else if (lengthRemoved > 0 && lengthAdded == 0)
            {
                yield return CreateAnchorMarker(lineAdded, endIndexIdenticalAdded, GetRemovedForeColor());
            }

            startIndexIdenticalRemoved = endIndexDifferentRemoved;
            startIndexIdenticalAdded = endIndexDifferentAdded;
        }

        yield break;

        TextMarker CreateAnchorMarker(ISegment line, int offset, Color color)
            => new(line.Offset + beginOffset + offset, length: 0, TextMarkerType.InterChar, color);

        TextMarker CreateDimmedMarker(ISegment line, int offset, int length, Color color)
            => CreateTextMarker(line.Offset + beginOffset + offset, length, ColorHelper.DimColor(ColorHelper.DimColor(color)));

        static TextMarker CreateTextMarker(int offset, int length, Color color)
            => new(offset, length, TextMarkerType.SolidBlock, color, ColorHelper.GetForeColorForBackColor(color));

        static Color GetAddedBackColor() => AppColor.AnsiTerminalGreenBackNormal.GetThemeColor();
        static Color GetAddedForeColor() => AppColor.AnsiTerminalGreenForeBold.GetThemeColor();
        static Color GetRemovedBackColor() => AppColor.AnsiTerminalRedBackNormal.GetThemeColor();
        static Color GetRemovedForeColor() => AppColor.AnsiTerminalRedForeBold.GetThemeColor();
    }
}
