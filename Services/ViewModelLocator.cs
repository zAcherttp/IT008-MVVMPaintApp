using Microsoft.Extensions.DependencyInjection;
using MVVMPaintApp.ViewModels;

namespace MVVMPaintApp.Services
{
    public class ViewModelLocator(IServiceProvider serviceProvider)
    {
        public DashboardViewModel DashboardViewModel => serviceProvider.GetRequiredService<DashboardViewModel>();
        public NewFileViewModel NewFileViewModel => serviceProvider.GetRequiredService<NewFileViewModel>();
        public MainCanvasViewModel MainCanvasViewModel => serviceProvider.GetRequiredService<MainCanvasViewModel>();
        public DrawingCanvasViewModel DrawingCanvasViewModel => serviceProvider.GetRequiredService<DrawingCanvasViewModel>();
        public ColorPaletteViewModel ColorPaletteViewModel => serviceProvider.GetRequiredService<ColorPaletteViewModel>();
        public LayerViewModel LayerViewModel => serviceProvider.GetRequiredService<LayerViewModel>();
        public ToolboxViewModel ToolboxViewModel => serviceProvider.GetRequiredService<ToolboxViewModel>();
        public AboutViewModel AboutViewModel => serviceProvider.GetRequiredService<AboutViewModel>();
        public ResizeViewModel ResizeViewModel => serviceProvider.GetRequiredService<ResizeViewModel>();
        public SaveChangesViewModel SaveChangesViewModel => serviceProvider.GetRequiredService<SaveChangesViewModel>();
    }      
}
