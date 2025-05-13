using System.Text;
using Asp.Versioning;
using Microsoft.IdentityModel.Tokens;
using MostafaSabriForex.Classes;
using MostafaSabriForex.Interfaces;
using MostafaSabriForex.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Registry;
using Serilog;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

var zipkinUrl = builder.Configuration.GetValue<string>("Zipkin:Url");

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Log to console
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day) // Optional: Log to file with daily rolling
    .WriteTo.Seq(builder.Configuration["Seq:Url"]) // Log to Seq (centralized logging system)
    .CreateLogger();

builder.Host.UseSerilog();  // Use Serilog for logging

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var user = httpContext.User.Identity?.Name ?? "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(user, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 5 // This enables throttling (5 requests will be queued if limit is exceeded)
        });
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Add OpenTelemetry Tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(builder =>
    {
        builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyApiService"))
            .AddAspNetCoreInstrumentation() // ASP.NET Core instrumentation
            .AddHttpClientInstrumentation() // HttpClient instrumentation
            .AddZipkinExporter(opt =>
            {
                opt.Endpoint = new Uri(zipkinUrl); // Specify Zipkin URL
            });
    });

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

// Define and register policies in the PolicyRegistry
builder.Services.AddSingleton<IPolicyRegistry<string>>(services =>
{
    var policyRegistry = new PolicyRegistry();

    var retryPolicy = Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .OrResult(r => !r.IsSuccessStatusCode)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    var circuitBreakerPolicy = Policy<HttpResponseMessage>
        .Handle<HttpRequestException>()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

    policyRegistry.Add("RetryPolicy", retryPolicy);
    policyRegistry.Add("CircuitBreakerPolicy", circuitBreakerPolicy);

    return policyRegistry;
});

// Apply policies to all HttpClients
builder.Services.AddHttpClient<IExternalApiProvider>()
    .AddPolicyHandler((services, request) =>
    {
        var policyRegistry = services.GetRequiredService<IPolicyRegistry<string>>();
        return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("RetryPolicy");
    })
    .AddPolicyHandler((services, request) =>
    {
        var policyRegistry = services.GetRequiredService<IPolicyRegistry<string>>();
        return policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("CircuitBreakerPolicy");
    });

// Factory and Providers
builder.Services.AddTransient<Frankfurter>();
builder.Services.AddSingleton<IApiProviderFactory, ApiProviderFactory>();
builder.Services.AddScoped<ExternalApiService>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "FrankfurterCache_";
});

builder.Services.Configure<FrankfurterOptions>(
    builder.Configuration.GetSection("FrankfurterApi"));

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(); // Logs incoming HTTP requests and responses

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
