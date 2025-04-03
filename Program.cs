using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Read connection string
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var app = builder.Build();

// Global error handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature?.Error is not null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Internal Server Error",
                details = exceptionHandlerPathFeature.Error.Message
            });
        }
    });
});

// Check database connection on startup
try
{
    using var connection = new SqlConnection(connectionString);
    connection.Open();

    using var command = new SqlCommand("SELECT GETDATE();", connection);
    var result = command.ExecuteScalar();
    Console.WriteLine($"✅ Database connected! Server time: {result}");
}
catch (SqlException sqlEx)
{
    Console.WriteLine($"❌ Database connection failed: {sqlEx.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Unexpected error: {ex.Message}");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
