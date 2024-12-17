using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Services;
using MVVMPaintApp.Models;

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
        private int viewPortWidth = 1600;

        [ObservableProperty]
        private int viewPortHeight = 900;

        [RelayCommand]
        public void SetProject(Project project)
        {
            ProjectManager.SetProject(project);
            WindowTitle = "Home - " + project.Name;
            DrawingCanvasViewModel.SetProject(project);
            DrawingCanvasViewModel.SetViewPortSize(ViewPortWidth, ViewPortHeight);

            ColorPaletteViewModel.SetProjectColors(project.ColorsList);
        }

        public MainCanvasViewModel(
            IWindowManager windowManager,
            ProjectManager projectManager,
            DrawingCanvasViewModel drawingCanvasViewModel,
            ColorPaletteViewModel colorPaletteViewModel)
        {
            this.windowManager = windowManager;
            this.drawingCanvasViewModel = drawingCanvasViewModel;
            this.colorPaletteViewModel = colorPaletteViewModel;

            ProjectManager = projectManager;
        }
    }
}
