using Data; // Added
using Core.Interfaces; // Added
using Core; // Added

var builder = WebApplication.CreateBuilder(args);

// Register custom services
builder.Services.AddSingleton<Data.IDbConnectionFactory, Data.DbConnectionFactory>();
builder.Services.AddScoped<Core.Interfaces.IExampleRepository, Data.ExampleRepository>();

// Register Product services and repositories
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductColorRepository, ProductColorRepository>();
builder.Services.AddScoped<ProductService>();

// Register Order services and repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<OrderService>();

// Configure Shopify API Settings
builder.Services.Configure<ShopifyApiSettings>(builder.Configuration.GetSection("ShopifyApiSettings"));

// Add services to the container.
builder.Services.AddControllers(); // Added for MVC Controllers
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers(); // Added to map controller routes

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
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

app.MapGet("/examples", async (Core.Interfaces.IExampleRepository repo) =>
{
    try
    {
        var examples = await repo.GetAllAsync();
        return Results.Ok(examples);
    }
    catch (System.Exception ex)
    {
        // Log the exception (implementation depends on logging setup)
        // For now, return a generic error response
        return Results.Problem($"An error occurred: {ex.Message}");
    }
})
.WithName("GetAllExamples")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
