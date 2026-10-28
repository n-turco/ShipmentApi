using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ShipmentApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    // Serialize enums as names ("InTransit") and reject numeric values like 99
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false)));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Add the database context to the services container
builder.Services.AddDbContext<ShipmentContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ShipmentContext") ?? throw new InvalidOperationException("Connection string 'ShipmentContext' not found.")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
