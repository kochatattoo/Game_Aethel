using CodeBase.Infrastructure;
using CodeBase.Infrastructure.Services;

namespace Assets._project.CodeBase.Infrastructure.Services.Input
{
    public interface IInputHandlerService: IService
    {
        ClickInputHandler ClickInputHandler { get; }
    }
}