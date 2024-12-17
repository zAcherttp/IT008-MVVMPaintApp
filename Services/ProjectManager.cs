using MVVMPaintApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;

namespace MVVMPaintApp.Services
{
    public partial class ProjectManager : ObservableObject
    {
        private const string PROJECT_JSON_FILENAME = "project.json";

        [ObservableProperty]
        private Project currentProject;

        [ObservableProperty]
        private bool hasUnsavedChanges;

        public ProjectManager()
        {
            CurrentProject = new Project();
        }

        public ProjectManager(Project project)
        {
            CurrentProject = project;
        }

        public void SetProject(Project project)
        {
            CurrentProject = project;
        }

        public void AddLayer(int index)
        {
            HasUnsavedChanges = true;
            Layer layer = new(index, CurrentProject.Width, CurrentProject.Height);
            CurrentProject.Layers.Insert(index, layer);
        }

        public void RemoveLayer(int index)
        {
            HasUnsavedChanges = true;
            CurrentProject.Layers.RemoveAt(index);
        }

        public void SetColorListColorAtIndex(int index, Color color)
        {
            HasUnsavedChanges = true;
            CurrentProject.ColorsList[index] = color;
        }

        public void SaveProject()
        {
            Debug.WriteLine("Saving project...");
            Directory.CreateDirectory(CurrentProject.ProjectFolderPath);
            CurrentProject.GenerateThumbnail();
            try
            {
                string projectJson = JsonConvert.SerializeObject(new SerializableProject(CurrentProject), Formatting.Indented);
                File.WriteAllText(Path.Combine(CurrentProject.ProjectFolderPath, PROJECT_JSON_FILENAME), projectJson);
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

        public static Project LoadProject(string projectFolder)
        {
            Debug.WriteLine("Loading project...");
            try
            {
                string projectJson = File.ReadAllText(Path.Combine(projectFolder, PROJECT_JSON_FILENAME));
                SerializableProject? serializableProject =
                    JsonConvert.DeserializeObject<SerializableProject>(projectJson) ?? throw new InvalidDataException("Deserialized project is null.");
                Project project = serializableProject.ToProject();
                Debug.WriteLine("Project loaded successfully.");
                return project;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to load project.", ex);
            }
        }

        public static string GetDefaultProjectName()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string appFolderPath = Path.Combine(documentsPath, "MyPaint");

            var existingDirs = Directory.GetDirectories(appFolderPath).Select(Path.GetFileName).ToList();

            string projectName = "untitled";
            if (!existingDirs.Contains(projectName))
            {
                return projectName;
            }

            int counter = 1;
            while (existingDirs.Contains($"untitled{counter}"))
            {
                counter++;
            }
            return $"untitled{counter}";
        }
    }
}
