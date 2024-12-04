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

namespace MVVMPaintApp.ViewModels
{
    public partial class MainCanvasViewModel : ObservableObject
    {
        private Project? currentProject;
        private readonly IWindowManager windowManager;
        private readonly ViewModelLocator viewModelLocator;

        [ObservableProperty]
        private int viewPortWidth = 1600;

        [ObservableProperty]
        private int viewPortHeight = 900;

        [RelayCommand]
        public void SetProject(Project project)
        {
            currentProject = project;
            viewModelLocator.DrawingCanvasViewModel.SetProject(project);
            viewModelLocator.DrawingCanvasViewModel.SetViewPortSize(ViewPortWidth, ViewPortHeight);

            viewModelLocator.ColorPaletteViewModel.SetProjectColors(project.ColorsList);
        }

        public MainCanvasViewModel(IWindowManager windowManager, ViewModelLocator viewModelLocator)
        {
            this.windowManager = windowManager;
            this.viewModelLocator = viewModelLocator;
        }
    }
}
