using MVVMPaintApp.Models;
using MVVMPaintApp.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace MVVMPaintApp.Commands
{
    public class LayerStateCommand(Layer layer, object oldValue, object newValue, Action<object> setProperty)
        : IUndoable
    {
        protected readonly Layer _layer = layer;
        protected readonly object _oldValue = oldValue;
        protected readonly object _newValue = newValue;
        protected readonly Action<object> _setProperty = setProperty;

        public void Undo() => _setProperty(_oldValue);
        public void Redo() => _setProperty(_newValue);
    }

    public partial class LayerVisibilityCommand(Layer layer, bool oldValue, bool newValue)
        : LayerStateCommand(layer, oldValue, newValue, value => layer.IsVisible = (bool)value)
    {
        [RelayCommand]
        private void ToggleVisibility()
        {
            _layer.IsVisible = !_layer.IsVisible;
        }
    }
}