using MVVMPaintApp.Interfaces;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;

namespace MVVMPaintApp.Models
{
    public class ProjectFactory : IProjectFactory
    {
        public Project CreateDefault()
        {
            string defaultFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyPaint");
            string name = GetDefaultProjectName();
            int width = 1152;
            int height = 648;
            var project = new Project()
            {
                Width = width,
                Height = height,
                Name = name,
                ProjectFolderPath = Path.Combine(defaultFolder, name),
                Layers = [new(0, width, height)],
                Background = Colors.White,
                ColorsList = Enumerable.Repeat(Colors.Transparent, 18).ToList()
            };
            return project;
        }

        public Project Load(string projectFolder)
        {
            Debug.WriteLine("Loading project...");
            try
            {
                string projectJson = File.ReadAllText(Path.Combine(projectFolder, "project.json"));
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

        public string GetDefaultProjectName()
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
