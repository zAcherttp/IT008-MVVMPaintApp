using MVVMPaintApp.Interfaces;
using MVVMPaintApp.Services;
namespace MVVMPaintApp.Models
{
    public abstract class HistoryEntry : IUndoable
    {
        public abstract void Undo(ProjectManager projectManager);
        public abstract void Redo(ProjectManager projectManager);
    }
}
