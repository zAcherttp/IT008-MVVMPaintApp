using System.Collections.ObjectModel;
using System.IO;
using MVVMPaintApp.Interfaces;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Win32;
using MVVMPaintApp.Services;

namespace MVVMPaintApp.Models
{
    public class Project : IProject
    {
        private const int THUMBNAIL_HEIGHT = 100;

        public string Name { get; set; }
        public string FilePath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public BitmapImage Thumbnail { get; set; }
        public ObservableCollection<Layer> Layers { get; set; }
        public Color Background { get; set; }
        public List<Color> ColorsList { get; set; }
        public double DashboardViewListViewItemWidth
        {
            get
            {
                return (double)Width / Height * THUMBNAIL_HEIGHT;
            }
        }
        public Project(bool createDefault = false)
        {
            if (createDefault)
            {
                string defaultFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "MyPaint");
                Name = GetDefaultProjectName(defaultFolder);
                FilePath = Path.Combine(defaultFolder, Name);
                Thumbnail = new BitmapImage();
                Width = 1152;
                Height = 648;
                Layers = [new(0, Width, Height)];
                Background = Colors.White;
                ColorsList = [];
                // Create the project directory
                //
                // Disabled for development purposes
                //
                //Directory.CreateDirectory(FilePath);
            }
            else
            {
                Name = "";
                FilePath = "";
                Thumbnail = new BitmapImage();
                Width = 1152;
                Height = 648;
                Layers = [];
                Background = Colors.Transparent;
                ColorsList = [];
            }
        }

        public Project(int width, int height)
        {
            string defaultFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "MyPaint");
            Name = GetDefaultProjectName(defaultFolder);
            FilePath = Path.Combine(defaultFolder, Name);
            Thumbnail = new BitmapImage();
            Width = width;
            Height = height;
            Layers = [new(0, Width, Height)];
            Background = Colors.White;
            ColorsList = [];
        }

        public void LoadProject(string filePath)
        {
            Project result = ProjectSerializer.Deserialize(filePath);
            Name = result.Name;
            FilePath = result.FilePath;
            Width = result.Width;
            Height = result.Height;
            Thumbnail = result.Thumbnail;
            Layers = result.Layers;
            Background = result.Background;
            ColorsList = result.ColorsList;
        }

        public void SaveProject()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                SaveProjectAs();
            }
            else
            {
                ProjectSerializer.Serialize(this, FilePath);
            }
        }

        public void SaveProjectAs()
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "MyPaint Project (*.mpproj)|*.mpproj",
                FileName = Name
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                FilePath = saveFileDialog.FileName;
                ProjectSerializer.Serialize(this, FilePath);
            }
        }

        public void ExportProject(string outputPath)
        {
            // Create a render target
            RenderTargetBitmap renderTarget = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);

            // Create a drawing visual to render the layers
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                // Draw the background
                drawingContext.DrawRectangle(new SolidColorBrush(Background), null, new System.Windows.Rect(0, 0, Width, Height));

                // Draw each layer
                foreach (var layer in Layers)
                {
                    if (layer.IsVisible)
                    {
                        drawingContext.DrawImage(layer.Content, new System.Windows.Rect(0, 0, Width, Height));
                    }
                }
            }

            // Render the visual to the bitmap
            renderTarget.Render(drawingVisual);

            // Encode the bitmap as PNG
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTarget));

            // Save the PNG to the specified path
            using var fileStream = new FileStream(outputPath, FileMode.Create);
            encoder.Save(fileStream);
        }

        public void GenerateThumbnail(BitmapImage toBitmap)
        {
            ArgumentNullException.ThrowIfNull(toBitmap);

            // Calculate scaled dimensions while maintaining aspect ratio
            double scaleFactor = (double)THUMBNAIL_HEIGHT / toBitmap.Height;
            int scaledWidth = (int)(toBitmap.Width * scaleFactor);

            // Create a new TransformedBitmap for scaling
            TransformedBitmap transformedBitmap = new TransformedBitmap();
            transformedBitmap.BeginInit();
            transformedBitmap.Source = toBitmap;
            transformedBitmap.Transform = new ScaleTransform(scaleFactor, scaleFactor);
            transformedBitmap.EndInit();

            // Convert to PNG stream
            using (MemoryStream stream = new MemoryStream())
            {
                // Create PNG encoder and save the scaled image
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(transformedBitmap));
                encoder.Save(stream);
                stream.Position = 0;

                // Create the final BitmapImage
                BitmapImage thumbnailImage = new BitmapImage();
                thumbnailImage.BeginInit();
                thumbnailImage.CacheOption = BitmapCacheOption.OnLoad;
                thumbnailImage.StreamSource = stream;
                thumbnailImage.EndInit();
                thumbnailImage.Freeze(); // Important for performance

                Thumbnail = thumbnailImage;
            }
        }

        public string GetDefaultProjectName(string defaultFolder)
        {
            Directory.CreateDirectory(defaultFolder);

            var existingDirs = Directory.GetDirectories(defaultFolder)
                                      .Select(Path.GetFileName)
                                      .ToList();

            string projectName = "Untitled";
            if (!existingDirs.Contains(projectName))
            {
                return projectName;
            }

            int counter = 1;
            while (existingDirs.Contains($"Untitled{counter}"))
            {
                counter++;
            }
            return $"Untitled{counter}";
        }
    }
}
