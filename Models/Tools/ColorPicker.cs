using MVVMPaintApp.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models.Tools
{
    public class ColorPicker(ProjectManager projectManager) : ToolBase(projectManager)
    {
        public override void OnMouseDown(object sender, MouseButtonEventArgs e, Point p)
        {
            HitCheck(ref p);
            if (e.LeftButton == MouseButtonState.Pressed)
                ProjectManager.PrimaryColor = ProjectManager.SelectedLayer.Content.GetPixel((int)p.X, (int)p.Y);
            else if (e.RightButton == MouseButtonState.Pressed)
                ProjectManager.SecondaryColor = ProjectManager.SelectedLayer.Content.GetPixel((int)p.X, (int)p.Y);
        }

        public override void OnMouseMove(object sender, MouseEventArgs e, Point imagePoint)
        {
        }
    }
}
