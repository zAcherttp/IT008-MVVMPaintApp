using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Models;
using MVVMPaintApp.UserControls;
using System.Windows;
using System.Windows.Controls;

namespace MVVMPaintApp.Services
{
    public class DialogService : IDialogService
    {
        private readonly UserControlMapper userControlMapper = new();

        public (bool dialogResult, T viewModel) ShowDialog<T>(T viewModel) where T : DialogViewModelBase
        {
            try
            {
                var userControlType = userControlMapper.GetViewType(viewModel.GetType());
                var content = Activator.CreateInstance(userControlType!) as UserControl;

                var owner = Application.Current.Windows.OfType<Window>().FirstOrDefault();

                var dialog = new DialogWindow
                {
                    Title = viewModel.Title,
                    DataContext = viewModel,
                    Content = content,
                    Owner = owner,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };

                viewModel.RequestClose += result =>
                {
                    dialog.DialogResult = result == MessageBoxResult.OK || result == MessageBoxResult.Yes;
                    dialog.Close();
                };

                var result = dialog.ShowDialog();
                return (result ?? false, viewModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Dialog creation failed: {ex.Message}");
                return (false, viewModel);
            }
        }
    }
}
