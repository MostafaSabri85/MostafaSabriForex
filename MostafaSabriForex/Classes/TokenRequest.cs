namespace MostafaSabriForex.Classes
{
    public class TokenRequest
    {
        public string Username { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
}
