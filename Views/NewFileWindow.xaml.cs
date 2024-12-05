using MVVMPaintApp.ViewModels;
using System.Windows;

namespace MVVMPaintApp.Views
{
    /// <summary>
    /// Interaction logic for NewFileView.xaml
    /// </summary>
    public partial class NewFileWindow : Window
    {
        public NewFileWindow()
        {
            InitializeComponent();
        }

        public NewFileWindow(NewFileViewModel newFileViewModel)
        {
            InitializeComponent();
            DataContext = newFileViewModel;
        }
    }
}
