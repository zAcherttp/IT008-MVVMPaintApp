using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MVVMPaintApp.Models;
using MVVMPaintApp.Models.Tools;
using MVVMPaintApp.Services;

namespace MVVMPaintApp.UserControls
{
    /// <summary>
    /// Interaction logic for ToolboxControl.xaml
    /// </summary>
    public partial class ToolboxControl : UserControl
    {
        private Popup currentPopup;

        public ToolboxControl()
        {
            InitializeComponent();
            currentPopup = new();
        }

        private void RadioButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not RadioButton radioButton) return;

            // Close any existing popup
            if (currentPopup != null)
            {
                currentPopup.IsOpen = false;
            }

            var template = radioButton.Template;

            if (template.FindName("ToolPopup", radioButton) is Popup popup &&
                template.FindName("PopupContent", radioButton) is ContentControl popupContent)
            {
                // Set the appropriate content based on the tool type
                var toolType = GetToolTypeFromRadioButton(radioButton);
                switch (toolType)
                {
                    case ToolType.Brush:
                    case ToolType.Pencil:
                    case ToolType.Eraser:
                        popupContent.Content = FindResource("BrushToolPopupContent");
                        break;
                    case ToolType.Shape:
                        popupContent.Content = FindResource("ShapeToolPopupContent");
                        break;
                }

                currentPopup = popup;
                popup.IsOpen = true;

                // Handle popup closing
                popup.Closed += (s, args) => currentPopup = new();

                e.Handled = true;
            }
        }

        private static ToolType GetToolTypeFromRadioButton(RadioButton radioButton)
        {
            var parameter = radioButton.GetValue(RadioButton.IsCheckedProperty) as ToolType?;
            return parameter ?? ToolType.Pencil;
        }
    }
}
