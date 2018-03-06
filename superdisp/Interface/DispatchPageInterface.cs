using renstech.NET.SupernovaDispatcher.Model;

namespace renstech.NET.SupernovaDispatcher.Interface
{
    public interface IDispatchPage
    {
        Subsystem GetSubsystem();

        bool Initialize();
    }
}