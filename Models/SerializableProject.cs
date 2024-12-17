using System.IO;

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
        public double ThumbnailWidth { get; set; } = 0;
        public double ThumbnailHeight { get; set; } = 0;
        public List<SerializableLayer> Layers { get; set; } = [];
        public byte[] Background { get; set; } = [];
        public List<byte[]> ColorsList { get; set; } = [];

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
            Background = ColorHelper.ToByteArray(project.Background);
            ColorsList = project.ColorsList.Select(color => ColorHelper.ToByteArray(color)).ToList();
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
                Background = ColorHelper.FromByteArray(Background),
                ColorsList = ColorsList.Select(color => ColorHelper.FromByteArray(color)).ToList()
            };
            return newProject;
        }
    }
}
