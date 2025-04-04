﻿using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Models;
using MVVMPaintApp.Services;

namespace MVVMPaintApp.ViewModels
{
    public partial class NewFileViewModel : ObservableValidator
    {
        private bool isChanging = false;
        private readonly IWindowManager windowManager;
        private readonly IProjectFactory projectFactory;
        private readonly ViewModelLocator viewModelLocator;

        [ObservableProperty]
        [Required(ErrorMessage = "Project name is required")]
        [RegularExpression(@"^[a-zA-Z0-9\s]*$", ErrorMessage = "Project name can only contain letters and numbers")]
        [StringLength(256, ErrorMessage = "Project name is too long")]
        [ProjectNameAvailability(ErrorMessage = "Project name already exists")]
        private string projectName;

        [ObservableProperty]
        [Range(100, 7680, ErrorMessage = "Width must be between 100 - 7680")]
        private int selectedWidth;

        [ObservableProperty]
        [Range(100, 7680, ErrorMessage = "Height must be between 100 - 7680")]
        private int selectedHeight;

        [ObservableProperty]
        [Range(1, 100, ErrorMessage = "Aspect ratio for width must be between 1 - 100")]
        private double selectedAspectRatioW;

        [ObservableProperty]
        [Range(1, 100, ErrorMessage = "Aspect ratio for height must be between 1 - 100")]
        private double selectedAspectRatioH;

        [ObservableProperty]
        private ProjectPreset selectedPreset;

        [ObservableProperty]
        private ObservableCollection<ProjectPreset> presets = [];

        [RelayCommand]
        public void OpenDashboardWindow()
        {
            windowManager.ShowWindow(viewModelLocator.DashboardViewModel);
            windowManager.CloseWindow(this);
        }

        [RelayCommand]
        public void CreateNewFileAndOpenMainWindow()
        {
            ValidateAllProperties();

            if (HasErrors)
            {
                var errorMessages = GetErrors(null).Cast<ValidationResult>().Select(e => e.ErrorMessage).ToList();
                MessageBox.Show(string.Join("\n", errorMessages), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SelectedWidth > 0 && SelectedHeight > 0)
            {
                var project = new Project(ProjectName, SelectedWidth, SelectedHeight);
                windowManager.ShowWindow(viewModelLocator.MainCanvasViewModel);
                viewModelLocator.MainCanvasViewModel.SetProject(project);
                viewModelLocator.MainCanvasViewModel.ProjectManager.HasUnsavedChanges = true;
                windowManager.CloseWindow(this);
            }
        }

        public NewFileViewModel(IWindowManager windowManager, IProjectFactory projectFactory, ViewModelLocator viewModelLocator)
        {
            this.windowManager = windowManager;
            this.viewModelLocator = viewModelLocator;
            this.projectFactory = projectFactory;

            PopulateDefaultPresets();
            SelectedPreset = Presets[0];
            ProjectName = projectFactory.GetDefaultProjectName();
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
                SelectedWidth = value.Width;
                SelectedHeight = value.Height;
                SelectedAspectRatioW = Math.Round(value.AspectRatioW);
                SelectedAspectRatioH = Math.Round(value.AspectRatioH);
                isChanging = false;
            }
        }

        partial void OnSelectedWidthChanged(int value)
        {
            if (!isChanging && value > 100)
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
