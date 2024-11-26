using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows;
using MVVMPaintApp.Interfaces;

namespace MVVMPaintApp.Commands
{
    public class BitmapChangeRegion(Int32Rect region, byte[] pixelData)
    {
        public Int32Rect Region { get; } = region;
        public byte[] PixelData { get; } = pixelData;
    }

    public class BitmapChangeCommand(WriteableBitmap bitmap, Int32Rect region, byte[] beforePixels, byte[] afterPixels)
        : IUndoable
    {
        private readonly WriteableBitmap Bitmap = bitmap;
        private readonly BitmapChangeRegion BeforeState = new(region, beforePixels);
        private readonly BitmapChangeRegion AfterState = new(region, afterPixels);

        public void Undo() => ApplyPixels(BeforeState);
        public void Redo() => ApplyPixels(AfterState);

        private void ApplyPixels(BitmapChangeRegion state)
        {
            Bitmap.WritePixels(
                state.Region,
                state.PixelData,
                state.Region.Width * 4,
                0
            );
        }
    }
}
