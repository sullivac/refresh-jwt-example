using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;
using RefreshJwtExample.DependencyInjection;
using RefreshJwtExample.Http;
using RefreshJwtExample.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

builder.Services.AddSingleton(
    typeof(IServiceFactory<>),
    typeof(ServiceFactory<>));

builder.Services.AddSingleton<
    ISalesforceTokenService,
    SalesforceTokenService>();

builder.Services.AddTransient<SalesforceTokenHandler>();

builder.Services
    .AddHttpClient<ISalesforceTokenClient, SalesforceTokenClient>(
        httpClient =>
        {
            httpClient.BaseAddress = new Uri("http://localhost:5001/");
        });

builder.Services
    .AddHttpClient(
        "SalesforceClient",
        httpClient =>
        {
            httpClient.BaseAddress = new Uri("http://localhost:5001/");
        })
    .AddPolicyHandler(
        SalesforceTokenRefreshPolicy.GetSalesforceTokenRefresPolicy)
    .AddHttpMessageHandler<SalesforceTokenHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet(
    "/weatherforecast",
    (HttpContext context, ILogger<Program> logger) =>
    {
        logger.LogInformation(
            "{Authorization}",
            context.Request.Headers.Authorization.ToString());

        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();
        return forecast;
    })
.WithName("GetWeatherForecast")
.WithOpenApi();

static async Task<Results<Ok<WeatherForecast[]>, ProblemHttpResult>>
    GetRandom(IHttpClientFactory httpClientFactory)
{
    var client = httpClientFactory.CreateClient("SalesforceClient");

    var response = await client.GetAsync("weatherforecast");

    if (!response.IsSuccessStatusCode)
    {
        return TypedResults.Problem(
            "Failed to get weather forecast",
            statusCode: (int)response.StatusCode);
    }

    var result =
        await response.Content.ReadFromJsonAsync<WeatherForecast[]>()
        ?? Array.Empty<WeatherForecast>();

    return TypedResults.Ok(result);
}

app.MapGet("/random", GetRandom);

app.MapGet(
    "/oauth/token",
    () =>
        new AccessTokenResponse
        {
            AccessToken = Guid.NewGuid().ToString(),
            RefreshToken = Guid.NewGuid().ToString(),
            ExpiresIn = 3600
        });

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
