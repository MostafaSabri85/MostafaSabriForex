using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using MostafaSabriForex.Interfaces;
using MostafaSabriForex.Services;

namespace MostafaSabriForex.Tests.ServiceTests
{
    public class ExternalApiServiceTests
    {
        private readonly Mock<IApiProviderFactory> _mockApiProviderFactory;
        private readonly Mock<IDistributedCache> _mockDistributedCache;
        private readonly ExternalApiService _service;

        public ExternalApiServiceTests()
        {
            _mockApiProviderFactory = new Mock<IApiProviderFactory>();
            _mockDistributedCache = new Mock<IDistributedCache>();
            _service = new ExternalApiService(_mockApiProviderFactory.Object, _mockDistributedCache.Object);
        }

        [Fact]
        public async Task GetLatestRatesAsync_ShouldReturnRates_WhenProviderReturnsData()
        {
            // Arrange
            var baseCurrency = "USD";
            var expectedRates = new Dictionary<string, decimal>
            {
                { "EUR", 0.84m },
                { "GBP", 0.75m }
            };

            _mockApiProviderFactory.Setup(p => p.GetProvider("Frankfurter").GetLatestRatesAsync(baseCurrency))
                .Returns(expectedRates.ToString);

            // Act
            var result = await _service.GetLatestRatesAsync("Frankfurter", baseCurrency);

            // Assert
            result.Should().BeEquivalentTo(expectedRates.ToString());
        }

        [Fact]
        public async Task GetLatestRatesAsync_ShouldThrowException_WhenProviderFails()
        {
            // Arrange
            var baseCurrency = "USD";

            _mockApiProviderFactory.Setup(p => p.GetProvider("Frankfurter").GetLatestRatesAsync(baseCurrency))
                .ThrowsAsync(new System.Exception("Failed to fetch data"));

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => _service.GetLatestRatesAsync("Frankfurter", baseCurrency));
        }
    }
}
