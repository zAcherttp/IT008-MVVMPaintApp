using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Models;
using MVVMPaintApp.Services;
using MVVMPaintApp.UserControls;

namespace MVVMPaintApp.ViewModels
{
    public partial class MainCanvasViewModel : ObservableObject
    {
        private Project? currentProject;
        private readonly IWindowManager windowManager;

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
            currentProject = project;
            DrawingCanvasViewModel.SetProject(project);
            DrawingCanvasViewModel.SetViewPortSize(ViewPortWidth, ViewPortHeight);

            ColorPaletteViewModel.SetProjectColors(project.ColorsList);
        }

        public MainCanvasViewModel(
            IWindowManager windowManager,
            DrawingCanvasViewModel drawingCanvasViewModel,
            ColorPaletteViewModel colorPaletteViewModel)
        {
            this.windowManager = windowManager;
            this.drawingCanvasViewModel = drawingCanvasViewModel;
            this.colorPaletteViewModel = colorPaletteViewModel;
        }
    }
}
