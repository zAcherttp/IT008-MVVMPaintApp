using MVVMPaintApp.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models.Tools
{
    public class Eraser(ProjectManager projectManager) : ToolBase(projectManager)
    {
        private int BrushSize { get; set; } = 30;
        private const float MinDistance = 1f;

        public override void OnMouseDown(object sender, MouseEventArgs e, Point imagePoint)
        {
            IsDrawing = true;
            LastPoint = imagePoint;
            ProjectManager.SelectedLayer.Content.FillRectangle((int)imagePoint.X - BrushSize / 2, (int)imagePoint.Y - BrushSize / 2, (int)imagePoint.X + BrushSize / 2, (int)imagePoint.Y + BrushSize / 2, Colors.Transparent);
            ProjectManager.SelectedLayer.RenderThumbnail();
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

            int steps = Math.Max(1, (int)(distance / (BrushSize * 0.3)));

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                int x = (int)Math.Round(LastPoint.X + (hitCheck.X - LastPoint.X) * t);
                int y = (int)Math.Round(LastPoint.Y + (hitCheck.Y - LastPoint.Y) * t);

                ProjectManager.SelectedLayer.Content.FillRectangle(x - BrushSize / 2, y - BrushSize / 2, x + BrushSize / 2, y + BrushSize / 2, Colors.Transparent);
            }

            LastPoint = hitCheck;
            ProjectManager.SelectedLayer.RenderThumbnail();
        }
    }
}
