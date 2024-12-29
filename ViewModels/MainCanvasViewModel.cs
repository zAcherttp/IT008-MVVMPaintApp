using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Services;
using MVVMPaintApp.Models;
using System.Diagnostics;

namespace MVVMPaintApp.ViewModels
{
    public partial class MainCanvasViewModel : ObservableObject
    {
        private readonly IWindowManager windowManager;

        [ObservableProperty]
        private bool showGridBorders = false;

        [ObservableProperty]
        private ProjectManager projectManager;

        [ObservableProperty]
        private string windowTitle = "Home";

        [ObservableProperty]
        private DrawingCanvasViewModel drawingCanvasViewModel;

        [ObservableProperty]
        private ColorPaletteViewModel colorPaletteViewModel;

        [ObservableProperty]
        private LayerViewModel layerViewModel;

        [ObservableProperty]
        private ToolboxViewModel toolboxViewModel;

        [ObservableProperty]
        private int windowWidth = 1600;

        [ObservableProperty]
        private int windowHeight = 900;

        [RelayCommand]
        public void SetProject(Project project)
        {
            ProjectManager.LoadProject(project);
            WindowTitle = "Home - " + project.Name;

            DrawingCanvasViewModel.SetProjectManager(ProjectManager);
            ColorPaletteViewModel.SetProjectColors(project.ColorsList);
            LayerViewModel.SetProjectManager(ProjectManager);
            ToolboxViewModel.SetProjectManager(ProjectManager);
        }

        public MainCanvasViewModel(
            IWindowManager windowManager,
            ProjectManager projectManager,
            DrawingCanvasViewModel drawingCanvasViewModel,
            ColorPaletteViewModel colorPaletteViewModel,
            LayerViewModel layerViewModel,
            ToolboxViewModel toolboxViewModel)
        {
            this.windowManager = windowManager;
            DrawingCanvasViewModel = drawingCanvasViewModel;
            ColorPaletteViewModel = colorPaletteViewModel;
            LayerViewModel = layerViewModel;
            ToolboxViewModel = toolboxViewModel;
            ProjectManager = projectManager;
        }
    }
}
