using MVVMPaintApp.Models;
using MVVMPaintApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MVVMPaintApp.UserControls;

namespace MVVMPaintApp.Views
{
    /// <summary>
    /// Interaction logic for MainCanvasView.xaml
    /// </summary>
    public partial class MainCanvasView : Window
    {
        public MainCanvasView(Project project)
        {
            InitializeComponent();

            this.WindowState = WindowState.Maximized;

            DrawingCanvasViewModel drawingCanvasViewModel = new DrawingCanvasViewModel(project);

            Canva.DataContext = drawingCanvasViewModel;


        }
    }
}
