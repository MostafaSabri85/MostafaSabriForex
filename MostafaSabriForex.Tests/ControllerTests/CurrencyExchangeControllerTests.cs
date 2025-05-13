using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using MostafaSabriForex.Controllers.V1;
using MostafaSabriForex.Services;

namespace MostafaSabriForex.Tests.ControllerTests
{
    public class CurrencyExchangeControllerTests
    {
        private readonly Mock<ExternalApiService> _mockExternalApiService;
        private readonly Mock<ILogger<CurrencyExchangeController>> _mockLogger;
        private readonly CurrencyExchangeController _controller;

        public CurrencyExchangeControllerTests()
        {
            _mockExternalApiService = new Mock<ExternalApiService>();
            _mockLogger = new Mock<ILogger<CurrencyExchangeController>>();
            _controller = new CurrencyExchangeController(_mockExternalApiService.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetLatestRates_ShouldReturnOk_WhenValidRequest()
        {
            // Arrange
            var baseCurrency = "USD";
            var expectedRates = new Dictionary<string, decimal>
            {
                { "EUR", 0.84m },
                { "GBP", 0.75m }
            };

            _mockExternalApiService.Setup(s => s.GetLatestRatesAsync("Frankfurter", baseCurrency))
                .Returns(expectedRates.ToString);

            // Act
            var result = await _controller.GetLatestRates("Frankfurter", baseCurrency);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            okResult.Value.Should().BeEquivalentTo(expectedRates);
        }

        [Fact]
        public async Task GetLatestRates_ShouldReturnBadRequest_WhenServiceFails()
        {
            // Arrange
            var baseCurrency = "USD";
            _mockExternalApiService.Setup(s => s.GetLatestRatesAsync("Frankfurter", baseCurrency))
                .ThrowsAsync(new System.Exception("Failed to fetch data"));

            // Act
            var result = await _controller.GetLatestRates(baseCurrency);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            badRequestResult.Value.Should().Be("Failed to fetch data");
        }
    }
}
