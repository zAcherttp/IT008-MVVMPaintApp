using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using MVVMPaintApp.ViewModels;

namespace MVVMPaintApp.Models
{
    class ViewModelLocator
    {
        private readonly IServiceProvider serviceProvider;

        public ViewModelLocator()
        {
            var services = new ServiceCollection();

            services.AddTransient<DrawingCanvasViewModel>();
            services.AddTransient<ColorPaletteViewModel>();
            services.AddTransient<MainCanvasViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<NewFileViewModel>();

            serviceProvider = services.BuildServiceProvider();
        }

        public DrawingCanvasViewModel DrawingCanvasViewModel => serviceProvider.GetRequiredService<DrawingCanvasViewModel>();
        public ColorPaletteViewModel ColorPaletteViewModel => serviceProvider.GetRequiredService<ColorPaletteViewModel>();
        public MainCanvasViewModel MainCanvasViewModel => serviceProvider.GetRequiredService<MainCanvasViewModel>();
        public DashboardViewModel HomeViewModel => serviceProvider.GetRequiredService<DashboardViewModel>();
        public NewFileViewModel NewFileViewModel => serviceProvider.GetRequiredService<NewFileViewModel>();
    }
}
