using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace MVVMPaintApp.Models
{
    public partial class ProjectPreset : ObservableObject
    {
        private const int THUMBNAIL_HEIGHT = 100;

        [ObservableProperty]
        private string presetName = "Preset";

        [ObservableProperty]
        private int width;

        [ObservableProperty]
        private int height;
        
        [ObservableProperty]
        private string aspectRatioString = "W : H";

        [ObservableProperty]
        private double aspectRatioW;

        [ObservableProperty]
        private double aspectRatioH;

        public double ThumbnailWidth
        {
            get
            {
                return (double)AspectRatioW / AspectRatioH * THUMBNAIL_HEIGHT;
            }
        }

        public ProjectPreset(string name, string aspectRatio, int width, int height)
        {
            PresetName = name;

            Width = width;
            Height = height;

            AspectRatioString = aspectRatio;
            string[] aspectRatioParts = aspectRatio.Split(':');
            AspectRatioW = double.Parse(aspectRatioParts[0]);
            AspectRatioH = double.Parse(aspectRatioParts[1]);
        }
    }
}
