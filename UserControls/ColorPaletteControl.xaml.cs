using MVVMPaintApp.ViewModels;
using MVVMPaintApp.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;

namespace MVVMPaintApp.UserControls
{
    /// <summary>
    /// Interaction logic for ColorPaletteControl.xaml
    /// </summary>
    public partial class ColorPaletteControl : UserControl
    {
        public ColorPaletteControl()
        {
            InitializeComponent();
        }

        public ColorPaletteControl(ColorPaletteViewModel colorPaletteViewModel)
        {
            InitializeComponent();
            DataContext = colorPaletteViewModel;
        }
    }
}
