using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using MVVMPaintApp.Interfaces;
using MVVMPaintApp.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MVVMPaintApp.Services
{
    public class WindowManager(WindowMapper windowMapper) : IWindowManager
    {
        private readonly WindowMapper windowMapper = windowMapper;

        public void ShowWindow(ObservableObject viewModel)
        {
            if (windowMapper.GetWindowType(viewModel.GetType()) is Type windowType) 
            {
                if (Activator.CreateInstance(windowType, viewModel) is Window window)
                {
                    if (Application.Current.MainWindow == null)
                    {
                        Application.Current.MainWindow = window;
                    }
                    window.Show();
                }
            }
        }

        public void CloseWindow(ObservableObject viewModel)
        {
            var window = Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w.DataContext == viewModel);
            window?.Close();
        }
        

        public void CloseAll()
        {
            foreach (var window in Application.Current.Windows)
            {
                if (window is Window w)
                {
                    w.Close();
                }
            }
        }
    }
}

