using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.ViewModels
{
    internal partial class DrawingCanvasViewModel : ObservableObject
    {
        private const double ZOOM_STEP_PERCENTAGE = 0.1;

        [ObservableProperty]
        private Project currentProject;

        [ObservableProperty]
        private WriteableBitmap canvasRenderTarget;

        // Canvas properties
        [ObservableProperty]
        private double panOffsetX;

        [ObservableProperty]
        private double panOffsetY;

        [ObservableProperty]
        private double zoomFactor = 1.0;

        // Mode properties
        // to be changed to Tools class using enum tool types
        [ObservableProperty]
        private bool isZoomMode;

        [ObservableProperty]
        private bool isPanMode;


        // Debugging properties
        [ObservableProperty]
        private string modeText = "";

        [ObservableProperty]
        private string mouseInfo = "0, 0";


        public DrawingCanvasViewModel()//Project project)
        {
            this.currentProject = new Project();
            canvasRenderTarget = new WriteableBitmap(currentProject.Width, currentProject.Height, 96, 96, PixelFormats.Pbgra32, null);

            RenderProject();
        }

        public void RenderProject()
        {
            if (CurrentProject == null || CanvasRenderTarget == null) return;

            using var context = CanvasRenderTarget.GetBitmapContext();
            // Clear the canvas
            CanvasRenderTarget.Clear(CurrentProject.Background);

            // Render each layer
            foreach (var layer in CurrentProject.Layers)
            {
                if (layer.IsVisible)
                {
                    CanvasRenderTarget.Blit(new Rect(0, 0, layer.Content.PixelWidth, layer.Content.PixelHeight), layer.Content, new Rect(0, 0, layer.Content.PixelWidth, layer.Content.PixelHeight), WriteableBitmapExtensions.BlendMode.Alpha);
                }
            }
        }


        partial void OnIsZoomModeChanged(bool value)
        {
            UpdateModeText();
        }

        partial void OnIsPanModeChanged(bool value)
        {
            UpdateModeText();
        }

        private void UpdateModeText()
        {
            ModeText = IsZoomMode ? "ZOOM" : (IsPanMode ? "PAN" : "");
        }

        [RelayCommand]
        private void ToggleZoomMode()
        {
            IsZoomMode ^= true;
            IsPanMode = false;
        }

        [RelayCommand]
        private void TogglePanMode()
        {
            IsPanMode ^= true;
            IsZoomMode = false;
        }

        [RelayCommand]
        private async Task Reset()
        {
            const double duration = 300; // duration in milliseconds
            const double frameRate = 240; // frames per second
            const double totalFrames = duration / (1000.0 / frameRate);

            double startZoomFactor = ZoomFactor;
            double startPanOffsetX = PanOffsetX;
            double startPanOffsetY = PanOffsetY;

            double targetZoomFactor = 1.0;
            double targetPanOffsetX = 0.0;
            double targetPanOffsetY = 0.0;

            for (int i = 0; i <= totalFrames; i++)
            {
                double t = i / totalFrames; // normalized time (0.0 to 1.0)

                // Apply easing (ease-in-out cubic)
                t = t < 0.5 ? 4 * t * t * t : 1 - Math.Pow(-2 * t + 2, 3) / 2;

                ZoomFactor = Lerp(startZoomFactor, targetZoomFactor, t);
                PanOffsetX = Lerp(startPanOffsetX, targetPanOffsetX, t);
                PanOffsetY = Lerp(startPanOffsetY, targetPanOffsetY, t);

                await Task.Delay((int)(1000 / frameRate)); // delay for frame rate
            }
        }
            
        private static double Lerp(double start, double end, double t)
        {
            return start + (end - start) * t;
        }

        public void UpdateMouseInfo(Point position, bool isPressed)
        {
            MouseInfo = $"{position.X:F0}, {position.Y:F0}" + (isPressed ? " [DOWN]" : "");
        }

        public void HandleMouseWheel(MouseWheelEventArgs e)
        {
            if (!IsZoomMode) return;

            double zoomStep = e.Delta > 0 ? (ZOOM_STEP_PERCENTAGE + 1f) : (1f / (ZOOM_STEP_PERCENTAGE + 1f));
            double newZoomFactor = ZoomFactor * zoomStep;

            ZoomFactor = Math.Max(0.1, Math.Min(newZoomFactor, 10.0));
        }

        public void HandleMousePan(Point startPoint, Point currentPoint)
        {
            if (!IsPanMode) return;

            double deltaX = currentPoint.X - startPoint.X;
            double deltaY = currentPoint.Y - startPoint.Y;

            PanOffsetX += deltaX;
            PanOffsetY += deltaY;
        }
    }
}