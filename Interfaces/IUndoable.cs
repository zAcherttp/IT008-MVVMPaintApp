using MVVMPaintApp.Services;

namespace MVVMPaintApp.Interfaces
{
    public interface IUndoable
    {
        void Undo(ProjectManager projectManager);
        void Redo(ProjectManager projectManager);
    }
}
