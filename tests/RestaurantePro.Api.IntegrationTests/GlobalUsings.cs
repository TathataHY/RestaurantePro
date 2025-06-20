global using Xunit;
global using Xunit.Abstractions;
global using FluentAssertions;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.EntityFrameworkCore;
global using System.Net;
global using System.Net.Http.Json;
global using System.Text.Json;
global using RestaurantePro.Api;
global using RestaurantePro.Infrastructure.Persistence.Contexts;
global using RestaurantePro.Domain.Core.Productos.Entities;
global using RestaurantePro.Domain.Core.Productos.ValueObjects;
global using RestaurantePro.Domain.Comercial.Clientes.Entities;
global using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
global using RestaurantePro.Domain.Operaciones.Comandas.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
global using RestaurantePro.Api.Common;

// Application Commands & Queries - Comercial
global using RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente;
global using RestaurantePro.Application.Comercial.Clientes.DTOs;

// Application Common
global using RestaurantePro.Application.Common.Models;
global using RestaurantePro.Application.Common.DTOs;

// Test Base Classes
global using RestaurantePro.Api.IntegrationTests.TestBase; 