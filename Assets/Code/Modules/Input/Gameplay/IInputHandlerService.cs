using CodeBase.Infrastructure;
using CodeBase.Infrastructure.Services;

namespace CodeBase.Infrastructure.Services.Input
{
    public interface IInputHandlerService: IService
    {
        ClickInputHandler ClickInputHandler { get; }
    }
}