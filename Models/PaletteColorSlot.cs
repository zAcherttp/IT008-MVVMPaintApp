using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MVVMPaintApp.Models
{
    public partial class PaletteColorSlot : ObservableObject
    {
        [ObservableProperty]
        private bool isEmpty;

        [ObservableProperty]
        private Color color;

        public PaletteColorSlot()
        {
            SetColor(Colors.Transparent);
        }

        public PaletteColorSlot(Color initialColor)
        {
            SetColor(initialColor);
        }

        public void SetColor(Color newColor)
        {
            Color = newColor;
            if(newColor.Equals(Colors.Transparent))
                IsEmpty = true;
            else
                IsEmpty = false;
        }
    }
}
