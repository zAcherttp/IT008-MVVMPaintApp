using MVVMPaintApp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MVVMPaintApp.Models.Tools
{
    public class ZoomPan(ProjectManager projectManager) : ToolBase(projectManager)
    {
        public override void OnMouseDown(object sender, MouseButtonEventArgs e, Point p)
        {
            IsDrawing = e.RightButton == MouseButtonState.Pressed;
            LastPoint = p;
        }

        public override void OnMouseUp(object sender, MouseButtonEventArgs e, Point p)
        {
            IsDrawing = false;
        }

        public override void OnMouseMove(object sender, MouseEventArgs e, Point p)
        {
            if (IsDrawing)
            {
                ProjectManager.PanOffsetX.Value += p.X - LastPoint.X;
                ProjectManager.PanOffsetY.Value += p.Y - LastPoint.Y;
            }
            LastPoint = p;
        }

        public override Cursor GetCursor()
        {
            return Cursors.SizeAll;
        }

        public async Task HandleMouseWheel(MouseWheelEventArgs e)
        {
            double zoomChange = e.Delta > 0 ? 1.0 : -1.0;
            if(ProjectManager.ZoomFactor.Value < 2.0)
            {
                zoomChange *= 0.1;
            }
            else
            {
                zoomChange *= 0.25;
            }
            double newZoomFactor = Math.Clamp(ProjectManager.ZoomFactor.Value + zoomChange, 0.1, 8.0);
            await ProjectManager.ZoomFactor.EaseToAsync(newZoomFactor, Easing.EasingType.EaseInOutCubic, 30);
        }
    }
}
