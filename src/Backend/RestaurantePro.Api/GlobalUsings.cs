// ===================================================================
// GlobalUsings.cs - RestaurantePro API
// ===================================================================
// Importaciones globales para toda la capa de API
// Elimina la necesidad de repetir los mismos 'using' en cada archivo
// ===================================================================

// .NET Core & ASP.NET Core
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Hosting;
global using Microsoft.AspNetCore.Mvc.ModelBinding;
global using Microsoft.AspNetCore.Mvc.Filters;

// Entity Framework Core
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Query;

// MediatR para CQRS
global using MediatR;

// AutoMapper para mapeo de objetos
global using AutoMapper;

// FluentValidation para validaciones
global using FluentValidation;

// Swagger/OpenAPI
global using Microsoft.OpenApi.Models;
global using Swashbuckle.AspNetCore.SwaggerGen;
global using Swashbuckle.AspNetCore.Annotations;

// System & Collections
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.ComponentModel.DataAnnotations;
global using System.Text.Json;
global using System.Text.Json.Serialization;

// API Common Components
global using RestaurantePro.Api.Common;

// Domain - SharedKernel
global using RestaurantePro.Domain.Core.SharedKernel.Results;
global using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
global using RestaurantePro.Domain.Core.SharedKernel.Exceptions;
global using RestaurantePro.Domain.Core.SharedKernel.Interfaces;

// Domain - Core Entities
global using RestaurantePro.Domain.Core.Productos.Entities;
global using RestaurantePro.Domain.Core.Usuarios.Entities;
global using RestaurantePro.Domain.Core.Notificaciones.Entities;
global using RestaurantePro.Domain.Core.Notificaciones.Enums;

// Domain - Comercial Entities
global using RestaurantePro.Domain.Comercial.Clientes.Entities;
global using RestaurantePro.Domain.Comercial.Facturacion.Entities;
global using RestaurantePro.Domain.Comercial.Promociones.Entities;

// Domain - Operaciones Entities
global using RestaurantePro.Domain.Operaciones.Comandas.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
global using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;

// Domain - Inventario Entities
global using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;

// Domain - Proveedores Entities
global using RestaurantePro.Domain.Proveedores.Entities;

// Infrastructure - Persistence
global using RestaurantePro.Infrastructure.Persistence.Contexts;
global using RestaurantePro.Infrastructure.Persistence.Repositories;
global using RestaurantePro.Infrastructure.Persistence.Configurations;

// Application - Common
global using RestaurantePro.Application.Common.DTOs;
global using RestaurantePro.Application.Common.Interfaces;
global using RestaurantePro.Application.Common.Exceptions;

// Application - Core DTOs
global using RestaurantePro.Application.Core.Productos.DTOs;
global using RestaurantePro.Application.Core.Usuarios.DTOs;
global using RestaurantePro.Application.Core.Notificaciones.DTOs;

// Application - Core Commands
global using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
global using RestaurantePro.Application.Core.Productos.Commands.ActualizarProducto;
global using RestaurantePro.Application.Core.Productos.Commands.EliminarProducto;
global using RestaurantePro.Application.Core.Notificaciones.Commands.CrearNotificacion;
global using RestaurantePro.Application.Core.Notificaciones.Commands.MarcarComoLeida;
global using RestaurantePro.Application.Core.Notificaciones.Commands.MarcarTodasComoLeidas;
global using RestaurantePro.Application.Core.Notificaciones.Commands.EliminarNotificacion;

// Application - Core Queries
global using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductoPorId;
global using RestaurantePro.Application.Core.Notificaciones.Queries.ObtenerNotificaciones;
global using RestaurantePro.Application.Core.Notificaciones.Queries.ObtenerNotificacionPorId;
global using RestaurantePro.Application.Core.Notificaciones.Queries.ObtenerContadorNoLeidas;

// Application - Comercial DTOs
global using RestaurantePro.Application.Comercial.Clientes.DTOs;
global using RestaurantePro.Application.Comercial.Facturacion.DTOs;
global using RestaurantePro.Application.Comercial.Promociones.DTOs;

// Application - Operaciones DTOs
global using RestaurantePro.Application.Operaciones.Comandas.DTOs;
global using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
global using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;

// Application - Inventario DTOs
global using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
global using RestaurantePro.Application.Inventario.OrdenesCompra.DTOs;

// Application - Proveedores DTOs
global using RestaurantePro.Application.Proveedores.Proveedores.DTOs;

// Infrastructure - Services
global using RestaurantePro.Infrastructure.Services;
global using RestaurantePro.Infrastructure.Identity;
global using RestaurantePro.Infrastructure.Caching;
global using RestaurantePro.Infrastructure.ExternalServices;
global using RestaurantePro.Infrastructure.Monitoring;

// Infrastructure - Background Tasks
global using RestaurantePro.Infrastructure.BackgroundTasks.Jobs;
global using RestaurantePro.Infrastructure.BackgroundTasks.Workers;

// Infrastructure - Dependency Injection
global using RestaurantePro.Infrastructure.DependencyInjection; 