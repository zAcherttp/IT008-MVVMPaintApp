using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Models;
using MVVMPaintApp.Services;

namespace MVVMPaintApp.ViewModels
{
    public partial class NewFileViewModel : ObservableObject
    {
        private bool isChanging = false;
        private readonly IWindowManager windowManager;
        private readonly ViewModelLocator viewModelLocator;

        [ObservableProperty]
        private int selectedWidth;

        [ObservableProperty]
        private int selectedHeight;

        [ObservableProperty]
        private double selectedAspectRatioW;

        [ObservableProperty]
        private double selectedAspectRatioH;

        [ObservableProperty]
        private ProjectPreset selectedPreset;

        [ObservableProperty]
        private ObservableCollection<ProjectPreset> presets = [];

        [RelayCommand]
        public void CreateNewFileAndOpenMainWindow()
        {
            if (SelectedWidth > 0 && SelectedHeight > 0)
            {
                var project = new Project(SelectedWidth, SelectedHeight);
                windowManager.ShowWindow(viewModelLocator.MainCanvasViewModel);
                viewModelLocator.MainCanvasViewModel.SetProject(project);
                windowManager.CloseWindow(this);
            }
        }

        public NewFileViewModel(IWindowManager windowManager, ViewModelLocator viewModelLocator)
        {
            this.windowManager = windowManager;
            this.viewModelLocator = viewModelLocator;

            PopulateDefaultPresets();
            SelectedPreset = Presets[0];
        }

        private void PopulateDefaultPresets()
        {
            Presets = [];
            Presets.Add(new ProjectPreset("Landscape", "16 : 9", 1920, 1080));
            Presets.Add(new ProjectPreset("Portrait", "9 : 16", 1080, 1920));
            Presets.Add(new ProjectPreset("Square", "1 : 1", 1000, 1000));
        }

        partial void OnSelectedPresetChanged(ProjectPreset value)
        {
            if (value != null && !isChanging)
            {
                isChanging = true;
                SelectedWidth = value.DefaultWidth;
                SelectedHeight = value.DefaultHeight;
                SelectedAspectRatioW = Math.Round(value.AspectRatioW);
                SelectedAspectRatioH = Math.Round(value.AspectRatioH);
                isChanging = false;
            }
        }

        partial void OnSelectedWidthChanged(int value)
        {
            if (!isChanging && value > 0)
            {
                isChanging = true;
                double ratio = (double)SelectedAspectRatioH / SelectedAspectRatioW;
                SelectedHeight = (int)(value * ratio);
                isChanging = false;
            }
        }

        partial void OnSelectedHeightChanged(int value)
        {
            if (!isChanging && value > 0)
            {
                isChanging = true;
                double ratio = (double)SelectedAspectRatioW / SelectedAspectRatioH;
                SelectedWidth = (int)(value * ratio);
                isChanging = false;
            }
        }

        partial void OnSelectedAspectRatioWChanged(double value)
        {
            if (!isChanging && value > 0)
            {
                isChanging = true;
                double ratio = (double)SelectedAspectRatioH / value;
                SelectedHeight = (int)(SelectedWidth * ratio);
                isChanging = false;
            }
        }

        partial void OnSelectedAspectRatioHChanged(double value)
        {
            if (!isChanging && value > 0)
            {
                isChanging = true;
                double ratio = (double)value / SelectedAspectRatioW;
                SelectedHeight = (int)(SelectedWidth * ratio);
                isChanging = false;
            }
        }
    }
}
