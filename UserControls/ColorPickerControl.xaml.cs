using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static MVVMPaintApp.Models.ColorHelper;

namespace MVVMPaintApp.UserControls
{
    /// <summary>
    /// Interaction logic for ColorPickerControl.xaml
    /// </summary>
    public partial class ColorPickerControl : UserControl
    {
        #region Public Properties
        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public Color CurrentSpectrumColor
        {
            get => (Color)GetValue(CurrentSpectrumColorProperty);
            set => SetValue(CurrentSpectrumColorProperty, value);
        }

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(
                nameof(SelectedColor),
                typeof(Color),
                typeof(ColorPickerControl),
                new FrameworkPropertyMetadata(
                    Colors.White,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedColorChanged));

        public static readonly DependencyProperty CurrentSpectrumColorProperty =
            DependencyProperty.Register(
                nameof(CurrentSpectrumColor),
                typeof(Color),
                typeof(ColorPickerControl),
                new PropertyMetadata(Colors.Transparent));

        public event EventHandler<Color>? ColorChanged;
        #endregion

        #region Private Fields
        private bool _isDragging;
        private bool _isMouseCaptured;
        private Color _currentColor;

        private const double BlackToColorPoint = 0.33;
        private const double ColorToWhitePoint = 0.66;
        #endregion

        #region Constructor
        public ColorPickerControl()
        {
            InitializeComponent();
            AlphaSlider.Value = 255;
            UpdateControls();
        }
        #endregion

        #region Event Handlers
        private void ColorSpectrum_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _isMouseCaptured = ColorSpectrum.CaptureMouse();
            Cursor = Cursors.None;
            UpdateColorFromSpectrum(e.GetPosition(ColorSpectrum));
        }

        private void ColorSpectrum_MouseMove(object sender, MouseEventArgs e)
        {
            if (!HandleSpectrumMouseMove(e)) return;
            UpdateColorFromSpectrum(e.GetPosition(ColorSpectrum));
        }

        private void ColorSpectrum_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            ColorChanged?.Invoke(this, SelectedColor);
        }

        private void ColorSpectrum_LostMouseCapture(object sender, MouseEventArgs e)
        {
            ReleaseMouseCapture();
        }

        private void ColorValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            UpdateColorFromRgbTextBoxes();
            UpdateTextBoxes();
            ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void ColorValue_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateColorFromRgbTextBoxes();
        }

        private void HexValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            UpdateColorFromHexTextBox();
            ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void HexValue_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateColorFromHexTextBox();
        }

        private void ColorPicker_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateColorFromSpectrum(new Point(0, ColorSpectrum.ActualHeight));
        }

        private void AlphaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _currentColor.A = (byte)AlphaSlider.Value;
            UpdateControls();
            SelectedColor = _currentColor;
        }
        #endregion

        #region UI Updates
        private void UpdateControls()
        {
            var brush = new SolidColorBrush(_currentColor);
            ColorPreview.Fill = brush;
            ColorSelector.Fill = brush;
            ColorChanged?.Invoke(this, _currentColor);
        }

        private void UpdateTextBoxes()
        {
            try
            {
                RedTextBox.Text = _currentColor.R.ToString();
                GreenTextBox.Text = _currentColor.G.ToString();
                BlueTextBox.Text = _currentColor.B.ToString();
                HexTextBox.Text = $"#{_currentColor.R:X2}{_currentColor.G:X2}{_currentColor.B:X2}";
            }
            catch
            {
                // Maintain current values if update fails
            }
        }

        public void UpdateColorSelectorPosition()
        {
            if (_isDragging) return;

            double hueScale = GetHue(_currentColor) / 360.0;
            double x = hueScale * ColorSpectrum.ActualWidth;

            Color baseColor = GetRainbowColor(Math.Min(hueScale, 0.9999));
            double brightness = GetRelativeBrightness(_currentColor, baseColor);

            double y = CalculateYPosition(brightness);

            UpdateSelectorPosition(x, y);
        }

        private double CalculateYPosition(double brightness)
        {
            double y = brightness switch
            {
                < 0 => (brightness + 1) * BlackToColorPoint,
                > 0 => ColorToWhitePoint + (brightness * (1 - ColorToWhitePoint)),
                _ => (BlackToColorPoint + ColorToWhitePoint) / 2
            };

            return y * ColorSpectrum.ActualHeight;
        }

        private void UpdateSelectorPosition(double x, double y)
        {
            double halfSize = ColorSelector.Width / 2;
            Canvas.SetLeft(ColorSelector, x - halfSize);
            Canvas.SetTop(ColorSelector, y - halfSize);
        }
        #endregion

        #region Color Updates
        private void UpdateColorFromSpectrum(Point position)
        {
            var (x, y) = ClampPosition(position);
            UpdateSelectorPosition(x, y);

            double huePosition = Math.Min(x / ColorSpectrum.ActualWidth, 0.9999);
            Color baseColor = GetRainbowColor(huePosition);

            _currentColor = CalculateColorFromPosition(baseColor, y / ColorSpectrum.ActualHeight, BlackToColorPoint, ColorToWhitePoint);
            _currentColor.A = (byte)AlphaSlider.Value;

            CurrentSpectrumColor = _currentColor;
            SelectedColor = _currentColor;

            UpdateControls();
            UpdateTextBoxes();
        }

        private (double x, double y) ClampPosition(Point position) => (
            Math.Clamp(position.X, 0, ColorSpectrum.ActualWidth),
            Math.Clamp(position.Y, 0, ColorSpectrum.ActualHeight)
        );

        private void UpdateColorFromRgbTextBoxes()
        {
            if (_isDragging) return;

            if (TryParseRgbValues(RedTextBox.Text, GreenTextBox.Text, BlueTextBox.Text, out Color color))
            {
                UpdateColorAndUI(color);
            }
            else
            {
                UpdateTextBoxes();
            }
        }

        private void UpdateColorFromHexTextBox()
        {
            if (_isDragging) return;

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(HexTextBox.Text.PadLeft(6, '0'));
                UpdateColorAndUI(color);
            }
            catch
            {
                UpdateTextBoxes();
            }
        }

        private void UpdateColorAndUI(Color color)
        {
            SelectedColor = color;
            CurrentSpectrumColor = _currentColor;
            UpdateColorSelectorPosition();
            UpdateControls();
        }
        #endregion

        #region Mouse Handling
        private bool HandleSpectrumMouseMove(MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
                return _isMouseCaptured;

            MyReleaseMouseCapture();
            return false;
        }

        private void MyReleaseMouseCapture()
        {
            if (_isMouseCaptured)
                ColorSpectrum.ReleaseMouseCapture();

            _isDragging = false;
            _isMouseCaptured = false;
            Cursor = Cursors.Arrow;
        }
        #endregion

        #region Callback
        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPickerControl colorPicker)
            {
                colorPicker._currentColor = (Color)e.NewValue;
                colorPicker.UpdateControls();
            }
        }
        #endregion


    }
}
