using MVVMPaintApp.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models.Tools
{
    public class Pencil : ToolBase
    {
        public int BrushSize { get; set; } = 1;
        private readonly List<Point> currentStroke = [];
        public const int PREVIEW_STROKE_REGION_PADDING = 4;

        public Pencil(ProjectManager projectManager) : base(projectManager)
        {
            DrawPreview(LastPoint, ProjectManager.PrimaryColor);
        }

        public override void OnMouseDown(object sender, MouseButtonEventArgs e, Point p)
        {

            base.OnMouseDown(sender, e, p);
            if (!IsValidDrawingState()) return;

            currentStroke.Clear();
            currentStroke.Add(p);
            DrawPoint(p, GetCurrentColor(e));
        }

        public override void OnMouseMove(object sender, MouseEventArgs e, Point p)
        {

            DrawPreview(p, GetCurrentColor(e));
            if (!IsDrawing || !IsValidDrawingState()) return;

            if (currentStroke.Count > 0)
            {
                Point lastStoredPoint = currentStroke[^1];
                if (CalculateDistance(lastStoredPoint, p) < CalculateStrokeSpacing()) return;
                currentStroke.Add(p);
                CalculateStrokeRegion(currentStroke);
            }
            else
            {
                currentStroke.Add(p);
            }
            DrawLine(LastPoint, p, GetCurrentColor(e));
            LastPoint = p;
        }

        public override void OnMouseUp(object sender, MouseButtonEventArgs e, Point p)
        {
            if (currentStroke.Count > 0)
            {
                CalculateStrokeRegion(currentStroke);
                if(CurrentStrokeRegion!.Value.IsEmpty) return;

                OldState = new((int)CurrentStrokeRegion.Value.Width, (int)CurrentStrokeRegion.Value.Height, 96, 96, PixelFormats.Bgra32, null);
                OldState.Blit(new Rect(0.0, 0.0, CurrentStrokeRegion.Value.Width, CurrentStrokeRegion.Value.Height), ProjectManager.SelectedLayer.Content, CurrentStrokeRegion.Value);
                BlitStrokeLayer();
                ProjectManager.UndoRedoManager.AddHistoryEntry(
                new LayerHistoryEntry(
                    ProjectManager.SelectedLayer,
                    CurrentStrokeRegion.Value,
                    OldState));

                currentStroke.Clear();
            }

            ProjectManager.StrokeLayer.Clear(Colors.Transparent);
            ProjectManager.Render(new Rect(0, 0, ProjectManager.CurrentProject.Width, ProjectManager.CurrentProject.Height));
            ProjectManager.HasUnsavedChanges = true;
            CurrentStrokeRegion = null;
            OldState = null;

            base.OnMouseUp(sender, e, p);
        }

        public override Cursor GetCursor()
        {
            return Cursors.Pen;
        }

        private float CalculateStrokeSpacing()
        {
            return Math.Max(MIN_INTERP_DISTANCE, 0.05f * BrushSize);
        }

        public override void DrawPreview(Point p, Color color)
        {
            int radius = Math.Max(1, BrushSize / 2);
            var region = new Rect(
                p.X - radius - PREVIEW_STROKE_REGION_PADDING,
                p.Y - radius - PREVIEW_STROKE_REGION_PADDING,
                (radius * 2) + (PREVIEW_STROKE_REGION_PADDING * 2),
                (radius * 2) + (PREVIEW_STROKE_REGION_PADDING * 2));

            if (IsValidDrawingState() && CurrentStrokeRegion == null)
            {
                ProjectManager.StrokeLayer.FillRectangle(
                    (int)(p.X - radius - PREVIEW_STROKE_REGION_PADDING),
                    (int)(p.Y - radius - PREVIEW_STROKE_REGION_PADDING),
                    (int)(p.X + radius + PREVIEW_STROKE_REGION_PADDING),
                    (int)(p.Y + radius + PREVIEW_STROKE_REGION_PADDING),
                    Colors.Transparent
                );
                DrawPoint(p, color);
                ProjectManager.Render(region);
            }
        }

        public override void DrawPoint(Point point, Color color)
        {
            if(BrushSize == 1)
            {
                for (int dx = -BrushSize / 2; dx <= BrushSize / 2; dx++)
                {
                    for (int dy = -BrushSize / 2; dy <= BrushSize / 2; dy++)
                    {
                        int x = (int)point.X + dx;
                        int y = (int)point.Y + dy;
                        if (x >= 0 && x < ProjectManager.StrokeLayer.PixelWidth &&
                            y >= 0 && y < ProjectManager.StrokeLayer.PixelHeight)
                        {
                            ProjectManager.StrokeLayer.SetPixel(x, y, color);
                        }
                    }
                }
            } else
            {
                int radius = Math.Max(1, BrushSize / 2);
                ProjectManager.StrokeLayer.FillEllipseCentered(
                    (int)point.X,
                    (int)point.Y,
                    radius,
                    radius,
                    color
                );
            }
        }

        public override void DrawLine(Point start, Point end, Color color)
        {
            int x0 = (int)start.X;
            int y0 = (int)start.Y;
            int x1 = (int)end.X;
            int y1 = (int)end.Y;

            // Bresenham's line algorithm
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                DrawPoint(new Point(x0, y0), color);

                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }

            var bounds = CalculateSegmentRegion(start, end, BrushSize);
            ProjectManager.Render(bounds);
        }
        private void CalculateStrokeRegion(List<Point> points)
        {
            if (points.Count == 0)
            {
                CurrentStrokeRegion = new Rect();
                return;
            }

            double minX = points[0].X;
            double minY = points[0].Y;
            double maxX = points[0].X;
            double maxY = points[0].Y;

            foreach (var point in points)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }

            var rect = new Rect(minX, minY, Math.Max(1, maxX - minX), Math.Max(1, maxY - minY));
            rect.Inflate(BrushSize + STROKE_REGION_PADDING, BrushSize + STROKE_REGION_PADDING);
            rect.Intersect(new Rect(0, 0, ProjectManager.CurrentProject.Width, ProjectManager.CurrentProject.Height));
            CurrentStrokeRegion = rect;
            return;
        }
    }
}

