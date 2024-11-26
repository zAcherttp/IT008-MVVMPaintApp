using System.Windows.Input;

namespace MVVMPaintApp.Interfaces
{
    public interface ITool
    {
        void OnMouseDown(object o, MouseEventArgs e);
        void OnMouseUp(object o, MouseEventArgs e);
        void OnMouseMove(object o, MouseEventArgs e);
    }
}
