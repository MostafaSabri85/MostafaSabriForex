using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using MostafaSabriForex.Classes;
using MostafaSabriForex.Interfaces;

namespace MostafaSabriForex.Services
{
    public class Frankfurter : IExternalApiProvider
    {
        private readonly HttpClient _httpClient;
        private readonly IDistributedCache _cache;
        private readonly FrankfurterOptions _options;


        public Frankfurter(HttpClient httpClient, IDistributedCache cache, IOptions<FrankfurterOptions> options)
        {
            _httpClient = httpClient;
            _cache = cache;
            _options = options.Value;
        }

        public async Task<string> GetLatestRatesAsync(string baseCurrency)
        {
            var cacheKey = $"{baseCurrency}_latestrates_{DateTime.Now:yyyyMMdd}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<string>(cachedData) ?? string.Empty;
            }

            var response = await _httpClient.GetAsync($"{_options.BaseUrl}/latest?base={baseCurrency}");

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();

                // Parse the JSON
                using var jsonDoc = JsonDocument.Parse(jsonString);
                var root = jsonDoc.RootElement;

                // Get the 'rates' part of the response
                var ratesElement = root.GetProperty("rates");

                // Filter the data directly using LINQ and excluding unwanted currencies
                var filteredRates = ratesElement
                    .EnumerateObject()
                    .SelectMany(dateElement => dateElement.Value
                        .EnumerateObject()
                        .Where(currency => !_options.ExcludedCurrencies.Contains(currency.Name)) // Exclude the unwanted currencies
                        .Select(currency => new CurrencyRate
                        {
                            Date = DateTime.Parse(dateElement.Name),
                            CurrencyCode = currency.Name,
                            Rate = currency.Value.GetDecimal()
                        }))
                    .ToList();

                // Cache the data for future requests
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(filteredRates), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheDurationInMinutes)
                });
                //

                return jsonString;
            }

            throw new Exception("Failed to fetch the needed rates.");
        }

        public async Task<string> ConvertCurrencyAsync(string from, string to, decimal amount)
        {
            if (_options.ExcludedCurrencies.Contains(from) || _options.ExcludedCurrencies.Contains(to))
            {
                throw new ArgumentException("Invalid currency: " + String.Join(",", _options.ExcludedCurrencies));
            }

            var response = await _httpClient.GetAsync($"{_options.BaseUrl}/latest?base={from}&symbols={to}");
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            if (!json.RootElement.TryGetProperty("rates", out var rates) || !rates.TryGetProperty(to, out var rateElement))
                return $"Conversion rate not available for {from} to {to}.";

            var rate = rateElement.GetDecimal();
            var converted = amount * rate;

            return $"{amount} {from} = {converted:F2} {to}";
        }

        public async Task<string> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate, int page, int pageSize)
        {
            var cacheKey = $"{baseCurrency}_historical_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}_page_{page}_pageSize_{pageSize}";
            var cachedData = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<string>(cachedData) ?? string.Empty;
            }

            var totalDays = (endDate - startDate).Days + 1;
            var totalPages = (int)Math.Ceiling(totalDays / (double)pageSize);

            if (page < 1 || page > totalPages)
                return $"Invalid page number. Please use a page between 1 and {totalPages}.";

            var skipDays = (page - 1) * pageSize;
            var pageStart = startDate.AddDays(skipDays);
            var pageEnd = pageStart.AddDays(pageSize - 1);

            if (pageEnd > endDate)
                pageEnd = endDate;

            var from = pageStart.ToString("yyyy-MM-dd");
            var to = pageEnd.ToString("yyyy-MM-dd");

            var url = $"{_options.BaseUrl}/{from}..{to}?base={baseCurrency}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                // Read the JSON response as a string
                var jsonString = await response.Content.ReadAsStringAsync();

                // Parse the JSON
                using var jsonDoc = JsonDocument.Parse(jsonString);
                var root = jsonDoc.RootElement;

                // Get the 'rates' part of the response
                var ratesElement = root.GetProperty("rates");

                // Filter the data directly using LINQ and excluding unwanted currencies
                var filteredRates = ratesElement
                    .EnumerateObject()
                    .SelectMany(dateElement => dateElement.Value
                        .EnumerateObject()
                        .Where(currency => !_options.ExcludedCurrencies.Contains(currency.Name)) // Exclude the unwanted currencies
                        .Select(currency => new CurrencyRate
                        {
                            Date = DateTime.Parse(dateElement.Name),
                            CurrencyCode = currency.Name,      
                            Rate = currency.Value.GetDecimal()
                        }))
                    .ToList();

                // Cache the data for future requests
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(filteredRates), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.CacheDurationInMinutes)
                });

                return jsonString;
            }

            throw new Exception("Failed to fetch historical rates.");
        }
    }
}
