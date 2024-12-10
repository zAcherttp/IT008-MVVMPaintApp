using System.IO;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models
{
    [Serializable]
    public class SerializableLayer
    {
        public string ContentPath { get; set; } = "";
        public bool IsVisible { get; set; } = true;
        public int Index { get; set; } = 0;
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;

        public SerializableLayer(Layer layer, string projectFolderPath)
        {
            ContentPath = Path.Combine(projectFolderPath, $"layer_{layer.Index}.png");

            using (var fileStream = new FileStream(ContentPath, FileMode.Create))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(layer.Content));
                encoder.Save(fileStream);
            }

            IsVisible = layer.IsVisible;
            Index = layer.Index;
            Width = layer.Width;
            Height = layer.Height;
        }

        public SerializableLayer() { }

        public Layer ToLayer()
        {
            Layer newLayer = new(Index, Width, Height);
            if (File.Exists(ContentPath))
            {
                using var stream = new FileStream(ContentPath, FileMode.Open, FileAccess.Read);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                newLayer.Content = new WriteableBitmap(bitmap);
            }
            newLayer.IsVisible = IsVisible;
            return newLayer;
        }
    }
}
