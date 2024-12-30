using System.Windows;
using MVVMPaintApp.ViewModels;

namespace MVVMPaintApp.Views
{
    /// <summary>
    /// Interaction logic for ResizeDialog.xaml
    /// </summary>
    public partial class ResizeDialog : Window
    {
        public ResizeDialog(int currentWidth, int currentHeight)
        {
            InitializeComponent();
            DataContext = new ResizeDialogViewModel(currentWidth, currentHeight);
            ((ResizeDialogViewModel)DataContext).RequestClose += (result) =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}
