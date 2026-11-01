using clinica_dental_api_cloud.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Get the connection string from the configuration
var connectionString = configuration.GetConnectionString("ClinicaDental")
                       ?? throw new InvalidOperationException(
                           "Falta la cadena de conexión 'ClinicaDental'. Defina ConnectionStrings__ClinicaDental.");

builder.Services.AddDbContext<ClinicaDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Clinica Dental API";
        options.Theme = ScalarTheme.DeepSpace;
        options.ForceThemeMode = ThemeMode.Dark;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();