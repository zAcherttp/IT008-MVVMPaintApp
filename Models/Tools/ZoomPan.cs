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
        private const double ZOOM_STEP_PERCENTAGE = 0.1;

        public override void OnMouseDown(object sender, MouseEventArgs e, Point p)
        {
            IsDrawing = e.RightButton == MouseButtonState.Pressed;
            LastPoint = p;
        }

        public override void OnMouseUp(object sender, MouseEventArgs e, Point p)
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
            double delta = e.Delta / 120.0;
            double newZoomFactor = Math.Clamp(
                ProjectManager.ZoomFactor.Value + (delta * ZOOM_STEP_PERCENTAGE),
                0.1,
                10
            );

            await ProjectManager.ZoomFactor.EaseToAsync(
                newZoomFactor,
                Easing.EasingType.EaseInOutCubic,
                30
            );
        }
    }
}
