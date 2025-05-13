using Microsoft.Extensions.Caching.Distributed;
using MostafaSabriForex.Interfaces;

namespace MostafaSabriForex.Services
{
    public class ExternalApiService
    {
        private readonly IApiProviderFactory _factory;
        private readonly IDistributedCache _cache;

        public ExternalApiService(IApiProviderFactory factory, IDistributedCache cache)
        {
            _factory = factory;
            _cache = cache;
        }

        public async Task<string> GetLatestRatesAsync(string providerKey, string baseCurrency)
        {
            var provider = _factory.GetProvider(providerKey);

            return await provider.GetLatestRatesAsync(baseCurrency);
        }

        public async Task<string> ConvertCurrencyUsingFrankfurter(string providerKey, string from, string to, decimal amount)
        {
            var provider = _factory.GetProvider(providerKey);

            return await provider.ConvertCurrencyAsync(from, to, amount);
        }

        public async Task<string> GetHistoricalRatesAsync(string providerKey, string baseCurrency, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            var provider = _factory.GetProvider(providerKey);

            return await provider.GetHistoricalRatesAsync(baseCurrency, startDate, endDate, page, pageSize);
        }
    }
}
