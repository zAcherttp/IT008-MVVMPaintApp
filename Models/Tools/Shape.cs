using MVVMPaintApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace MVVMPaintApp.Models.Tools
{
    public class Shape(ProjectManager projectManager) : ToolBase(projectManager)
    {
        public enum ShapeState
        {
            None,
            Creating,
            Editing
        }

        public enum ShapeType
        {
            // to be implemented
            //lines
            //Line,
            //Bezier,
            //shapes
            Ellipse,
            Rectangle,
            //RoundedRectangle,
            //Polygon,
            Triangle,
            //RightTriangle,
            //Diamond,
            //Pentagon,
            //Hexagon,
            //RightArrow,
            //LeftArrow,
            //UpArrow,
            //DownArrow,
            //FourPointStar,
            //FivePointStar,
            //SixPointStar,
            //RectangularCallout,
            //OvalCallout,
            //Heart,
            //Lightning,
            //UIT,
        }

        public Selection ShapeBounds { get; set; } = new Selection();
        public ShapeState CurrentState { get; private set; } = ShapeState.None;
        public ShapeType CurrentShapeType { get; set; } = ShapeType.Rectangle;
        public bool IsFilled { get; set; } = false;
        public Point Start { get; set; }
        public WriteableBitmap? ShapeContent { get; set; }
        private WriteableBitmap? ShapeContentBackup;
        private bool IsMovingShape;
        private bool IsResizingShape;
        private int? ActiveHandleIndex;
        private Rect? PreviousRenderBounds;
        private Color ShapeColor = Colors.Black;

        public override void OnMouseDown(object sender, MouseButtonEventArgs e, Point p)
        {
            base.OnMouseDown(sender, e, p);
            ShapeColor = GetCurrentColor(e);
            if (CurrentState == ShapeState.Editing)
            {
                // Check for handle interaction first
                ActiveHandleIndex = ShapeBounds.GetHandleIndex(p);
                if (ActiveHandleIndex.HasValue)
                {
                    ShapeContentBackup = ShapeContent?.Clone();
                    IsResizingShape = true;
                    Start = p;
                    PreviousRenderBounds = ShapeBounds.RenderBounds;
                    return;
                }

                // Then check for movement
                if (ShapeBounds.IsPointInsideBounds(p))
                {
                    IsMovingShape = true;
                    Start = p;
                    PreviousRenderBounds = ShapeBounds.RenderBounds;
                    return;
                }
                else
                {
                    CommitShape();
                }
            }

            ProjectManager.HasUnsavedChanges = true;
            HitCheck(ref p);
            Start = p;
            CurrentState = ShapeState.Creating;
            ShapeBounds.UpdateBounds(Start, Start);
        }

        public override void OnMouseMove(object sender, MouseEventArgs e, Point p)
        {
            if (CurrentState == ShapeState.Editing)
            {
                if (IsResizingShape && ActiveHandleIndex.HasValue)
                {
                    ResizeShape(p);
                    return;
                }
                if (IsMovingShape)
                {
                    MoveShape(p);
                    return;
                }
                return;
            }

            if (!IsDrawing || !IsValidDrawingState()) return;

            ShapeBounds.UpdateBounds(Start, p);
            DrawShapePreview(p);
        }

        public override void OnMouseUp(object sender, MouseButtonEventArgs e, Point p)
        {
            if (IsResizingShape)
            {
                IsResizingShape = false;
                ActiveHandleIndex = null;
                PreviousRenderBounds = null;
                return;
            }

            if (IsMovingShape)
            {
                IsMovingShape = false;
                PreviousRenderBounds = null;
                return;
            }

            if (CurrentState == ShapeState.Creating)
            {
                ShapeBounds.UpdateBounds(Start, p);

                if (ShapeBounds.Bounds.Width > 10 && ShapeBounds.Bounds.Height > 10)
                {
                    CreateNewShape();
                    CurrentState = ShapeState.Editing;
                }
                else
                {
                    CurrentState = ShapeState.None;
                    ProjectManager.StrokeLayer.Clear(Colors.Transparent);
                    ProjectManager.Render(ShapeBounds.RenderBounds);
                }
            }

            base.OnMouseUp(sender, e, p);
        }

        private void ResizeShape(Point p)
        {
            ProjectManager.StrokeLayer.Clear(Colors.Transparent);

            Point start = ShapeBounds.Bounds.TopLeft;
            Point end = ShapeBounds.Bounds.BottomRight;

            // Update appropriate corner based on handle index with bounds checking
            switch (ActiveHandleIndex)
            {
                case 0: // Top-left
                    p.X = Math.Min(p.X, end.X - 30);
                    p.Y = Math.Min(p.Y, end.Y - 30);
                    start = p;
                    break;
                case 1: // Top-right
                    p.X = Math.Max(p.X, start.X + 30);
                    p.Y = Math.Min(p.Y, end.Y - 30);
                    start = new Point(start.X, p.Y);
                    end = new Point(p.X, end.Y);
                    break;
                case 2: // Bottom-right
                    p.X = Math.Max(p.X, start.X + 30);
                    p.Y = Math.Max(p.Y, start.Y + 30);
                    end = p;
                    break;
                case 3: // Bottom-left
                    p.X = Math.Min(p.X, end.X - 30);
                    p.Y = Math.Max(p.Y, start.Y + 30);
                    start = new Point(p.X, start.Y);
                    end = new Point(end.X, p.Y);
                    break;
                case 4: // Top-middle
                    p.Y = Math.Min(p.Y, end.Y - 30);
                    start = new Point(start.X, p.Y);
                    break;
                case 5: // Middle-right
                    p.X = Math.Max(p.X, start.X + 30);
                    end = new Point(p.X, end.Y);
                    break;
                case 6: // Bottom-middle
                    p.Y = Math.Max(p.Y, start.Y + 30);
                    end = new Point(end.X, p.Y);
                    break;
                case 7: // Middle-left
                    p.X = Math.Min(p.X, end.X - 30);
                    start = new Point(p.X, start.Y);
                    break;
            }

            var newBounds = new Rect(start, end);
            if (newBounds.Width < 10 || newBounds.Height < 10) return;

            // Create new bitmap for resized shape
            var newContent = new WriteableBitmap(
                (int)newBounds.Width,
                (int)newBounds.Height,
                96, 96,
                PixelFormats.Bgra32,
                null
            );

            // Update shape bounds
            ShapeBounds.UpdateBounds(newBounds.TopLeft, newBounds.BottomRight);
            ShapeContent = newContent;

            // Calculate combined render region for clean updates
            var renderRegion = PreviousRenderBounds ?? ShapeBounds.RenderBounds;
            renderRegion = Rect.Union(renderRegion, ShapeBounds.RenderBounds);
            PreviousRenderBounds = ShapeBounds.RenderBounds;

            // Draw resized shape
            DrawShape(ShapeContent, ShapeBounds.Bounds);
            ProjectManager.StrokeLayer.Blit(
                ShapeBounds.Bounds,
                ShapeContent,
                new Rect(0, 0, ShapeBounds.Bounds.Width, ShapeBounds.Bounds.Height),
                WriteableBitmapExtensions.BlendMode.Alpha
            );

            ShapeBounds.Draw(ProjectManager.StrokeLayer);
            ProjectManager.Render(renderRegion);
        }

        private void MoveShape(Point p)
        {
            ProjectManager.StrokeLayer.Clear(Colors.Transparent);

            Vector offset = p - Start;
            var newBounds = new Rect(
                ShapeBounds.Bounds.X + offset.X,
                ShapeBounds.Bounds.Y + offset.Y,
                ShapeBounds.Bounds.Width,
                ShapeBounds.Bounds.Height
            );

            ShapeBounds.UpdateBounds(
                new Point(newBounds.X, newBounds.Y),
                new Point(newBounds.X + newBounds.Width, newBounds.Y + newBounds.Height)
            );

            Start = p;

            // Calculate combined render region for clean updates
            var renderRegion = PreviousRenderBounds ?? ShapeBounds.RenderBounds;
            renderRegion = Rect.Union(renderRegion, ShapeBounds.RenderBounds);
            PreviousRenderBounds = ShapeBounds.RenderBounds;

            // Draw shape at new position
            ProjectManager.StrokeLayer.Blit(
                ShapeBounds.Bounds,
                ShapeContent,
                new Rect(0, 0, ShapeBounds.Bounds.Width, ShapeBounds.Bounds.Height),
                WriteableBitmapExtensions.BlendMode.Alpha
            );

            ShapeBounds.Draw(ProjectManager.StrokeLayer);
            ProjectManager.Render(renderRegion);
        }

        private void CreateNewShape()
        {
            var shapeBitmap = new WriteableBitmap(
                (int)ShapeBounds.Bounds.Width,
                (int)ShapeBounds.Bounds.Height,
                96, 96,
                PixelFormats.Bgra32,
                null
            );

            shapeBitmap.Clear(Colors.Transparent);
            DrawShape(shapeBitmap, new Rect(0, 0, ShapeBounds.Bounds.Width, ShapeBounds.Bounds.Height));

            ShapeContent = shapeBitmap;

            ProjectManager.StrokeLayer.Blit(
                ShapeBounds.Bounds,
                ShapeContent,
                new Rect(0, 0, ShapeBounds.Bounds.Width, ShapeBounds.Bounds.Height),
                WriteableBitmapExtensions.BlendMode.Alpha
            );

            ShapeBounds.Draw(ProjectManager.StrokeLayer);
            ProjectManager.Render(ShapeBounds.RenderBounds);
        }

        private void CommitShape()
        {
            if (ShapeContent != null)
            {
                // Store the current bounds before clearing shape
                var commitBounds = ShapeBounds.RenderBounds;

                // Blit the shape content to the layer
                ProjectManager.SelectedLayer.Content.Blit(
                    ShapeBounds.Bounds,
                    ShapeContent,
                    new Rect(0, 0, ShapeBounds.Bounds.Width, ShapeBounds.Bounds.Height),
                    WriteableBitmapExtensions.BlendMode.Alpha
                );

                // Clear shape state
                ProjectManager.StrokeLayer.Clear(Colors.Transparent);
                ShapeContent = null;
                CurrentState = ShapeState.None;

                // Render the affected area
                ProjectManager.Render(commitBounds);
            }
        }

        private void DrawShapePreview(Point p)
        {
            if (IsValidDrawingState())
            {
                ProjectManager.StrokeLayer.Clear(Colors.Transparent);
                HitCheck(ref p);

                var bounds = new Rect(Start, p);
                DrawShape(ProjectManager.StrokeLayer, bounds);
                ProjectManager.Render(ShapeBounds.RenderBounds);
            }
        }

        private void DrawShape(WriteableBitmap target, Rect bounds)
        {
            switch (CurrentShapeType)
            {
                case ShapeType.Rectangle:
                    if (IsFilled)
                    {
                        target.FillRectangle(
                            (int)bounds.X,
                            (int)bounds.Y,
                            (int)(bounds.X + bounds.Width),
                            (int)(bounds.Y + bounds.Height),
                            ShapeColor
                        );
                    }
                    else
                    {
                        target.DrawRectangle(
                            (int)bounds.X,
                            (int)bounds.Y,
                            (int)(bounds.X + bounds.Width),
                            (int)(bounds.Y + bounds.Height),
                            ShapeColor
                        );
                    }
                    break;

                case ShapeType.Ellipse:
                    if (IsFilled)
                    {
                        target.FillEllipse(
                            (int)bounds.X,
                            (int)bounds.Y,
                            (int)(bounds.X + bounds.Width),
                            (int)(bounds.Y + bounds.Height),
                            ShapeColor
                        );
                    }
                    else
                    {
                        target.DrawEllipse(
                            (int)bounds.X,
                            (int)bounds.Y,
                            (int)(bounds.X + bounds.Width),
                            (int)(bounds.Y + bounds.Height),
                            ShapeColor
                        );
                    }
                    break;

                case ShapeType.Triangle:
                    Point[] points =
                    [
                        new Point(bounds.X + bounds.Width / 2, bounds.Y),
                        new Point(bounds.X, bounds.Y + bounds.Height),
                        new Point(bounds.X + bounds.Width, bounds.Y + bounds.Height)
                    ];

                    int[] ints = new int[points.Length * 2];
                    for (int i = 0; i < points.Length; i++)
                    {
                        ints[i * 2] = (int)points[i].X;
                        ints[i * 2 + 1] = (int)points[i].Y;
                    }

                    if (IsFilled)
                    {
                        target.FillPolygon(ints, ShapeColor);
                    }
                    else
                    {
                        target.DrawPolyline(ints, ShapeColor);
                        target.DrawLine(
                            (int)points[2].X,
                            (int)points[2].Y,
                            (int)points[0].X,
                            (int)points[0].Y,
                            ShapeColor
                        );
                    }
                    break;
            }
        }

        public void SetShapeColor(Color color)
        {
            ShapeColor = color;
        }

        public void SetFilled(bool filled)
        {
            IsFilled = filled;
        }

        public void CancelShape()
        {
            if (CurrentState != ShapeState.None)
            {
                ProjectManager.StrokeLayer.Clear(Colors.Transparent);
                ShapeContent = null;
                CurrentState = ShapeState.None;
                ProjectManager.Render(ShapeBounds.RenderBounds);
            }
        }

        public static List<ShapeType> ShapeTypes
        {
            get
            {
                return [.. Enum.GetValues<ShapeType>().Cast<ShapeType>()];
            }
        }
    }
}