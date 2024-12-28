using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using MVVMPaintApp.Services;
using System.Diagnostics;

namespace MVVMPaintApp.Models.Tools
{
    public class RectSelect(ProjectManager projectManager) : ToolBase(projectManager)
    {
        public Selection Selection { get; set; } = new Selection();
        public Point Start { get; set; }
        public WriteableBitmap? SelectedContent { get; set; }
        private WriteableBitmap? SelectedContentBackup;
        private bool IsMovingSelection;
        private bool IsResizingSelection;
        private int? ActiveHandleIndex;
        private Rect? PreviousRenderBounds;
        private const int MIN_SELECTION_SIZE = 25;

        public override void OnMouseDown(object sender, MouseButtonEventArgs e, Point p)
        {
            base.OnMouseDown(sender, e, p);

            if (Selection.IsActive)
            {
                // Check for handle interaction first
                ActiveHandleIndex = Selection.GetHandleIndex(p);
                if (ActiveHandleIndex.HasValue)
                {
                    SelectedContentBackup = SelectedContent?.Clone();
                    IsResizingSelection = true;
                    Start = p;
                    PreviousRenderBounds = Selection.RenderBounds;
                    //Debug.WriteLine($"Resizing selection with handle {ActiveHandleIndex}");
                    return;
                }

                // Then check for movement
                if (Selection.IsPointInsideBounds(p))
                {
                    IsMovingSelection = true;
                    Start = p;
                    PreviousRenderBounds = Selection.RenderBounds;
                    //Debug.WriteLine("Moving selection");
                    return;
                }
                else
                {
                    //Debug.WriteLine("Committing selection");
                    CommitSelection();
                }
            }

            HitCheck(ref p);
            Start = p;
            Selection.UpdateBounds(Start, Start);
        }

        public override void OnMouseMove(object sender, MouseEventArgs e, Point p)
        {
            if (Selection.IsActive)
            {
                if (IsResizingSelection && ActiveHandleIndex.HasValue)
                {
                    ResizeSelection(p);
                    return;
                }
                if (IsMovingSelection)
                {
                    MoveSelection(p);
                    return;
                }
                return;
            }

            if (!IsDrawing || !IsValidDrawingState()) return;

            Selection.UpdateBounds(Start, p);
            DrawPreview(p, Colors.Black);
        }

        public override void OnMouseUp(object sender, MouseButtonEventArgs e, Point p)
        {
            if (IsResizingSelection)
            {
                IsResizingSelection = false;
                ActiveHandleIndex = null;
                PreviousRenderBounds = null;
                //Debug.WriteLine("Finished resizing selection");
                return;
            }

            if (IsMovingSelection)
            {
                IsMovingSelection = false;
                PreviousRenderBounds = null;
                //Debug.WriteLine("Finished moving selection");
                return;
            }

            Selection.UpdateBounds(Start, p);

            if (Selection.Bounds.Width > MIN_SELECTION_SIZE && Selection.Bounds.Height > MIN_SELECTION_SIZE && !Selection.IsActive)
            {
                //Debug.WriteLine("Creating new selection");
                CreateNewSelection();
            }
            else if (!IsMovingSelection && !IsResizingSelection)
            {
                Selection.IsActive = false;
                ProjectManager.StrokeLayer.Clear(Colors.Transparent);
                ProjectManager.Render(Selection.RenderBounds);
            }

            base.OnMouseUp(sender, e, p);
        }

        private void ResizeSelection(Point p)
        {
            ProjectManager.StrokeLayer.Clear(Colors.Transparent);

            Point start = Selection.Bounds.TopLeft;
            Point end = Selection.Bounds.BottomRight;


            switch (ActiveHandleIndex)
            {
                case 0: // Top-left
                    p.X = Math.Min(p.X, end.X - MIN_SELECTION_SIZE);
                    p.Y = Math.Min(p.Y, end.Y - MIN_SELECTION_SIZE);
                    start = p;
                    break;
                case 1: // Top-right
                    p.X = Math.Max(p.X, start.X + MIN_SELECTION_SIZE);
                    p.Y = Math.Min(p.Y, end.Y - MIN_SELECTION_SIZE);
                    start = new Point(start.X, p.Y);
                    end = new Point(p.X, end.Y);
                    break;
                case 2: // Bottom-right
                    p.X = Math.Max(p.X, start.X + MIN_SELECTION_SIZE);
                    p.Y = Math.Max(p.Y, start.Y + MIN_SELECTION_SIZE);
                    end = p;
                    break;
                case 3: // Bottom-left
                    p.X = Math.Min(p.X, end.X - MIN_SELECTION_SIZE);
                    p.Y = Math.Max(p.Y, start.Y + MIN_SELECTION_SIZE);
                    start = new Point(p.X, start.Y);
                    end = new Point(end.X, p.Y);
                    break;
                case 4: // Top-middle
                    p.Y = Math.Min(p.Y, end.Y - MIN_SELECTION_SIZE);
                    start = new Point(start.X, p.Y);
                    break;
                case 5: // Middle-right
                    p.X = Math.Max(p.X, start.X + MIN_SELECTION_SIZE);
                    end = new Point(p.X, end.Y);
                    break;
                case 6: // Bottom-middle
                    p.Y = Math.Max(p.Y, start.Y + MIN_SELECTION_SIZE);
                    end = new Point(end.X, p.Y);
                    break;
                case 7: // Middle-left
                    p.X = Math.Min(p.X, end.X - MIN_SELECTION_SIZE);
                    start = new Point(p.X, start.Y);
                    break;
            }

            var newBounds = new Rect(start, end);
            if (newBounds.Width < 10 || newBounds.Height < 10) return;

            // Create new bitmap for resized content
            var newContent = new WriteableBitmap(
                (int)newBounds.Width,
                (int)newBounds.Height,
                96, 96,
                PixelFormats.Bgra32,
                null
            );

            // Scale the content to new size
            if(SelectedContent != null)
            {
                // Set SelectedContentBackup to SelectedContent for cool blur effect
                newContent = SelectedContentBackup.Resize(
                    (int)newBounds.Width,
                    (int)newBounds.Height,
                    WriteableBitmapExtensions.Interpolation.Bilinear
                );
            }

            // Update selection
            Selection.UpdateBounds(newBounds.TopLeft, newBounds.BottomRight);
            SelectedContent = newContent;

            // Calculate combined render region for clean updates
            var renderRegion = PreviousRenderBounds ?? Selection.RenderBounds;
            renderRegion = Rect.Union(renderRegion, Selection.RenderBounds);
            PreviousRenderBounds = Selection.RenderBounds;

            // Draw resized selection
            ProjectManager.StrokeLayer.Blit(
                Selection.Bounds,
                SelectedContent,
                Selection.SourceBounds,
                WriteableBitmapExtensions.BlendMode.Alpha
            );

            Selection.Draw(ProjectManager.StrokeLayer);
            ProjectManager.Render(renderRegion);
        }

        private void CommitSelection()
        {
            if (SelectedContent != null)
            {
                Debug.WriteLine("Committing selection to layer");
                // Store the current bounds before clearing selection
                var commitBounds = Selection.RenderBounds;

                // Blit the selected content back to the layer
                ProjectManager.SelectedLayer.Content.Blit(
                    Selection.Bounds,
                    SelectedContent,
                    Selection.SourceBounds,
                    WriteableBitmapExtensions.BlendMode.Alpha
                );

                // Clear selection state
                ProjectManager.StrokeLayer.Clear(Colors.Transparent);
                SelectedContent = null;
                Selection.IsActive = false;

                // Render the affected area
                ProjectManager.Render(commitBounds);
            }
        }

        private void MoveSelection(Point p)
        {
            ProjectManager.StrokeLayer.Clear(Colors.Transparent);

            Vector offset = p - Start;
            var newBounds = new Rect(
                Selection.Bounds.X + offset.X,
                Selection.Bounds.Y + offset.Y,
                Selection.Bounds.Width,
                Selection.Bounds.Height
            );

            Selection.UpdateBounds(
                new Point(newBounds.X, newBounds.Y),
                new Point(newBounds.X + newBounds.Width, newBounds.Y + newBounds.Height)
            );

            Start = p;

            // Calculate combined render region for clean updates
            var renderRegion = PreviousRenderBounds ?? Selection.RenderBounds;
            renderRegion = Rect.Union(renderRegion, Selection.RenderBounds);
            PreviousRenderBounds = Selection.RenderBounds;

            // Draw selection at new position
            ProjectManager.StrokeLayer.Blit(
                Selection.Bounds,
                SelectedContent,
                new Rect(0, 0, Selection.Bounds.Width, Selection.Bounds.Height),
                WriteableBitmapExtensions.BlendMode.Alpha
            );

            Selection.Draw(ProjectManager.StrokeLayer);
            ProjectManager.Render(renderRegion);
        }

        private void CreateNewSelection()
        {
            var selectionBitmap = new WriteableBitmap(
                (int)Selection.Bounds.Width,
                (int)Selection.Bounds.Height,
                96, 96,
                PixelFormats.Bgra32,
                null
            );

            selectionBitmap.Clear(Colors.Transparent);
            Selection.IsActive = true;

            SelectedContent = selectionBitmap.Clone();
            SelectedContent.Blit(
                Selection.SourceBounds,
                ProjectManager.SelectedLayer.Content,
                Selection.Bounds
            );

            ProjectManager.SelectedLayer.Content.Blit(
                Selection.Bounds,
                selectionBitmap,
                Selection.SourceBounds,
                WriteableBitmapExtensions.BlendMode.None
            );

            ProjectManager.StrokeLayer.Blit(
                Selection.Bounds,
                SelectedContent,
                Selection.SourceBounds,
                WriteableBitmapExtensions.BlendMode.Alpha
            );

            Selection.Draw(ProjectManager.StrokeLayer);
            ProjectManager.Render(Selection.RenderBounds);
        }

        public override void DrawPreview(Point p, Color color)
        {
            if (IsValidDrawingState() && CurrentStrokeRegion == null)
            {
                ProjectManager.StrokeLayer.Clear(Colors.Transparent);

                HitCheck(ref p);
                Rect region = new(Start, p);
                int x1 = (int)region.X;
                int y1 = (int)region.Y;
                int x2 = (int)(region.X + region.Width);
                int y2 = (int)(region.Y + region.Height);

                // Draw dotted selection rectangle
                ProjectManager.StrokeLayer.DrawLineDotted(x1 + 1, y1, x2, y1, 5, 5, color);
                ProjectManager.StrokeLayer.DrawLineDotted(x2, y1 + 1, x2, y2, 5, 5, color);
                ProjectManager.StrokeLayer.DrawLineDotted(x2 + 1, y2, x1 + 1, y2, 5, 5, color);
                ProjectManager.StrokeLayer.DrawLineDotted(x1, y2, x1, y1 + 1, 5, 5, color);

                ProjectManager.Render(Selection.RenderBounds);
            }
        }
    }
}