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
builder.Services.AddScoped<IProductosApiService, ProductosApiService>();
builder.Services.AddScoped<IAuthApiService, AuthApiService>();
builder.Services.AddScoped<IUsuariosApiService, UsuariosApiService>();
builder.Services.AddScoped<ICategoriasApiService, CategoriasApiService>();
builder.Services.AddScoped<IMesasApiService, MesasApiService>();
builder.Services.AddScoped<IDashboardApiService, DashboardApiService>();
builder.Services.AddScoped<IPromocionesApiService, PromocionesApiService>();
builder.Services.AddScoped<IReportesApiService, ReportesApiService>();
builder.Services.AddScoped<IClientesApiService, ClientesApiService>();
builder.Services.AddScoped<IFacturasApiService, FacturasApiService>();
builder.Services.AddScoped<IComandasApiService, ComandasApiService>();
builder.Services.AddScoped<IReservacionesApiService, ReservacionesApiService>();
builder.Services.AddScoped<IPreparacionesApiService, PreparacionesApiService>(); // Added for Semana 8
builder.Services.AddScoped<IInventarioApiService, InventarioApiService>(); // Added for Semana 9
builder.Services.AddScoped<IProveedoresApiService, ProveedoresApiService>(); // Added for Semana 10
builder.Services.AddScoped<INotificacionesApiService, NotificacionesApiService>(); // Added for Semana 11
builder.Services.AddScoped<IRecetasApiService, RecetasApiService>(); // Added for Semana 11
builder.Services.AddScoped<IConfiguracionApiService, ConfiguracionApiService>(); // Added for Semana 12
builder.Services.AddScoped<IReportesComercialesApiService, ReportesComercialesApiService>(); // Added for Semana 12
builder.Services.AddScoped<IReportesInventarioApiService, ReportesInventarioApiService>(); // Added for Semana 12
builder.Services.AddScoped<IHistorialComandasApiService, HistorialComandasApiService>(); // Added for Historial de Comandas
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
builder.Services.AddScoped<PermissionService>();

var app = builder.Build();

// Inicializar el servicio de logout
LogoutService.Initialize(app.Services);

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
