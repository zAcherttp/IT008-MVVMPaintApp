using System.Collections.ObjectModel;
using System.Windows.Media;
using MVVMPaintApp.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Services;

namespace MVVMPaintApp.ViewModels
{
    public partial class ColorPaletteViewModel : ObservableObject
    {
        #region Constants
        private const int DEFAULT_PALETTE_ROWS = 3;
        private const int DEFAULT_PALETTE_COLUMNS = 9;
        private const int CUSTOM_PALETTE_START_OFFSET_INDEX = 9;
        private const int MAX_CUSTOM_COLORS = 18;
        #endregion

        [ObservableProperty]
        private ProjectManager projectManager;

        [ObservableProperty]
        private ObservableCollection<PaletteColorSlot> paletteColors = [];

        [ObservableProperty]
        private Color paletteButtonColor = Colors.Transparent;

        [ObservableProperty]
        private Color primaryColor = Colors.Black;

        [ObservableProperty]
        private Color secondaryColor = Colors.White;

        [ObservableProperty]
        private Color colorPickerColor;

        [ObservableProperty]
        private bool isColorPickerOpen;

        private int nextColorButtonIndex = 0;
        private bool isPrimaryColorSelected = true;

        [RelayCommand]
        private void ToggleColorPicker() => IsColorPickerOpen ^= true;

        [RelayCommand]
        private void AddPickedColorToPalette()
        {
            AddColorToPalette(ColorPickerColor);
        }

        [RelayCommand]
        private void SetPrimaryColor()
        {
            isPrimaryColorSelected = ProjectManager.IsPrimaryColorSelected = true;
        }

        [RelayCommand]
        private void SetSecondaryColor()
        {
            isPrimaryColorSelected = ProjectManager.IsPrimaryColorSelected = false;
        }

        [RelayCommand]
        private void PaletteButtonRightClick(object? param)
        {
            if (param is not PaletteColorSlot slot) return;
            if (isPrimaryColorSelected)
                ProjectManager.SecondaryColor = SecondaryColor = slot.Color;
            else
                ProjectManager.PrimaryColor = PrimaryColor = slot.Color;
        }

        [RelayCommand]
        private void PaletteButtonLeftClick(object? param)
        {
            if (param is not PaletteColorSlot slot) return;
            if(isPrimaryColorSelected)
                ProjectManager.PrimaryColor = PrimaryColor = slot.Color;
            else
                ProjectManager.SecondaryColor = SecondaryColor = slot.Color;
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
            PaletteButtonColor = ProjectManager.PrimaryColor;
        }

        public ColorPaletteViewModel(ProjectManager projectManager)
        {
            this.projectManager = projectManager;
            CreateEmptyPalette();
        }

        public void SetProjectColors(List<Color> colorsList)
        {
            PopulatePaletteWithDefaultColors();
            int startIndexOffset = 0;
            for (int i = 0; i < colorsList.Count; i++)
            {
                PaletteColors[CUSTOM_PALETTE_START_OFFSET_INDEX + i].SetColor(colorsList[i]);
                if (!colorsList[i].Equals(Colors.Transparent))
                {
                    startIndexOffset++;
                }
            }
            nextColorButtonIndex = startIndexOffset;
        }

        private void CreateEmptyPalette()
        {
            PaletteColors = [];
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
        }

        private void AddColorToPalette(Color newColor)
        {
            if (PaletteColors.Any(slot => slot.Color.Equals(newColor))) return;
            int index = GetNextCustomColorIndex();
            PaletteColors[index].SetColor(newColor);
            ProjectManager.SetColorListColorAtIndex(index - 9, newColor);
        }

        private int GetNextCustomColorIndex()
        {
            nextColorButtonIndex++;
            return CUSTOM_PALETTE_START_OFFSET_INDEX + ((nextColorButtonIndex - 1) % MAX_CUSTOM_COLORS);
        }
    }
}