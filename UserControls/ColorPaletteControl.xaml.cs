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

        private void OnPrimaryColorChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is ColorPaletteViewModel vm)
            {
                vm.SetActiveColor(true);
            }
        }

        private void OnSecondaryColorChecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is ColorPaletteViewModel vm)
            {
                vm.SetActiveColor(false);
            }
        }

        private void OnColorButtonMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Button button && button.DataContext is PaletteColorSlot slot)
            {
                //Debug.WriteLine($"Hovered Color: {slot.Color}");

                var viewModel = (ColorPaletteViewModel)DataContext;
                if (viewModel != null)
                {
                    viewModel.PaletteButtonColor = slot.Color;
                }
                //else
                //{
                //    Debug.WriteLine("ViewModel is null.");
                //}
            }
            //else
            //{
            //    Debug.WriteLine("Button DataContext is not a PaletteColorSlot.");
            //}

        }

        private void OnColorButtonMouseLeave(object sender, MouseEventArgs e)
        {
            var viewModel = (ColorPaletteViewModel)DataContext;
            if (viewModel != null)
            {
                viewModel.PaletteButtonColor = viewModel.PrimaryColor;
            }
            //else
            //{
            //    Debug.WriteLine("ViewModel is null.");
            //}
        }
    }
}
