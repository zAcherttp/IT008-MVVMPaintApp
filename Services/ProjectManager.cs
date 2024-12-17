using MVVMPaintApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.IO;

namespace MVVMPaintApp.Services
{
    public partial class ProjectManager : ObservableObject
    {
        private const string PROJECT_JSON_FILENAME = "project.json";

        [ObservableProperty]
        private Project currentProject;

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

        public void AddLayer(Layer newLayer)
        {
            CurrentProject.Layers.Add(newLayer);
        }

        public void SaveProject()
        {
            Directory.CreateDirectory(CurrentProject.ProjectFolderPath);
            CurrentProject.GenerateThumbnail();
            try
            {
                string projectJson = JsonConvert.SerializeObject(new SerializableProject(CurrentProject), Formatting.Indented);
                File.WriteAllText(Path.Combine(CurrentProject.ProjectFolderPath, PROJECT_JSON_FILENAME), projectJson);
            }
            catch
            {
                throw new ApplicationException("Failed to save project.");
            }
        }

        public static void SaveProjectAs(Project project)
        {
            //to be changed to save as Png/Jpeg/Bmp/Gif/Tiff
        }

        public static Project LoadProject(string projectFolder)
        {
            try
            {
                string projectJson = File.ReadAllText(Path.Combine(projectFolder, PROJECT_JSON_FILENAME));
                SerializableProject? serializableProject =
                    JsonConvert.DeserializeObject<SerializableProject>(projectJson) ?? throw new InvalidDataException("Deserialized project is null.");
                Project project = serializableProject.ToProject();
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
