using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models
{
    public class Selection
    {
        public Rect Bounds { get; set; }
        public Rect RenderBounds { get; private set; }
        public Rect SourceBounds { get; set; }
        public List<Point> ControlPoints { get; private set; }
        public bool IsActive { get; set; }
        private const int HANDLE_SIZE = 8;
        private const int HANDLE_PADDING = 4;

        public Selection()
        {
            ControlPoints = [];
        }

        public void UpdateBounds(Point start, Point end)
        {
            Bounds = new Rect(
                Math.Min(start.X, end.X),
                Math.Min(start.Y, end.Y),
                Math.Abs(end.X - start.X),
                Math.Abs(end.Y - start.Y)
            );
            SourceBounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
            RenderBounds = new Rect(
                Bounds.Left - HANDLE_SIZE,
                Bounds.Top - HANDLE_SIZE,
                Bounds.Width + HANDLE_SIZE * 2,
                Bounds.Height + HANDLE_SIZE * 2
            );
        }

        private void UpdateControlPoints()
        {
            ControlPoints.Clear();
            // Add corners
            ControlPoints.Add(new Point(Bounds.Left, Bounds.Top));
            ControlPoints.Add(new Point(Bounds.Right, Bounds.Top));
            ControlPoints.Add(new Point(Bounds.Right, Bounds.Bottom));
            ControlPoints.Add(new Point(Bounds.Left, Bounds.Bottom));
            // Add midpoints
            ControlPoints.Add(new Point(Bounds.Left + Bounds.Width / 2, Bounds.Top));
            ControlPoints.Add(new Point(Bounds.Right, Bounds.Top + Bounds.Height / 2));
            ControlPoints.Add(new Point(Bounds.Left + Bounds.Width / 2, Bounds.Bottom));
            ControlPoints.Add(new Point(Bounds.Left, Bounds.Top + Bounds.Height / 2));
        }

        public void Draw(WriteableBitmap canvas)
        {
            var color = Color.FromArgb(150, 255, 255, 255);
            UpdateControlPoints();
            canvas.DrawLineDotted((int)Bounds.Left, (int)Bounds.Top, (int)Bounds.Right, (int)Bounds.Top, 5, 5, color);
            canvas.DrawLineDotted((int)Bounds.Right, (int)Bounds.Top, (int)Bounds.Right, (int)Bounds.Bottom, 5, 5, color);
            canvas.DrawLineDotted((int)Bounds.Right, (int)Bounds.Bottom, (int)Bounds.Left, (int)Bounds.Bottom, 5, 5, color);
            canvas.DrawLineDotted((int)Bounds.Left, (int)Bounds.Bottom, (int)Bounds.Left, (int)Bounds.Top, 5, 5, color);
            DrawHandles(canvas);
        }

        private void DrawHandles(WriteableBitmap canvas)
        {
            foreach (var point in ControlPoints)
            {
                canvas.FillRectangle(
                    (int)(point.X - HANDLE_SIZE / 2),
                    (int)(point.Y - HANDLE_SIZE / 2),
                    (int)(point.X + HANDLE_SIZE / 2),
                    (int)(point.Y + HANDLE_SIZE / 2),
                    Colors.White
                );
                canvas.DrawRectangle(
                    (int)(point.X - HANDLE_SIZE / 2),
                    (int)(point.Y - HANDLE_SIZE / 2),
                    (int)(point.X + HANDLE_SIZE / 2),
                    (int)(point.Y + HANDLE_SIZE / 2),
                    Colors.Black
                );
            }
        }

        public int? GetHandleIndex(Point point)
        {
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                if (IsPointNearHandle(point, ControlPoints[i]))
                    return i;
            }
            return null;
        }

        public bool IsPointInsideBounds(Point p)
        { 
            return p.X >= Bounds.Left + HANDLE_SIZE + HANDLE_PADDING && p.X <= Bounds.Right - HANDLE_SIZE - HANDLE_PADDING &&
                   p.Y >= Bounds.Top + HANDLE_SIZE + HANDLE_PADDING && p.Y <= Bounds.Bottom - HANDLE_SIZE - HANDLE_PADDING;
        }

        private static bool IsPointNearHandle(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) <= HANDLE_SIZE + HANDLE_PADDING &&
                   Math.Abs(p1.Y - p2.Y) <= HANDLE_SIZE + HANDLE_PADDING;
        }
    }
}
