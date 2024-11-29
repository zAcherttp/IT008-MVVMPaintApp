using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MVVMPaintApp.ViewModels;

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

        public DrawingCanvas(DrawingCanvasViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
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

            if (viewModel.IsZoomMode || viewModel.IsPanMode)
            {
                lastMousePoint = currentPoint;
                CaptureMouse();
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
            }
            viewModel.UpdateMouseInfo(e.GetPosition(MainCanvasArea), isPressed);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
        }
    }
}