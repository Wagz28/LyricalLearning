using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Configure logging to log to console and to Azure App Service's default logs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();  // Add console logging (important for Azure App Service log stream)
builder.Logging.SetMinimumLevel(LogLevel.Information); // Set log level to Information

// Other services like database, MVC, etc.
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

// Log that app started
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("✅ App started logging."); // Log that the app started

app.Run();
