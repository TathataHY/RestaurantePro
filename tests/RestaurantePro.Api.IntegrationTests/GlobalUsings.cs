global using Xunit;
global using Xunit.Abstractions;
global using FluentAssertions;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using System.Net;
global using System.Net.Http.Json;
global using System.Text;
global using System.Text.Json;
global using RestaurantePro.Api.IntegrationTests.TestBase;
global using RestaurantePro.Domain.Core.SharedKernel.Results;
global using RestaurantePro.Domain.Core.SharedKernel;
global using RestaurantePro.Application.Common.Interfaces;
global using RestaurantePro.Infrastructure.Persistence.Contexts;
global using RestaurantePro.Domain.Core.Productos.Entities;
global using RestaurantePro.Domain.Core.Productos.ValueObjects;
global using RestaurantePro.Domain.Core.Usuarios.Entities;
global using RestaurantePro.Domain.Core.Usuarios.Enums;
global using RestaurantePro.Domain.Comercial.Clientes.Entities;
global using RestaurantePro.Domain.Comercial.Clientes.Enums;
global using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
global using RestaurantePro.Domain.Operaciones.Comandas.Entities;
global using RestaurantePro.Domain.Operaciones.Comandas.Enums;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
global using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
global using RestaurantePro.Domain.Proveedores.Entities;
global using RestaurantePro.Domain.Proveedores.Enums;
global using RestaurantePro.Domain.Comercial.Facturacion.Enums;
global using RestaurantePro.Domain.Comercial.Pagos.Enums;
global using RestaurantePro.Domain.Comercial.Promociones.Enums;
global using RestaurantePro.Application.Common.DTOs;
global using RestaurantePro.Application.Core.Productos.DTOs;
global using RestaurantePro.Application.Core.Usuarios.DTOs;
global using RestaurantePro.Application.Core.Notificaciones.DTOs;
global using RestaurantePro.Application.Core.Notificaciones.Commands.CrearNotificacion;
global using RestaurantePro.Application.Comercial.Clientes.DTOs;
global using RestaurantePro.Application.Operaciones.Comandas.DTOs;
global using RestaurantePro.Application.Operaciones.Mesas.DTOs;
global using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
global using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
global using RestaurantePro.Application.Comercial.Facturacion.DTOs;
global using RestaurantePro.Application.Comercial.Promociones.DTOs;
global using RestaurantePro.Application.Comercial.Reportes.DTOs;
global using RestaurantePro.Application.Operaciones.Reportes.DTOs;
global using RestaurantePro.Application.Inventario.Reportes.DTOs;
global using RestaurantePro.Api.Common;
global using RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders;
global using RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Comercial;
global using RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Operaciones;
global using RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Core;
global using RestaurantePro.Domain.Core.Notificaciones.Entities;
global using RestaurantePro.Domain.Core.Notificaciones.Enums;

// 🔧 CONFIGURACIÓN GLOBAL PARA TESTS DE INTEGRACIÓN
[assembly: CollectionBehavior(DisableTestParallelization = true)]

// 🔧 CONFIGURACIÓN PARA REDUCIR CONFLICTOS DE CONCURRENCIA
namespace RestaurantePro.Api.IntegrationTests;

/// <summary>
/// Configuración global para tests de integración
/// </summary>
public static class TestConfiguration
{
    /// <summary>
    /// Configuración de timeout para tests
    /// </summary>
    public const int TestTimeoutSeconds = 30;
    
    /// <summary>
    /// Configuración de retry para tests inestables
    /// </summary>
    public const int MaxRetries = 2;
    
    /// <summary>
    /// Configuración de delay entre retries
    /// </summary>
    public const int RetryDelayMs = 100;
} 