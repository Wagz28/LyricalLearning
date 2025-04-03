using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;  // Make sure this is added

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add logging to both console and Azure Web App Diagnostics (filesystem).
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();  // Logs to the console (useful during local dev)
    logging.AddAzureWebAppDiagnostics();  // Logs to Azure file system (visible in Log Stream)
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

// ✅ Test Database Connection (Without Breaking the App)
var logger = LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger("DatabaseLogger");
logger.LogInformation("✅ App started logging.");

try
{
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

// Run the app
app.Run();
