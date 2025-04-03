using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Clear default providers and add logging providers.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddAzureWebAppDiagnostics();  // Ensure this NuGet package is installed.
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add Razor Pages (or other services)
builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();

// Log that the app has started
var logger = app.Logger;
logger.LogInformation("✅ App started logging.");

// Test DB connection with error handling
try
{
    // Retrieve connection string from configuration
    string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    using var connection = new SqlConnection(connectionString);
    connection.Open();
    logger.LogInformation("✅ Database connection successful!");
}
catch (SqlException sqlEx)
{
    logger.LogError(sqlEx, "❌ Database connection failed!");
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ Unexpected error occurred.");
}

app.Run();