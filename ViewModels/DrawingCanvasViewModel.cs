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
        private readonly KeyHandler keyHandler;

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
        private Easing panOffsetY = new(0.0);

        [ObservableProperty]
        private Easing zoomFactor = new(1.0);

        // Mode properties
        // to be changed to Tools class using enum tool types
        [ObservableProperty]
        private bool isZoomMode;

        [ObservableProperty]
        private bool isPanMode;

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
            keyHandler = new KeyHandler();
            ProjectManager.Render();
            RegisterKeyCommands();
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
            IsPanMode = false;
            IsZoomMode = false;
            Debug.WriteLine("Selected tool: " + SelectedTool.GetType().Name + " - Layer: " + ProjectManager.SelectedLayer.Index);
        }

        public void SetUserControlSize(int width, int height)
        {
            UserControlWidth = width;
            UserControlHeight = height;
        }

        public void RegisterKeyCommands()
        {
            keyHandler.RegisterCommand(
                Key.Z,
                () => ProjectManager.UndoRedoManager.Undo(),
                "Undo last action",
                [Key.LeftCtrl, Key.Z]);

            keyHandler.RegisterCommand(
                Key.Y,
                () => ProjectManager.UndoRedoManager.Redo(),
                "Redo last action",
                [Key.LeftCtrl, Key.Y]);

            keyHandler.RegisterCommand(
                Key.Z,
                () => ToggleZoomMode(),
                "Toggle zoom mode",
                [Key.Z]);

            keyHandler.RegisterCommand(
                Key.V,
                () => TogglePanMode(),
                "Toggle pan mode",
                [Key.V]);

            keyHandler.RegisterCommand(
                Key.Escape,
                () =>
                {
                    IsZoomMode = false;
                    IsPanMode = false;
                },
                "Exit pan/zoom mode",
                [Key.Escape]);
        }

        public void HandleKey(Key key, Key[] currentlyPressedKeys)
        {
            keyHandler.HandleKeyPress(key, currentlyPressedKeys);
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

        public Cursor GetCursor()
        {
            if (IsZoomMode)
            {
                return Cursors.Cross;
            }
            else if (IsPanMode)
            {
                return Cursors.SizeAll;
            }
            else
            {
                return Cursors.Arrow;
            }
        }

        private void ToggleZoomMode()
        {
            IsZoomMode ^= true;
            IsPanMode = false;
        }

        private void TogglePanMode()
        {
            IsPanMode ^= true;
            IsZoomMode = false;
        }

        public void UpdateMouseInfo(Point position, bool isPressed)
        {
            MouseInfo = $"{position.X:F0}, {position.Y:F0}" + (isPressed ? " [DOWN]" : "");
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

            PanOffsetX.Value += currentPoint.X - startPoint.X;
            PanOffsetY.Value += currentPoint.Y - startPoint.Y;
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
    }
}