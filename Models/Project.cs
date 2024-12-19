using System.Collections.ObjectModel;
using System.IO;
using MVVMPaintApp.Interfaces;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using Microsoft.Win32;
using MVVMPaintApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MVVMPaintApp.Models
{
    public enum PictureFormat
    {
        Png,
        Jpeg,
        Bmp,
        Gif,
        Tiff
    }

    public partial class Project : ObservableObject, IProject
    {
        private const string THUMBNAIL_FILE_NAME = "thumbnail.png";

        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string projectFolderPath = "";

        [ObservableProperty]
        private int width = 0;

        [ObservableProperty]
        private int height = 0;

        [ObservableProperty]
        private double thumbnailHeight = 100;

        [ObservableProperty]
        private ObservableCollection<Layer> layers = [];

        [ObservableProperty]
        private Color background = Colors.Transparent;

        [ObservableProperty]
        private bool isBackgroundVisible = true;

        [ObservableProperty]
        private List<Color> colorsList = [];

        internal string defaultFolder = "";

        public double ThumbnailWidth
        {
            get
            {
                return (double)Width / Height * ThumbnailHeight;
            }
        }

        public Project(string projectName, int width, int height)
        {
            Width = width;
            Height = height;
            defaultFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MyPaint");
            Name = projectName;
            ProjectFolderPath = Path.Combine(defaultFolder, Name);
            Layers = [new(0, Width, Height), new(1, Width, Height)];
            Background = Colors.White;
            ColorsList = Enumerable.Repeat(Colors.Transparent, 18).ToList();
        }

        public Project()
        {
        }

        public void GenerateThumbnail()
        {
            RenderTargetBitmap renderBitmap = new(Width, Height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual drawingVisual = new();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(new SolidColorBrush(Background), null, new Rect(0, 0, Width, Height));
                foreach (var layer in Layers.Where(l => l.IsVisible))
                {
                    drawingContext.DrawImage(layer.Content, new Rect(0, 0, layer.Width, layer.Height));
                }
            }
            renderBitmap.Render(drawingVisual);

            TransformedBitmap thumbnailBitmap = new(renderBitmap, new ScaleTransform(ThumbnailHeight / (double)Height, ThumbnailHeight / (double)Height));

            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(thumbnailBitmap));

            if (string.IsNullOrEmpty(ProjectFolderPath) || !Directory.Exists(ProjectFolderPath))
            {
                throw new InvalidOperationException("Project folder path is not set or does not exist.");
            }

            string thumbnailPath = Path.Combine(ProjectFolderPath, THUMBNAIL_FILE_NAME);
            using FileStream fileStream = new(thumbnailPath, FileMode.Create);
            encoder.Save(fileStream);
        }
    }
}
