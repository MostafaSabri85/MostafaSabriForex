namespace MostafaSabriForex.Handlers
{
    public class SerilogLoggingHandler : DelegatingHandler
    {
        private readonly ILogger<SerilogLoggingHandler> _logger;

        public SerilogLoggingHandler(ILogger<SerilogLoggingHandler> logger)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Log the request details
            _logger.LogInformation("Sending request to {Url} with method {Method} and headers {@Headers}",
                request.RequestUri, request.Method, request.Headers);

            var response = await base.SendAsync(request, cancellationToken);

            // Log the response details
            _logger.LogInformation("Received response from {Url} with status code {StatusCode}",
                request.RequestUri, response.StatusCode);

            return response;
        }
    }
}
