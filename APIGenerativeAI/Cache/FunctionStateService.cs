using Microsoft.Extensions.Caching.Memory;

namespace APIGenerativeAI.Cache
{
    public class FunctionStateService : IFunctionStateService
    {
        private readonly IMemoryCache _cache;

        public FunctionStateService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<string> GetFunctionAsync(string sessionId)
        {
            _cache.TryGetValue(sessionId, out string functionFound);
            return Task.FromResult(functionFound);
        }

        public Task SetFunctionAsync(string sessionId, string functionFound)
        {
            _cache.Set(sessionId, functionFound);
            return Task.CompletedTask;
        }
    }
}
