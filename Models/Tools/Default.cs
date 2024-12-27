using CommunityToolkit.Mvvm.ComponentModel;
using MVVMPaintApp.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models.Tools
{
    public class Default(ProjectManager projectManager) : ToolBase(projectManager)
    {
        private readonly Window window = Application.Current.MainWindow;

        public override void OnMouseMove(object sender, MouseEventArgs e, Point imagePoint)
        {
            if (window != null)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    window.DragMove();
                }
            }
        }

        public override void OnMouseDown(object sender, MouseEventArgs e, Point imagePoint)
        {
            if (window != null && e.LeftButton == MouseButtonState.Pressed)
            {
                window.DragMove();
            }
        }

        public override void OnMouseUp(object sender, MouseEventArgs e, Point imagePoint)
        {
        }

        public override Cursor GetCursor()
        {
            return Cursors.Arrow;
        }
    }
}