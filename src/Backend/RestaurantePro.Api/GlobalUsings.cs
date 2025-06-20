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
global using Microsoft.AspNetCore.Builder;

// System
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using DataAnnotations = System.ComponentModel.DataAnnotations;

// MediatR
global using MediatR;

// RestaurantePro - Application Layer (Commands, Queries, DTOs)
global using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
global using RestaurantePro.Application.Core.Productos.Commands.ActualizarProducto;
global using RestaurantePro.Application.Core.Productos.Commands.EliminarProducto;
global using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductoPorId;
global using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPaginados;
global using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPorCategoria;
global using RestaurantePro.Application.Core.Productos.DTOs;

// RestaurantePro - Application Common
global using RestaurantePro.Application.Common.DTOs;
global using RestaurantePro.Application.Common.Models;
global using RestaurantePro.Application.Common.Exceptions;

// Alias para evitar conflictos de ValidationException
global using AppValidationException = RestaurantePro.Application.Common.Exceptions.ValidationException;

// RestaurantePro - Domain Common
global using RestaurantePro.Domain.Core.SharedKernel.Results;

// RestaurantePro - API Common
global using RestaurantePro.Api.Common; 