using MVVMPaintApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp.Interfaces
{
    public interface IProject
    {
        void LoadProject(string filePath);
        void SaveProject();
        void SaveProjectAs();
        string GetDefaultProjectName(string defaultFolder);
        void ExportProject(string outputPath);
        void GenerateThumbnail(BitmapImage toBitmap);
    }
}
