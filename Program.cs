using JulesPanel.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddHttpClient<JulesApiService>(client =>
{
    client.BaseAddress = new Uri("https://jules.googleapis.com/v1alpha/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddSingleton<JulesApiService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<JulesPanel.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
