using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace MVVMPaintApp.Models
{
    internal class ProjectPreset
    {
        private const int THUMBNAIL_HEIGHT = 100;

        private string presetName = "Preset";
        private int defaultWidth;
        private int defaultHeight;
        private string aspectRatioString = "W : H";
        private double aspectRatioW;
        private double aspectRatioH;

        public string PresetName { get => presetName; set => presetName = value; }
        public double ThumbnailWidth {
            get
            {
                return (double) AspectRatioW / AspectRatioH * THUMBNAIL_HEIGHT;
            }
        }
        public string AspectRatioString { get => aspectRatioString; set => aspectRatioString = value; }
        public double AspectRatioW { get => aspectRatioW; set => aspectRatioW = value; }
        public double AspectRatioH { get => aspectRatioH; set => aspectRatioH = value; }
        public int DefaultWidth { get => defaultWidth; set => defaultWidth = value; }
        public int DefaultHeight { get => defaultHeight; set => defaultHeight = value; }

        public ProjectPreset(string name, string aspectRatio, int defaultWidth, int defaultHeight)
        {
            PresetName = name;

            DefaultWidth = defaultWidth;
            DefaultHeight = defaultHeight;

            AspectRatioString = aspectRatio;
            string[] aspectRatioParts = aspectRatio.Split(':');
            AspectRatioW = double.Parse(aspectRatioParts[0]);
            AspectRatioH = double.Parse(aspectRatioParts[1]);
        }
    }
}
