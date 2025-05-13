namespace MostafaSabriForex.Classes
{
    public class FrankfurterOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public List<string> ExcludedCurrencies { get; set; } = new();
        public int CacheDurationInMinutes { get; set; }
    }
}
