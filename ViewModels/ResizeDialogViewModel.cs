using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Models;

namespace MVVMPaintApp.ViewModels
{
    public partial class ResizeDialogViewModel : DialogViewModelBase
    {
        [ObservableProperty]
        private int width;

        [ObservableProperty]
        private int height;

        [ObservableProperty]
        private bool isPixels;

        public ResizeDialogViewModel(int currentWidth, int currentHeight)
        {
            Width = currentWidth;
            Height = currentHeight;
            IsPixels = true;
        }

        [RelayCommand]
        private void Ok()
        {
            OnRequestClose(true);
        }

        [RelayCommand]
        private void Cancel()
        {
            OnRequestClose(false);
        }
    }
}
