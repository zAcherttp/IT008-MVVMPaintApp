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
        void OnMouseDown(object o, Point imagePoint);
        void OnMouseUp(object o, Point imagePoint);
        void OnMouseMove(object o, Point imagePoint);
    }
}
