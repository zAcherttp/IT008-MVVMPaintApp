using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MVVMPaintApp.ViewModels;
using MVVMPaintApp.Models;  

namespace MVVMPaintApp.UserControls
{
    public partial class DrawingCanvas : UserControl
    {
        private Point? lastMousePoint;
        private bool isPressed;

        public DrawingCanvas()
        {
            InitializeComponent();
        }

        public DrawingCanvas(DrawingCanvasViewModel drawingCanvasViewModel)
        {
            InitializeComponent();
            DataContext = drawingCanvasViewModel;
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            Focus();
            var viewModel = (DrawingCanvasViewModel)DataContext;

            if (e.Key == Key.Z)
            {
                viewModel.ToggleZoomModeCommand.Execute(null);
                Cursor = viewModel.IsZoomMode
                    ? Cursors.Cross
                    : Cursors.Arrow;
            }
            else if (e.Key == Key.V)
            {
                viewModel.TogglePanModeCommand.Execute(null);
                Cursor = viewModel.IsPanMode
                    ? Cursors.Hand
                    : Cursors.Arrow;
            }
            else if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                viewModel.HandleCtrlKeyPress(true);
            }
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            var viewModel = (DrawingCanvasViewModel)DataContext;
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                viewModel.HandleCtrlKeyPress(false);
            }
        }
  
        private void UserControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var viewModel = (DrawingCanvasViewModel)DataContext;
            _ = viewModel.HandleMouseWheel(e);
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            var viewModel = (DrawingCanvasViewModel)DataContext;
            Point currentPoint = e.GetPosition(MainCanvasArea);
            isPressed = true;

            CaptureMouse();
            if (viewModel.IsZoomMode || viewModel.IsPanMode)
            {
                lastMousePoint = currentPoint;
            } else if (viewModel.ProjectManager.SelectedLayer.IsVisible)
            {
                viewModel.HandleMouseDown(sender, e, MainCanvas);
            }
            viewModel.UpdateMouseInfo(currentPoint, isPressed);
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (DrawingCanvasViewModel)DataContext;
            isPressed = false;

            if (lastMousePoint.HasValue)
            {
                ReleaseMouseCapture();
                lastMousePoint = null;
            } else if (viewModel.ProjectManager.SelectedLayer.IsVisible)
            {
                viewModel.HandleMouseUp(sender, e, MainCanvas);
                viewModel.ProjectManager.HasUnsavedChanges = true;
            }
            viewModel.UpdateMouseInfo(e.GetPosition(MainCanvasArea), isPressed);
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            var viewModel = (DrawingCanvasViewModel)DataContext;
            Point currentPoint = e.GetPosition(MainCanvasArea);
            viewModel.UpdateMouseInfo(currentPoint, isPressed);
            if ((viewModel.IsZoomMode || viewModel.IsPanMode) && lastMousePoint.HasValue)
            {
                if (viewModel.IsPanMode)
                {
                    viewModel.HandleMousePan(lastMousePoint.Value, currentPoint);
                    lastMousePoint = currentPoint;
                }
            }
            else if (viewModel.ProjectManager.SelectedLayer.IsVisible)
            {
                viewModel.HandleMouseMove(sender, e, MainCanvas);
            }
        }
       
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
        }
    
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var viewModel = (DrawingCanvasViewModel)DataContext;

            if (viewModel != null)
            {
                viewModel.UserControlWidth = e.NewSize.Width;
                viewModel.UserControlHeight = e.NewSize.Height;
            }
        }
    }
}