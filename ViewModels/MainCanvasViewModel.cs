using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MVVMPaintApp.ViewModels
{
    partial class MainCanvasViewModel : ObservableObject
    {
        [ObservableProperty]
        private int viewPortWidth = 1600;

        [ObservableProperty]
        private int viewPortHeight = 900;
    }
}
