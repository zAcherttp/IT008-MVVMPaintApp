using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MVVMPaintApp.UserControls
{
    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPickerControl : UserControl
    {
        #region Public

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
                new PropertyMetadata(
                    Colors.Transparent));

        public ColorPickerControl()
        {
            InitializeComponent();
            AlphaSlider.Value = 255;
            UpdateControls();
        }

        public event EventHandler<Color>? ColorChanged;

        #endregion

        #region Private

        private bool _isDragging;
        private bool _isMouseCaptured;
        private Color _currentColor;

        private const double blackToColorPoint = 0.33;
        private const double colorToWhitePoint = 0.66;

        private void ColorSpectrum_LostMouseCapture(object sender, MouseEventArgs e)
        {
            _isDragging = false;
            _isMouseCaptured = false;
        }

        private void ColorSpectrum_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _isMouseCaptured = ColorSpectrum.CaptureMouse();
            Cursor = Cursors.None;
            UpdateColorFromSpectrum(e.GetPosition(ColorSpectrum));
        }

        private void ColorSpectrum_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point mousePos = e.GetPosition(ColorSpectrum);
                // Allow dragging outside the color spectrum while maintaining color selection
                if (_isMouseCaptured)
                {
                    UpdateColorFromSpectrum(mousePos);
                }
            }
            else
            {
                if (_isMouseCaptured)
                {
                    ColorSpectrum.ReleaseMouseCapture();
                }
                _isDragging = false;
                _isMouseCaptured = false;
            }
        }

        private void ColorSpectrum_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMouseCaptured)
            {
                ColorSpectrum.ReleaseMouseCapture();
            }
            _isDragging = false;
            _isMouseCaptured = false;
            Cursor = Cursors.Arrow;

            ColorChanged?.Invoke(this, SelectedColor);
        }

        private void ColorValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateColorFromRgbTextBoxes();
                UpdateTextBoxes();
                ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
        private void ColorValue_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateColorFromRgbTextBoxes();
        }

        private void HexValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateColorFromHexTextBox();
                ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
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

        private void UpdateControls()
        {
            ColorPreview.Fill = new SolidColorBrush(_currentColor);
            ColorSelector.Fill = new SolidColorBrush(_currentColor);
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
                return;
            }
        }

        private void UpdateColorFromRgbTextBoxes()
        {
            if (_isDragging) return;

            if (byte.TryParse(RedTextBox.Text, out byte r) &&
                byte.TryParse(GreenTextBox.Text, out byte g) &&
                byte.TryParse(BlueTextBox.Text, out byte b))
            {
                SelectedColor = Color.FromRgb(r, g, b);
                CurrentSpectrumColor = _currentColor;
                UpdateColorSelectorPosition();
                UpdateControls();
            }
            else
            {
                UpdateTextBoxes();
            }
        }

        private void UpdateColorFromHexTextBox()
        {
            if (_isDragging) return;

            string hex = HexTextBox.Text.PadLeft(6, '0');
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);
                SelectedColor = color;
                CurrentSpectrumColor = _currentColor;
                UpdateColorSelectorPosition();
                UpdateControls();
            }
            catch
            {
                UpdateTextBoxes();
            }
        }

        private void UpdateColorFromSpectrum(Point position)
        {
            double x = Math.Clamp(position.X, 0, ColorSpectrum.ActualWidth);
            double y = Math.Clamp(position.Y, 0, ColorSpectrum.ActualHeight);

            // Update selector position with clamping to prevent overlap with borders
            int halfSize = (int)ColorSelector.Width / 2;
            Canvas.SetLeft(ColorSelector, Math.Clamp(x - halfSize, -halfSize, ColorSpectrum.ActualWidth - halfSize));
            Canvas.SetTop(ColorSelector, Math.Clamp(y - halfSize, -halfSize, ColorSpectrum.ActualHeight - halfSize));

            // Normalize x position to prevent color wrapping at the edge
            double huePosition = Math.Min(x / ColorSpectrum.ActualWidth, 0.9999);
            Color baseColor = GetRainbowColor(huePosition);

            double normalizedY = y / ColorSpectrum.ActualHeight;

            // Apply black || white
            if (normalizedY < blackToColorPoint)
            {
                double blackAmount = 1.0 - (normalizedY / blackToColorPoint);
                _currentColor = MixColors(baseColor, Colors.Black, blackAmount);
            }
            else if (normalizedY > colorToWhitePoint)
            {
                double whiteAmount = (normalizedY - colorToWhitePoint) / (1.0 - colorToWhitePoint);
                _currentColor = MixColors(baseColor, Colors.White, whiteAmount);
            }
            else
            {
                _currentColor = baseColor;
            }

            CurrentSpectrumColor = _currentColor;
            _currentColor.A = (byte)AlphaSlider.Value;
            SelectedColor = _currentColor;

            UpdateControls();
            UpdateTextBoxes();
        }

        public void UpdateColorSelectorPosition()
        {
            // Calculate x position based on hue
            double hueScale = GetHue(_currentColor) / 360.0;
            int x = (int)(hueScale * ColorSpectrum.ActualWidth);

            // Get the pure hue color at this position
            double huePosition = Math.Min(hueScale, 0.9999);
            Color baseColor = GetRainbowColor(huePosition);

            // Calculate brightness relative to the pure color
            double brightness = GetRelativeBrightness(_currentColor, baseColor);

            // Map brightness to y position:
            // -1 to 0 (darker) maps to 0 to 0.33
            // 0 (pure) maps to 0.5
            // 0 to 1 (lighter) maps to 0.66 to 1
            double y;
            if (brightness < 0)
            {
                // Map -1 to 0 range to 0 to blackToColorPoint
                y = (brightness + 1) * blackToColorPoint;
            }
            else if (brightness > 0)
            {
                // Map 0 to 1 range to colorToWhitePoint to 1
                y = colorToWhitePoint + (brightness * (1 - colorToWhitePoint));
            }
            else
            {
                // Pure color in the middle of the gap
                y = (blackToColorPoint + colorToWhitePoint) / 2;
            }

            y *= ColorSpectrum.ActualHeight;

            // Update selector position
            Canvas.SetLeft(ColorSelector, x - ColorSelector.Width / 2);
            Canvas.SetTop(ColorSelector, y - ColorSelector.Height / 2);
        }

        private static Color GetRainbowColor(double position)
        {
            position *= 6;
            int index = (int)position;
            double remainder = position - index;

            byte r = 0, g = 0, b = 0;

            switch (index)
            {
                case 0: // Red to Yellow
                    r = 255;
                    g = (byte)(255 * remainder);
                    break;
                case 1: // Yellow to Green
                    r = (byte)(255 * (1 - remainder));
                    g = 255;
                    break;
                case 2: // Green to Cyan
                    g = 255;
                    b = (byte)(255 * remainder);
                    break;
                case 3: // Cyan to Blue
                    g = (byte)(255 * (1 - remainder));
                    b = 255;
                    break;
                case 4: // Blue to Magenta
                    b = 255;
                    r = (byte)(255 * remainder);
                    break;
                default: // Magenta to Red
                    b = (byte)(255 * (1 - remainder));
                    r = 255;
                    break;
            }

            return Color.FromRgb(r, g, b);
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
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;
            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            if (max == min)
            {
                return 0;
            }
            double hue;
            double delta = max - min;
            if (max == r)
            {
                hue = (g - b) / delta;
            }
            else if (max == g)
            {
                hue = 2.0 + (b - r) / delta;
            }
            else
            {
                hue = 4.0 + (r - g) / delta;
            }
            hue *= 60.0;
            if (hue < 0)
            {
                hue += 360.0;
            }
            return hue;
        }

        private static double GetRelativeBrightness(Color color, Color baseColor)
        {
            // Get brightness value from -1 (black) through 0 (base color) to 1 (white)

            // Calculate relative brightness of the current color
            double colorBrightness = (color.R * 0.299 + color.G * 0.587 + color.B * 0.114) / 255.0;
            double baseBrightness = (baseColor.R * 0.299 + baseColor.G * 0.587 + baseColor.B * 0.114) / 255.0;

            if (colorBrightness < baseBrightness)
            {
                // Color is darker than base - map to -1 to 0
                return (colorBrightness / baseBrightness) - 1;
            }
            else if (colorBrightness > baseBrightness)
            {
                // Color is lighter than base - map to 0 to 1
                double maxPossibleBrightness = 1.0;
                return (colorBrightness - baseBrightness) / (maxPossibleBrightness - baseBrightness);
            }
            else
            {
                // Color is the base color
                return 0;
            }
        }
        
        #endregion

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPickerControl colorPicker)
            {
                colorPicker._currentColor = (Color)e.NewValue;
                colorPicker.UpdateControls();
            }
        }
    }
}
