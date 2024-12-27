using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MVVMPaintApp.ViewModels;
using MVVMPaintApp.Models.Tools;

namespace MVVMPaintApp.UserControls
{
    public partial class DrawingCanvas : UserControl
    {
        private DrawingCanvasViewModel ViewModel => (DrawingCanvasViewModel)DataContext;
        private bool isPressed;

        public DrawingCanvas()
        {
            InitializeComponent();
        }

        public DrawingCanvas(DrawingCanvasViewModel drawingCanvasViewModel)
        {
            InitializeComponent();
            Focusable = true;
            DataContext = drawingCanvasViewModel;

            MouseDown += (s, e) => Focus();
            Loaded += (s, e) => Focus();
        }
  
        private void DrawingCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _ = ViewModel.HandleMouseWheel(e);
        }

        private void DrawingCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(MainCanvas);
            CaptureMouse();
            isPressed = true;
            ViewModel.UpdateMouseInfo(p, isPressed);

            if (ViewModel.ProjectManager.SelectedLayer.IsVisible && ViewModel.SelectedTool is not ZoomPan)
            {
                ViewModel.HandleMouseDown(sender, e, p);
            }
            else
            {
                ViewModel.HandleMouseDown(sender, e, e.GetPosition(MainCanvasArea));
            }
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point p = e.GetPosition(MainCanvas);
            ReleaseMouseCapture();
            isPressed = false;
            ViewModel.UpdateMouseInfo(p, isPressed);

            if (ViewModel.ProjectManager.SelectedLayer.IsVisible && ViewModel.SelectedTool is not ZoomPan)
            {
                ViewModel.HandleMouseUp(sender, e, p);
                ViewModel.ProjectManager.HasUnsavedChanges = true;
            }
            else
            {
                ViewModel.HandleMouseDown(sender, e, e.GetPosition(MainCanvasArea));
            }
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point p = e.GetPosition(MainCanvas);
            ViewModel.UpdateMouseInfo(p, isPressed);

            if (ViewModel.ProjectManager.SelectedLayer.IsVisible && ViewModel.SelectedTool is not ZoomPan)
            {
                ViewModel.HandleMouseMove(sender, e, p);
            }
            else
            {
                ViewModel.HandleMouseMove(sender, e, e.GetPosition(MainCanvasArea));
            }
        }
    
        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {   
            ViewModel?.SetUserControlSize(e.NewSize.Width, e.NewSize.Height);
        }
    }
}