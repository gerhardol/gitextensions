using ICSharpCode.TextEditor.Document;

namespace GitUI.Editor.Diff;

public struct Segment : ISegment
{
    public int Offset { get; set; }
    public int Length { get; set; }
}

public class DiffLineInfo
{
    public static readonly int NotApplicableLineNum = -1;
    public int LineNumInDiff { get; set; }
    public int LeftLineNumber { get; set; }
    public int RightLineNumber { get; set; }
    public DiffLineType LineType { get; set; }
    public ISegment? Segment { get; set; } // offset and length, set for line type Minus/Plus
    public bool IsAddedRemoved { get; set; } // Heuristics expected to be added or removed
}
