using Azure.Identity;
using Microsoft.Data.SqlClient;
using Azure.Core.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
AzureEventSourceListener.CreateConsoleLogger(); // Debugging logs

var credential = new DefaultAzureCredential();
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var app = builder.Build();

try
{
    using var connection = new SqlConnection(connectionString);
    connection.AccessToken = credential.GetToken(
        new Azure.Core.TokenRequestContext(["https://database.windows.net/.default"])
    ).Token;

    connection.Open();
    Console.WriteLine("Database connection successful!");
}
catch (Exception ex)
{
    Console.WriteLine($"Database connection failed: {ex.Message}");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();


