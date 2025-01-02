using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Models;
using System.Windows;

namespace MVVMPaintApp.ViewModels
{
    public partial class AboutViewModel : DialogViewModelBase
    {
        private const string title = "About";

        [ObservableProperty]
        private string version = "v0.6.1";

        [ObservableProperty]
        private string appName = "My Paint App";

        public AboutViewModel() : base(title) { }

        [RelayCommand]
        private void Cancel()
        {
            CloseDialog(MessageBoxResult.Cancel);
        }
    }
}
