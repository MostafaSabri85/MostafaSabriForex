# Currency Exchange API

A secure, scalable, and resilient .NET 9 Web API integrating with the Frankfurter external currency exchange service using modern best practices including JWT-based RBAC, caching, resilience policies, observability, and versioning.

---

## ✨ Setup Instructions

1. **Clone the repository**
   ```bash
   git clone https://github.com/MostafaSabri85/MostafaSabriForex.git
   cd currency-exchange-api
   ```

2. **Configure your environment**
   - Set up Redis locally or use a remote instance.
   - In `appsettings.Development.json` and `appsettings.Production.json`, add:
     ```json
     {
       "FrankfurterApi": {
         "BaseUrl": "https://api.frankfurter.dev/v1"
       },
       "Redis": "localhost:6379"
     }
     ```

3. **Run the application**
   ```bash
   dotnet run --project CurrencyExchangeAPI.csproj
   ```

4. **Run unit tests**
   ```bash
   dotnet test
   ```

5. **View logs (if using Seq)**
   - Access logs at `http://localhost:5341`

6.  Rate limiting

    The API uses fixed window rate limiting (10 requests per minute per user).

    Adjust this in Program.cs inside AddRateLimiter().

---

## 🧠 Assumptions Made

- Only one external provider (Frankfurter) is needed for exchange rate data.
- Currency codes `TRY`, `PLN`, `THB`, and `MXN` must be excluded from all responses.
- JWT is used for securing API endpoints without persisting user credentials in a database.
- Redis is configured and available for distributed caching.
- Circuit breaker and retry policies will cover basic API resilience requirements.
- Pagination for historical data is fixed at 5 days per page.
- client-side control over pagination size.
- .NET built-in rate limiter is used to throttle requests to avoid abuse.

---

## 🚀 Possible Future Enhancements

- Add support for additional exchange rate providers with fallback and load-balancing logic.
- Persist user roles and tokens in a database for full auth control.
- Add real-time WebSocket support for currency updates.
- Integrate Swagger UI with full versioned documentation.
- Add metrics export with Prometheus/Grafana.
- Enable feature toggles and A/B testing with LaunchDarkly or similar.
- Containerize with Docker and deploy with Kubernetes.
- Add caching invalidation policies and expiration control per endpoint.
- Enhance rate limiting to support IP-based throttling and client quotas.