using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Input;
using System;
using System.Linq;
using MVVMPaintApp.Commands;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using MVVMPaintApp.UserControls;
using System.Runtime.CompilerServices;

namespace MVVMPaintApp.ViewModels
{
    public class ColorPaletteViewModel : INotifyPropertyChanged
    {
        #region Constants
        private const int DEFAULT_PALETTE_ROWS = 3;
        private const int DEFAULT_PALETTE_COLUMNS = 9;
        private const int CUSTOM_COLORS_START_OFFSET_INDEX = 9;
        private const int MAX_CUSTOM_COLORS = 18;
        #endregion

        #region Private Fields
        private readonly ColorPickerControl _colorPicker;
        private Color _primaryColor = Colors.White;
        private Color _secondaryColor = Colors.Black;
        private int _nextCustomColorIndex;
        private bool _isColorPickerOpen;
        private bool _isPrimarySelected = true;
        #endregion

        #region Public Properties
        public Color PrimaryColor
        {
            get => _primaryColor;
            set
            {
                _primaryColor = value;
                OnPropertyChanged(nameof(PrimaryColor));
            }
        }

        public Color SecondaryColor
        {
            get => _secondaryColor;
            set
            {
                _secondaryColor = value;
                OnPropertyChanged(nameof(SecondaryColor));
            }
        }

        public bool IsColorPickerOpen
        {
            get => _isColorPickerOpen;
            set
            {
                _isColorPickerOpen = value;
                OnPropertyChanged(nameof(IsColorPickerOpen));
            }
        }

        public ObservableCollection<PaletteColorSlot> ColorsList { get; }

        public ICommand ToggleColorPickerCommand { get; }
        public ICommand AddColorCommand { get; }
        public ICommand ColorLeftClickCommand { get; }
        public ICommand ColorRightClickCommand { get; }
        #endregion

        #region Constructor
        public ColorPaletteViewModel(ColorPickerControl colorPicker)
        {
            _colorPicker = colorPicker;
            ColorsList = [];

            ToggleColorPickerCommand = new RelayCommand(_ => ToggleColorPicker());
            AddColorCommand = new RelayCommand(_ => AddSelectedColorToPalette());
            ColorLeftClickCommand = new RelayCommand(HandleColorLeftClick);
            ColorRightClickCommand = new RelayCommand(HandleColorRightClick);
            InitializePalette();
        }
        #endregion

        #region Commands
        private void ToggleColorPicker() =>
            IsColorPickerOpen ^= true;

        private void AddSelectedColorToPalette() =>
            AddColorToPalette(_colorPicker.SelectedColor);
        #endregion

        #region Color Selection Handlers
        private void HandleColorLeftClick(object? parameter)
        {
            if (parameter is not PaletteColorSlot colorSlot) return;

            if (_isPrimarySelected)
                PrimaryColor = colorSlot.Color;
            else
                SecondaryColor = colorSlot.Color;
        }

        private void HandleColorRightClick(object? parameter)
        {
            if (parameter is not PaletteColorSlot colorSlot) return;

            if (_isPrimarySelected)
                SecondaryColor = colorSlot.Color;
            else
                PrimaryColor = colorSlot.Color;
        }

        public void SetActiveColor(bool isPrimary)
        {
            _isPrimarySelected = isPrimary;
        }
        #endregion

        #region Palette Management
        private void InitializePalette()
        {
            CreateEmptyPalette();
            PopulateDefaultColors();
        }

        private void CreateEmptyPalette()
        {
            ColorsList.Clear();
            int totalSlots = DEFAULT_PALETTE_ROWS * DEFAULT_PALETTE_COLUMNS;

            for (int i = 0; i < totalSlots; i++)
            {
                ColorsList.Add(new PaletteColorSlot());
            }
        }

        private void PopulateDefaultColors()
        {
            Color[] defaultColors = GetDefaultColors();

            for (int i = 0; i < defaultColors.Length; i++)
            {
                ColorsList[i].SetColor(defaultColors[i]);
            }

            _nextCustomColorIndex = 0;
        }

        private static Color[] GetDefaultColors() =>
        [
            Colors.Black,
            Colors.Gray,
            Colors.DarkRed,
            Colors.Red,
            Colors.Orange,
            Colors.Yellow,
            Colors.Green,
            Colors.Blue,
            Colors.Purple
        ];

        private void AddColorToPalette(Color color)
        {
            if (ColorAlreadyExists(color)) return;

            ColorsList[GetNextCustomColorIndex()].SetColor(color);
        }

        private bool ColorAlreadyExists(Color color) =>
            ColorsList.Any(slot => slot.Color.Equals(color));

        private int GetNextCustomColorIndex()
        { 
            int index = CUSTOM_COLORS_START_OFFSET_INDEX + (_nextCustomColorIndex % MAX_CUSTOM_COLORS);
            _nextCustomColorIndex++;
            return index;
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