namespace CodeBase.Infrastructure.Services
{
    public interface ICameraInput
    {
        PointerInputProvider PointerInput { get; }
        bool PointerInputAllowed { get; }
        bool KeyboardInputAllowed { get; }
        bool PointerOverUi { get; }
    }
}
