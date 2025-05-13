namespace MostafaSabriForex.Interfaces
{
    public interface IExternalApiProvider
    {
        Task<string> GetLatestRatesAsync(string baseCurrency);
        Task<string> ConvertCurrencyAsync(string from, string to, decimal amount);
        Task<string> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate, int page, int pageSize);
    }
}
