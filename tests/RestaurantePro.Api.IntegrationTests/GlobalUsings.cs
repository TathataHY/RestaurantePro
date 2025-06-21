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

// Application DTOs - Inventario
global using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;
global using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

// Application DTOs - Operaciones
global using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
global using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
global using RestaurantePro.Application.Operaciones.Comandas.DTOs;
global using RestaurantePro.Application.Operaciones.Mesas.DTOs;

// Application DTOs - Core
global using RestaurantePro.Application.Core.Productos.DTOs;
global using RestaurantePro.Application.Core.Usuarios.DTOs;

// Application DTOs - Comercial
global using RestaurantePro.Application.Comercial.Facturacion.DTOs;
global using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;
global using RestaurantePro.Application.Comercial.Promociones.DTOs;

// Enums de dominio para tests de integración
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
global using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;

// Application Commands - Inventario
global using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.CrearOrdenCompra;
global using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.ActualizarOrdenCompra;
global using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.AprobarOrdenCompra;
global using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.RechazarOrdenCompra;
global using RestaurantePro.Application.Inventario.OrdenesCompra.Commands.RecibirOrdenCompra;

// Application Commands - Operaciones
global using RestaurantePro.Application.Operaciones.Preparaciones.Commands.CrearPreparacion;
global using RestaurantePro.Application.Operaciones.Preparaciones.Commands.ActualizarPreparacion;
global using RestaurantePro.Application.Operaciones.Preparaciones.Commands.IniciarPreparacion;
global using RestaurantePro.Application.Operaciones.Preparaciones.Commands.CompletarPreparacion;
global using RestaurantePro.Application.Operaciones.Preparaciones.Commands.CancelarPreparacion;

// Test Base Classes
global using RestaurantePro.Api.IntegrationTests.TestBase; 