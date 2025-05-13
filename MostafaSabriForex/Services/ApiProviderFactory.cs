using MostafaSabriForex.Interfaces;

namespace MostafaSabriForex.Services
{
    public class ApiProviderFactory : IApiProviderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ApiProviderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IExternalApiProvider GetProvider(string providerKey) => providerKey switch
        {
            "Frankfurter" => _serviceProvider.GetRequiredService<Frankfurter>(),
            _ => throw new ArgumentException("Invalid provider key")
        };
    }
}
