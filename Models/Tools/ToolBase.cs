using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Services;
using MVVMPaintApp.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;


namespace MVVMPaintApp.Models.Tools
{
    public abstract class ToolBase(ProjectManager projectManager) : ITool
    {
        public ProjectManager ProjectManager { get; set; } = projectManager;
        protected bool IsDrawing { get; set; }
        protected Point LastPoint { get; set; }

        public virtual void OnMouseDown(object sender, Point imagePoint)
        {
            IsDrawing = true;
            LastPoint = imagePoint;
        }

        public virtual void OnMouseUp(object sender, Point imagePoint)
        {
            IsDrawing = false;
        }

        public abstract void OnMouseMove(object sender, Point imagePoint);
    }
}
