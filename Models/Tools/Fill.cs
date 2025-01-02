using MVVMPaintApp.Services;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Models.Tools
{
    public class Fill(ProjectManager projectManager) : ToolBase(projectManager)
    {
        public Color FillColor { get; set; }
        public int ColorTolerance { get; set; } = 0;

        public override async void OnMouseDown(object sender, MouseButtonEventArgs e, Point p)
        {
            if (!IsValidDrawingState()) return;

            HitCheck(ref p);
            var bitmap = ProjectManager.SelectedLayer.Content;
            int startX = (int)p.X;
            int startY = (int)p.Y;

            CurrentStrokeRegion = new Rect(p, new Size(1, 1));
            Color targetColor = bitmap.GetPixel(startX, startY);
            FillColor = GetCurrentColor(e);

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int[] pixels = new int[width * height];

            bitmap.CopyPixels(pixels, bitmap.BackBufferStride, 0);

            var modifiedPixels = await Task.Run(() =>
                ScanlineFill(pixels, width, height, startX, startY, targetColor, FillColor));

            OldState = new((int)CurrentStrokeRegion.Value.Width, (int)CurrentStrokeRegion.Value.Height, 96, 96, PixelFormats.Bgra32, null);
            OldState.Blit(new Rect(0.0, 0.0, CurrentStrokeRegion.Value.Width, CurrentStrokeRegion.Value.Height), ProjectManager.SelectedLayer.Content, CurrentStrokeRegion.Value);

            Application.Current.Dispatcher.Invoke(() =>
            {
                var wb = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, null);
                wb.WritePixels(new Int32Rect(0, 0, width, height), modifiedPixels, wb.BackBufferStride, 0);
                ProjectManager.SelectedLayer.Content = wb;
                ProjectManager.Render(CurrentStrokeRegion.Value);
            });

            ProjectManager.UndoRedoManager.AddHistoryEntry(
                    new LayerHistoryEntry(
                        ProjectManager.SelectedLayer,
                        CurrentStrokeRegion.Value,
                        OldState));
            ProjectManager.HasUnsavedChanges = true;
        }

        private int[] ScanlineFill(int[] pixels, int width, int height, int x, int y, Color targetColor, Color fillColor)
        {
            if (IsColorSimilar(targetColor, fillColor)) return pixels;

            int targetArgb = (targetColor.A << 24) | (targetColor.R << 16) | (targetColor.G << 8) | targetColor.B;
            int fillArgb = (fillColor.A << 24) | (fillColor.R << 16) | (fillColor.G << 8) | fillColor.B;

            int minX = x, maxX = x, minY = y, maxY = y;
            Stack<(int y, int left, int right)> segments = new();
            segments.Push((y, x, x));

            while (segments.Count > 0)
            {
                var (curY, curLeft, curRight) = segments.Pop();
                if (curY < 0 || curY >= height) continue;

                int rowOffset = curY * width;

                int left = curLeft;
                while (left >= 0 && IsPixelSimilar(pixels[rowOffset + left], targetArgb, ColorTolerance))
                    left--;
                left++;

                int right = curRight;
                while (right < width && IsPixelSimilar(pixels[rowOffset + right], targetArgb, ColorTolerance))
                    right++;
                right--;

                minX = Math.Min(minX, left);
                maxX = Math.Max(maxX, right);
                minY = Math.Min(minY, curY);
                maxY = Math.Max(maxY, curY);

                for (int i = left; i <= right; i++)
                    pixels[rowOffset + i] = fillArgb;

                if (curY > 0)
                    CheckAndAddSegments(pixels, width, curY - 1, left, right, targetArgb, segments);
                if (curY < height - 1)
                    CheckAndAddSegments(pixels, width, curY + 1, left, right, targetArgb, segments);
            }

            CurrentStrokeRegion = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
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
                bool matchesTarget = IsPixelSimilar(pixels[rowOffset + x], targetArgb, ColorTolerance);

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

        public override void OnMouseMove(object sender, MouseEventArgs e, Point imagePoint)
        {
        }
    }
}
