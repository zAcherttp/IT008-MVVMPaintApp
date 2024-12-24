using MVVMPaintApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using MVVMPaintApp.Interfaces;
using System.Windows.Media.Imaging;
using System.Windows;

namespace MVVMPaintApp.Services
{
    public partial class ProjectManager : ObservableObject
    {
        private IProjectFactory projectFactory;

        [ObservableProperty]
        private Project currentProject;

        [ObservableProperty]
        private Layer selectedLayer;

        [ObservableProperty]
        private Color primaryColor = Colors.Black;

        [ObservableProperty]
        private Color secondaryColor = Colors.White;

        [ObservableProperty]
        private WriteableBitmap renderTarget;

        [ObservableProperty]
        private bool hasUnsavedChanges;

        public ProjectManager(IProjectFactory projectFactory)
        {
            this.projectFactory = projectFactory;
            CurrentProject = projectFactory.CreateDefault();
            RenderTarget = new WriteableBitmap(CurrentProject.Width, CurrentProject.Height, 96, 96, PixelFormats.Bgra32, null);
            selectedLayer = CurrentProject.Layers[0];
        }

        public void LoadProject(object project)
        {
            if (project is Project projectToLoad)
            {
                CurrentProject = projectToLoad;
                RenderTarget = new WriteableBitmap(CurrentProject.Width, CurrentProject.Height, 96, 96, PixelFormats.Bgra32, null);
                SelectedLayer = CurrentProject.Layers[0];
                Debug.WriteLine("Project " + CurrentProject.Name + " loaded.");
                Debug.WriteLine("Project width: " + CurrentProject.Width + ", height: " + CurrentProject.Height);
            } else if (project is string projectPath)
            {
                string projectJson = File.ReadAllText(projectPath);
                CurrentProject = JsonConvert.DeserializeObject<SerializableProject>(projectJson)!.ToProject() ??
                throw new InvalidDataException("Deserialized project is null.");
                RenderTarget = new WriteableBitmap(CurrentProject.Width, CurrentProject.Height, 96, 96, PixelFormats.Bgra32, null);
                SelectedLayer = CurrentProject.Layers[0];
                Debug.WriteLine("Project " + CurrentProject.Name + " loaded.");
                Debug.WriteLine("Project width: " + CurrentProject.Width + ", height: " + CurrentProject.Height);
            }
        }

        public void Render()
        {
            if (CurrentProject == null || RenderTarget == null) return;

            using var context = RenderTarget.GetBitmapContext();
            RenderTarget.Clear(CurrentProject.Background);

            for (int i = CurrentProject.Layers.Count - 1; i >= 0; i--)
            {
                var layer = CurrentProject.Layers[i];
                if (layer.IsVisible)
                {
                    Rect rect = new(0, 0, layer.Content.PixelWidth, layer.Content.PixelHeight);
                    RenderTarget.Blit(rect, layer.Content, rect, WriteableBitmapExtensions.BlendMode.Alpha);
                }
            }
        }

        public void ToggleLayerVisibility(Layer? layer)
        {
            if (layer != null)
            {
                HasUnsavedChanges = true;
                CurrentProject.Layers[CurrentProject.Layers.IndexOf(layer)].IsVisible ^= true;
                Render();
            }
        }

        public void AddLayer()
        {
            HasUnsavedChanges = true;
            int count = CurrentProject.Layers.Count;
            Layer layer = new(count + 1, CurrentProject.Width, CurrentProject.Height);
            CurrentProject.Layers.Add(layer);
        }

        public void RemoveLayer(int index)
        {
            HasUnsavedChanges = true;
            if(CurrentProject.Layers.Count == 1)
            {
                return;
            }
            CurrentProject.Layers.RemoveAt(index);
            for (int i = 0; i < CurrentProject.Layers.Count; i++)
            {
                CurrentProject.Layers[i].Index = i;
            }
            Render();
        }

        public void RemoveLayer(Layer layer)
        {
            HasUnsavedChanges = true;
            if (CurrentProject.Layers.Count == 1)
            {
                return;
            }
            CurrentProject.Layers.Remove(layer);
            for (int i = 0; i < CurrentProject.Layers.Count; i++)
            {
                CurrentProject.Layers[i].Index = i;
            }
            Render();
        }

        public void Move(int oldIndex, int newIndex)
        {
            HasUnsavedChanges = true;
            CurrentProject.Layers[oldIndex].Index = newIndex;
            CurrentProject.Layers[newIndex].Index = oldIndex;
            CurrentProject.Layers.Move(oldIndex, newIndex);
            Render();
        }

        public void SetColorListColorAtIndex(int index, Color color)
        {
            HasUnsavedChanges = true;
            CurrentProject.ColorsList[index] = color;
        }

        public void SaveProject()
        {
            Debug.WriteLine("Saving project...");
            if (Directory.Exists(CurrentProject.ProjectFolderPath))
            {
                Directory.Delete(CurrentProject.ProjectFolderPath, true);
            }
            Directory.CreateDirectory(CurrentProject.ProjectFolderPath);

            CurrentProject.GenerateThumbnail();
            try
            {
                string projectJson = JsonConvert.SerializeObject(new SerializableProject(CurrentProject), Formatting.Indented);
                File.WriteAllText(Path.Combine(CurrentProject.ProjectFolderPath, "project.json"), projectJson);
                Debug.WriteLine("Project saved successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to save project.");
                throw new ApplicationException("Failed to save project.", ex);
            }
        }

        public static void SaveProjectAs(Project project)
        {
            //to be changed to save as Png/Jpeg/Bmp/Gif/Tiff
        }
    }
}
