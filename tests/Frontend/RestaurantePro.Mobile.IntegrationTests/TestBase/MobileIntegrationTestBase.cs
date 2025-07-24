namespace RestaurantePro.Mobile.IntegrationTests.TestBase;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RestaurantePro.Api;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Mobile.Core.Services.Authentication;
using RestaurantePro.Mobile.Core.Services.Api;
using RestaurantePro.Mobile.Core.Services.Platform;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Text;
using System.Text.Json;
using AutoFixture;
using AutoFixture.Xunit2;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Identity;
using RestaurantePro.Infrastructure.Identity.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RestaurantePro.Infrastructure.DependencyInjection;

/// <summary>
/// Clase base para pruebas de integración móvil con backend real en memoria
/// </summary>
public class MobileIntegrationTestBase : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Configuración mínima para tests
            var testConfig = new Dictionary<string, string?>
            {
                {"UseInMemoryDatabase", "true"},
                {"ConnectionStrings:DefaultConnection", "TestConnection"},
                {"JwtSettings:Secret", "SuperSecretKeyForTestingPurposesOnly123456789"},
                {"JwtSettings:Issuer", "TestIssuer"},
                {"JwtSettings:Audience", "TestAudience"},
                {"JwtSettings:ExpirationInMinutes", "60"},
                {"IdentitySettings:PasswordSettings:RequireDigit", "false"},
                {"IdentitySettings:PasswordSettings:RequireLowercase", "false"},
                {"IdentitySettings:PasswordSettings:RequireUppercase", "false"},
                {"IdentitySettings:PasswordSettings:RequireNonAlphanumeric", "false"},
                {"IdentitySettings:PasswordSettings:RequiredLength", "3"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(testConfig)
                .Build();

            // ⚠️ NO registrar servicios móviles en el backend
            // Los servicios móviles se crearán localmente en cada test
            // para evitar conflictos de dependencias
        });
    }
    }

    /// <summary>
/// Configuración de JWT para tests
    /// </summary>
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; } = 60;
} 