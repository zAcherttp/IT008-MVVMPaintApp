using MVVMPaintApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using MVVMPaintApp.Interfaces;
using System.Windows.Media.Imaging;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Microsoft.Win32;

namespace MVVMPaintApp.Services
{
    public enum ImageFileType
    {
        PNG,
        JPEG,
        BMP
    }

    public partial class ProjectManager : ObservableObject
    {
        public UndoRedoManager UndoRedoManager { get; }

        [ObservableProperty]
        private Project currentProject;
        
        [ObservableProperty]
        private bool hasUnsavedChanges;

        [ObservableProperty]
        private Layer selectedLayer;

        [ObservableProperty]
        private WriteableBitmap strokeLayer;

        [ObservableProperty]
        private Color primaryColor = Colors.Black;

        [ObservableProperty]
        private Color secondaryColor = Colors.White;

        [ObservableProperty]
        private bool isPrimaryColorSelected = true;

        [ObservableProperty]
        private Cursor cursor = Cursors.Arrow;

        [ObservableProperty]
        private Point cursorPositionOnCanvas;

        [ObservableProperty]
        private Easing panOffsetX = new(0.0);

        [ObservableProperty]
        private Easing panOffsetY = new(0.0);

        [ObservableProperty]
        private Easing zoomFactor = new(1.0);

        [ObservableProperty]
        private double drawingCanvasControlWidth = 1600;

        [ObservableProperty]
        private double drawingCanvasControlHeight = 700;

        [ObservableProperty]
        private WriteableBitmap renderTarget;


        public ProjectManager(IProjectFactory projectFactory)
        {
            CurrentProject = projectFactory.CreateDefault();
            StrokeLayer = new WriteableBitmap(CurrentProject.Width, CurrentProject.Height, 96, 96, PixelFormats.Bgra32, null);
            RenderTarget = new WriteableBitmap(CurrentProject.Width, CurrentProject.Height, 96, 96, PixelFormats.Bgra32, null);
            
            selectedLayer = CurrentProject.Layers[0];

            UndoRedoManager = new UndoRedoManager(this);
        }

        public void LoadProject(object project)
        {
            if (project is Project projectToLoad)
            {
                CurrentProject = projectToLoad;
                RenderTarget = new WriteableBitmap(CurrentProject.Width, CurrentProject.Height, 96, 96, PixelFormats.Bgra32, null);
                StrokeLayer = new WriteableBitmap(CurrentProject.Width, CurrentProject.Height, 96, 96, PixelFormats.Bgra32, null);
                SelectedLayer = CurrentProject.Layers[0];
                Debug.WriteLine("Project " + CurrentProject.Name + " loaded.");
                Debug.WriteLine("Project width: " + CurrentProject.Width + ", height: " + CurrentProject.Height);
            }
            else if (project is string projectPath)
            {
                string projectJson = File.ReadAllText(projectPath);
                CurrentProject = JsonConvert.DeserializeObject<SerializableProject>(projectJson)!.ToProject() ??
                throw new InvalidDataException("Deserialized project is null.");
                RenderTarget = new WriteableBitmap(CurrentProject.Width, CurrentProject.Height, 96, 96, PixelFormats.Bgra32, null);
                StrokeLayer = new WriteableBitmap(CurrentProject.Width, CurrentProject.Height, 96, 96, PixelFormats.Bgra32, null);
                SelectedLayer = CurrentProject.Layers[0];
                Debug.WriteLine("Project " + CurrentProject.Name + " loaded.");
                Debug.WriteLine("Project width: " + CurrentProject.Width + ", height: " + CurrentProject.Height);
            }
        }

        public void Render(Rect? dirtyRect = null)
        {
            if (CurrentProject == null || RenderTarget == null) return;

            var region = dirtyRect ?? new Rect(0, 0, RenderTarget.PixelWidth, RenderTarget.PixelHeight);
            region.Intersect(new Rect(0, 0, RenderTarget.PixelWidth, RenderTarget.PixelHeight));
            if (region.IsEmpty) return;

            using var context = RenderTarget.GetBitmapContext();

            RenderTarget.FillRectangle(
                (int)region.X,
                (int)region.Y,
                (int)(region.X + region.Width),
                (int)(region.Y + region.Height),
                CurrentProject.Background
            );

            for (int i = CurrentProject.Layers.Count - 1; i >= 0; i--)
            {
                var layer = CurrentProject.Layers[i];
                if (layer.IsVisible && layer.Content != null)
                {
                    if (layer == SelectedLayer)
                    {
                        RenderTarget.Blit(region, layer.Content, region, WriteableBitmapExtensions.BlendMode.Alpha);
                        RenderTarget.Blit(region, StrokeLayer, region, WriteableBitmapExtensions.BlendMode.Alpha);
                    }
                    else
                    {
                        RenderTarget.Blit(region, layer.Content, region, WriteableBitmapExtensions.BlendMode.Alpha);
                    }
                }
            }

            // Draw render region for debugging
            //RenderTarget.DrawRectangle(
            //    (int)region.X,
            //    (int)region.Y,
            //    (int)(region.X + region.Width),
            //    (int)(region.Y + region.Height),
            //    Colors.Red
            //);
        }

        public void ToggleLayerVisibility(Layer? layer)
        {
            if (layer != null)
            {
                HasUnsavedChanges = true;
                CurrentProject.Layers[CurrentProject.Layers.IndexOf(layer)].IsVisible ^= true;
                Render();
            }
        }

        public void AddLayer()
        {
            HasUnsavedChanges = true;
            foreach (var l in CurrentProject.Layers)
            {
                l.Index++;
            }
            Layer layer = new(0, CurrentProject.Width, CurrentProject.Height);
            CurrentProject.Layers.Insert(0, layer);
        }

        public void AddLayer(WriteableBitmap content)
        {
            HasUnsavedChanges = true;
            foreach (var l in CurrentProject.Layers)
            {
                l.Index++;
            }
            Layer layer = new(0, CurrentProject.Width, CurrentProject.Height, content);
            CurrentProject.Layers.Insert(0, layer);
        }

        public void RemoveLayer(object layer)
        {
            if (CurrentProject.Layers.Count == 1)
            {
                return;
            }
            HasUnsavedChanges = true;
            if (layer is Layer layerToRemove)
            {
                CurrentProject.Layers.Remove(layerToRemove);
            }
            else if (layer is int index)
            {
                CurrentProject.Layers.RemoveAt(index);
            }
            for (int i = 0; i < CurrentProject.Layers.Count; i++)
            {
                CurrentProject.Layers[i].Index = i;
            }
            Render();
        }

        public void Move(int oldIndex, int newIndex)
        {
            HasUnsavedChanges = true;
            CurrentProject.Layers[oldIndex].Index = newIndex;
            CurrentProject.Layers[newIndex].Index = oldIndex;
            CurrentProject.Layers.Move(oldIndex, newIndex);
            Render();
        }

        public void SetColorListColorAtIndex(int index, Color color)
        {
            HasUnsavedChanges = true;
            CurrentProject.ColorsList[index] = color;
        }

        public void SaveProject()
        {
            Debug.WriteLine("Saving project...");
            if (Directory.Exists(CurrentProject.ProjectFolderPath))
            {
                Directory.Delete(CurrentProject.ProjectFolderPath, true);
            }
            Directory.CreateDirectory(CurrentProject.ProjectFolderPath);

            CurrentProject.GenerateThumbnail();
            try
            {
                string projectJson = JsonConvert.SerializeObject(new SerializableProject(CurrentProject), Formatting.Indented);
                File.WriteAllText(Path.Combine(CurrentProject.ProjectFolderPath, "project.json"), projectJson);
                HasUnsavedChanges = false;
                Debug.WriteLine("Project saved successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to save project.");
                throw new ApplicationException("Failed to save project.", ex);
            }
        }

        public static void SaveProjectAs(Project project, ImageFileType fileType)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Save Image As",
                Filter = GetFileFilter(fileType),
                DefaultExt = GetFileExtension(fileType),
                AddExtension = true
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var renderTarget = new WriteableBitmap(
                        project.Width,
                        project.Height,
                        96, 96,
                        PixelFormats.Bgra32,
                        null
                    );

                    renderTarget.FillRectangle(
                        0, 0,
                        project.Width,
                        project.Height,
                        project.Background
                    );

                    for (int i = project.Layers.Count - 1; i >= 0; i--)
                    {
                        var layer = project.Layers[i];
                        if (layer.IsVisible && layer.Content != null)
                        {
                            renderTarget.Blit(
                                new Rect(0, 0, project.Width, project.Height),
                                layer.Content,
                                new Rect(0, 0, project.Width, project.Height),
                                WriteableBitmapExtensions.BlendMode.Alpha
                            );
                        }
                    }

                    using var fileStream = new FileStream(dialog.FileName, FileMode.Create);
                    BitmapEncoder encoder = CreateEncoder(fileType);
                    encoder.Frames.Add(BitmapFrame.Create(renderTarget));
                    encoder.Save(fileStream);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to save image: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
            }
        }

        private static string GetFileFilter(ImageFileType fileType)
        {
            return fileType switch
            {
                ImageFileType.PNG => "PNG Image|*.png",
                ImageFileType.JPEG => "JPEG Image|*.jpg",
                ImageFileType.BMP => "Bitmap Image|*.bmp",
                _ => "PNG Image|*.png"
            };
        }

        private static string GetFileExtension(ImageFileType fileType)
        {
            return fileType switch
            {
                ImageFileType.PNG => ".png",
                ImageFileType.JPEG => ".jpg",
                ImageFileType.BMP => ".bmp",
                _ => ".png"
            };
        }

        private static BitmapEncoder CreateEncoder(ImageFileType fileType)
        {
            return fileType switch
            {
                ImageFileType.PNG => new PngBitmapEncoder(),
                ImageFileType.JPEG => new JpegBitmapEncoder { QualityLevel = 95 },
                ImageFileType.BMP => new BmpBitmapEncoder(),
                _ => new PngBitmapEncoder()
            };
        }

        public void ResizeProject(int width, int height, bool isPixels)
        {
            var w = width;
            var h = height;
            if (!isPixels)
            {
                w = CurrentProject.Width * width / 100;
                h = CurrentProject.Height * height / 100;
            }
            if (w == CurrentProject.Width && h == CurrentProject.Height) return;
            if (w < 99 || h < 99) return;

            HasUnsavedChanges = true;
            CurrentProject.Width = w;
            CurrentProject.Height = h;
            RenderTarget = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
            StrokeLayer = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
            foreach (var layer in CurrentProject.Layers)
            {
                layer.Resize(w, h);
            }
            Render();
        }

        public void CropProject(Rect region)
        {
            var w = (int)region.Width;
            var h = (int)region.Height;
            if (w == CurrentProject.Width && h == CurrentProject.Height) return;
            if (w < 99 || h < 99) return;

            HasUnsavedChanges = true;
            CurrentProject.Width = w;
            CurrentProject.Height = h;
            RenderTarget = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
            StrokeLayer = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
            foreach (var layer in CurrentProject.Layers)
            {
                layer.Crop(region);
            }
            Render();
        }

        public void FlipProject(WriteableBitmapExtensions.FlipMode flipMode)
        {
            HasUnsavedChanges = true;
            foreach (var layer in CurrentProject.Layers)
            {
                layer.Flip(flipMode);
            }
            Render();
        }

        public void RotateProject(int degrees)
        {
            HasUnsavedChanges = true;
            (CurrentProject.Width, CurrentProject.Height) = (CurrentProject.Height, CurrentProject.Width);
            RenderTarget = new WriteableBitmap(CurrentProject.Width, CurrentProject.Height, 96, 96, PixelFormats.Bgra32, null);
            StrokeLayer = new WriteableBitmap(CurrentProject.Width, CurrentProject.Height, 96, 96, PixelFormats.Bgra32, null);
            foreach (var layer in CurrentProject.Layers)
            {
                layer.Rotate(degrees);
            }
            Render();
        }

        public void SetCursor(Cursor cursor)
        {
            Cursor = cursor;
        }

        public async void ZoomIn()
        {
            var newZoomFactor = ZoomFactor.Value - 0.5 > 0.1 ? ZoomFactor.Value - 0.5 : 0.1;
            if (Math.Abs(newZoomFactor - ZoomFactor.Value) < 0.01) return;
            await ZoomFactor.EaseToAsync(newZoomFactor, Easing.EasingType.EaseInOutCubic, 300);
        }

        public async void ZoomOut()
        {
            var newZoomFactor = ZoomFactor.Value + 0.5 < 8 ? ZoomFactor.Value + 0.5 : 10;
            if (Math.Abs(newZoomFactor - ZoomFactor.Value) < 0.01) return;
            await ZoomFactor.EaseToAsync(newZoomFactor, Easing.EasingType.EaseInOutCubic, 300);
        }

        [RelayCommand]
        private async Task FitToWindow()
        {
            var padding = DrawingCanvasControlHeight - 30;
            double newZoomFactor = Math.Min(
                padding / CurrentProject.Width,
                padding / CurrentProject.Height);
            double newPanOffsetY = (padding - CurrentProject.Height) / 2;

            newPanOffsetY = newZoomFactor >= 1 ? 0 : newPanOffsetY;

            var tasks = new[]
            {
                PanOffsetX.EaseToAsync(0, Easing.EasingType.EaseInOutCubic , 300),
                PanOffsetY.EaseToAsync(newPanOffsetY, Easing.EasingType.EaseInOutCubic, 300),
                ZoomFactor.EaseToAsync(newZoomFactor, Easing.EasingType.EaseInOutCubic, 300)
            };
            await Task.WhenAll(tasks);
        }

    }
}
