using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

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

        #region Color Calculations
        private static Color GetRainbowColor(double position)
        {
            var (r, g, b) = CalculateRainbowComponents(position * 6);
            return Color.FromRgb(r, g, b);
        }

        private static (byte r, byte g, byte b) CalculateRainbowComponents(double position)
        {
            int index = (int)position;
            double remainder = position - index;

            return index switch
            {
                0 => (255, (byte)(255 * remainder), 0),                    // Red to Yellow
                1 => ((byte)(255 * (1 - remainder)), 255, 0),             // Yellow to Green
                2 => (0, 255, (byte)(255 * remainder)),                   // Green to Cyan
                3 => (0, (byte)(255 * (1 - remainder)), 255),            // Cyan to Blue
                4 => ((byte)(255 * remainder), 0, 255),                   // Blue to Magenta
                _ => (255, 0, (byte)(255 * (1 - remainder)))             // Magenta to Red
            };
        }

        private static Color MixColors(Color color1, Color color2, double amount)
        {
            return Color.FromRgb(
                (byte)(color1.R * (1 - amount) + color2.R * amount),
                (byte)(color1.G * (1 - amount) + color2.G * amount),
                (byte)(color1.B * (1 - amount) + color2.B * amount)
            );
        }

        private static double GetHue(Color color)
        {
            var (h, _, _) = RgbToHsv(color);
            return h;
        }

        private static (double h, double s, double v) RgbToHsv(Color color)
        {
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            double hue = delta == 0 ? 0 :
                max == r ? 60 * ((g - b) / delta) :
                max == g ? 60 * (2 + (b - r) / delta) :
                          60 * (4 + (r - g) / delta);

            if (hue < 0) hue += 360;

            double saturation = max == 0 ? 0 : delta / max;
            double value = max;

            return (hue, saturation, value);
        }

        private static double GetRelativeBrightness(Color color, Color baseColor)
        {
            double colorBrightness = CalculateBrightness(color);
            double baseBrightness = CalculateBrightness(baseColor);

            if (colorBrightness < baseBrightness)
                return (colorBrightness / baseBrightness) - 1;
            if (colorBrightness > baseBrightness)
                return (colorBrightness - baseBrightness) / (1.0 - baseBrightness);
            return 0;
        }

        private static double CalculateBrightness(Color color) =>
            (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) / 255.0;
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

            _currentColor = CalculateColorFromPosition(y / ColorSpectrum.ActualHeight, baseColor);
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

        private static Color CalculateColorFromPosition(double normalizedY, Color baseColor)
        {
            if (normalizedY < BlackToColorPoint)
            {
                double blackAmount = 1.0 - (normalizedY / BlackToColorPoint);
                return MixColors(baseColor, Colors.Black, blackAmount);
            }

            if (normalizedY > ColorToWhitePoint)
            {
                double whiteAmount = (normalizedY - ColorToWhitePoint) / (1.0 - ColorToWhitePoint);
                return MixColors(baseColor, Colors.White, whiteAmount);
            }

            return baseColor;
        }

        private void UpdateColorFromRgbTextBoxes()
        {
            if (_isDragging) return;

            if (TryParseRgbValues(out var color))
            {
                UpdateColorAndUI(color);
            }
            else
            {
                UpdateTextBoxes();
            }
        }

        private bool TryParseRgbValues(out Color color)
        {
            color = Colors.Black;
            if (byte.TryParse(RedTextBox.Text, out byte r) &&
                byte.TryParse(GreenTextBox.Text, out byte b) &&
                byte.TryParse(BlueTextBox.Text, out byte g))
            {
                color = Color.FromRgb(r, g, b);
                return true;
            }
            return false;
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
