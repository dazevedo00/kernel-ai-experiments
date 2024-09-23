namespace APIGenerativeAI.Cache
{
    public interface IFunctionStateService
    {
        Task<string> GetFunctionAsync(string sessionId);
        Task SetFunctionAsync(string sessionId, string functionFound);
    }
}
