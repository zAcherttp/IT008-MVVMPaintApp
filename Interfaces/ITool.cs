using MVVMPaintApp.Services;
using MVVMPaintApp.ViewModels;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Interfaces
{
    public interface ITool
    {
        public ProjectManager ProjectManager { get; set; }
        void OnMouseDown(object o, MouseEventArgs e, Point imagePoint);
        void OnMouseUp(object o, MouseEventArgs e, Point imagePoint);
        void OnMouseMove(object o, MouseEventArgs e, Point imagePoint);
    }
}
