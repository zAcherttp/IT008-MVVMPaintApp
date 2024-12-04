﻿using CommunityToolkit.Mvvm.ComponentModel;
using MVVMPaintApp.Interfaces;
using System.ComponentModel;
using System.Windows;

namespace MVVMPaintApp.Services
{
    public class WindowManager(WindowMapper windowMapper) : IWindowManager
    {
        private readonly WindowMapper windowMapper = windowMapper;

        public void ShowWindow(ObservableObject viewModel)
        {
            var windowType = windowMapper.GetWindowType(viewModel.GetType());
            if (windowType != null)
            {
                if (Activator.CreateInstance(windowType) is Window window)
                {
                    window.DataContext = viewModel;
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

