using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MostafaSabriForex.Services;

namespace MostafaSabriForex.Controllers.V1
{
    [EnableRateLimiting("fixed")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1")]
    [Authorize(Roles = "User")]
    [ApiController]
    public class CurrencyExchangeController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly ExternalApiService _service;

        public CurrencyExchangeController(ExternalApiService service, ILogger logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("{providerKey}/latest-rates")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetLatestRates(string providerKey, [FromQuery] string baseCurrency = "USD")
        {
            _logger.LogInformation("Calling GetLatestRates API to get latest rates. Base: {BaseCurrency}", baseCurrency);

            var result = await _service.GetLatestRatesAsync(providerKey, baseCurrency);

            return Ok(result);
        }

        [HttpGet("{providerKey}/convert")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Convert(string providerKey,
            [FromQuery] string from,
            [FromQuery] string to,
            [FromQuery] decimal amount)
        {
            _logger.LogInformation("Calling Convert API. From: {from}, To: {to}, Amount: {amount}", from, to, amount);

            var result = await _service.ConvertCurrencyUsingFrankfurter(providerKey, from, to, amount);

            return Ok(result);
        }

        [HttpGet("{providerKey}/historical-rates")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetHistoricalRates(string providerKey, [FromQuery] string baseCurrency = "USD",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            startDate = startDate ?? DateTime.Now.AddDays(-15);
            endDate = endDate ?? DateTime.Now;

            _logger.LogInformation("Calling GetHistoricalRates API. StartDate: {startDate}, EndDate: {endDate}, Page: {page}, PageSize: {pageSize}", startDate, endDate, page, pageSize);

            var result = await _service.GetHistoricalRatesAsync(providerKey, baseCurrency, startDate.Value, endDate.Value, page, pageSize);

            return Ok(result);
        }
    }
}
