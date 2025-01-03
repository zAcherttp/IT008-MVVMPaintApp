using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Services;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MVVMPaintApp.Models.Tools
{
    public abstract class ToolBase : ITool
    {
        public const float MIN_INTERP_DISTANCE = 1f;
        public const float INTERP_FACTOR = 0.3f;
        public const int STROKE_REGION_PADDING = 2;

        protected ProjectManager ProjectManager { get; set; }
        protected Point LastPoint { get; set; }
        protected Rect? CurrentStrokeRegion { get; set; }
        protected WriteableBitmap? OldState { get; set; }
        protected bool IsDrawing { get; set; }

        public ToolBase(ProjectManager projectManager)
        {
            ProjectManager = projectManager;
            LastPoint = ProjectManager.CursorPositionOnCanvas;
        }

        public virtual void OnMouseDown(object sender, MouseButtonEventArgs e, Point p)
        {
            IsDrawing = true;
            LastPoint = p;
        }

        public virtual void OnMouseUp(object sender, MouseButtonEventArgs e, Point p)
        {
            IsDrawing = false;
            ProjectManager.SelectedLayer.RenderThumbnail();
        }

        public abstract void OnMouseMove(object sender, MouseEventArgs e, Point p);

        public virtual Cursor GetCursor()
        {
            return Cursors.Cross;
        }

        public virtual void DrawPreview(Point p, Color color)
        {
        }

        public virtual void DrawPoint(Point p, Color color)
        {
        }

        public virtual void DrawLine(Point p1, Point p2, Color color)
        {
        }

        public bool IsValidDrawingState() =>
            ProjectManager.SelectedLayer?.Content != null;

        public Color GetCurrentColor(MouseEventArgs e)
        {
            bool isPrimary = ProjectManager.IsPrimaryColorSelected;
            if (e.RightButton == MouseButtonState.Pressed)
            {
                return isPrimary ? ProjectManager.SecondaryColor : ProjectManager.PrimaryColor;
            } else
            {
                return isPrimary ? ProjectManager.PrimaryColor : ProjectManager.SecondaryColor;
            }
        }

        public static bool IsColorSimilar(Color c1, Color c2, int tolerance = 0)
        {
            return Math.Abs(c1.R - c2.R) <= tolerance &&
                   Math.Abs(c1.G - c2.G) <= tolerance &&
                   Math.Abs(c1.B - c2.B) <= tolerance &&
                   Math.Abs(c1.A - c2.A) <= tolerance;
        }

        public static bool IsPixelSimilar(int pixel1, int pixel2, int tolerance)
        {
            byte b1 = (byte)pixel1;
            byte g1 = (byte)(pixel1 >> 8);
            byte r1 = (byte)(pixel1 >> 16);
            byte a1 = (byte)(pixel1 >> 24);

            byte b2 = (byte)pixel2;
            byte g2 = (byte)(pixel2 >> 8);
            byte r2 = (byte)(pixel2 >> 16);
            byte a2 = (byte)(pixel2 >> 24);

            return Math.Abs(r1 - r2) <= tolerance &&
                   Math.Abs(g1 - g2) <= tolerance &&
                   Math.Abs(b1 - b2) <= tolerance &&
                   Math.Abs(a1 - a2) <= tolerance;
        }

        public static float CalculateDistance(Point p1, Point p2)
        {
            return (float)Math.Sqrt(
                Math.Pow(p2.X - p1.X, 2) +
                Math.Pow(p2.Y - p1.Y, 2)
            );
        }

        public static Rect CalculateSegmentRegion(Point p1, Point p2, int brushSize)
        {
            var totalPadding = brushSize + STROKE_REGION_PADDING;
            return new Rect(
                Math.Min(p1.X, p2.X) - totalPadding,
                Math.Min(p1.Y, p2.Y) - totalPadding,
                Math.Abs(p2.X - p1.X) + totalPadding * 2,
                Math.Abs(p2.Y - p1.Y) + totalPadding * 2
            );
        }

        public static Point Lerp(Point start, Point end, float t) => new(
            (int)Math.Round(start.X + (end.X - start.X) * t),
            (int)Math.Round(start.Y + (end.Y - start.Y) * t)
        );

        public void HitCheck(ref Point point)
        {
            if (point.X <= 0) point.X = 0;
            if (point.Y <= 0) point.Y = 0;
            if (point.X >= ProjectManager.CurrentProject.Width) point.X = ProjectManager.CurrentProject.Width - 1;
            if (point.Y >= ProjectManager.CurrentProject.Height) point.Y = ProjectManager.CurrentProject.Height - 1;
        }

        public void BlitStrokeLayer()
        {
            if (CurrentStrokeRegion == null) return;
            ProjectManager.SelectedLayer.Content.Blit((Rect)CurrentStrokeRegion,
                ProjectManager.StrokeLayer,
                (Rect)CurrentStrokeRegion,
                WriteableBitmapExtensions.BlendMode.Alpha
            );
        }
    }
}
