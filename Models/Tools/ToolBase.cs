using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Services;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace MVVMPaintApp.Models.Tools
{
    public abstract class ToolBase(ProjectManager projectManager) : ITool
    {
        protected ProjectManager ProjectManager { get; set; } = projectManager;
        protected Point LastPoint { get; set; }
        protected Rect? CurrentStrokeRegion { get; set; }
        protected bool IsDrawing { get; set; }

        public virtual void OnMouseDown(object sender, MouseEventArgs e, Point imagePoint)
        {
            IsDrawing = true;
            LastPoint = imagePoint;
        }

        public virtual void OnMouseUp(object sender, MouseEventArgs e, Point imagePoint)
        {
            IsDrawing = false;
        }

        public abstract void OnMouseMove(object sender, MouseEventArgs e, Point imagePoint);

        public bool IsValidDrawingState() =>
            ProjectManager.SelectedLayer?.Content != null;

        public Color GetCurrentColor(MouseEventArgs e) =>
            e.LeftButton == MouseButtonState.Pressed
                ? ProjectManager.PrimaryColor
                : ProjectManager.SecondaryColor;

        public static float CalculateDistance(Point p1, Point p2)
        {
            return (float)Math.Sqrt(
                Math.Pow(p2.X - p1.X, 2) +
                Math.Pow(p2.Y - p1.Y, 2)
            );
        }

        public static Point Lerp(Point start, Point end, float t) => new(
            (int)Math.Round(start.X + (end.X - start.X) * t),
            (int)Math.Round(start.Y + (end.Y - start.Y) * t)
        );

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
