using MVVMPaintApp.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models.Tools
{
    public class Eraser : ToolBase
    {
        public int BrushSize { get; set; } = 50;
        public const int PREVIEW_STROKE_REGION_PADDING = 20;

        public Eraser(ProjectManager projectManager) : base(projectManager)
        {
            DrawPreview(LastPoint, Colors.Transparent);
        }

        public override void OnMouseDown(object sender, MouseButtonEventArgs e, Point p)
        {
            base.OnMouseDown(sender, e, p);
            if (!IsValidDrawingState()) return;

            OldState = ProjectManager.SelectedLayer.Content.Clone();

            CurrentStrokeRegion = new Rect(p, new Size(1, 1));

            DrawPoint(p, Colors.Transparent);
        }

        public override void OnMouseMove(object sender, MouseEventArgs e, Point p)
        {
            DrawPreview(p, ProjectManager.CurrentProject.Background);

            if (!IsDrawing || !IsValidDrawingState()) return;

            float distance = CalculateDistance(LastPoint, p);
            if (distance < MIN_INTERP_DISTANCE) return;

            DrawLine(LastPoint, p, Colors.Transparent);
            LastPoint = p;
        }

        public override void OnMouseUp(object sender, MouseButtonEventArgs e, Point p)
        {
            if (ProjectManager.StrokeLayer != null && CurrentStrokeRegion != null)
            {
                ProjectManager.UndoRedoManager.AddHistoryEntry(
                    new LayerHistoryEntry(
                        ProjectManager.SelectedLayer,
                        CurrentStrokeRegion.Value,
                        OldState.Crop(CurrentStrokeRegion.Value))); 
            }

            ProjectManager.StrokeLayer.Clear(Colors.Transparent);
            DrawPreview(p, ProjectManager.CurrentProject.Background);
            ProjectManager.Render(new Rect(0, 0, ProjectManager.CurrentProject.Width, ProjectManager.CurrentProject.Height));
            CurrentStrokeRegion = null;
            OldState = null;

            base.OnMouseUp(sender, e, p);
        }

        public override void DrawPreview(Point p, Color color)
        {
            var h = BrushSize / 2;
            int x1 = (int)(p.X - h);
            int y1 = (int)(p.Y - h);
            int x2 = (int)(p.X + h);
            int y2 = (int)(p.Y + h);
            var region = new Rect(
                x1 - PREVIEW_STROKE_REGION_PADDING,
                y1 - PREVIEW_STROKE_REGION_PADDING,
                x2 - x1 + PREVIEW_STROKE_REGION_PADDING * 2,
                y2 - y1 + PREVIEW_STROKE_REGION_PADDING * 2);

            ProjectManager.StrokeLayer.FillRectangle(
            x1 - PREVIEW_STROKE_REGION_PADDING,
            y1 - PREVIEW_STROKE_REGION_PADDING,
            x2 + PREVIEW_STROKE_REGION_PADDING,
            y2 + PREVIEW_STROKE_REGION_PADDING,
            Colors.Transparent);
            ProjectManager.StrokeLayer.FillRectangle(x1, y1, x2, y2, color);
            ProjectManager.StrokeLayer.DrawRectangle(x1 - 1, y1 - 1, x2 + 1, y2 + 1, Colors.Black);
            ProjectManager.Render(region);
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

                if (CurrentStrokeRegion.HasValue)
                {
                    CurrentStrokeRegion = Rect.Union(CurrentStrokeRegion.Value, region);
                }

                var diagonal = Math.Sqrt(2) * BrushSize / 2;
                ProjectManager.SelectedLayer.Content.FillRectangle(
                    (int)(point.X - diagonal), (int)(point.Y - diagonal), (int)(point.X + diagonal), (int)(point.Y + diagonal), color);


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

                    var diagonal = Math.Sqrt(2) * BrushSize / 2;

                    ProjectManager.SelectedLayer.Content.FillRectangle(
                        (int)(currentPoint.X - diagonal), (int)(currentPoint.Y - diagonal), (int)(currentPoint.X + diagonal), (int)(currentPoint.Y + diagonal), color);
                    
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