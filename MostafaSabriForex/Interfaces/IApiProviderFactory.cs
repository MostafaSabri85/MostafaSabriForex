namespace MostafaSabriForex.Interfaces
{
    public interface IApiProviderFactory
    {
        IExternalApiProvider GetProvider(string providerKey);
    }
}
