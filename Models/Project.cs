using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Dynamic;
using MVVMPaintApp.Models;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows;

namespace MVVMPaintApp.Models
{
    public class Project
    {
        private const int THUMBNAIL_HEIGHT = 100;

        public string Name { get; set; }
        public string FilePath { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public BitmapImage Thumbnail { get; set; }
        public ObservableCollection<Layer> Layers { get; set; }
        public Color Background { get; set; }
        public ObservableCollection<PaletteColorSlot> ColorsList { get; set; }
        public double HomeViewCanvasWidth
        {
            get
            {
                return (double)Width / Height * THUMBNAIL_HEIGHT;
            }
        }
        public Project()
        {
            string defaultFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "MyPaint"
            );

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
            //Directory.CreateDirectory(FilePath);
        }

        public static string GetDefaultProjectName(string baseFolder)
        {
            Directory.CreateDirectory(baseFolder);

            var existingDirs = Directory.GetDirectories(baseFolder)
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

        public void Load(string path)
        {
            Project result = ProjectSerializer.Deserialize(path);
            Name = result.Name;
            FilePath = result.FilePath;
            Width = result.Width;
            Height = result.Height;
            Thumbnail = result.Thumbnail;
            Layers = result.Layers;
            Background = result.Background;
            ColorsList = result.ColorsList;
            Directory.CreateDirectory(FilePath);
        }

        public void Save()
        {
            // Prompt the user to select a folder for saving
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "MyPaint Project files (*.mpproj)|*.mpproj|All files (*.*)|*.*",
                DefaultExt = ".mpproj",
                FileName = Name
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Get the path chosen by the user and create a folder for the project
                string projectRootPath = Path.Combine(Path.GetDirectoryName(saveFileDialog.FileName) ?? "", Name);
                Directory.CreateDirectory(projectRootPath);

                // Serialize the main project file (e.g., in JSON format)
                ProjectSerializer.Serialize(this, projectRootPath);
            }
        }

        public void SaveAs()
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "Portable Network Graphics files (*.png)|*.png|All files (*.*)|*.*",
                DefaultExt = ".png",
                FileName = Name
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string path = saveFileDialog.FileName;
                RenderAndSaveAsPng(path);
            }
        }

        public void RenderAndSaveAsPng(string outputPath)
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

        public void GenerateThumbnail(BitmapImage originalBitmap)
        {
            if (originalBitmap == null)
                throw new ArgumentNullException(nameof(originalBitmap));

            // Calculate scaled dimensions while maintaining aspect ratio
            double scaleFactor = (double)THUMBNAIL_HEIGHT / originalBitmap.Height;
            int scaledWidth = (int)(originalBitmap.Width * scaleFactor);

            // Create a new TransformedBitmap for scaling
            TransformedBitmap transformedBitmap = new TransformedBitmap();
            transformedBitmap.BeginInit();
            transformedBitmap.Source = originalBitmap;
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
    }
}
