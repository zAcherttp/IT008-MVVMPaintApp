using MVVMPaintApp.Models;
using System.Windows.Forms;

namespace MVVMPaintApp.Interfaces
{
    public interface IDialogService
    {
        (bool dialogResult, T viewModel) ShowDialog<T>(T viewModel) where T : DialogViewModelBase;
    }
}
