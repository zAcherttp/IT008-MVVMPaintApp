using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MVVMPaintApp.Models
{
    public class PaletteColorSlot : INotifyPropertyChanged
    {
        #region Private Fields
        private bool _isEmpty = true;
        private Color _color = Colors.Transparent;
        #endregion

        #region Public Properties
        public bool IsEmpty
        {
            get => _isEmpty;
            private set
            {
                _isEmpty = value;
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        public Color Color
        {
            get => _color;
            private set
            {
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }
        #endregion

        #region Constructors
        public PaletteColorSlot()
        {
        }

        public PaletteColorSlot(Color initialColor)
        {
            SetColor(initialColor);
        }
        #endregion

        #region Method
        public void SetColor(Color newColor)
        {
            Color = newColor;
            IsEmpty = false;
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string? propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
