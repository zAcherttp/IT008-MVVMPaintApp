using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace MVVMPaintApp.Models
{
    internal class NewFileTextBoxValidation
    {
        public static readonly DependencyProperty IsNumericProperty =
            DependencyProperty.RegisterAttached(
                "IsNumeric",
                typeof(bool),
                typeof(NewFileTextBoxValidation),
                new PropertyMetadata(false, OnIsNumericChanged));

        public static bool GetIsNumeric(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsNumericProperty);
        }

        public static void SetIsNumeric(DependencyObject obj, bool value)
        {
            obj.SetValue(IsNumericProperty, value);
        }

        public static readonly DependencyProperty AllowDecimalProperty =
            DependencyProperty.RegisterAttached(
                "AllowDecimal",
                typeof(bool),
                typeof(NewFileTextBoxValidation),
                new PropertyMetadata(false));

        public static bool GetAllowDecimal(DependencyObject obj)
        {
            return (bool)obj.GetValue(AllowDecimalProperty);
        }

        public static void SetAllowDecimal(DependencyObject obj, bool value)
        {
            obj.SetValue(AllowDecimalProperty, value);
        }

        private static void OnIsNumericChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    textBox.PreviewTextInput += OnPreviewTextInput;
                    textBox.PreviewKeyDown += OnPreviewKeyDown;
                    DataObject.AddPastingHandler(textBox, OnPasting);
                }
                else
                {
                    textBox.PreviewTextInput -= OnPreviewTextInput;
                    textBox.PreviewKeyDown -= OnPreviewKeyDown;
                    DataObject.RemovePastingHandler(textBox, OnPasting);
                }
            }
        }

        private static void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                bool allowDecimal = GetAllowDecimal(textBox);
                string newText = textBox.Text.Insert(textBox.CaretIndex, e.Text);

                // Allow one decimal point if decimals are allowed
                if (allowDecimal && e.Text == "." && !textBox.Text.Contains('.'))
                {
                    e.Handled = false;
                    return;
                }

                // Check if the new text is a valid number
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
            // Allow control keys like Backspace, Delete, Arrow keys etc.
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

                    if (allowDecimal)
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
        }
    }
}
