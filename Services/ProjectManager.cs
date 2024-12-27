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

namespace MVVMPaintApp.Services
{
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

            // Draw dirty rect region for debugging
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
            int count = CurrentProject.Layers.Count;
            Layer layer = new(count + 1, CurrentProject.Width, CurrentProject.Height);
            CurrentProject.Layers.Add(layer);
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
                Debug.WriteLine("Project saved successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to save project.");
                throw new ApplicationException("Failed to save project.", ex);
            }
        }

        public static void SaveProjectAs(Project project)
        {
            //to be changed to save as Png/Jpeg/Bmp/Gif/Tiff
        }

        public void SetCursor(Cursor cursor)
        {
            Cursor = cursor;
        }

        [RelayCommand]
        private async Task FitToWindow()
        {
            double newZoomFactor = Math.Min(
                DrawingCanvasControlWidth / CurrentProject.Width,
                DrawingCanvasControlHeight / CurrentProject.Height);
            double newPanOffsetY = (DrawingCanvasControlHeight - CurrentProject.Height) / 2;
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
