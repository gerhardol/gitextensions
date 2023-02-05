using System.Drawing.Drawing2D;
using GitExtUtils.GitUI;
using GitUI.UserControls.RevisionGrid.Graph;
using GitUIPluginInterfaces;
using Microsoft;

namespace GitUI.UserControls.RevisionGrid.GraphDrawer
{
    internal class SegmentDrawer
    {
        internal int RowHeight { get; init; }

        private readonly Graphics _g;
        private readonly Pen _pen;

        private Point? _fromPoint;
        private bool _fromPerpendicularly = true;

        internal SegmentDrawer(Graphics g, Pen pen, int rowHeight)
        {
            _g = g;
            _pen = pen;
            RowHeight = rowHeight;
        }

        internal void DrawTo(int x, int y, bool toPerpendicularly = true)
            => DrawTo(new Point(x, y), toPerpendicularly);

        internal void DrawTo(Point toPoint, bool toPerpendicularly = true)
        {
            try
            {
                if (_fromPoint is null)
                {
                    return;
                }

                DrawTo(_g, _pen, _fromPoint.Value, toPoint, _fromPerpendicularly, toPerpendicularly, RowHeight);
            }
            finally
            {
                _fromPoint = toPoint;
                _fromPerpendicularly = toPerpendicularly;
            }
        }

        private static void DrawTo(Graphics g, Pen pen, Point fromPoint, Point toPoint, bool fromPerpendicularly, bool toPerpendicularly, int rowHeight)
        {
            if (fromPoint.X == toPoint.X)
            {
                // direct line without anti-aliasing
                g.SmoothingMode = SmoothingMode.None;
                g.DrawLine(pen, fromPoint, toPoint);
                return;
            }

            // Anti-aliasing with bezier & PixelOffsetMode.HighQuality introduces an offset of ~1/5 px - compensate it.
            g.SmoothingMode = SmoothingMode.AntiAlias;
            const float antiAliasOffset = -1f / 5f;
            PointF e0 = new(antiAliasOffset + fromPoint.X, fromPoint.Y);
            PointF e1 = new(antiAliasOffset + toPoint.X, toPoint.Y);

            if (!fromPerpendicularly && !toPerpendicularly)
            {
                // direct line with anti-aliasing
                g.DrawLine(pen, e0, e1);
            }
            else
            {
                // control points for bezier curve
                PointF c0 = e0;
                PointF c1 = e1;

                if (fromPerpendicularly && toPerpendicularly)
                {
                    float midY = 1f / 2f * (fromPoint.Y + toPoint.Y);
                    c0.Y = midY;
                    c1.Y = midY;
                }
                else
                {
                    int laneWidth = toPoint.X - fromPoint.X;
                    int height = toPoint.Y - fromPoint.Y;

                    float diagonalFractionStraight = height < rowHeight ? 2f / 5f : 1f / 2f;
                    float diagonalFractionCurve = 1f / 4f;
                    float perpendicularFraction = diagonalFractionCurve;
                    float perpendicularOffset = perpendicularFraction * Math.Min(height, rowHeight);

                    if (fromPerpendicularly)
                    {
                        // draw diagonally to e1
                        c1.X -= diagonalFractionStraight * laneWidth;
                        c1.Y -= diagonalFractionStraight * rowHeight;
                        g.DrawLine(pen, c1, e1);

                        // prepare remaining curve
                        e1 = c1;
                        c1.X -= diagonalFractionCurve * laneWidth;
                        c1.Y -= diagonalFractionCurve * rowHeight;
                        c0.Y += perpendicularOffset;
                    }
                    else
                    {
                        // draw diagonally from e0
                        c0.X += diagonalFractionStraight * laneWidth;
                        c0.Y += diagonalFractionStraight * rowHeight;
                        g.DrawLine(pen, e0, c0);

                        // prepare remaining curve
                        e0 = c0;
                        c0.X += diagonalFractionCurve * laneWidth;
                        c0.Y += diagonalFractionCurve * rowHeight;
                        c1.Y -= perpendicularOffset;
                    }
                }

                g.DrawBezier(pen, e0, c0, c1, e1);
            }
        }
    }
}
