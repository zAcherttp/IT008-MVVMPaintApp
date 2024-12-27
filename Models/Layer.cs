using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using MVVMPaintApp.Services;

namespace MVVMPaintApp.Models
{
    public partial class Layer : ObservableObject
    {
        [ObservableProperty]
        private WriteableBitmap content;

        [ObservableProperty]
        private bool isVisible = true;

        [ObservableProperty]
        private int index;

        [ObservableProperty]
        private int width;

        [ObservableProperty]
        private int height;

        [ObservableProperty]
        private WriteableBitmap layerThumbnail;

        private WriteableBitmap layerThumbnailBackupTexture;
        private int thumbnailSizeWidth;
        private int thumbnailSizeHeight;
        private int thumbnailCheckerSize;
        public Layer(int index, int width, int height)
        {
            Index = index;
            Width = width;
            Height = height;
            Content = BitmapFactory.New(width, height);
            thumbnailSizeWidth = 80;
            thumbnailSizeHeight = (int)(80 / (Width / (double)Height));
            LayerThumbnail = BitmapFactory.New(thumbnailSizeWidth, thumbnailSizeHeight);
            thumbnailCheckerSize = 8;
            layerThumbnailBackupTexture = BitmapFactory.New(thumbnailSizeWidth, thumbnailSizeHeight);
            InitializeThumbnail();
        }

        public UndoRedoManager? UndoRedoManager { get; set; }

        public void MergeDown(Layer layer)
        {
            if (Content != null && layer.Content != null)
            {
                Content.Blit(new Rect(0, 0, Width, Height), layer.Content, new Rect(0, 0, layer.Width, layer.Height), WriteableBitmapExtensions.BlendMode.Alpha);
            }
        }

        public void InitializeThumbnail()
        {
            // Create checkerboard pattern
            layerThumbnailBackupTexture.Clear(Colors.Transparent);
            for (int y = 0; y < thumbnailSizeHeight; y += thumbnailCheckerSize)
            {
                for (int x = 0; x < thumbnailSizeWidth; x += thumbnailCheckerSize)
                {
                    var color = ((x / thumbnailCheckerSize) + (y / thumbnailCheckerSize)) % 2 == 0 ? Colors.LightGray : Colors.Gray;
                    layerThumbnailBackupTexture.FillRectangle(x, y, x + thumbnailCheckerSize, y + thumbnailCheckerSize, color);
                }
            }

            LayerThumbnail = layerThumbnailBackupTexture.Clone();
            RenderThumbnail();
        }

        public void RenderThumbnail()
        {
            if (Content != null)
            {
                LayerThumbnail = layerThumbnailBackupTexture.Clone();
                var scaledContent = Content.Resize(thumbnailSizeWidth, thumbnailSizeHeight, WriteableBitmapExtensions.Interpolation.NearestNeighbor);
                LayerThumbnail.Blit(new Rect(0, 0, thumbnailSizeWidth, thumbnailSizeHeight), scaledContent, new Rect(0, 0, scaledContent.PixelWidth, scaledContent.PixelHeight), WriteableBitmapExtensions.BlendMode.Alpha);
            }
        }
    }
}
