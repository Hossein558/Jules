using JulesPanel.Services;
using MudBlazor.Services;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB — prevents circuit crashes on large prompts
    });

builder.Services.AddMudServices();
builder.Services.AddSyncfusionBlazor();

builder.Services.AddHttpClient<JulesApiService>(client =>
{
    client.BaseAddress = new Uri("https://jules.googleapis.com/v1alpha/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddSingleton<AccountService>();

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MTIzQDMzMzEyZTMwMmUzMDNiMzMzMTNiR0VoN2NVYVlJaHVIRHpqeTgxakxVVktQUmhUWkgvdzlUQVRtTW9XYXNmVT0=");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// In production (published), serve wwwroot/_content properly
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<JulesPanel.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
