using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace MVVMPaintApp.Models
{
    [Serializable]
    public class SerializableProject
    {
        public string Name { get; set; } = "";
        public string ProjectFolderPath { get; set; } = "";
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public string ThumbnailPath { get; set; } = "";
        [JsonIgnore]
        public BitmapImage? Thumbnail { get; set; } = null;
        public double ThumbnailWidth { get; set; } = 0;
        public double ThumbnailHeight { get; set; } = 0;
        public List<SerializableLayer> Layers { get; set; } = [];
        public Color Background { get; set; } = Colors.Transparent;
        public List<Color> ColorsList { get; set; } = [];

        public SerializableProject(Project project)
        {
            Name = project.Name;
            ProjectFolderPath = project.ProjectFolderPath;
            Width = project.Width;
            Height = project.Height;
            ThumbnailPath = Path.Combine(ProjectFolderPath, "thumbnail.png");
            ThumbnailWidth = project.ThumbnailWidth;
            ThumbnailHeight = project.ThumbnailHeight;
            Layers = project.Layers.Select(layer => new SerializableLayer(layer, ProjectFolderPath)).ToList();
            Background = project.Background;
            ColorsList = project.ColorsList;
        }

        public SerializableProject() { }

        public Project ToProject()
        {
            Project newProject = new()
            {
                Name = Name,
                ProjectFolderPath = ProjectFolderPath,
                Width = Width,
                Height = Height,
                Layers = [.. Layers.Select(layer => layer.ToLayer())],
                Background = Background,
                ColorsList = ColorsList
            };
            return newProject;
        }
    }
}
