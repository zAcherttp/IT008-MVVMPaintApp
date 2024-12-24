using MVVMPaintApp.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models.Tools
{
    public class ColorPicker(ProjectManager projectManager) : ToolBase(projectManager)
    {
        public override void OnMouseDown(object sender, MouseEventArgs e, Point imagePoint)
        {
            ProjectManager.PrimaryColor = ProjectManager.SelectedLayer.Content.GetPixel((int)imagePoint.X, (int)imagePoint.Y);
        }

        public override void OnMouseMove(object sender, MouseEventArgs e, Point imagePoint)
        {
        }
    }
}
