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
        private int windowWidth = 1600;

        [ObservableProperty]
        private int windowHeight = 900;

        [RelayCommand]
        public void SetProject(Project project)
        {
            ProjectManager.CurrentProject = project;
            WindowTitle = "Home - " + project.Name;

            DrawingCanvasViewModel.SetProject(project);
            ColorPaletteViewModel.SetProjectColors(project.ColorsList);
            LayerViewModel.ProjectManager = ProjectManager;
        }

        public MainCanvasViewModel(
            IWindowManager windowManager,
            ProjectManager projectManager,
            DrawingCanvasViewModel drawingCanvasViewModel,
            ColorPaletteViewModel colorPaletteViewModel,
            LayerViewModel layerViewModel)
        {
            this.windowManager = windowManager;
            DrawingCanvasViewModel = drawingCanvasViewModel;
            ColorPaletteViewModel = colorPaletteViewModel;
            LayerViewModel = layerViewModel;
            ProjectManager = projectManager;
        }
    }
}
