using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using MVVMPaintApp.Commands;
using MVVMPaintApp.Services;

namespace MVVMPaintApp.Models
{
    public partial class Layer(int index, int width, int height) : ObservableObject
    {
        [ObservableProperty]
        private WriteableBitmap content = BitmapFactory.New(width, height);

        [ObservableProperty]
        private bool isVisible = true;

        [ObservableProperty]
        private int index = index;

        [ObservableProperty]
        private int width = width;

        [ObservableProperty]
        private int height = height;

        public WriteableBitmap LayerThumbnail
        {
            get
            {
                var thumbnailSizeWidth = 80;
                var thumbnailSizeHeight = (int)(80 / (Width / (double)Height));
                var checkerSize = 8;

                var layerThumbnail = BitmapFactory.New(thumbnailSizeWidth, thumbnailSizeHeight);
                layerThumbnail.Clear(Colors.Transparent);

                // Create checkerboard pattern
                for (int y = 0; y < thumbnailSizeHeight; y += checkerSize)
                {
                    for (int x = 0; x < thumbnailSizeWidth; x += checkerSize)
                    {
                        var color = ((x / checkerSize) + (y / checkerSize)) % 2 == 0 ? Colors.LightGray : Colors.Gray;
                        layerThumbnail.FillRectangle(x, y, x + checkerSize, y + checkerSize, color);
                    }
                }

                if (Content != null)
                {
                    var scaledContent = Content.Resize(thumbnailSizeWidth, thumbnailSizeHeight, WriteableBitmapExtensions.Interpolation.Bilinear);
                    layerThumbnail.Blit(new Rect(0, 0, thumbnailSizeWidth, thumbnailSizeHeight), scaledContent, new Rect(0, 0, scaledContent.PixelWidth, scaledContent.PixelHeight), WriteableBitmapExtensions.BlendMode.Alpha);
                }

                return layerThumbnail;
            }
        }

        public UndoRedoManager? UndoRedoManager { get; set; }

        public void MergeDown(Layer layer)
        {
            if (Content != null && layer.Content != null)
            {
                Content.Blit(new Rect(0, 0, Width, Height), layer.Content, new Rect(0, 0, layer.Width, layer.Height), WriteableBitmapExtensions.BlendMode.Alpha);
            }
        }

        // Helper method to track bitmap changes
        public void TrackBitmapChange(Int32Rect region, byte[] beforePixels, byte[] afterPixels)
        {
            if (Content != null && UndoRedoManager != null)
            {
                var command = new BitmapChangeCommand(Content, region, beforePixels, afterPixels);
                UndoRedoManager.AddCommand(command);
            }
        }
    }
}
