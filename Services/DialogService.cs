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
            var userControlType = userControlMapper.GetViewType(viewModel.GetType());
            var dialog = new DialogWindow
            {
                Title = viewModel.Title,
                DataContext = viewModel,
                Content = Activator.CreateInstance(userControlType!) as UserControl,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            viewModel.RequestClose += result =>
            {
                dialog.DialogResult = result == MessageBoxResult.OK || result == MessageBoxResult.Yes;
                dialog.Close();
            };

            var result = dialog.ShowDialog();
            return (result ?? false, viewModel);
        }
    }
}
