using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using MVVMPaintApp.Commands;
using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Services;

namespace MVVMPaintApp.Models
{
    public partial class Layer(int index, int width, int height) : ObservableObject
    {
        [ObservableProperty]
        private WriteableBitmap content = BitmapFactory.New(width, height);

        [ObservableProperty]
        private bool isVisible = true;

        [ObservableProperty]
        private int index = index;

        [ObservableProperty]
        private int width = width;

        [ObservableProperty]
        private int height = height;

        public UndoRedoManager? UndoRedoManager { get; set; }

        // Helper method to track bitmap changes
        public void TrackBitmapChange(Int32Rect region, byte[] beforePixels, byte[] afterPixels)
        {
            if (Content != null && UndoRedoManager != null)
            {
                var command = new BitmapChangeCommand(Content, region, beforePixels, afterPixels);
                UndoRedoManager.AddCommand(command);
            }
        }
    }
}
