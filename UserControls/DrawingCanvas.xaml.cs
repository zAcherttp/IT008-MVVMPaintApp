using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MVVMPaintApp.ViewModels;

namespace MVVMPaintApp.UserControls
{
    public partial class DrawingCanvas : UserControl
    {
        private DrawingCanvasViewModel ViewModel => (DrawingCanvasViewModel)DataContext;
        private Point? lastMousePoint;
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
            Point currentPoint = e.GetPosition(MainCanvasArea);
            isPressed = true;

            CaptureMouse();
            if (ViewModel.IsZoomMode || ViewModel.IsPanMode)
            {
                lastMousePoint = currentPoint;
            } else if (ViewModel.ProjectManager.SelectedLayer.IsVisible)
            {
                ViewModel.HandleMouseDown(sender, e, MainCanvas);
            }
            ViewModel.UpdateMouseInfo(currentPoint, isPressed);
        }

        private void DrawingCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isPressed = false;

            if (lastMousePoint.HasValue)
            {
                ReleaseMouseCapture();
                lastMousePoint = null;
            } else if (ViewModel.ProjectManager.SelectedLayer.IsVisible)
            {
                ViewModel.HandleMouseUp(sender, e, MainCanvas);
                ViewModel.ProjectManager.HasUnsavedChanges = true;
            }
            ViewModel.UpdateMouseInfo(e.GetPosition(MainCanvasArea), isPressed);
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point currentPoint = e.GetPosition(MainCanvas);
            ViewModel.UpdateMouseInfo(currentPoint, isPressed);
            if ((ViewModel.IsZoomMode || ViewModel.IsPanMode) && lastMousePoint.HasValue)
            {
                if (ViewModel.IsPanMode)
                {
                    currentPoint = e.GetPosition(MainCanvasArea);
                    ViewModel.HandleMousePan(lastMousePoint.Value, currentPoint);
                    lastMousePoint = currentPoint;
                }
            }
            else if (ViewModel.ProjectManager.SelectedLayer.IsVisible)
            {
                ViewModel.HandleMouseMove(sender, e, MainCanvas);
            }
        }
    
        private void DrawingCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {   
            if (ViewModel != null)
            {
                ViewModel.UserControlWidth = e.NewSize.Width;
                ViewModel.UserControlHeight = e.NewSize.Height;
            }
        }
    }
}