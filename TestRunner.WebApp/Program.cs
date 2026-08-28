using TestRunner.WebApp.Application.DependencyInjection;
using TestRunner.WebApp.Application.Logging;
using TestRunner.WebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Logging.InitFileLogger();
builder.Services.InitAppLogging();
builder.Services.InitStores();
builder.Services.InitSharedServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Lifecycle is developer detail, so it goes to the logging pipeline and not to the log panel.
// The category is spelled out because the generated Program class has no namespace and would not
// match the filters configured for the application.
var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("TestRunner.WebApp.Lifetime");
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() => logger.LogDebug("Application started."));
lifetime.ApplicationStopping.Register(() => logger.LogDebug("Application is shutting down (stopping)."));
lifetime.ApplicationStopped.Register(() => logger.LogDebug("Application is shutting down (stopped)."));

app.Run();