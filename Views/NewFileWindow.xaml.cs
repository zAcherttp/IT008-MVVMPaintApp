using MVVMPaintApp.ViewModels;
using System.Windows;

namespace MVVMPaintApp.Views
{
    /// <summary>
    /// Interaction logic for NewFileView.xaml
    /// </summary>
    public partial class NewFileWindow : Window
    {
        public NewFileWindow(NewFileViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        public NewFileWindow()
        {
            InitializeComponent();
        }
    }
}
