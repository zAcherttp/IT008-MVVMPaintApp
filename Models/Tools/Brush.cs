using MVVMPaintApp.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models.Tools
{
    public class Brush(ProjectManager projectManager) : ToolBase(projectManager)
    {
        private const float MIN_DISTANCE = 1f;
        private const float INTERPOLATION_FACTOR = 0.3f;
        private int REGION_PADDING = 2;

        private int brushSize = 20;

        public int BrushSize
        {
            get => brushSize;
            set
            {
                brushSize = value;
                REGION_PADDING = Math.Max(2, value / 10);
            }
        }

        public override void OnMouseDown(object sender, MouseEventArgs e, Point p)
        {
            base.OnMouseDown(sender, e, p);

            if (!IsValidDrawingState()) return;

            CurrentStrokeRegion = new Rect(p, new Size(1,1));

            DrawPoint(p, GetCurrentColor(e));
        }

        public override void OnMouseMove(object sender, MouseEventArgs e, Point p)
        {
            if (!IsDrawing || !IsValidDrawingState()) return;

            float distance = CalculateDistance(LastPoint, p);
            if (distance < MIN_DISTANCE) return;

            //DrawRect(CurrentStrokeRegion.Value, Colors.Red);

            DrawLine(LastPoint, p, GetCurrentColor(e));
            LastPoint = p;
        }

        // Draw CurrentStrokeRegion for debugging
        //public void DrawRect(Rect rect, Color color)
        //{
        //    ProjectManager.SelectedLayer.Content.DrawRectangle(
        //        (int)rect.X,
        //        (int)rect.Y,
        //        (int)rect.Right,
        //        (int)rect.Bottom,
        //        color
        //    );
        //    ProjectManager.InvalidateRegion(rect, ProjectManager.SelectedLayer);
        //}

        public override void OnMouseUp(object sender, MouseEventArgs e, Point imagePoint)
        {
            if(ProjectManager.StrokeLayer != null && CurrentStrokeRegion != null)
            {
                // Add history entry

                BlitStrokeLayer();
            }

            ProjectManager.StrokeLayer.Clear(Colors.Transparent);
            CurrentStrokeRegion = null;

            base.OnMouseUp(sender, e, imagePoint);
        }   

        private void DrawPoint(Point point, Color color)
        {
            try
            {
                var totalPadding = BrushSize + REGION_PADDING;
                var region = new Rect(
                    point.X - totalPadding,
                    point.Y - totalPadding,
                    totalPadding * 2,
                    totalPadding * 2
                );

                ProjectManager.SelectedLayer.Content.FillEllipseCentered(
                    (int)point.X,
                    (int)point.Y,
                    BrushSize,
                    BrushSize,
                    color
                );

                if(CurrentStrokeRegion.HasValue)
                {
                    CurrentStrokeRegion = Rect.Union(CurrentStrokeRegion.Value, region);
                }

                ProjectManager.InvalidateRegion(region, ProjectManager.SelectedLayer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error drawing point: {ex.Message}");
            }
        }

        private void DrawLine(Point start, Point end, Color color)
        {
            try
            {
                float distance = CalculateDistance(start, end);
                int steps = Math.Max(1, (int)(distance / (BrushSize * INTERPOLATION_FACTOR)));

                Point lastDrawnPoint = start;

                for (int i = 0; i <= steps; i++)
                {
                    float t = i / (float)steps;
                    var currentPoint = Lerp(start, end, t);

                    var region = CalculateSegmentRegion(lastDrawnPoint, currentPoint);

                    ProjectManager.SelectedLayer.Content.FillEllipseCentered(
                        (int)currentPoint.X,
                        (int)currentPoint.Y,
                        BrushSize,
                        BrushSize,
                        color
                    );

                    if (CurrentStrokeRegion.HasValue)
                    {
                        CurrentStrokeRegion = Rect.Union(CurrentStrokeRegion.Value, region);
                    }

                    ProjectManager.InvalidateRegion(region, ProjectManager.SelectedLayer);
                    lastDrawnPoint = currentPoint;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error drawing line: {ex.Message}");
            }
        }

        private Rect CalculateSegmentRegion(Point p1, Point p2)
        {
            var totalPadding = BrushSize + REGION_PADDING;
            return new Rect(
                Math.Min(p1.X, p2.X) - totalPadding,
                Math.Min(p1.Y, p2.Y) - totalPadding,
                Math.Abs(p2.X - p1.X) + totalPadding * 2,
                Math.Abs(p2.Y - p1.Y) + totalPadding * 2
            );
        }
    }
}