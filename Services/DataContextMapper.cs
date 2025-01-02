using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using MVVMPaintApp.UserControls;
using System.Windows.Controls;
using MVVMPaintApp.ViewModels;
using MVVMPaintApp.Views;

namespace MVVMPaintApp.Services
{
    public class WindowMapper
    {
        private readonly Dictionary<Type, Type> mappings = [];

        public WindowMapper()
        {
            Register<DashboardViewModel, DashboardWindow>();
            Register<NewFileViewModel, NewFileWindow>();
            Register<MainCanvasViewModel, MainCanvasWindow>();
        }

        public void Register<TViewModel, TWindow>() where TViewModel : ObservableObject where TWindow : Window
        {
            mappings[typeof(TViewModel)] = typeof(TWindow);
        }

        public Type? GetWindowType(Type viewModelType)
        {
            mappings.TryGetValue(viewModelType, out Type? windowType);
            return windowType;
        }
    }

    public class UserControlMapper
    {
        private readonly Dictionary<Type, Type> mappings = [];
        public UserControlMapper()
        {
            Register<DrawingCanvasViewModel, DrawingCanvas>();
            Register<ColorPaletteViewModel, ColorPaletteControl>();
            Register<LayerViewModel, LayerControl>();
            Register<ToolboxViewModel, ToolboxControl>();
            Register<ResizeViewModel, ResizeControl>();
            Register<SaveChangesViewModel, SaveChangesControl>();
            Register<AboutViewModel, AboutControl>();
        }
        public void Register<TViewModel, TView>() where TViewModel : ObservableObject where TView : UserControl
        {
            mappings[typeof(TViewModel)] = typeof(TView);
        }
        public Type? GetViewType(Type viewModelType)
        {
            mappings.TryGetValue(viewModelType, out Type? viewType);
            return viewType;
        }
    }
}
