using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVVMPaintApp.Models
{
    internal class NumericTextBoxValidation
    {
        public static readonly DependencyProperty IsNumericProperty =
            DependencyProperty.RegisterAttached("IsNumeric", typeof(bool), typeof(NumericTextBoxValidation),
                new PropertyMetadata(false, OnIsNumericChanged));

        public static readonly DependencyProperty AllowDecimalProperty =
            DependencyProperty.RegisterAttached("AllowDecimal", typeof(bool), typeof(NumericTextBoxValidation),
                new PropertyMetadata(false));

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.RegisterAttached("MinValue", typeof(double), typeof(NumericTextBoxValidation),
                new PropertyMetadata(double.MinValue));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.RegisterAttached("MaxValue", typeof(double), typeof(NumericTextBoxValidation),
                new PropertyMetadata(double.MaxValue));


        public static bool GetIsNumeric(DependencyObject obj) => (bool)obj.GetValue(IsNumericProperty);
        public static void SetIsNumeric(DependencyObject obj, bool value) => obj.SetValue(IsNumericProperty, value);

        public static bool GetAllowDecimal(DependencyObject obj) => (bool)obj.GetValue(AllowDecimalProperty);
        public static void SetAllowDecimal(DependencyObject obj, bool value) => obj.SetValue(AllowDecimalProperty, value);

        public static double GetMinValue(DependencyObject obj) => (double)obj.GetValue(MinValueProperty);
        public static void SetMinValue(DependencyObject obj, double value) => obj.SetValue(MinValueProperty, value);

        public static double GetMaxValue(DependencyObject obj) => (double)obj.GetValue(MaxValueProperty);
        public static void SetMaxValue(DependencyObject obj, double value) => obj.SetValue(MaxValueProperty, value);

        private static void OnIsNumericChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.PreviewTextInput += OnPreviewTextInput;
                    textBox.PreviewKeyDown += OnPreviewKeyDown;
                    textBox.TextChanged += OnTextChanged;
                    DataObject.AddPastingHandler(textBox, OnPasting);
                }
                else
                {
                    textBox.PreviewTextInput -= OnPreviewTextInput;
                    textBox.PreviewKeyDown -= OnPreviewKeyDown;
                    textBox.TextChanged -= OnTextChanged;
                    DataObject.RemovePastingHandler(textBox, OnPasting);
                }
            }
        }

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
            {
                double minValue = GetMinValue(textBox);
                double maxValue = GetMaxValue(textBox);

                if (double.TryParse(textBox.Text, out double value))
                {
                    if (value < minValue)
                    {
                        textBox.Text = minValue.ToString();
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                    else if (value > maxValue)
                    {
                        textBox.Text = maxValue.ToString();
                        textBox.CaretIndex = textBox.Text.Length;
                    }
                }
            }
        }

        private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                bool allowDecimal = GetAllowDecimal(textBox);
                string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

                if (allowDecimal && e.Text == "." && !textBox.Text.Contains('.'))
                {
                    e.Handled = false;
                    return;
                }

                if (allowDecimal)
                {
                    e.Handled = !double.TryParse(newText, out _);
                }
                else
                {
                    e.Handled = !int.TryParse(newText, out _);
                }
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private static void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (e.DataObject.GetDataPresent(typeof(string)))
                {
                    string text = (string)e.DataObject.GetData(typeof(string));
                    bool allowDecimal = GetAllowDecimal(textBox);
                    double minValue = GetMinValue(textBox);
                    double maxValue = GetMaxValue(textBox);

                    if (double.TryParse(text, out double value))
                    {
                        if (value < minValue || value > maxValue)
                        {
                            e.CancelCommand();
                        }
                        else if (allowDecimal)
                        {
                            if (!double.TryParse(text, out _))
                            {
                                e.CancelCommand();
                            }
                        }
                        else
                        {
                            if (!int.TryParse(text, out _))
                            {
                                e.CancelCommand();
                            }
                        }
                    }
                    else
                    {
                        e.CancelCommand();
                    }
                }
                else
                {
                    e.CancelCommand();
                }
            }
        }
    }
}
