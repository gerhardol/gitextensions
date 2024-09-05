using System.Text.RegularExpressions;
using GitCommands;
using GitExtensions.Extensibility;
using GitExtUtils.GitUI.Theming;
using GitUI.Theming;
using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

public partial class DiffLineNumAnalyzer
{
    [GeneratedRegex(@"\-(?<leftStart>\d{1,})\,{0,}(?<leftCount>\d{0,})\s\+(?<rightStart>\d{1,})\,{0,}(?<rightCount>\d{0,})", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture)]
    private static partial Regex DiffRegex();

    public static DiffLinesInfo Analyze(string text, List<TextMarker> allTextMarkers, bool isCombinedDiff, bool isGitWordDiff = false)
    {
        DiffLinesInfo ret = new();
        int lineNumInDiff = 0;
        int leftLineNum = DiffLineInfo.NotApplicableLineNum;
        int rightLineNum = DiffLineInfo.NotApplicableLineNum;
        bool isHeaderLineLocated = false;
        string[] lines = text.Split(Delimiters.LineFeed);
        int textOffset = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (i == lines.Length - 1 && string.IsNullOrEmpty(line))
            {
                break;
            }

            int textLength = lines[i].Length + 1;
            Lazy<List<TextMarker>> textMarkers = new(()
                => allTextMarkers.Where(i => (i.Offset >= textOffset && i.Offset < textOffset + textLength)
                    || (i.EndOffset > textOffset && i.EndOffset < textOffset + textLength)).ToList());
            lineNumInDiff++;
            if (line.StartsWith("@@"))
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                    RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                    LineType = DiffLineType.Header
                };

                Match lineNumbers = DiffRegex().Match(line);
                leftLineNum = int.Parse(lineNumbers.Groups["leftStart"].Value);
                rightLineNum = int.Parse(lineNumbers.Groups["rightStart"].Value);

                ret.Add(meta);
                isHeaderLineLocated = true;
            }
            else if (isHeaderLineLocated && isCombinedDiff)
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                    RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                    Segment = new Segment() { Offset = textOffset, Length = textLength },
                    IsAddedRemoved = true,
                };

                if (IsMinusLineInCombinedDiff(line))
                {
                    // left line is from two documents, so undefined
                    meta.LineType = DiffLineType.Minus;
                    meta.LeftLineNumber = leftLineNum;
                    leftLineNum++;
                }
                else if (IsPlusLineInCombinedDiff(line))
                {
                    meta.LineType = DiffLineType.Plus;
                    meta.RightLineNumber = rightLineNum;
                    rightLineNum++;
                }
                else
                {
                    meta.LineType = DiffLineType.Context;
                    meta.RightLineNumber = rightLineNum;
                    rightLineNum++;
                }

                ret.Add(meta);
            }
            else if (isHeaderLineLocated && !isGitWordDiff ? IsMinusLine(line)

                // Heuristics: For GitWordDiff AppSettings.ReverseGitColoring is assumed, otherwise just DiffLineType.MinusPlus is detected
                : (textMarkers.Value.Count > 0 && textMarkers.Value.All(i => i.Color == AppColor.AnsiTerminalRedBackNormal.GetThemeColor())))
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = leftLineNum,
                    RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                    LineType = isGitWordDiff ? DiffLineType.MinusLeft : DiffLineType.Minus,
                    Segment = new Segment() { Offset = textOffset, Length = textLength },

                    // Heuristics, Git coloring uses other colors for moved lines
                    IsAddedRemoved = textMarkers.Value.Count == 0 || textMarkers.Value.All(i => i.Color == AppColor.AnsiTerminalRedBackNormal.GetThemeColor()),
                };
                ret.Add(meta);

                leftLineNum++;
            }
            else if (isHeaderLineLocated && !isGitWordDiff ? IsPlusLine(line)
                : (textMarkers.Value.Count > 0 && textMarkers.Value.All(i => i.Color == AppColor.AnsiTerminalGreenBackNormal.GetThemeColor())))
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                    RightLineNumber = rightLineNum,
                    LineType = isGitWordDiff ? DiffLineType.PlusRight : DiffLineType.Plus,
                    Segment = new Segment() { Offset = textOffset, Length = textLength },
                    IsAddedRemoved = textMarkers.Value.Count == 0 || textMarkers.Value.All(i => i.Color == AppColor.AnsiTerminalGreenBackNormal.GetThemeColor()),
                };
                ret.Add(meta);
                rightLineNum++;
            }
            else if (isHeaderLineLocated && isGitWordDiff && textMarkers.Value.Count > 0)
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = leftLineNum,
                    RightLineNumber = rightLineNum,
                    LineType = DiffLineType.MinusPlus,
                };
                ret.Add(meta);
                leftLineNum++;
                rightLineNum++;
            }
            else if (i == lines.Length - 1 && line.StartsWith(GitModule.NoNewLineAtTheEnd))
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = DiffLineInfo.NotApplicableLineNum,
                    RightLineNumber = DiffLineInfo.NotApplicableLineNum,
                    LineType = DiffLineType.Header
                };
                ret.Add(meta);
            }
            else if (isHeaderLineLocated)
            {
                DiffLineInfo meta = new()
                {
                    LineNumInDiff = lineNumInDiff,
                    LeftLineNumber = leftLineNum,
                    RightLineNumber = rightLineNum,
                    LineType = DiffLineType.Context,
                };
                ret.Add(meta);

                leftLineNum++;
                rightLineNum++;
            }

            textOffset += textLength;
        }

        return ret;
    }

    private static bool IsMinusLine(string line)
    {
        return line.StartsWith('-');
    }

    private static bool IsPlusLine(string line)
    {
        return line.StartsWith('+');
    }

    private static bool IsPlusLineInCombinedDiff(string line)
    {
        return line.StartsWith("++") || line.StartsWith("+ ") || line.StartsWith(" +");
    }

    private static bool IsMinusLineInCombinedDiff(string line)
    {
        return line.StartsWith("--") || line.StartsWith("- ") || line.StartsWith(" -");
    }
}
