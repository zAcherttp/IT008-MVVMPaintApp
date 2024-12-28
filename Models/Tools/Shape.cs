//using MVVMPaintApp.Services;
//using System.Windows;
//using System.Windows.Input;
//using System.Windows.Media.Imaging;
//using System.Windows.Media;
//using System.Diagnostics;

//namespace MVVMPaintApp.Models.Tools
//{
//    public class Shape(ProjectManager projectManager) : ToolBase(projectManager)
//    {
//        public ShapeType CurrentShapeType { get; set; } = ShapeType.Rectangle;
//        private Point StartPoint;
//        private int StrokeThickness = 5;
//        private List<Point> BezierPoints = [];
//        private Selection SelectionBox = new();
//        private int? SelectedHandleIndex;
//        private bool IsAdjusting;

//        public override void OnMouseDown(object sender, MouseButtonEventArgs e, Point p)
//        {
//            base.OnMouseDown(sender, e, p);
//            if (!IsValidDrawingState()) return;

//            if (IsAdjusting)
//            {
//                SelectedHandleIndex = SelectionBox.GetHandleIndex(p);
//                return;
//            }

//            StartPoint = p;
//            if (CurrentShapeType == ShapeType.Bezier)
//            {
//                BezierPoints.Add(p);
//            }
//            else
//            {
//                CurrentStrokeRegion = new Rect(p, new Size(1, 1));
//                ProjectManager.StrokeLayer.Clear(Colors.Transparent);
//            }
//        }

//        public override void OnMouseMove(object sender, MouseEventArgs e, Point p)
//        {   
//            if (!IsDrawing || !IsValidDrawingState()) return;

//            if (IsAdjusting && SelectedHandleIndex.HasValue)
//            {
//                // Update control point position
//                SelectionBox.ControlPoints[SelectedHandleIndex.Value] = p;
//                RedrawShape();
//                return;
//            }

//            ProjectManager.StrokeLayer.Clear(Colors.Transparent);

//            switch (CurrentShapeType)
//            {
//                case ShapeType.Line:
//                    DrawLineShape(StartPoint, p);
//                    break;
//                case ShapeType.Rectangle:
//                    DrawRectangle(StartPoint, p);
//                    break;
//                case ShapeType.Ellipse:
//                    DrawEllipse(StartPoint, p);
//                    break;
//                case ShapeType.Bezier:
//                    DrawBezierPreview(p);
//                    break;
//            }

//            SelectionBox.UpdateBounds(StartPoint, p);
//            SelectionBox.DrawHandles(ProjectManager.StrokeLayer);

//            ProjectManager.Render(SelectionBox.Bounds);
//        }

//        public override void OnMouseUp(object sender, MouseButtonEventArgs e, Point p)
//        {
//            if (!IsValidDrawingState()) return;

//            if (CurrentShapeType == ShapeType.Bezier)
//            {
//                if (e.ClickCount == 2)
//                {
//                    // Complete bezier curve on double click
//                    CompleteBezier();
//                }
//                return;
//            }

//            if (IsAdjusting)
//            {
//                SelectedHandleIndex = null;
//                return;
//            }

//            BlitStrokeLayer();
//            ProjectManager.StrokeLayer.Clear(Colors.Transparent);
//            IsAdjusting = true;

//            base.OnMouseUp(sender, e, p);
//        }

//        private void RedrawShape()
//        {
//            ProjectManager.StrokeLayer.Clear(Colors.Transparent);

//            // Redraw shape based on current selection box bounds
//            switch (CurrentShapeType)
//            {
//                case ShapeType.Line:
//                    DrawLineShape(SelectionBox.ControlPoints[0], SelectionBox.ControlPoints[1]);
//                    break;
//                case ShapeType.Rectangle:
//                    ProjectManager.StrokeLayer.DrawRectangle(
//                        (int)SelectionBox.Bounds.Left,
//                        (int)SelectionBox.Bounds.Top,
//                        (int)SelectionBox.Bounds.Right,
//                        (int)SelectionBox.Bounds.Bottom,
//                        ProjectManager.PrimaryColor
//                    );
//                    break;
//                    // Add other shape redraw logic here
//            }

//            SelectionBox.DrawHandles(ProjectManager.StrokeLayer);
//            ProjectManager.Render(SelectionBox.Bounds);
//        }

//        public void DrawLineShape(Point start, Point end)
//        {
//            try
//            {
//                float distance = CalculateDistance(start, end);
//                int steps = Math.Max(1, (int)(distance / (StrokeThickness * INTERP_FACTOR)));

//                for (int i = 0; i <= steps; i++)
//                {
//                    float t = i / (float)steps;
//                    var currentPoint = Lerp(start, end, t);

//                    ProjectManager.StrokeLayer.FillEllipseCentered(
//                        (int)currentPoint.X,
//                        (int)currentPoint.Y,
//                        StrokeThickness / 2,
//                        StrokeThickness / 2,
//                        ProjectManager.PrimaryColor
//                    );
//                }
//            }
//            catch (Exception ex)
//            {
//                Debug.WriteLine($"Error drawing line: {ex.Message}");
//            }
//        }

//        private void DrawRectangle(Point start, Point end)
//        {
//            var rect = new Rect(start, end);
//            CurrentStrokeRegion = rect;
//            ProjectManager.StrokeLayer.DrawRectangle(
//                (int)rect.Left,
//                (int)rect.Top,
//                (int)rect.Right,
//                (int)rect.Bottom,
//                ProjectManager.PrimaryColor
//            );
//        }

//        private void DrawEllipse(Point start, Point end)
//        {
//            var rect = new Rect(start, end);
//            CurrentStrokeRegion = rect;
//            ProjectManager.StrokeLayer.DrawEllipse(
//                (int)rect.Left,
//                (int)rect.Top,
//                (int)rect.Right,
//                (int)rect.Bottom,
//                ProjectManager.PrimaryColor
//            );
//        }

//        private void DrawBezierPreview(Point currentPoint)
//        {
//            if (BezierPoints.Count < 2) return;

//            var points = new List<Point>(BezierPoints)
//            {
//                currentPoint
//            };

//            // Draw bezier curve through points
//            for (float t = 0; t <= 1.0f; t += 0.01f)
//            {
//                Point p = CalculateBezierPoint(t, points);
//                ProjectManager.StrokeLayer.FillEllipseCentered((int)p.X, (int)p.Y, StrokeThickness, StrokeThickness, ProjectManager.PrimaryColor);
//            }
//        }

//        private static Point CalculateBezierPoint(float t, List<Point> points)
//        {
//            if (points.Count == 1) return points[0];

//            var newPoints = new List<Point>();
//            for (int i = 0; i < points.Count - 1; i++)
//            {
//                newPoints.Add(new Point(
//                    points[i].X + (points[i + 1].X - points[i].X) * t,
//                    points[i].Y + (points[i + 1].Y - points[i].Y) * t
//                ));
//            }

//            return CalculateBezierPoint(t, newPoints);
//        }

//        private void CompleteBezier()
//        {
//            // Draw final bezier curve
//            DrawBezierPreview(BezierPoints[^1]);
//            BlitStrokeLayer();

//            // Reset for next curve
//            BezierPoints.Clear();
//            ProjectManager.StrokeLayer.Clear(Colors.Transparent);
//            IsDrawing = false;
//        }
//    }

//    public enum ShapeType
//    {
//        //lines
//        Line,
//        Bezier,
//        //shapes
//        Ellipse,
//        Rectangle,
//        RoundedRectangle,
//        Polygon,
//        Triangle,
//        RightTriangle,
//        Diamond,
//        Pentagon,
//        Hexagon,
//        RightArrow,
//        LeftArrow,
//        UpArrow,
//        DownArrow,
//        FourPointStar,
//        FivePointStar,
//        SixPointStar,
//        RectangularCallout,
//        OvalCallout,
//        Heart,
//        Lightning,
//        UIT,
//    }

//}
