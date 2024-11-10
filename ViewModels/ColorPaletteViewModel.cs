using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Input;
using MVVMPaintApp.Commands;
using MVVMPaintApp.UserControls;
using MVVMPaintApp.Models;

namespace MVVMPaintApp.ViewModels
{
    public class ColorPaletteViewModel : INotifyPropertyChanged
    {
        #region Constants
        private const int DEFAULT_PALETTE_ROWS = 3;
        private const int DEFAULT_PALETTE_COLUMNS = 9;
        private const int CUSTOM_PALETTE_START_OFFSET_INDEX = 9;
        private const int MAX_CUSTOM_COLORS = 18;
        #endregion

        #region Private Fields
        private Color _primaryColor = Colors.White;
        private Color _secondaryColor = Colors.Black;
        private Color _colorPickerColor;
        private Color _paletteButtonColor = Colors.Green;
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

        public Color PaletteButtonColor
        {
            get => _paletteButtonColor;
            set
            {
                _paletteButtonColor = value;
                OnPropertyChanged(nameof(PaletteButtonColor));
            }
        }

        public Color ColorPickerColor
        {
            get => _colorPickerColor;
            set
            {
                _colorPickerColor = value;
                OnPropertyChanged(nameof(ColorPickerColor));
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
        public ColorPaletteViewModel( )
        {
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
            AddColorToPalette(_colorPickerColor);
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

        public void SetPreviewColor(Color? color)
        {
            if (color.HasValue)
            {
                _paletteButtonColor = color.Value;
            }
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

        /// <summary>
        /// To be implemented with ProjectManager class
        /// </summary>
        /// <returns></returns>
        //private void SaveCustomPalette()
        //{
        //}

        //private void LoadCustomPalette()
        //{
        //}

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
            int index = CUSTOM_PALETTE_START_OFFSET_INDEX + (_nextCustomColorIndex % MAX_CUSTOM_COLORS);
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
}