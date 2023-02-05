using System.Diagnostics;
using System.Drawing.Drawing2D;
using GitCommands;
using GitExtUtils.GitUI;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.UserControls.RevisionGrid.GraphDrawer
{
    internal static class GraphDrawer
    {
        internal const int MaxLanes = RevisionGraph.MaxLanes;

        internal static readonly int LaneLineWidth = DpiUtil.Scale(2);
        internal static readonly int LaneWidth = DpiUtil.Scale(16);
        internal static readonly int NodeDimension = DpiUtil.Scale(10);

        private const int _noLane = -10;

        internal static void DrawItem(Graphics g, int index, int width, int rowHeight,
            Func<int, IRevisionGraphRow?> getSegmentsForRow,
            RevisionGraphDrawStyleEnum revisionGraphDrawStyle,
            ObjectId headId)
        {
            SmoothingMode oldSmoothingMode = g.SmoothingMode;
            Region oldClip = g.Clip;

            int top = g.RenderingOrigin.Y;
            Rectangle laneRect = new(0, top, width, rowHeight);
            Region newClip = new(laneRect);
            newClip.Intersect(oldClip);
            g.Clip = newClip;
            g.Clear(Color.Transparent);

            DrawItem();

            // Restore graphics options
            g.Clip = oldClip;
            g.SmoothingMode = oldSmoothingMode;

            return;

            void DrawItem()
            {
                IRevisionGraphRow? currentRow = getSegmentsForRow(index);
                if (currentRow is null)
                {
                    return;
                }

                IRevisionGraphRow? previousRow = getSegmentsForRow(index - 1);
                IRevisionGraphRow? nextRow = getSegmentsForRow(index + 1);

                SegmentPoints p = new();
                p.Center.Y = top + (rowHeight / 2);
                p.Start.Y = p.Center.Y - rowHeight;
                p.End.Y = p.Center.Y + rowHeight;

                LaneInfo? currentRowRevisionLaneInfo = null;

                foreach (RevisionGraphSegment revisionGraphSegment in currentRow.Segments.Reverse().OrderBy(s => s.Child.IsRelative))
                {
                    SegmentLanes lanes = GetLanes(revisionGraphSegment, previousRow, currentRow, nextRow, li => currentRowRevisionLaneInfo = li);
                    if (!lanes.DrawFromStart && !lanes.DrawToEnd)
                    {
                        continue;
                    }

                    int originX = g.RenderingOrigin.X;
                    p.Start.X = originX + (int)((lanes.StartLane + 0.5) * LaneWidth);
                    p.Center.X = originX + (int)((lanes.CenterLane + 0.5) * LaneWidth);
                    p.End.X = originX + (int)((lanes.EndLane + 0.5) * LaneWidth);

                    Brush laneBrush = GetBrushForLaneInfo(revisionGraphSegment.LaneInfo, revisionGraphSegment.Child.IsRelative, revisionGraphDrawStyle);
                    using Pen lanePen = new(laneBrush, LaneLineWidth);
                    SegmentDrawer segmentDrawer = new(g, lanePen, rowHeight);

                    if (AppSettings.ReduceGraphCurves)
                    {
                        Lazy<SegmentLanes> previousLanes = new(() =>
                        {
                            Validates.NotNull(previousRow);
                            return GetLanes(revisionGraphSegment, getSegmentsForRow(index - 2), previousRow, currentRow);
                        });
                        Lazy<SegmentLanes> nextLanes = new(() =>
                        {
                            Validates.NotNull(nextRow);
                            return GetLanes(revisionGraphSegment, currentRow, nextRow, getSegmentsForRow(index + 2));
                        });
                        Lazy<SegmentLanes> farLanesDontMatter = null;

                        Lazy<SegmentLaneFlags> previousLaneFlags = new(() =>
                        {
                            return GetStraightenedLaneFlags(previousLanes: farLanesDontMatter, currentLanes: previousLanes.Value, nextLanes: new(() => lanes));
                        });
                        Lazy<SegmentLaneFlags> nextLaneFlags = new(() =>
                        {
                            return GetStraightenedLaneFlags(previousLanes: new(() => lanes), currentLanes: nextLanes.Value, nextLanes: farLanesDontMatter);
                        });
                        SegmentLaneFlags currentLaneFlags = GetStraightenedLaneFlags(previousLanes, lanes, nextLanes);

                        DrawSegmentStraightened(segmentDrawer, p, previousLaneFlags, currentLaneFlags, nextLaneFlags);
                    }
                    else
                    {
                        DrawSegmentCurvy(segmentDrawer, p, lanes);
                    }
                }

                if (currentRow.GetCurrentRevisionLane() < MaxLanes)
                {
                    int centerX = g.RenderingOrigin.X + (int)((currentRow.GetCurrentRevisionLane() + 0.5) * LaneWidth);
                    Rectangle nodeRect = new(centerX - (NodeDimension / 2), p.Center.Y - (NodeDimension / 2), NodeDimension, NodeDimension);

                    bool square = currentRow.Revision.GitRevision.Refs.Count > 0;
                    bool hasOutline = currentRow.Revision.GitRevision.ObjectId == headId;

                    Brush brush = GetBrushForLaneInfo(currentRowRevisionLaneInfo, currentRow.Revision.IsRelative, revisionGraphDrawStyle);
                    if (square)
                    {
                        g.SmoothingMode = SmoothingMode.None;
                        g.FillRectangle(brush, nodeRect);
                    }
                    else //// Circle
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.FillEllipse(brush, nodeRect);
                    }

                    if (hasOutline)
                    {
                        nodeRect.Inflate(1, 1);

                        Color outlineColor = SystemColors.WindowText;

                        using Pen pen = new(outlineColor, 2);
                        if (square)
                        {
                            g.SmoothingMode = SmoothingMode.None;
                            g.DrawRectangle(pen, nodeRect);
                        }
                        else //// Circle
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.DrawEllipse(pen, nodeRect);
                        }
                    }
                }
            }
        }

        private static SegmentLanes GetLanes(RevisionGraphSegment revisionGraphSegment,
            IRevisionGraphRow? previousRow,
            IRevisionGraphRow currentRow,
            IRevisionGraphRow? nextRow,
            Action<LaneInfo?>? setLaneInfo = null)
        {
            SegmentLanes lanes = new() { StartLane = _noLane, EndLane = _noLane };

            if (revisionGraphSegment.Parent == currentRow.Revision)
            {
                // This lane ends here
                lanes.StartLane = GetLaneForRow(previousRow, revisionGraphSegment);
                lanes.CenterLane = GetLaneForRow(currentRow, revisionGraphSegment);
                setLaneInfo?.Invoke(revisionGraphSegment.LaneInfo);
            }
            else
            {
                if (revisionGraphSegment.Child == currentRow.Revision)
                {
                    // This lane starts here
                    lanes.CenterLane = GetLaneForRow(currentRow, revisionGraphSegment);
                    lanes.EndLane = GetLaneForRow(nextRow, revisionGraphSegment);
                    setLaneInfo?.Invoke(revisionGraphSegment.LaneInfo);
                }
                else
                {
                    // This lane crosses
                    lanes.StartLane = GetLaneForRow(previousRow, revisionGraphSegment);
                    lanes.CenterLane = GetLaneForRow(currentRow, revisionGraphSegment);
                    lanes.EndLane = GetLaneForRow(nextRow, revisionGraphSegment);
                }
            }

            lanes.DrawFromStart = lanes.StartLane >= 0 && lanes.CenterLane >= 0 && (lanes.StartLane <= MaxLanes || lanes.CenterLane <= MaxLanes);
            lanes.DrawToEnd = lanes.EndLane >= 0 && lanes.CenterLane >= 0 && (lanes.EndLane <= MaxLanes || lanes.CenterLane <= MaxLanes);

            return lanes;
        }

        private static SegmentLaneFlags GetStraightenedLaneFlags(Lazy<SegmentLanes>? previousLanes,
            SegmentLanes currentLanes,
            Lazy<SegmentLanes>? nextLanes)
        {
            SegmentLaneFlags flags = new()
            {
                DrawFromStart = currentLanes.DrawFromStart,
                DrawToEnd = currentLanes.DrawToEnd
            };

            // Go perpendicularly through the center in order to avoid crossing independend nodes
            flags.DrawCenterToStartPerpendicularly = flags.DrawFromStart
                && (Math.Abs(currentLanes.CenterLane - currentLanes.StartLane) != 1
                    || (currentLanes.EndLane < 0 && previousLanes?.Value.StartLane is < 0));
            flags.DrawCenterToEndPerpendicularly = flags.DrawToEnd
                && (Math.Abs(currentLanes.CenterLane - currentLanes.EndLane) != 1
                    || (currentLanes.StartLane < 0 && nextLanes?.Value.EndLane is < 0));
            flags.DrawCenterPerpendicularly
                //// lane shifted by one at end, not starting a diagonal over multiple lanes
                //// (lane end is classed as diagonal, i.e. shall also not be a lane end, i.e. DrawToEnd be true)
                = ((currentLanes.StartLane < 0 || currentLanes.StartLane == currentLanes.CenterLane)
                    && Math.Abs(currentLanes.CenterLane - currentLanes.EndLane) == 1
                    && (nextLanes?.Value.EndLane is not >= 0
                        || currentLanes.CenterLane - currentLanes.EndLane
                            != currentLanes.EndLane - nextLanes!.Value.EndLane))
                //// lane shifted by one at start, not starting a diagonal over multiple lanes
                || ((currentLanes.EndLane < 0 || currentLanes.EndLane == currentLanes.CenterLane)
                    && Math.Abs(currentLanes.CenterLane - currentLanes.StartLane) == 1
                    && (previousLanes?.Value.StartLane is not >= 0
                        || currentLanes.CenterLane - currentLanes.StartLane
                            != currentLanes.StartLane - previousLanes!.Value.StartLane))
                //// bow to the right
                || (currentLanes.StartLane >= 0 && currentLanes.EndLane >= 0
                    && currentLanes.StartLane < currentLanes.CenterLane && currentLanes.EndLane < currentLanes.CenterLane)
                //// bow to the left
                || (currentLanes.StartLane > currentLanes.CenterLane && currentLanes.EndLane > currentLanes.CenterLane);
            flags.DrawCenter = flags.DrawCenterPerpendicularly
                || !flags.DrawFromStart
                || !flags.DrawToEnd
                || (!flags.DrawCenterToStartPerpendicularly && !flags.DrawCenterToEndPerpendicularly);

            return flags;
        }

        private static void DrawSegmentCurvy(SegmentDrawer segmentDrawer, SegmentPoints p, SegmentLanes lanes)
        {
            if (lanes.DrawFromStart)
            {
                segmentDrawer.DrawTo(p.Start);
            }

            segmentDrawer.DrawTo(p.Center);

            if (lanes.DrawToEnd)
            {
                segmentDrawer.DrawTo(p.End);
            }
        }

        private static void DrawSegmentStraightened(SegmentDrawer segmentDrawer,
            SegmentPoints p,
            Lazy<SegmentLaneFlags> previousLaneFlags,
            SegmentLaneFlags currentLaneFlags,
            Lazy<SegmentLaneFlags> nextLaneFlags)
        {
            int halfPerpendicularHeight = segmentDrawer.RowHeight / 5;
            int diagonalLaneEndOffset = halfPerpendicularHeight / 2;

            if (currentLaneFlags.DrawFromStart)
            {
                SegmentLaneFlags previous = previousLaneFlags.Value;
                Debug.Assert(previous.DrawToEnd, nameof(previous.DrawToEnd));
                if (previous.DrawCenterToEndPerpendicularly)
                {
                    segmentDrawer.DrawTo(p.Start.X, p.Start.Y + halfPerpendicularHeight);
                }
                else if (previous.DrawCenter)
                {
                    // shift diagonal lane end
                    if (!previous.DrawCenterPerpendicularly && !previous.DrawFromStart)
                    {
                        segmentDrawer.DrawTo(p.Start.X, p.Start.Y + diagonalLaneEndOffset, toPerpendicularly: false);
                    }
                    else
                    {
                        segmentDrawer.DrawTo(p.Start, previous.DrawCenterPerpendicularly);
                    }
                }
                else
                {
                    segmentDrawer.DrawTo(p.Start.X, p.Start.Y - halfPerpendicularHeight);
                }
            }

            if (currentLaneFlags.DrawCenterToStartPerpendicularly)
            {
                segmentDrawer.DrawTo(p.Center.X, p.Center.Y - halfPerpendicularHeight);
            }

            if (currentLaneFlags.DrawCenter)
            {
                // shift diagonal lane ends
                if (!currentLaneFlags.DrawCenterPerpendicularly && !currentLaneFlags.DrawToEnd)
                {
                    segmentDrawer.DrawTo(p.Center.X, p.Center.Y - diagonalLaneEndOffset, toPerpendicularly: false);
                }
                else if (!currentLaneFlags.DrawCenterPerpendicularly && !currentLaneFlags.DrawFromStart)
                {
                    segmentDrawer.DrawTo(p.Center.X, p.Center.Y + diagonalLaneEndOffset, toPerpendicularly: false);
                }
                else
                {
                    segmentDrawer.DrawTo(p.Center, currentLaneFlags.DrawCenterPerpendicularly);
                }
            }

            if (currentLaneFlags.DrawCenterToEndPerpendicularly)
            {
                segmentDrawer.DrawTo(p.Center.X, p.Center.Y + halfPerpendicularHeight);
            }

            if (currentLaneFlags.DrawToEnd)
            {
                SegmentLaneFlags next = nextLaneFlags.Value;
                Debug.Assert(next.DrawFromStart, nameof(next.DrawFromStart));
                if (next.DrawCenterToStartPerpendicularly)
                {
                    segmentDrawer.DrawTo(p.End.X, p.End.Y - halfPerpendicularHeight);
                }
                else if (next.DrawCenter)
                {
                    // shift diagonal lane end
                    if (!next.DrawCenterPerpendicularly && !next.DrawToEnd)
                    {
                        segmentDrawer.DrawTo(p.End.X, p.End.Y - diagonalLaneEndOffset, toPerpendicularly: false);
                    }
                    else
                    {
                        segmentDrawer.DrawTo(p.End, next.DrawCenterPerpendicularly);
                    }
                }
                else
                {
                    segmentDrawer.DrawTo(p.End.X, p.End.Y + halfPerpendicularHeight);
                }
            }
        }

        private static Brush GetBrushForLaneInfo(LaneInfo? laneInfo, bool isRelative, RevisionGraphDrawStyleEnum revisionGraphDrawStyle)
        {
            // laneInfo can be null for revisions without parents and children, especially when filtering, draw them gray, too
            if (laneInfo is null
                || (!isRelative && (revisionGraphDrawStyle is RevisionGraphDrawStyleEnum.DrawNonRelativesGray or RevisionGraphDrawStyleEnum.HighlightSelected)))
            {
                return RevisionGraphLaneColor.NonRelativeBrush;
            }

            return RevisionGraphLaneColor.GetBrushForLane(laneInfo.Color);
        }

        private static int GetLaneForRow(IRevisionGraphRow? row, RevisionGraphSegment revisionGraphRevision)
        {
            if (row is not null)
            {
                int lane = row.GetLaneIndexForSegment(revisionGraphRevision);
                if (lane >= 0)
                {
                    return lane;
                }
            }

            return _noLane;
        }
    }
}
