using CommunityToolkit.Mvvm.ComponentModel;
using MVVMPaintApp.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models.Tools
{
    public class Brush : ToolBase
    {
        public int BrushSize { get; set; } = 40;
        private const int PREVIEW_STROKE_REGION_PADDING = 10;

        public Brush(ProjectManager projectManager) : base(projectManager)
        {
            DrawPreview(LastPoint, ProjectManager.PrimaryColor);
        }

        public override void OnMouseDown(object sender, MouseEventArgs e, Point p)
        {
            base.OnMouseDown(sender, e, p);
            if (!IsValidDrawingState()) return;
            CurrentStrokeRegion = new Rect(p, new Size(1, 1));
            DrawPoint(p, GetCurrentColor(e));
        }

        public override void OnMouseMove(object sender, MouseEventArgs e, Point p)
        {
            DrawPreview(p, ProjectManager.PrimaryColor);
            if (!IsDrawing || !IsValidDrawingState()) return;
            float distance = CalculateDistance(LastPoint, p);
            if (distance < MIN_INTERP_DISTANCE) return;
            DrawLine(LastPoint, p, GetCurrentColor(e));
            LastPoint = p;
        }

        public override void OnMouseUp(object sender, MouseEventArgs e, Point p)
        {
            if (ProjectManager.StrokeLayer != null && CurrentStrokeRegion != null)
            {
                OldState = new((int)CurrentStrokeRegion.Value.Width, (int)CurrentStrokeRegion.Value.Height, 96,96, PixelFormats.Bgra32, null);
                OldState.Blit(new Rect(0.0, 0.0, CurrentStrokeRegion.Value.Width, CurrentStrokeRegion.Value.Height), ProjectManager.SelectedLayer.Content, CurrentStrokeRegion.Value);
                BlitStrokeLayer();
                ProjectManager.UndoRedoManager.AddHistoryEntry(
                    new LayerHistoryEntry(
                        ProjectManager.SelectedLayer,
                        CurrentStrokeRegion.Value,
                        OldState));
            }

            ProjectManager.StrokeLayer.Clear(Colors.Transparent);
            ProjectManager.Render(new Rect(0, 0, ProjectManager.CurrentProject.Width, ProjectManager.CurrentProject.Height));
            CurrentStrokeRegion = null;
            OldState = null;

            base.OnMouseUp(sender, e, p);
        }

        public override void DrawPreview(Point p, Color color)
        {
            int x1 = (int)(p.X - BrushSize);
            int y1 = (int)(p.Y - BrushSize);
            int x2 = (int)(p.X + BrushSize);
            int y2 = (int)(p.Y + BrushSize);

            var region = new Rect(
                x1 - PREVIEW_STROKE_REGION_PADDING,
                y1 - PREVIEW_STROKE_REGION_PADDING,
                x2 - x1 + PREVIEW_STROKE_REGION_PADDING * 2,
                y2 - y1 + PREVIEW_STROKE_REGION_PADDING * 2);


            if (IsValidDrawingState() && CurrentStrokeRegion == null)
            {
                ProjectManager.StrokeLayer.FillRectangle(
                    x1 - PREVIEW_STROKE_REGION_PADDING,
                    y1 - PREVIEW_STROKE_REGION_PADDING,
                    x2 + PREVIEW_STROKE_REGION_PADDING,
                    y2 + PREVIEW_STROKE_REGION_PADDING,
                    Colors.Transparent
                );
                ProjectManager.StrokeLayer.FillEllipseCentered(
                    (int)p.X,
                    (int)p.Y,
                    BrushSize / 2,
                    BrushSize / 2,
                    color
                );
                ProjectManager.Render(region);
            }
        }

        public override void DrawPoint(Point point, Color color)
        {
            try
            {
                var totalPadding = BrushSize + STROKE_REGION_PADDING;
                var region = new Rect(
                    point.X - totalPadding,
                    point.Y - totalPadding,
                    totalPadding * 2,
                    totalPadding * 2
                );

                ProjectManager.StrokeLayer.FillEllipseCentered(
                    (int)point.X,
                    (int)point.Y,
                    BrushSize / 2,
                    BrushSize / 2,
                    color
                );

                if (CurrentStrokeRegion.HasValue)
                {
                    CurrentStrokeRegion = Rect.Union(CurrentStrokeRegion.Value, region);
                }

                ProjectManager.Render(region);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error drawing point: {ex.Message}");
            }
        }

        public override void DrawLine(Point start, Point end, Color color)
        {
            try
            {
                float distance = CalculateDistance(start, end);
                int steps = Math.Max(1, (int)(distance / (BrushSize * INTERP_FACTOR)));

                Point lastDrawnPoint = start;

                for (int i = 0; i <= steps; i++)
                {
                    float t = i / (float)steps;
                    var currentPoint = Lerp(start, end, t);

                    var region = CalculateSegmentRegion(lastDrawnPoint, currentPoint, BrushSize);

                    ProjectManager.StrokeLayer.FillEllipseCentered(
                        (int)currentPoint.X,
                        (int)currentPoint.Y,
                        BrushSize / 2,
                        BrushSize / 2,
                        color
                    );

                    if (CurrentStrokeRegion.HasValue)
                    {
                        CurrentStrokeRegion = Rect.Union(CurrentStrokeRegion.Value, region);
                    }

                    ProjectManager.Render(region);
                    lastDrawnPoint = currentPoint;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error drawing line: {ex.Message}");
            }
        }
    }
}