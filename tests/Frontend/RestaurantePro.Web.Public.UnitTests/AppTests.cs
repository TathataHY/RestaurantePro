using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Web.Public;

namespace RestaurantePro.Web.Public.UnitTests;

public class AppTests : TestContext
{
    [Fact]
    public void NotFound_Renderiza_Mensaje()
    {
        // Registrar router
        Services.AddSingleton<NavigationManager>(new TestNavigationManager());
        // Configurar JSInterop para PageTitle
        JSInterop.Setup<string>("Blazor._internal.PageTitle.getAndRemoveExistingTitle").SetResult("RestaurantePro");

        var cut = RenderComponent<App>();
        // Simular navegación a ruta inexistente dentro del dispatcher
        var nav = Services.GetRequiredService<NavigationManager>() as TestNavigationManager;
        cut.InvokeAsync(() => nav!.NavigateTo("/ruta-inexistente", forceLoad: false));

        cut.Markup.Should().Contain("Página no encontrada");
    }
}

internal sealed class TestNavigationManager : NavigationManager
{
    public TestNavigationManager()
    {
        Initialize("http://localhost/", "http://localhost/");
    }

    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        Uri = ToAbsoluteUri(uri).ToString();
        NotifyLocationChanged(isInterceptedLink: false);
    }
}


