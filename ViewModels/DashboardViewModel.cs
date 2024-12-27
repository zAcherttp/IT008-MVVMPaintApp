using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Models;
using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly IWindowManager windowManager;
        private readonly ViewModelLocator viewModelLocator;

        [ObservableProperty]
        private List<SerializableProject> recentProjects = [];

        [ObservableProperty]
        private SerializableProject? selectedProject;

        [RelayCommand]
        public void OpenNewFileWindow()
        {
            windowManager.ShowWindow(viewModelLocator.NewFileViewModel);
            windowManager.CloseWindow(this);
        }

        [RelayCommand]
        public void OpenProject()
        {
            if(SelectedProject == null)
            {
                return;
            }
            var project = SelectedProject.ToProject();
            windowManager.ShowWindow(viewModelLocator.MainCanvasViewModel);
            viewModelLocator.MainCanvasViewModel.SetProject(project);
            windowManager.CloseWindow(this);
        }

        public DashboardViewModel(IWindowManager windowManager, ViewModelLocator viewModelLocator)
        {
            this.windowManager = windowManager;
            this.viewModelLocator = viewModelLocator;
            LoadProjects();
        }

        private void LoadProjects()
        {
            string myPaintPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyPaint");
            if (Directory.Exists(myPaintPath))
            {
                foreach (var projectFolder in Directory.GetDirectories(myPaintPath))
                {
                    string projectFilePath = Path.Combine(projectFolder, "project.json");

                    if (File.Exists(projectFilePath))
                    {
                        try
                        {
                            string json = File.ReadAllText(projectFilePath);
                            SerializableProject? project = JsonConvert.DeserializeObject<SerializableProject>(json);

                            if (project != null)
                            {
                                // Load thumbnail if path exists and file is present
                                if (!string.IsNullOrWhiteSpace(project.ThumbnailPath) &&
                                    File.Exists(project.ThumbnailPath))
                                {
                                    try
                                    {
                                        BitmapImage thumbnailImage = new();
                                        thumbnailImage.BeginInit();
                                        thumbnailImage.CacheOption = BitmapCacheOption.OnLoad;
                                        thumbnailImage.UriSource = new Uri(project.ThumbnailPath);
                                        thumbnailImage.EndInit();
                                        thumbnailImage.Freeze(); // Make thread-safe

                                        project.Thumbnail = thumbnailImage;
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.WriteLine($"Failed to load thumbnail for project {project.Name}: {ex.Message}");
                                        project.Thumbnail = null;
                                    }
                                }

                                RecentProjects.Add(project);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error loading project: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}
