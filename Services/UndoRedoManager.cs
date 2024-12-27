using MVVMPaintApp.Models;

namespace MVVMPaintApp.Services
{
    public class UndoRedoManager(ProjectManager projectManager)
    {
        private readonly Stack<HistoryEntry> undoStack = new();
        private readonly Stack<HistoryEntry> redoStack = new();
        private readonly ProjectManager projectManager = projectManager;
        private const int MAX_HISTORY = 10;

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        public void AddHistoryEntry(HistoryEntry entry)
        {
            undoStack.Push(entry);
            redoStack.Clear();

            if (undoStack.Count > MAX_HISTORY)
            {
                var tempStack = new Stack<HistoryEntry>();
                for (int i = 0; i < MAX_HISTORY; i++)
                {
                    tempStack.Push(undoStack.Pop());
                }
                undoStack.Clear();
                while (tempStack.Count > 0)
                {
                    undoStack.Push(tempStack.Pop());
                }
            }
        }

        public void Undo()
        {
            if (!CanUndo) return;

            var entry = undoStack.Pop();
            entry.Undo(projectManager);
            redoStack.Push(entry);
        }

        public void Redo()
        {
            if (!CanRedo) return;

            var entry = redoStack.Pop();
            entry.Redo(projectManager);
            undoStack.Push(entry);
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
        }   
    }
}
