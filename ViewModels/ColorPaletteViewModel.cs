using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Input;
using MVVMPaintApp.Commands;
using MVVMPaintApp.UserControls;
using MVVMPaintApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics.Eventing.Reader;
using System.DirectoryServices;
using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Services;

namespace MVVMPaintApp.ViewModels
{
    public partial class ColorPaletteViewModel : ObservableObject
    {
        private readonly ViewModelLocator viewModelLocator;

        #region Constants
        private const int DEFAULT_PALETTE_ROWS = 3;
        private const int DEFAULT_PALETTE_COLUMNS = 9;
        private const int CUSTOM_PALETTE_START_OFFSET_INDEX = 9;
        private const int MAX_CUSTOM_COLORS = 18;
        #endregion

        [ObservableProperty]
        private Color primaryColor = Colors.White;

        [ObservableProperty]
        private Color secondaryColor = Colors.Black;

        [ObservableProperty]
        private ObservableCollection<PaletteColorSlot> paletteColors = [];

        [ObservableProperty]
        private Color paletteButtonColor = Colors.Transparent;

        [ObservableProperty]
        private Color colorPickerColor;

        [ObservableProperty]
        private bool isColorPickerOpen;

        private int nextColorButtonIndex;
        private bool isPrimaryColorSelected;

        [RelayCommand]
        private void ToggleColorPicker() => IsColorPickerOpen ^= true;

        [RelayCommand]
        private void AddPickedColorToPalette()
        {
            AddColorToPalette(ColorPickerColor);
        }

        [RelayCommand]
        private void SetPrimaryColor() => isPrimaryColorSelected = true;

        [RelayCommand]
        private void SetSecondaryColor() => isPrimaryColorSelected = false;

        [RelayCommand]
        private void PaletteButtonRightClick(object? param)
        {
            if (param is not PaletteColorSlot slot) return;
            if (isPrimaryColorSelected)
                SecondaryColor = slot.Color;
            else
                PrimaryColor = slot.Color;
        }

        [RelayCommand]
        private void PaletteButtonLeftClick(object? param)
        {
            if (param is not PaletteColorSlot slot) return;
            if (isPrimaryColorSelected)
                PrimaryColor = slot.Color;
            else
                SecondaryColor = slot.Color;
        }

        [RelayCommand]
        private void OnPaletteButtonMouseEnter(object? param)
        {
            if (param is not PaletteColorSlot slot) return;
            PaletteButtonColor = slot.Color;
        }

        [RelayCommand]
        private void OnPaletteButtonMouseLeave()
        {
            PaletteButtonColor = PrimaryColor;
        }

        public ColorPaletteViewModel(ViewModelLocator viewModelLocator)
        {
            this.viewModelLocator = viewModelLocator;
            paletteColors = [];
            CreateEmptyPalette();
        }

        public void SetProjectColors(List<Color> colorsList)
        {
            PopulatePaletteWithDefaultColors();
            if(colorsList.Count != 0)
            {
                foreach (Color color in colorsList)
                {
                    AddColorToPalette(color);
                }
            }
        }

        private void CreateEmptyPalette()
        {
            PaletteColors.Clear();
            int totalSlots = DEFAULT_PALETTE_ROWS * DEFAULT_PALETTE_COLUMNS;
            for (int i = 0; i < totalSlots; i++)
            {
                PaletteColors.Add(new PaletteColorSlot());
            }
        }

        private void PopulatePaletteWithDefaultColors()
        {
            Color[] defaults =
            [
                Colors.Black, Colors.Gray, Colors.DarkRed,
                Colors.Red, Colors.Orange, Colors.Yellow,
                Colors.Green, Colors.Blue, Colors.Purple
            ];

            for (int i = 0; i < defaults.Length; i++)
            {
                PaletteColors[i].SetColor(defaults[i]);
            }

            nextColorButtonIndex = 0;
        }

        private void AddColorToPalette(Color newColor)
        {
            if (PaletteColors.Any(slot => slot.Color.Equals(newColor))) return;  
            PaletteColors[GetNextCustomColorIndex()].SetColor(newColor);
        }

        private int GetNextCustomColorIndex()
        {
            nextColorButtonIndex++;
            return CUSTOM_PALETTE_START_OFFSET_INDEX + ((nextColorButtonIndex - 1) % MAX_CUSTOM_COLORS);
        }
    }
}