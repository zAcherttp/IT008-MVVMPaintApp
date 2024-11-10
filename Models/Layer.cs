using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MVVMPaintApp.Commands;

namespace MVVMPaintApp.Models
{
    public partial class Layer(int index, int width, int height)
    {
        private WriteableBitmap _content = BitmapFactory.New(width, height);
        private bool _isVisible = true;
        private int _index = index;

        public WriteableBitmap Content
        {
            get => _content;
            set => _content = value;
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    var command = new LayerVisibilityCommand(this, _isVisible, value);
                    _isVisible = value;
                    UndoRedoManager?.AddCommand(command);
                }
            }
        }

        public int Index
        {
            get => _index;
            set => _index = value;
        }

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
