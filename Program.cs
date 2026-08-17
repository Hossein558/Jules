using JulesPanel.Services;
using MudBlazor.Services;
using Syncfusion.Blazor;
using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options =>
    {
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB — prevents circuit crashes on large prompts
    });

// Enable response compression for static files (CSS/JS)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "text/css", "application/javascript", "application/wasm" });
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
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

// Response compression middleware (must be before UseStaticFiles)
app.UseResponseCompression();

// In production (published), serve wwwroot/_content properly
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<JulesPanel.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();