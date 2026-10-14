using LimpeAgoraWeb.Components;
using LimpeAgoraWeb.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// Sessão e Provider de Autenticação do Blazor
builder.Services.AddScoped<AuthSessionService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();

// Configuração do HttpClient e ApiClient para comunicação com LimpeAgoraApi
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7051";
if (!apiBaseUrl.EndsWith("/"))
{
    apiBaseUrl += "/";
}

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AuthWebService>();
builder.Services.AddScoped<CategoriaWebService>();
builder.Services.AddScoped<ProdutoWebService>();
builder.Services.AddScoped<UsuarioWebService>();
builder.Services.AddScoped<PedidoWebService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
