using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MVVMPaintApp.Services;
using MVVMPaintApp.Models;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media;
using System.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MVVMPaintApp.ViewModels
{
    public partial class LayerViewModel : ObservableObject
    {
        [ObservableProperty]
        private ProjectManager projectManager;

        [ObservableProperty]
        private ObservableCollection<Layer> layers = [];

        [ObservableProperty]
        private Layer? selectedLayer;

        [RelayCommand]
        private void ToggleVisibility()
        {
            ProjectManager.ToggleLayerVisibility(SelectedLayer);
        }

        [RelayCommand]
        private void AddLayer()
        {
            ProjectManager.AddLayer();
        }

        [RelayCommand]
        private void DeleteLayer()
        {
            if (SelectedLayer != null)
            {
                ProjectManager.RemoveLayer(SelectedLayer);
            }
        }

        [RelayCommand(CanExecute = nameof(CanMoveLayerUp))]
        private void MoveLayerUp()
        {
            if (SelectedLayer != null)
            {
                int index = ProjectManager.CurrentProject.Layers.IndexOf(SelectedLayer);
                ProjectManager.Move(index, index - 1);
            }
        }
        private bool CanMoveLayerUp() => SelectedLayer != null && ProjectManager.CurrentProject.Layers.IndexOf(SelectedLayer) > 0;

        [RelayCommand(CanExecute = nameof(CanMoveLayerDown))]
        private void MoveLayerDown()
        {
            if (SelectedLayer != null)
            {
                int index = ProjectManager.CurrentProject.Layers.IndexOf(SelectedLayer);
                ProjectManager.Move(index, index + 1);
            }
        }
        private bool CanMoveLayerDown() => SelectedLayer != null && ProjectManager.CurrentProject.Layers.IndexOf(SelectedLayer) < ProjectManager.CurrentProject.Layers.Count - 1;

        [RelayCommand(CanExecute = nameof(CanMergeLayerDown))]
        private void MergeLayerDown()
        {
            if (SelectedLayer != null)
            {
                int index = ProjectManager.CurrentProject.Layers.IndexOf(SelectedLayer);
                if (index < ProjectManager.CurrentProject.Layers.Count - 1)
                {
                    Layer layer = ProjectManager.CurrentProject.Layers[index + 1];
                    SelectedLayer.MergeDown(layer);
                    ProjectManager.RemoveLayer(layer);
                }
            }
        }
        private bool CanMergeLayerDown() => SelectedLayer != null && ProjectManager.CurrentProject.Layers.IndexOf(SelectedLayer) < ProjectManager.CurrentProject.Layers.Count - 1;

        [RelayCommand]
        private void SelectionChanged()
        {
            MoveLayerUpCommand.NotifyCanExecuteChanged();
            MoveLayerDownCommand.NotifyCanExecuteChanged();
            MergeLayerDownCommand.NotifyCanExecuteChanged();
        }

        [ObservableProperty]
        public WriteableBitmap? backgroundLayer;

        public LayerViewModel(ProjectManager projectManager)
        {
            ProjectManager = projectManager;
            projectManager.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ProjectManager.CurrentProject))
                {
                    Layers = projectManager.CurrentProject.Layers;
                }
                if (e.PropertyName == nameof(ProjectManager.CurrentProject.Background)
                    || e.PropertyName == nameof(ProjectManager.CurrentProject.IsBackgroundVisible))
                {
                    UpdateBackgroundLayer();
                }
            };

            Layers = projectManager.CurrentProject.Layers;
            UpdateBackgroundLayer();
        }

        public void UpdateBackgroundLayer()
        {
            var thumbnailSize = 80;
            var checkerSize = 8;

            var layerThumbnail = BitmapFactory.New(thumbnailSize, thumbnailSize);
            layerThumbnail.Clear(Colors.Transparent);

            // Create checkerboard pattern
            for (int y = 0; y < thumbnailSize; y += checkerSize)
            {
                for (int x = 0; x < thumbnailSize; x += checkerSize)
                {
                    var color = ((x / checkerSize) + (y / checkerSize)) % 2 == 0 ? Colors.LightGray : Colors.Gray;
                    layerThumbnail.FillRectangle(x, y, x + checkerSize, y + checkerSize, color);
                }
            }

            if (ProjectManager.CurrentProject.IsBackgroundVisible)
            {
                layerThumbnail.Clear(ProjectManager.CurrentProject.Background);
            }

            BackgroundLayer = layerThumbnail;
        }
    }
}
