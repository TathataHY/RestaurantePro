#nullable enable

// System
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Reflection;
global using System.Text;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Linq.Expressions;

// Microsoft
global using Microsoft.AspNetCore.Identity;
global using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.AspNetCore.Authentication.JwtBearer;

// Application Interfaces
global using RestaurantePro.Application.Common.Interfaces;
global using RestaurantePro.Application.Common.Models;

// Domain - Base
global using RestaurantePro.Domain.Core.Base;
global using RestaurantePro.Domain.Core.Base.Events;
global using RestaurantePro.Domain.Core.Base.Interfaces;
global using RestaurantePro.Domain.Core.Base.Services;
global using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
global using RestaurantePro.Domain.Core.SharedKernel.Results;

// Domain - Core
global using RestaurantePro.Domain.Core.Productos.Entities;
global using RestaurantePro.Domain.Core.Productos.Interfaces;
global using RestaurantePro.Domain.Core.Usuarios.Entities;
global using RestaurantePro.Domain.Core.Usuarios.Interfaces;
global using RestaurantePro.Domain.Core.Notificaciones.Entities;
global using RestaurantePro.Domain.Core.Notificaciones.Interfaces;

// Domain - Comercial
global using RestaurantePro.Domain.Comercial.Clientes.Entities;
global using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
global using RestaurantePro.Domain.Comercial.Facturacion.Entities;
global using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;

// Domain - Operaciones
global using RestaurantePro.Domain.Operaciones.Comandas.Entities;
global using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
global using RestaurantePro.Domain.Operaciones.Preparaciones.Interfaces;

// Domain - Inventario
global using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Interfaces;

// Domain - Proveedores
global using RestaurantePro.Domain.Proveedores.Entities;
global using RestaurantePro.Domain.Proveedores.Interfaces;

// Infrastructure
global using RestaurantePro.Infrastructure.Identity.Models;
global using RestaurantePro.Infrastructure.Persistence.Contexts;
global using RestaurantePro.Infrastructure.Persistence.Interceptors;
global using RestaurantePro.Infrastructure.Persistence.Repositories.Base;
global using RestaurantePro.Infrastructure.Persistence.Repositories.Core;
global using RestaurantePro.Infrastructure.Persistence.Repositories.Comercial;
global using RestaurantePro.Infrastructure.Persistence.Repositories.Operaciones;
global using RestaurantePro.Infrastructure.Persistence.Repositories.Inventario;
global using RestaurantePro.Infrastructure.Persistence.Repositories.Proveedores;

// Alias para resolver ambigüedad
// Alias para tipos que pueden generar ambigüedades
global using IdentityApplicationUser = RestaurantePro.Infrastructure.Identity.Models.ApplicationUser;
global using DomainApplicationUser = RestaurantePro.Domain.Core.Usuarios.Entities.ApplicationUser; 