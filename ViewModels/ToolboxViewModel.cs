using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Models;
using MVVMPaintApp.Services;

namespace MVVMPaintApp.ViewModels
{
    public partial class ToolboxViewModel : ObservableObject
    {
        [ObservableProperty]
        private ProjectManager projectManager;

        [ObservableProperty]
        private ViewModelLocator viewModelLocator;

        public ToolboxViewModel(ProjectManager projectManager, ViewModelLocator viewModelLocator)
        {
            ProjectManager = projectManager;
            ViewModelLocator = viewModelLocator;
        }

        public void SetProjectManager(ProjectManager projectManager)
        {
            ProjectManager = projectManager;
        }
    }
}
