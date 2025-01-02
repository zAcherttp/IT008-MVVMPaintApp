using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Models;
using System.Windows;

namespace MVVMPaintApp.ViewModels
{
    public partial class ResizeViewModel : DialogViewModelBase
    {
        private const string title = "Resize";

        [ObservableProperty]
        private int width;

        [ObservableProperty]
        private int height;

        [ObservableProperty]
        private bool isPixels;

        public ResizeViewModel(int currentWidth, int currentHeight) : base(title)
        {
            Width = currentWidth;
            Height = currentHeight;
            IsPixels = true;
        }

        [RelayCommand]
        private void Ok() => CloseDialog(MessageBoxResult.OK);

        [RelayCommand]
        private void Cancel() => CloseDialog(MessageBoxResult.Cancel);
    }
}
