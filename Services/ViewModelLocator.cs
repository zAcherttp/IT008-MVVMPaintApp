using Microsoft.Extensions.DependencyInjection;
using MVVMPaintApp.ViewModels;

namespace MVVMPaintApp.Services
{
    public class ViewModelLocator
    {
        private readonly IServiceProvider serviceProvider;

        public ViewModelLocator()
        {
            serviceProvider = ((App)App.Current).serviceProvider;
        }

        public DashboardViewModel DashboardViewModel => serviceProvider.GetRequiredService<DashboardViewModel>();
        public NewFileViewModel NewFileViewModel => serviceProvider.GetRequiredService<NewFileViewModel>();
        public MainCanvasViewModel MainCanvasViewModel => serviceProvider.GetRequiredService<MainCanvasViewModel>();
        public DrawingCanvasViewModel DrawingCanvasViewModel => serviceProvider.GetRequiredService<DrawingCanvasViewModel>();
        public ColorPaletteViewModel ColorPaletteViewModel => serviceProvider.GetRequiredService<ColorPaletteViewModel>();
    }
}
