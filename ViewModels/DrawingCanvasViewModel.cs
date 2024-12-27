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
                ToolType.ZoomPan => new ZoomPan(ProjectManager),
                ToolType.Default => new Default(ProjectManager),

                _ => new Pencil(ProjectManager),
            };
            ProjectManager.SetCursor(SelectedTool.GetCursor());
            Debug.WriteLine("Selected tool: " + SelectedTool.GetType().Name + " - Layer: " + ProjectManager.SelectedLayer.Index);
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
                Key.Z,
                () => SetTool(ToolType.ZoomPan),
                "Tool: ZoomPan",
                [Key.Z]);

            keyHandler.RegisterCommand(
                Key.V,
                () => SetTool(ToolType.Pencil),
                "Tool: Pencil",
                [Key.V]);

            keyHandler.RegisterCommand(
                Key.Escape,
                () => SetTool(ToolType.Default),
                "Tool: Default",
                [Key.Escape]);
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