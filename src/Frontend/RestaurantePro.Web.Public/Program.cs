using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RestaurantePro.Web.Public;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Cargar ApiBaseUrl desde wwwroot/appsettings*.json
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<RestaurantePro.Web.Public.Services.MenuApiService>();
builder.Services.AddScoped<RestaurantePro.Web.Public.Services.ReviewsApiService>();
builder.Services.AddScoped<RestaurantePro.Web.Public.Services.ContactApiService>();

await builder.Build().RunAsync();
