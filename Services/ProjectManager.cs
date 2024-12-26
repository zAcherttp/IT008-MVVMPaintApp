using MVVMPaintApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using MVVMPaintApp.Interfaces;
using System.Windows.Media.Imaging;
using System.Windows;

namespace MVVMPaintApp.Services
{
    public partial class ProjectManager : ObservableObject
    {
        private bool needsFullRender = true;

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
        private WriteableBitmap renderTarget;


        public ProjectManager(IProjectFactory projectFactory)
        {
            CurrentProject = projectFactory.CreateDefault();
            StrokeLayer = new WriteableBitmap(CurrentProject.Width, CurrentProject.Height, 96, 96, PixelFormats.Bgra32, null);
            RenderTarget = new WriteableBitmap(CurrentProject.Width, CurrentProject.Height, 96, 96, PixelFormats.Bgra32, null);
            
            selectedLayer = CurrentProject.Layers[0];
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
            } else if (project is string projectPath)
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

        public void InvalidateRegion(Rect dirtyRect, Layer modifiedLayer)
        {
            if (CurrentProject == null || RenderTarget == null) return;

            // If the modified layer is below or at the current composite cache point,
            // we need to rebuild the composite from this layer up
            bool needsCompositeUpdate = CurrentProject.Layers.IndexOf(modifiedLayer) <= CurrentProject.Layers.IndexOf(SelectedLayer);

            if (needsCompositeUpdate)
            {
                RenderRegion(dirtyRect);
            }
            else
            {
                // Just render the modified layer's region
                using var context = RenderTarget.GetBitmapContext();
                if (modifiedLayer.IsVisible)
                {
                    RenderTarget.Blit(dirtyRect, modifiedLayer.Content, dirtyRect, WriteableBitmapExtensions.BlendMode.Alpha);
                }
            }

            // Draw dirty rect region for debugging
            //RenderTarget.DrawRectangle(
            //    (int)dirtyRect.X,
            //    (int)dirtyRect.Y,
            //    (int)(dirtyRect.X + dirtyRect.Width),
            //    (int)(dirtyRect.Y + dirtyRect.Height),
            //    Colors.Red
            //);
        }

        private void RenderRegion(Rect dirtyRect)
        {
            if (CurrentProject == null || RenderTarget == null) return;

            // Ensure dirty rect is within bounds
            dirtyRect.Intersect(new Rect(0, 0, RenderTarget.PixelWidth, RenderTarget.PixelHeight));
            if (dirtyRect.IsEmpty) return;

            using var context = RenderTarget.GetBitmapContext();

            // If we need a full render, clear everything and reset the composite
            if (needsFullRender)
            {
                RenderTarget.Clear(CurrentProject.Background);
                needsFullRender = false;
            }
            else
            {
                // Clear just the dirty region
                RenderTarget.FillRectangle(
                    (int)dirtyRect.X,
                    (int)dirtyRect.Y,
                    (int)(dirtyRect.X + dirtyRect.Width),
                    (int)(dirtyRect.Y + dirtyRect.Height),
                    CurrentProject.Background
                );
            }

            // Render layers in the dirty region
            for (int i = CurrentProject.Layers.Count - 1; i >= 0; i--)
            {
                var layer = CurrentProject.Layers[i];
                if (layer.IsVisible && layer.Content != null)
                {
                    if (layer == SelectedLayer)
                    {
                        RenderTarget.Blit(dirtyRect, layer.Content, dirtyRect, WriteableBitmapExtensions.BlendMode.Alpha);
                        RenderTarget.Blit(dirtyRect, StrokeLayer, dirtyRect, WriteableBitmapExtensions.BlendMode.Alpha);
                    }
                    else
                    {
                        RenderTarget.Blit(dirtyRect, layer.Content, dirtyRect, WriteableBitmapExtensions.BlendMode.Alpha);
                    }
                }
            }
        }

        public void Render()
        {
            needsFullRender = true;
            RenderRegion(new Rect(0, 0, RenderTarget.PixelWidth, RenderTarget.PixelHeight));
        }

        public void ToggleLayerVisibility(Layer? layer)
        {
            if (layer != null)
            {
                HasUnsavedChanges = true;
                CurrentProject.Layers[CurrentProject.Layers.IndexOf(layer)].IsVisible ^= true;
                needsFullRender = true;
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
            needsFullRender = true;
            Render();
        }

        public void Move(int oldIndex, int newIndex)
        {
            HasUnsavedChanges = true;
            CurrentProject.Layers[oldIndex].Index = newIndex;
            CurrentProject.Layers[newIndex].Index = oldIndex;
            CurrentProject.Layers.Move(oldIndex, newIndex);
            needsFullRender = true;
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
    }
}
