using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using RestaurantePro.Web.Admin.Data;
using System.Net.Http;
using RestaurantePro.Web.Admin.Services;
using RestaurantePro.Web.Admin.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<TokenStore>();
builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddHttpClient("Api", (sp, http) =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:8080";
    http.BaseAddress = new Uri(baseUrl);
    var useBasic = builder.Configuration.GetValue<bool>("ApiUseBasicAuth");
    if (useBasic)
    {
        var user = builder.Configuration["ApiBasicUser"];
        var pass = builder.Configuration["ApiBasicPass"];
        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
        {
            var raw = System.Text.Encoding.UTF8.GetBytes($"{user}:{pass}");
            var param = Convert.ToBase64String(raw);
            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", param);
        }
    }
}).AddHttpMessageHandler<AuthTokenHandler>();
builder.Services.AddScoped<ProductosApiService>();
builder.Services.AddScoped<AuthApiService>();
builder.Services.AddScoped<UsuariosApiService>();
builder.Services.AddScoped<CategoriasApiService>();
builder.Services.AddScoped<MesasApiService>();
builder.Services.AddScoped<DashboardApiService>();
builder.Services.AddScoped<PromocionesApiService>();
builder.Services.AddScoped<ReportesApiService>();
builder.Services.AddScoped<ClientesApiService>();
builder.Services.AddScoped<FacturasApiService>();
builder.Services.AddScoped<ComandasApiService>();
builder.Services.AddScoped<ReservacionesApiService>();
builder.Services.AddScoped<PreparacionesApiService>(); // Added for Semana 8
builder.Services.AddScoped<InventarioApiService>(); // Added for Semana 9
builder.Services.AddScoped<ProveedoresApiService>(); // Added for Semana 10
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}


app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
