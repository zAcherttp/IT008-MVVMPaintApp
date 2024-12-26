using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Models;
using MVVMPaintApp.Models.Tools;
using MVVMPaintApp.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace MVVMPaintApp.ViewModels
{
    public partial class DrawingCanvasViewModel : ObservableObject
    {
        private const double ZOOM_STEP_PERCENTAGE = 0.1;

        [ObservableProperty]
        private ProjectManager projectManager;

        [ObservableProperty]
        private Project currentProject;

        [ObservableProperty]
        private ToolBase selectedTool;

        [ObservableProperty]
        private double userControlWidth = 1600;

        [ObservableProperty]
        private double userControlHeight = 700;

        // Canvas properties
        [ObservableProperty]
        private Easing panOffsetX = new(0.0);

        [ObservableProperty]
        private Easing panOffsetY = new (0.0);

        [ObservableProperty]
        private Easing zoomFactor = new(1.0);

        // Mode properties
        // to be changed to Tools class using enum tool types
        [ObservableProperty]
        private bool isZoomMode;

        [ObservableProperty]
        private bool isPanMode;

        [ObservableProperty]
        private bool isCtrlPressed;


        // Debugging properties
        [ObservableProperty]
        private string modeText = "";

        [ObservableProperty]
        private string mouseInfo = "0, 0";

        public DrawingCanvasViewModel(ProjectManager projectManager)
        {
            ProjectManager = projectManager;
            CurrentProject = projectManager.CurrentProject;
            SelectedTool = new Pencil(projectManager);
            ProjectManager.Render();
        }

        public void SetProjectManager(ProjectManager projectManager)
        {
            ProjectManager = projectManager;
            CurrentProject = projectManager.CurrentProject;
            SelectedTool = new Pencil(projectManager);
            ProjectManager.Render();
        }

        public void SetTool(ToolType tool)
        {
            SelectedTool = tool switch
            {
                ToolType.Pencil => new Pencil(ProjectManager),
                ToolType.Brush => new Brush(ProjectManager),
                ToolType.Eraser => new Eraser(ProjectManager),
                ToolType.Fill => new Fill(ProjectManager),
                ToolType.ColorPicker => new ColorPicker(ProjectManager),

                _ => new Pencil(ProjectManager),
            };
            Debug.WriteLine("Selected tool: " + SelectedTool.GetType().Name + " - Layer: " + ProjectManager.SelectedLayer.Index);
        }

        public void SetUserControlSize(int width, int height)
        {
            UserControlWidth = width;
            UserControlHeight = height;
        }
        
        partial void OnIsZoomModeChanged(bool value)
        {
            UpdateModeText();
        }

        partial void OnIsPanModeChanged(bool value)
        {
            UpdateModeText();
        }

        private void UpdateModeText()
        {
            ModeText = IsZoomMode ? "ZOOM" : (IsPanMode ? "PAN" : "");
        }

        [RelayCommand]
        private void ToggleZoomMode()
        {
            IsZoomMode ^= true;
            IsPanMode = false;
        }

        [RelayCommand]
        private void TogglePanMode()
        {
            IsPanMode ^= true;
            IsZoomMode = false;
        }

        [RelayCommand]
        private async Task Reset()
        {
            var tasks = new[]
            {
                PanOffsetX.EaseDeltaAsync(-PanOffsetX.Value, Easing.EasingType.EaseInOutCubic , 300),
                PanOffsetY.EaseDeltaAsync(-PanOffsetY.Value, Easing.EasingType.EaseInOutCubic, 300),
                ZoomFactor.EaseDeltaAsync(-ZoomFactor.Value + 1f, Easing.EasingType.EaseInOutCubic, 300)
            };
            await Task.WhenAll(tasks);
        }

        [RelayCommand]
        private async Task FitToWindow()
        {
            double newZoomFactor = Math.Min(UserControlWidth / CurrentProject.Width, UserControlHeight / CurrentProject.Height);
            double newPanOffsetY = (UserControlHeight - CurrentProject.Height) / 2;
            var tasks = new[]
            {
                PanOffsetX.EaseToAsync(0, Easing.EasingType.EaseInOutCubic , 300),
                PanOffsetY.EaseToAsync(newPanOffsetY, Easing.EasingType.EaseInOutCubic, 300),
                ZoomFactor.EaseToAsync(newZoomFactor, Easing.EasingType.EaseInOutCubic, 300)
            };
            await Task.WhenAll(tasks);
        }

        public void UpdateMouseInfo(Point position, bool isPressed)
        {
            MouseInfo = $"{position.X:F0}, {position.Y:F0}" + (isPressed ? " [DOWN]" : "");
        }

        public void HandleCtrlKeyPress(bool isPressed)
        {
            IsCtrlPressed = isPressed;
        }

        public async Task HandleMouseWheel(MouseWheelEventArgs e)
        {
            await HandleMouseZoom(e);
        }

        public async Task HandleMouseZoom(MouseWheelEventArgs e)
        {
            if (!IsZoomMode) return;

            double delta = e.Delta / 120.0;
            double newZoomFactor = Math.Clamp(ZoomFactor.Value + (delta * ZOOM_STEP_PERCENTAGE), 0.1, 10);
            await ZoomFactor.EaseToAsync(newZoomFactor, Easing.EasingType.EaseInOutCubic, 30);
        }

        public void HandleMousePan(Point startPoint, Point currentPoint)
        {
            if (!IsPanMode) return;

            double deltaX = currentPoint.X - startPoint.X;
            double deltaY = currentPoint.Y - startPoint.Y;

            PanOffsetX.Value += deltaX;
            PanOffsetY.Value += deltaY;
        }

        public void HandleMouseDown(object sender, MouseButtonEventArgs e, FrameworkElement canvas)
        {
            if (SelectedTool != null && !IsZoomMode && !IsPanMode)
            {
                SelectedTool.OnMouseDown(sender, e, e.GetPosition(canvas));
            }
        }

        public void HandleMouseUp(object sender, MouseButtonEventArgs e, FrameworkElement canvas)
        {
            if (SelectedTool != null && !IsZoomMode && !IsPanMode)
            {
                SelectedTool.OnMouseUp(sender, e, e.GetPosition(canvas));
            }
        }

        public void HandleMouseMove(object sender, MouseEventArgs e, FrameworkElement canvas)
        {
            if (SelectedTool != null && !IsZoomMode && !IsPanMode)
            {
                SelectedTool.OnMouseMove(sender, e, e.GetPosition(canvas));
            }
        }
    }
}