namespace CodeBase.Infrastructure.Services.LogData
{
    public interface ILogDataService : IService
    {
        LogData logData {get;}
        void Initialize();
        void InitializeAsync();
    }
}
