// Testing Frameworks
global using Xunit;
global using FluentAssertions;
global using Moq;

// .NET Base
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Reflection;

// Microsoft Extensions
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

// MediatR para tests de handlers
global using MediatR;

// AutoMapper para tests de mapeo
global using AutoMapper;

// FluentValidation para tests de validadores
global using FluentValidation;

// Domain - Referencias para testing
global using RestaurantePro.Domain.Core.SharedKernel.Results;
global using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
global using RestaurantePro.Domain.Core.SharedKernel.Validation;
global using RestaurantePro.Domain.Core.Productos.Entities;
global using RestaurantePro.Domain.Core.Productos.ValueObjects;
global using RestaurantePro.Domain.Core.Productos.Interfaces;
global using RestaurantePro.Domain.Core.Productos.Builders;

// Application - Lo que vamos a testear
global using RestaurantePro.Application.Config.Mappings;
global using RestaurantePro.Application.Config.DependencyInjection;
global using RestaurantePro.Application.Core.Productos.DTOs;
global using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
global using RestaurantePro.Application.Core.Productos.Commands.ActualizarProducto;
global using RestaurantePro.Application.Core.Productos.Commands.EliminarProducto;
global using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductoPorId;
global using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPaginados;
global using RestaurantePro.Application.Core.Productos.Queries.ObtenerProductosPorCategoria;
global using RestaurantePro.Application.Common.DTOs;
global using RestaurantePro.Application.Common.Behaviors;
global using RestaurantePro.Application.Common.Exceptions;
global using RestaurantePro.Application.Common.Interfaces; 