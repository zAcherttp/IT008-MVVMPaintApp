using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media.Imaging;
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
        private readonly KeyHandler keyHandler;

        [ObservableProperty]
        private ProjectManager projectManager;

        [ObservableProperty]
        private Project currentProject;

        [ObservableProperty]
        private ToolBase selectedTool;

        [ObservableProperty]
        private ToolType selectedToolType;

        [ObservableProperty]
        private double userControlWidth = 1600;

        [ObservableProperty]
        private double userControlHeight = 700;

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
            RegisterKeyCommands();
            DebugPrintKeyCommandList();
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
            ProjectManager.StrokeLayer.Clear();
            ProjectManager.Render();
            SelectedToolType = tool;
            SelectedTool = tool switch
            {
                ToolType.Pencil => new Pencil(ProjectManager),
                ToolType.Brush => new Brush(ProjectManager),
                ToolType.Eraser => new Eraser(ProjectManager),
                ToolType.Fill => new Fill(ProjectManager),
                ToolType.ColorPicker => new ColorPicker(ProjectManager),
                ToolType.Shape => new Shape(ProjectManager),
                ToolType.ZoomPan => new ZoomPan(ProjectManager),
                ToolType.RectSelect => new RectSelect(ProjectManager),
                ToolType.Default => new Default(ProjectManager),

                _ => new Pencil(ProjectManager),
            };
            ProjectManager.SetCursor(SelectedTool.GetCursor());
            Debug.WriteLine("Selected tool - " + SelectedTool.GetType().Name + " - Layer: " + ProjectManager.SelectedLayer.Index);
        }

        partial void OnSelectedToolTypeChanged(ToolType value)
        {
            SetTool(value);
        }

        public void SetUserControlSize(double width, double height)
        {
            ProjectManager.DrawingCanvasControlWidth = UserControlWidth = width;
            ProjectManager.DrawingCanvasControlHeight = UserControlHeight = height;
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
                Key.V,
                () => SetTool(ToolType.Pencil),
                "Tool - Pencil",
                [Key.V]);

            keyHandler.RegisterCommand(
                Key.B,
                () => SetTool(ToolType.Brush),
                "Tool - Brush",
                [Key.B]);

            keyHandler.RegisterCommand(
                Key.E,
                () => SetTool(ToolType.Eraser),
                "Tool - Eraser",
                [Key.E]);

            keyHandler.RegisterCommand(
                Key.F,
                () => SetTool(ToolType.Fill),
                "Tool - Fill",
                [Key.F]);

            keyHandler.RegisterCommand(
                Key.Q,
                () => SetTool(ToolType.ColorPicker),
                "Tool - Color Picker",
                [Key.Q]);

            keyHandler.RegisterCommand(
                Key.Z,
                () => SetTool(ToolType.ZoomPan),
                "Tool - ZoomPan",
                [Key.Z]);

            keyHandler.RegisterCommand(
                Key.Escape,
                () => SetTool(ToolType.Default),
                "Tool - Default",
                [Key.Escape]);
        }

        public void DebugPrintKeyCommandList()
        {
            Debug.WriteLine("Key Commands:");
            var commands = keyHandler.GetKeyBindings();
            foreach (var command in commands)
            {
                Debug.WriteLine(command);
            }
        }

        public void HandleKey(Key key, Key[] currentlyPressedKeys)
        {
            keyHandler.HandleKeyPress(key, currentlyPressedKeys);
        }

        public void UpdateMouseInfo(Point position, bool isPressed)
        {
            MouseInfo = $"{position.X:F0}, {position.Y:F0}" + (isPressed ? " [DOWN]" : "");
        }

        public void HandleMouseDown(object sender, MouseButtonEventArgs e, Point p)
        {
            SelectedTool?.OnMouseDown(sender, e, p);
        }

        public void HandleMouseUp(object sender, MouseButtonEventArgs e, Point p)
        {
            SelectedTool?.OnMouseUp(sender, e, p);
        }

        public void HandleMouseMove(object sender, MouseEventArgs e, Point p)
        {
            SelectedTool?.OnMouseMove(sender, e, p);
            ProjectManager.CursorPositionOnCanvas = p;
        }

        public async Task HandleMouseWheel(MouseWheelEventArgs e)
        {
            if (SelectedTool is ZoomPan zoomPan)
            {
                await zoomPan.HandleMouseWheel(e);
            }
        }
    }
}