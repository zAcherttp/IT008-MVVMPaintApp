using MVVMPaintApp.Models;

namespace MVVMPaintApp.Interfaces
{
    public interface IProjectFactory
    {
        Project CreateDefault();
        Project Load(string projectFolder);
        string GetDefaultProjectName();
    }
}
