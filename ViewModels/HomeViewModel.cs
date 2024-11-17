using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Models;

namespace MVVMPaintApp.ViewModels
{
    internal partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<Project> recentProjects;

        public HomeViewModel()
        {
            //to do: check default folder for available projects
            recentProjects = [];
            LoadDummyProjects();
        }

        private void LoadDummyProjects()
        {
            var dummyBitmap = new BitmapImage();
            try
            {
                dummyBitmap.BeginInit();
                dummyBitmap.UriSource = new Uri("pack://application:,,,/Resources/placeholder.png");
                dummyBitmap.EndInit();
            }
            catch { }

            void AddDummyProject(string name, int width, int height)
            {
                var project = new Project
                {
                    Name = name,
                    FilePath = $"C:\\Users\\User\\Documents\\MyPaint\\{name}",
                    Width = width,
                    Height = height,
                    Layers = [new Layer(0, width, height)],
                    Background = Colors.White,
                    ColorsList = []
                };
                // Generate the thumbnail from this bitmap
                project.GenerateThumbnail(dummyBitmap);

                RecentProjects.Add(project);
            }

            AddDummyProject("project1.ptd", 800, 600);
            AddDummyProject("project2.ptd", 1920, 1080);
            AddDummyProject("project3.ptd", 2100, 900);
            AddDummyProject("project4.ptd", 300, 400);
            AddDummyProject("project5.ptd", 800, 800);
            AddDummyProject("project6.ptd", 800, 600);
            AddDummyProject("project1.ptd", 800, 600);
            AddDummyProject("project2.ptd", 1920, 1080);
            AddDummyProject("project3.ptd", 2100, 900);
            AddDummyProject("project4.ptd", 300, 400);
            AddDummyProject("project5.ptd", 800, 800);
            AddDummyProject("project6.ptd", 800, 600);
            AddDummyProject("project1.ptd", 800, 600);
            AddDummyProject("project2.ptd", 1920, 1080);
            AddDummyProject("project3.ptd", 2100, 900);
            AddDummyProject("project4.ptd", 300, 400);
            AddDummyProject("project5.ptd", 800, 800);
            AddDummyProject("project6.ptd", 800, 600);
        }
    }
}
