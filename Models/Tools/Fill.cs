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

        public override void OnMouseDown(object sender, MouseEventArgs e, Point imagePoint)
        {
            if (ProjectManager.SelectedLayer == null) return;

            FillColor = e.LeftButton == MouseButtonState.Pressed ? ProjectManager.PrimaryColor : ProjectManager.SecondaryColor;

            var bitmap = ProjectManager.SelectedLayer.Content;
            int startX = (int)imagePoint.X;
            int startY = (int)imagePoint.Y;

            Color targetColor = bitmap.GetPixel(startX, startY);
            ScanlineFill(bitmap, startX, startY, targetColor, FillColor);

            ProjectManager.SelectedLayer.RenderThumbnail();
        }

        private void ScanlineFill(WriteableBitmap bitmap, int x, int y, Color targetColor, Color fillColor)
        {
            if (IsColorSimilar(targetColor, fillColor)) return;

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            // Stack to store segments that need to be filled
            Stack<(int y, int left, int right)> segments = new Stack<(int y, int left, int right)>();

            // Add initial segment
            segments.Push((y, x, x));

            while (segments.Count > 0)
            {
                var (curY, curLeft, curRight) = segments.Pop();

                // Find leftmost boundary
                int left = curLeft;
                while (left >= 0 && IsColorSimilar(bitmap.GetPixel(left, curY), targetColor))
                    left--;
                left++;

                // Find rightmost boundary
                int right = curRight;
                while (right < width && IsColorSimilar(bitmap.GetPixel(right, curY), targetColor))
                    right++;
                right--;

                // Fill the current scanline
                for (int i = left; i <= right; i++)
                {
                    bitmap.SetPixel(i, curY, fillColor);
                }

                // Check scanlines above and below
                CheckAndAddSegments(bitmap, curY + 1, left, right, targetColor, fillColor, segments);
                CheckAndAddSegments(bitmap, curY - 1, left, right, targetColor, fillColor, segments);
            }
        }

        private void CheckAndAddSegments(WriteableBitmap bitmap, int y, int left, int right,
            Color targetColor, Color fillColor, Stack<(int y, int left, int right)> segments)
        {
            if (y < 0 || y >= bitmap.PixelHeight) return;

            bool inSegment = false;
            int segmentStart = 0;

            for (int x = left; x <= right; x++)
            {
                bool matchesTarget = IsColorSimilar(bitmap.GetPixel(x, y), targetColor);

                if (matchesTarget && !inSegment)
                {
                    // Start new segment
                    segmentStart = x;
                    inSegment = true;
                }
                else if ((!matchesTarget || x == right) && inSegment)
                {
                    // End current segment
                    int segmentEnd = matchesTarget ? x : x - 1;
                    segments.Push((y, segmentStart, segmentEnd));
                    inSegment = false;
                }
            }
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
