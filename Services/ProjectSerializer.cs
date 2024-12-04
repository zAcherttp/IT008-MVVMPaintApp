using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows;
using MVVMPaintApp.Models;

namespace MVVMPaintApp.Services
{
    // Serializable version of Layer that excludes WritableBitmap
    [Serializable]
    public class SerializableLayer
    {
        public required int Index { get; set; }
        public required bool IsVisible { get; set; }
    }

    [Serializable]
    public class SerializableProject
    {
        public required string Name { get; set; }
        public required string FilePath { get; set; }
        public required int Width { get; set; }
        public required int Height { get; set; }
        public required List<SerializableLayer> Layers { get; set; }
        public required Color Background { get; set; }
        public required List<Color> ColorsList { get; set; }
    }

    public class ProjectSerializer
    {
        private const string FileExtension = ".mpproj";
        private const string ThumbnailExtension = ".thumbnail";     // .png
        private const string LayerFileName = "layer.";              // .png

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(),
                new JsonColorConverter()
            }
        };

        public static void Serialize(Project project, string path)
        {
            if (string.IsNullOrEmpty(project.FilePath))
                throw new InvalidOperationException("Project file path cannot be empty.");

            string projectPath = path + project.Name + FileExtension;
            string thumbnailPath = path + "image" + ThumbnailExtension;
            string? directoryPath = Path.GetDirectoryName(path);

            if (directoryPath != null)
                Directory.CreateDirectory(directoryPath);
            else
                throw new InvalidOperationException("Could not resolve directory path!");

            // Create layers directory
            string layersPath = Path.Combine(directoryPath, "layers");
            Directory.CreateDirectory(layersPath);

            // Convert layers to serializable format and save their bitmaps
            var serializableLayers = new List<SerializableLayer>();
            foreach (var layer in project.Layers)
            {
                var serializableLayer = new SerializableLayer
                {
                    Index = layer.Index,
                    IsVisible = layer.IsVisible
                };

                // Save layer bitmap
                if (layer.Content != null)
                {
                    string layerPath = Path.Combine(layersPath, $"{LayerFileName}{layer.Index}");
                    SaveLayerBitmap(layer.Content, layerPath);
                }

                serializableLayers.Add(serializableLayer);
            }

            // Create serializable project
            var serializableProject = new SerializableProject
            {
                Name = project.Name,
                FilePath = project.FilePath,
                Width = project.Width,
                Height = project.Height,
                Layers = serializableLayers,
                Background = project.Background,
                ColorsList = [.. project.ColorsList]
            };

            // Serialize project data to JSON
            string jsonString = JsonSerializer.Serialize(serializableProject, _jsonOptions);
            File.WriteAllText(projectPath, jsonString);

            // Save thumbnail
            SaveThumbnail(project, thumbnailPath);
        }

        public static Project Deserialize(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("Project file not found.", path);

            string? directoryPath = Path.GetDirectoryName(path) ?? throw new InvalidOperationException("Invalid project path.");

            // Read and deserialize the JSON
            string jsonString = File.ReadAllText(path);
            var serializableProject = JsonSerializer.Deserialize<SerializableProject>(jsonString, _jsonOptions)
                ?? throw new InvalidOperationException("Failed to deserialize project file.");

            // Create new project instance
            Project project = new()
            {
                Name = serializableProject.Name,
                FilePath = serializableProject.FilePath,
                Width = serializableProject.Width,
                Height = serializableProject.Height,
                Background = serializableProject.Background,
                ColorsList = [.. serializableProject.ColorsList]
            };

            // Load layers
            var layers = new ObservableCollection<Layer>();
            string layersPath = Path.Combine(directoryPath, "layers");

            foreach (var serializableLayer in serializableProject.Layers)
            {
                var layer = new Layer(serializableLayer.Index, serializableProject.Width, serializableProject.Height)
                {
                    Index = serializableLayer.Index,
                    IsVisible = serializableLayer.IsVisible,
                };

                // Load layer bitmap if exists
                string layerPath = Path.Combine(layersPath, $"{LayerFileName}{serializableLayer.Index}");
                if (File.Exists(layerPath))
                {
                    layer.Content = LoadLayerBitmap(layerPath);
                }

                layers.Add(layer);
            }

            project.Layers = layers;

            // Load thumbnail if exists
            string thumbnailPath = Path.ChangeExtension(path, ThumbnailExtension);
            if (File.Exists(thumbnailPath))
            {
                project.Thumbnail = new BitmapImage();
                project.Thumbnail.BeginInit();
                project.Thumbnail.UriSource = new Uri(thumbnailPath, UriKind.Absolute);
                project.Thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                project.Thumbnail.EndInit();
                project.Thumbnail.Freeze();
            }

            return project;
        }

        private static void SaveLayerBitmap(WriteableBitmap bitmap, string path)
        {
            // Save the bitmap as PNG
            using FileStream stream = new(path, FileMode.Create);
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(stream);
        }

        private static WriteableBitmap LoadLayerBitmap(string path)
        {
            // Load the bitmap from PNG
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(path, UriKind.Absolute);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            // Convert BitmapImage to WriteableBitmap
            return new WriteableBitmap(bitmapImage);
        }

        public static void SaveThumbnail(Project project, string path)
        {
            // Create a render target bitmap with 1/10th the original width and height
            int thumbnailWidth = project.Width / 10;
            int thumbnailHeight = project.Height / 10;
            RenderTargetBitmap renderBitmap = new(thumbnailWidth, thumbnailHeight, 96, 96, PixelFormats.Pbgra32);

            // Create a drawing visual to compose the layers
            DrawingVisual drawingVisual = new();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                // Draw background
                dc.DrawRectangle(
                    new SolidColorBrush(Color.FromArgb(
                        project.Background.A,
                        project.Background.R,
                        project.Background.G,
                        project.Background.B)),
                    null,
                    new Rect(0, 0, thumbnailWidth, thumbnailHeight));

                // Draw each visible layer
                foreach (var layer in project.Layers)
                {
                    if (layer.IsVisible && layer.Content != null)
                    {
                        dc.DrawImage(layer.Content, new Rect(0, 0, thumbnailWidth, thumbnailHeight));
                        dc.Pop();
                    }
                }
            }

            // Render the composition
            renderBitmap.Render(drawingVisual);

            // Save as PNG
            using FileStream stream = new(path, FileMode.Create);
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            encoder.Save(stream);
        }
    }
    public class JsonColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string? hex = reader.GetString();
            return hex == null ? throw new JsonException("Expected a non-null string for Color.") : (Color)ColorConverter.ConvertFromString(hex);
        }

        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
        {
            string hex = value.ToString();
            writer.WriteStringValue(hex);
        }
    }

}