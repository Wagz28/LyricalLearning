using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var app = builder.Build();

try
{
    using var connection = new SqlConnection(connectionString);
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
