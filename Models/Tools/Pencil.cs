using MVVMPaintApp.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models.Tools
{
    public class Pencil(ProjectManager projectManager) : ToolBase(projectManager)
    {
        public int BrushSize { get; set; } = 1;

        public override void OnMouseMove(object sender, MouseEventArgs e, Point hitCheck)
        {
            if (!IsDrawing || ProjectManager.SelectedLayer == null) return;

            var Color = e.LeftButton == MouseButtonState.Pressed ? ProjectManager.PrimaryColor : ProjectManager.SecondaryColor;

            ProjectManager.SelectedLayer.Content.DrawLine(
                (int)LastPoint.X, (int)LastPoint.Y,
                (int)hitCheck.X, (int)hitCheck.Y,
                Color);

            LastPoint = hitCheck;
            ProjectManager.SelectedLayer.RenderThumbnail();
        }
    }
}
