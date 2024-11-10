using MVVMPaintApp.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMPaintApp.Models
{
    public class UndoRedoManager
    {
        private readonly Stack<IUndoable> UndoStack = new();
        private readonly Stack<IUndoable> RedoStack = new();
        private const int MaxStackSize = 50;  // Limit memory usage

        public bool CanUndo => UndoStack.Count > 0;
        public bool CanRedo => RedoStack.Count > 0;

        public void AddCommand(IUndoable command)
        {
            UndoStack.Push(command);
            RedoStack.Clear();  // Clear redo stack when new action is performed

            // Maintain stack size limit
            if (UndoStack.Count > MaxStackSize)
            {
                var tempStack = new Stack<IUndoable>();
                for (int i = 0; i < MaxStackSize; i++)
                {
                    tempStack.Push(UndoStack.Pop());
                }
                UndoStack.Clear();
                while (tempStack.Count > 0)
                {
                    UndoStack.Push(tempStack.Pop());
                }
            }
        }

        public void Undo()
        {
            if (CanUndo)
            {
                var command = UndoStack.Pop();
                command.Undo();
                RedoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                var command = RedoStack.Pop();
                command.Redo();
                UndoStack.Push(command);
            }
        }

        public void Clear()
        {
            UndoStack.Clear();
            RedoStack.Clear();
        }
    }
}
