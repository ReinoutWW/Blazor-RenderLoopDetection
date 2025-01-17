using Blazor_RenderLoopDetection.Components;
using Blazor_RenderLoopDetection.Detectors;
using Blazor_RenderLoopDetection.Detectors.CircuitHandler;
using Microsoft.AspNetCore.Components.Server.Circuits;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<CircuitHandler, CustomCircuitHandler>();

builder.Services.AddScoped<IUserExterminator, UserExterminator>();
builder.Services.AddScoped(sp =>
{
    var userExterminator = sp.GetRequiredService<IUserExterminator>();
    return new RenderLoopDetector(100, new TimeSpan(0, 0, 1), userExterminator);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
