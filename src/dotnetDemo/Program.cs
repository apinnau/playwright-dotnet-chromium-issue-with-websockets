using dotnetDemo.Components;
using dotnetDemo.Components.I18n;
using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped(typeof(IStringLocalizer<>), typeof(MyStringLocalizer<>));

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddSupportedCultures("en", ["en", "de"]);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStaticFiles();
app.UseAntiforgery();
app.UseRequestLocalization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program
{
    // Expose the Program class for use with WebApplicationFactory<T>
}
