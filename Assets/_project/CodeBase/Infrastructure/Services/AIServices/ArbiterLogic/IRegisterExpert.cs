namespace CodeBase.Infrastructure.Services.AIServices.ArbiterLogic
{
    public interface IRegisterExpert
    {
        void RegisterExpert(IExpert expert);
        void DeregisterExpert(IExpert expert);
    }
}
