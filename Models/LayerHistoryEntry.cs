using MVVMPaintApp.Services;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models
{
    public class LayerHistoryEntry : HistoryEntry
    {
        public Layer Layer { get; set; }
        public Rect ModifiedRegion { get; set; }
        public WriteableBitmap OldState { get; set; }
        public WriteableBitmap NewState { get; set; }

        public LayerHistoryEntry(Layer layer, Rect modifiedRegion, WriteableBitmap oldState)
        {
            Layer = layer;
            ModifiedRegion = modifiedRegion;
            OldState = oldState;

            NewState = new WriteableBitmap(
                (int)modifiedRegion.Width,
                (int)modifiedRegion.Height,
                96, 96,
                PixelFormats.Bgra32,
                null
            );

            using var sourceContext = Layer.Content.GetBitmapContext();
            using var targetContext = NewState.GetBitmapContext();
            unsafe
            {
                for (int y = 0; y < (int)modifiedRegion.Height; y++)
                {
                    for (int x = 0; x < (int)modifiedRegion.Width; x++)
                    {
                        int sourceX = x + (int)modifiedRegion.X;
                        int sourceY = y + (int)modifiedRegion.Y;

                        if (sourceX >= 0 && sourceX < Layer.Content.PixelWidth &&
                            sourceY >= 0 && sourceY < Layer.Content.PixelHeight)
                        {
                            int sourcePixel = sourceContext.Pixels[sourceY * Layer.Content.PixelWidth + sourceX];
                            targetContext.Pixels[y * NewState.PixelWidth + x] = sourcePixel;
                        }
                    }
                }
            }
        }

        public override void Redo(ProjectManager projectManager)
        {
            ModifiedRegion.Intersect(new Rect(0, 0, Layer.Content.PixelWidth, Layer.Content.PixelHeight));
            Layer.Content.Blit(
                ModifiedRegion,
                NewState,
                new Rect(0, 0, ModifiedRegion.Width, ModifiedRegion.Height),
                WriteableBitmapExtensions.BlendMode.None
            );
            Layer.RenderThumbnail();
            projectManager.Render(new Rect(0, 0, Layer.Content.PixelWidth, Layer.Content.PixelHeight));

            Debug.WriteLine(projectManager.CurrentProject.Name + " " + Layer.Index + " Redo");
        }

        public override void Undo(ProjectManager projectManager)
        {
            ModifiedRegion.Intersect(new Rect(0, 0, Layer.Content.PixelWidth, Layer.Content.PixelHeight));
            Layer.Content.Blit(
            ModifiedRegion,
            OldState,
            new Rect(0, 0, ModifiedRegion.Width, ModifiedRegion.Height),
            WriteableBitmapExtensions.BlendMode.None
                );
            Layer.RenderThumbnail();
            projectManager.Render(new Rect(0, 0, Layer.Content.PixelWidth, Layer.Content.PixelHeight));
        }
    }
}
