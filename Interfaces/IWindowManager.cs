using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMPaintApp.Interfaces
{
    public interface IWindowManager
    {
        void ShowWindow(ObservableObject viewModel);
        void CloseWindow(ObservableObject viewModel);
        void CloseAll();
    }
}
