using MVVMPaintApp.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models.Tools
{
    public class Brush(ProjectManager projectManager) : ToolBase(projectManager)
    {
        public int BrushSize { get; set; } = 20;
        public Color Color { get; set; } = Colors.Black;

        private const float MinDistance = 1f;

        public override void OnMouseDown(object sender, MouseEventArgs e, Point imagePoint)
        {
            IsDrawing = true;
            LastPoint = imagePoint;
            var Color = e.LeftButton == MouseButtonState.Pressed ? ProjectManager.PrimaryColor : ProjectManager.SecondaryColor;
            ProjectManager.SelectedLayer.Content.FillEllipseCentered((int)imagePoint.X, (int)imagePoint.Y, BrushSize, BrushSize, Color);
        }

        public override void OnMouseMove(object sender, MouseEventArgs e, Point hitCheck)
        {
            if (!IsDrawing || ProjectManager.SelectedLayer == null)
                return;

            float distance = (float)Math.Sqrt(
                Math.Pow(hitCheck.X - LastPoint.X, 2) +
                Math.Pow(hitCheck.Y - LastPoint.Y, 2)
            );

            if (distance < MinDistance)
                return;

            var Color = e.LeftButton == MouseButtonState.Pressed ? ProjectManager.PrimaryColor : ProjectManager.SecondaryColor;

            int steps = Math.Max(1, (int)(distance / (BrushSize * 0.3)));

                for (int i = 0; i <= steps; i++)
                {
                    float t = i / (float)steps;
                    int x = (int)Math.Round(LastPoint.X + (hitCheck.X - LastPoint.X) * t);
                    int y = (int)Math.Round(LastPoint.Y + (hitCheck.Y - LastPoint.Y) * t);

                    ProjectManager.SelectedLayer.Content.FillEllipseCentered(x, y, BrushSize, BrushSize, Color);
            }

            LastPoint = hitCheck;
            ProjectManager.SelectedLayer.RenderThumbnail();
        }
    }
}
