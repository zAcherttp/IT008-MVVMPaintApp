using MVVMPaintApp.ViewModels;
using System.Windows;
using System.Windows.Controls;

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

        private void Dimensions_PreviewTextInput(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (string.IsNullOrEmpty(tb.Text))
                {
                    tb.Text = "100";
                }
                else if (int.TryParse(tb.Text, out int value))
                {
                    if (value > 7680)
                    {
                        tb.Text = "7680";
                    }
                }
                else
                {
                    tb.Text = "100";
                }
            }
        }

        private void AspectRatioDimensions_PreviewTextInput(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (string.IsNullOrEmpty(tb.Text))
                {
                    tb.Text = "1";
                }
                else if (int.TryParse(tb.Text, out int value))
                {
                    if (value > 100)
                    {
                        tb.Text = "100";
                    }
                }
                else
                {
                    tb.Text = "1";
                }
            }
        }
    }
}
