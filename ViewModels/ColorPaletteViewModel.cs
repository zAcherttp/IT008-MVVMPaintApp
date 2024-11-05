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

namespace MVVMPaintApp.ViewModels
{
    public class ColorPaletteViewModel : INotifyPropertyChanged
    {
        private Color _PrimaryColor = Colors.Black;
        private Color _SecondaryColor = Colors.White;
        private int _ColorsListIndex = 0;
        private bool _IsColorPickerOpen = false;

        private ColorPickerControl _colorPickerControl;

        public Color PrimaryColor
        {
            get => _PrimaryColor;
            set
            {
                _PrimaryColor = value;
                OnPropertyChanged(nameof(PrimaryColor));
            }
        }

        public Color SecondaryColor
        {
            get => _SecondaryColor;
            set
            {
                _SecondaryColor = value;
                OnPropertyChanged(nameof(SecondaryColor));
            }
        }

        public bool IsColorPickerOpen
        {
            get => _IsColorPickerOpen;
            set
            {
                _IsColorPickerOpen = value;
                OnPropertyChanged(nameof(IsColorPickerOpen));
            }
        }

        public ObservableCollection<ColorSlot> ColorsList { get; }

        public ICommand ToggleColorPickerCommand { get; }
        public ICommand AddColorCommand { get; }
        public ICommand ColorClickCommand { get; }
        public ICommand ColorRightClickCommand { get; }

        public ColorPaletteViewModel(ref ColorPickerControl colorPickerControl)
        {
            ColorsList = [];
            InitializeColorSlots();

            _colorPickerControl = colorPickerControl;
            ToggleColorPickerCommand = new RelayCommand(_ => IsColorPickerOpen ^= true);
            AddColorCommand = new RelayCommand(_ => AddColorToList(_colorPickerControl.SelectedColor));
            ColorClickCommand = new RelayCommand(ExecuteColorClick);
            ColorRightClickCommand = new RelayCommand(ExecuteColorRightClick);
        }

        private void ExecuteColorClick(object? parameter)
        {
            
            if (parameter is ColorSlot colorSlot)
            {
                if (IsPrimarySelected)
                    PrimaryColor = colorSlot.Color;
                else if (IsSecondarySelected)
                    SecondaryColor = colorSlot.Color;
            }
        }

        private void ExecuteColorRightClick(object? parameter)
        {
            if (parameter is ColorSlot colorSlot)
            {
                if (IsPrimarySelected)
                    SecondaryColor = colorSlot.Color;
                else if (IsSecondarySelected)
                    PrimaryColor = colorSlot.Color;
            }
        }

        private bool IsPrimarySelected { get; set; } = true;
        private bool IsSecondarySelected { get; set; }

        public void SetActiveColor(bool isPrimary)
        {
            IsPrimarySelected = isPrimary;
            IsSecondarySelected = !isPrimary;
        }

        private void AddColorToList(Color color)
        {
            if (ColorsList.Any(x => x.Color == color))
                return;

            ColorsList[_ColorsListIndex++ % 18 + 9].Color = color;
        }

        private void InitializeColorSlots()
        {
            ColorsList.Clear();

            // Create 27 slots (3 rows x 9 columns)
            for (int i = 0; i < 27; i++)
            {
                ColorsList.Add(new ColorSlot());
            }

            // Add default colors
            var defaultColors = new[]
            {
                Colors.Black, Colors.Gray, Colors.DarkRed, Colors.Red,
                Colors.Orange, Colors.Yellow, Colors.Green, Colors.Blue, Colors.Purple
            };

            for (int i = 0; i < defaultColors.Length; i++)
            {
                ColorsList[i].Color = defaultColors[i];
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ColorSlot : INotifyPropertyChanged
    {
        private bool isEmpty = true;
        private Color color = Colors.Transparent;

        public ColorSlot()
        {
        }

        public ColorSlot(Color color)
        {
            Color = color;
            IsEmpty = false;
        }

        public bool IsEmpty
        {
            get => isEmpty;
            set
            {
                isEmpty = value;
                OnPropertyChanged(nameof(IsEmpty));
            }
        }

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                IsEmpty = false;
                OnPropertyChanged(nameof(Color));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}