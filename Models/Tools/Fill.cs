using MVVMPaintApp.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models.Tools
{
    public class Fill(ProjectManager projectManager) : ToolBase(projectManager)
    {
        public Color FillColor { get; set; } = Colors.Black;
        public int ColorTolerance { get; set; } = 0;

        public override async void OnMouseDown(object sender, MouseEventArgs e, Point imagePoint)
        {
            if (ProjectManager.SelectedLayer == null) return;

            FillColor = e.LeftButton == MouseButtonState.Pressed ? ProjectManager.PrimaryColor : ProjectManager.SecondaryColor;

            var bitmap = ProjectManager.SelectedLayer.Content;
            int startX = (int)imagePoint.X;
            int startY = (int)imagePoint.Y;

            // Get the color on the UI thread
            Color targetColor = bitmap.GetPixel(startX, startY);

            // Create buffer for pixels
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int[] pixels = new int[width * height];

            // Copy pixels to array on UI thread
            bitmap.CopyPixels(pixels, bitmap.BackBufferStride, 0);

            // Process the fill on a background thread
            var modifiedPixels = await Task.Run(() =>
                ScanlineFill(pixels, width, height, startX, startY, targetColor, FillColor));

            // Update the bitmap on the UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                wb.WritePixels(new Int32Rect(0, 0, width, height), modifiedPixels, wb.BackBufferStride, 0);
                ProjectManager.SelectedLayer.Content = wb;
                ProjectManager.SelectedLayer.RenderThumbnail();
            });
        }

        private int[] ScanlineFill(int[] pixels, int width, int height, int x, int y, Color targetColor, Color fillColor)
        {
            if (IsColorSimilar(targetColor, fillColor)) return pixels;

            int targetArgb = (targetColor.A << 24) | (targetColor.R << 16) | (targetColor.G << 8) | targetColor.B;
            int fillArgb = (fillColor.A << 24) | (fillColor.R << 16) | (fillColor.G << 8) | fillColor.B;

            Stack<(int y, int left, int right)> segments = new();
            segments.Push((y, x, x));

            while (segments.Count > 0)
            {
                var (curY, curLeft, curRight) = segments.Pop();

                if (curY < 0 || curY >= height) continue;

                int rowOffset = curY * width;

                // Find leftmost boundary
                int left = curLeft;
                while (left >= 0 && IsPixelSimilar(pixels[rowOffset + left], targetArgb))
                    left--;
                left++;

                // Find rightmost boundary
                int right = curRight;
                while (right < width && IsPixelSimilar(pixels[rowOffset + right], targetArgb))
                    right++;
                right--;

                // Fill the current scanline
                for (int i = left; i <= right; i++)
                {
                    pixels[rowOffset + i] = fillArgb;
                }

                // Check scanlines above and below
                if (curY > 0)
                    CheckAndAddSegments(pixels, width, curY - 1, left, right, targetArgb, segments);
                if (curY < height - 1)
                    CheckAndAddSegments(pixels, width, curY + 1, left, right, targetArgb, segments);
            }

            return pixels;
        }

        private void CheckAndAddSegments(int[] pixels, int width, int y, int left, int right,
            int targetArgb, Stack<(int y, int left, int right)> segments)
        {
            int rowOffset = y * width;
            bool inSegment = false;
            int segmentStart = 0;

            for (int x = left; x <= right; x++)
            {
                bool matchesTarget = IsPixelSimilar(pixels[rowOffset + x], targetArgb);

                if (matchesTarget && !inSegment)
                {
                    segmentStart = x;
                    inSegment = true;
                }
                else if ((!matchesTarget || x == right) && inSegment)
                {
                    int segmentEnd = matchesTarget ? x : x - 1;
                    segments.Push((y, segmentStart, segmentEnd));
                    inSegment = false;
                }
            }
        }

        private bool IsPixelSimilar(int pixel1, int pixel2)
        {
            byte b1 = (byte)pixel1;
            byte g1 = (byte)(pixel1 >> 8);
            byte r1 = (byte)(pixel1 >> 16);
            byte a1 = (byte)(pixel1 >> 24);

            byte b2 = (byte)pixel2;
            byte g2 = (byte)(pixel2 >> 8);
            byte r2 = (byte)(pixel2 >> 16);
            byte a2 = (byte)(pixel2 >> 24);

            return Math.Abs(r1 - r2) <= ColorTolerance &&
                   Math.Abs(g1 - g2) <= ColorTolerance &&
                   Math.Abs(b1 - b2) <= ColorTolerance &&
                   Math.Abs(a1 - a2) <= ColorTolerance;
        }

        private bool IsColorSimilar(Color c1, Color c2)
        {
            return Math.Abs(c1.R - c2.R) <= ColorTolerance &&
                   Math.Abs(c1.G - c2.G) <= ColorTolerance &&
                   Math.Abs(c1.B - c2.B) <= ColorTolerance &&
                   Math.Abs(c1.A - c2.A) <= ColorTolerance;
        }

        public override void OnMouseMove(object sender, MouseEventArgs e, Point imagePoint)
        {
        }
    }
}
