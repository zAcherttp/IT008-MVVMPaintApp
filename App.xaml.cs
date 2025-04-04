﻿using System.Windows;
using MVVMPaintApp.Services;
using MVVMPaintApp.ViewModels;
using MVVMPaintApp.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using MVVMPaintApp.Models;
using MVVMPaintApp.Models.Tools;
using System.Windows.Media.Imaging;

namespace MVVMPaintApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceCollection serviceCollection = [];
        public readonly ServiceProvider serviceProvider;

        public App()
        {
            ConfigureServices(serviceCollection);
            serviceProvider = serviceCollection.BuildServiceProvider(); 
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<NewFileViewModel>();

            services.AddSingleton<MainCanvasViewModel>();
            services.AddSingleton<DrawingCanvasViewModel>();
            services.AddSingleton<ColorPaletteViewModel>();
            services.AddSingleton<LayerViewModel>();
            services.AddSingleton<ToolboxViewModel>();
            services.AddSingleton<ResizeViewModel>();
            services.AddSingleton<SaveChangesViewModel>();
            services.AddSingleton<AboutViewModel>();

            services.AddSingleton<WindowMapper>();
            services.AddSingleton<UserControlMapper>();
            services.AddSingleton<ViewModelLocator>();
            services.AddSingleton<ProjectManager>();
            services.AddTransient<IDialogService, DialogService>();
            services.AddSingleton<IProjectFactory, ProjectFactory>();
            services.AddSingleton<IWindowManager, WindowManager>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var windowManager = serviceProvider.GetRequiredService<IWindowManager>();
            windowManager.ShowWindow(serviceProvider.GetRequiredService<DashboardViewModel>());
            
            base.OnStartup(e);
        }
    }

}
