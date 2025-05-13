using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MyApi;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace MostafaSabriForex.Tests.IntegrationTests
{
    public class CurrencyExchangeControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public CurrencyExchangeControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetLatestRates_ShouldReturnOk()
        {
            // Arrange
            var baseCurrency = "USD";

            // Act
            var response = await _client.GetAsync($"api/v1/currencyexchange/frankfurter/latest-rates?baseCurrency={baseCurrency}");

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetHistoricalRates_ShouldReturnOk()
        {
            // Arrange
            var baseCurrency = "USD";
            var startDate = "2020-01-01";
            var endDate = "2020-01-10";
            var page = 1;
            var pageSize = 5;

            // Act
            var response = await _client.GetAsync($"api/v1/currencyexchange/frankfurter/historical-rates?baseCurrency={baseCurrency}&startDate={startDate}&endDate={endDate}&page={page}&pageSize={pageSize}");

            // Assert
            response.EnsureSuccessStatusCode();
        }
    }
}
