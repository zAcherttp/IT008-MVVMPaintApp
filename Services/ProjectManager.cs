using MVVMPaintApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using MVVMPaintApp.Interfaces;

namespace MVVMPaintApp.Services
{
    public partial class ProjectManager : ObservableObject
    {
        private readonly IProjectFactory projectFactory;

        [ObservableProperty]
        private Project currentProject;

        [ObservableProperty]
        private bool hasUnsavedChanges;

        public ProjectManager(IProjectFactory projectFactory)
        {
            this.projectFactory = projectFactory;
            CurrentProject = projectFactory.CreateDefault();
        }

        public void ToggleLayerVisibility(Layer? layer)
        {
            if (layer != null)
            {
                HasUnsavedChanges = true;
                CurrentProject.Layers[CurrentProject.Layers.IndexOf(layer)].IsVisible ^= true;
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
            CurrentProject.Layers.RemoveAt(index);
            for (int i = 0; i < CurrentProject.Layers.Count; i++)
            {
                CurrentProject.Layers[i].Index = i;
            }
        }

        public void RemoveLayer(Layer layer)
        {
            HasUnsavedChanges = true;
            CurrentProject.Layers.Remove(layer);
            for (int i = 0; i < CurrentProject.Layers.Count; i++)
            {
                CurrentProject.Layers[i].Index = i;
            }
        }

        public void Move(int oldIndex, int newIndex)
        {
            HasUnsavedChanges = true;
            CurrentProject.Layers[oldIndex].Index = newIndex;
            CurrentProject.Layers[newIndex].Index = oldIndex;
            CurrentProject.Layers.Move(oldIndex, newIndex);

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
