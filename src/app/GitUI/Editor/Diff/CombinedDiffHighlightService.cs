using GitExtensions.Extensibility.Git;
using ICSharpCode.TextEditor;

namespace GitUI.Editor.Diff;

public class CombinedDiffHighlightService : DiffHighlightService
{
    private static readonly string[] _diffFullPrefixes = ["  ", "++", "+ ", " +", "--", "- ", " -"];

    public CombinedDiffHighlightService(ref string text, bool useGitColoring)
        : base(ref text, useGitColoring)
    {
    }

    public override void SetLineControl(DiffViewerLineNumberControl lineNumbersControl, TextEditorControl textEditor)
    {
        _diffLinesInfo = DiffLineNumAnalyzer.Analyze(textEditor, isCombinedDiff: true);
        lineNumbersControl.DisplayLineNum(_diffLinesInfo, showLeftColumn: true);
    }

    public static IGitCommandConfiguration GetGitCommandConfiguration(IGitModule module, bool useGitColoring)
        => GetGitCommandConfiguration(module, useGitColoring, "diff-tree");

    public override string[] GetFullDiffPrefixes() => _diffFullPrefixes;
}
