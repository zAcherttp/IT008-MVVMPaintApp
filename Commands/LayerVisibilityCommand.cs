using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
using MVVMPaintApp.Models;

namespace MVVMPaintApp.Commands
{
    public interface IUndoable
    {
        void Undo();
        void Redo();
    }

    // Base class for layer property changes
    public class LayerStateCommand(Layer layer, object oldValue, object newValue, Action<object> setProperty) : IUndoable
    {
        protected readonly Layer Layer = layer;
        protected readonly object OldValue = oldValue;
        protected readonly object NewValue = newValue;
        protected readonly Action<object> SetProperty = setProperty;

        public void Undo() => SetProperty(OldValue);
        public void Redo() => SetProperty(NewValue);
    }

    // Specific commands for layer properties
    public class LayerVisibilityCommand(Layer layer, bool oldValue, bool newValue)
        : LayerStateCommand(layer, oldValue, newValue, value => layer.IsVisible = (bool)value)
    {
    }
}