using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Models;
using System.Windows;

namespace MVVMPaintApp.ViewModels
{
    public partial class SaveChangesViewModel : DialogViewModelBase
    {
        private const string title = "Unsaved Changes";

        [ObservableProperty]
        private MessageBoxResult dialogResult;

        public SaveChangesViewModel() : base(title) { }

        [RelayCommand]
        private void Yes()
        {
            DialogResult = MessageBoxResult.Yes;
            CloseDialog(MessageBoxResult.Yes);
        }

        [RelayCommand]
        private void No()
        {
            DialogResult = MessageBoxResult.No;
            CloseDialog(MessageBoxResult.No);
        }

        [RelayCommand]
        private void Cancel()
        {
            DialogResult = MessageBoxResult.Cancel;
            CloseDialog(MessageBoxResult.Cancel);
        }
    }
}
