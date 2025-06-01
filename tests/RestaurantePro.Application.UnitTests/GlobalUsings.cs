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
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Storage;

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

// Application - Proveedores
global using RestaurantePro.Application.Proveedores.Proveedores.Commands.CrearProveedor;
global using RestaurantePro.Application.Proveedores.Proveedores.Commands.ActualizarProveedor;
global using RestaurantePro.Application.Proveedores.Proveedores.Commands.DesactivarProveedor;
global using RestaurantePro.Application.Proveedores.Proveedores.Queries.ObtenerProveedorPorId;
global using RestaurantePro.Application.Proveedores.Proveedores.Queries.ObtenerProveedoresPaginados;
global using RestaurantePro.Application.Proveedores.Proveedores.DTOs;

// Application - Contactos Proveedor
global using RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.AgregarContacto;
global using RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.ActualizarContacto;
global using RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.EliminarContacto;
global using RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs;

// Application - Operaciones Mesas
global using RestaurantePro.Application.Operaciones.Mesas.Commands.AsignarMesa;
global using RestaurantePro.Application.Operaciones.Mesas.Commands.LiberarMesa;
global using RestaurantePro.Application.Operaciones.Mesas.Commands.CambiarEstadoMesa;
global using RestaurantePro.Application.Operaciones.Mesas.DTOs;

// Application - Operaciones Reservaciones
global using RestaurantePro.Application.Operaciones.Reservaciones.EventHandlers.ReservacionCreada;

// Application - Inventario
global using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientePorId;
global using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesPaginados;
global using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesBajoStock;
global using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

// Domain - Core
global using RestaurantePro.Domain.Core.SharedKernel.Exceptions;

// Domain - Proveedores
global using RestaurantePro.Domain.Proveedores.Entities;
global using RestaurantePro.Domain.Proveedores.Interfaces;
global using RestaurantePro.Domain.Proveedores.Enums;

// Domain - Operaciones
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
global using RestaurantePro.Domain.Operaciones.Services;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion;

// Domain - Inventario
global using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
global using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

// Domain - Notificaciones
global using RestaurantePro.Domain.Core.Notificaciones.Services;
global using RestaurantePro.Domain.Core.Notificaciones.Enums;

// Domain - Clientes
global using RestaurantePro.Domain.Comercial.Clientes.Entities;
global using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
global using RestaurantePro.Domain.Comercial.Clientes.Builders;
global using RestaurantePro.Domain.Comercial.Clientes.Enums;

// Application - Comandas
global using RestaurantePro.Application.Operaciones.Comandas.DTOs;
global using RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerHistorialComandas;
global using RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandasActivas;
global using RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandaPorId;

// Domain - Comandas
global using RestaurantePro.Domain.Operaciones.Comandas.Entities;
global using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Comandas.Enums;
global using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;

// Domain - Clientes ValueObjects
global using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;

// Application - Common Testing
global using RestaurantePro.Application.UnitTests.Common;

// Application - Clientes
global using RestaurantePro.Application.Comercial.Clientes.Commands.ActualizarCliente;
global using RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente;
global using RestaurantePro.Application.Comercial.Clientes.Commands.DesactivarCliente;
global using RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientePorId;
global using RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientesPaginados;
global using RestaurantePro.Application.Comercial.Clientes.DTOs;

// Application - Core Productos
global using RestaurantePro.Application.Core.Productos.Queries.VerificarDisponibilidadProducto;
global using RestaurantePro.Domain.Core.Productos.Services;

// Domain - Inventario
global using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;

// Application - Comandas Commands
global using RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarEstadoComanda;
global using RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarItemComanda;
global using RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;
global using RestaurantePro.Application.Operaciones.Comandas.Commands.FinalizarComanda;

// Application - Comandas EventHandlers
global using RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaCreada;
global using RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaFinalizada;

// Application - Inventario
global using RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarStock;
global using RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;
global using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAnalisisInventario;

// Application - Facturación
global using RestaurantePro.Application.Comercial.Facturacion.Commands.AnularFactura;
global using RestaurantePro.Application.Comercial.Facturacion.DTOs;
global using RestaurantePro.Application.Comercial.Facturacion.EventHandlers.FacturaCreada;
global using RestaurantePro.Application.Operaciones.Reportes.Queries.ObtenerReporteVentasDiaria;
global using RestaurantePro.Application.Comercial.Facturacion.Commands.CrearFactura;

// Application - Fidelización
global using RestaurantePro.Application.Comercial.Fidelizacion.Commands.CanjearPuntos;
global using RestaurantePro.Application.Comercial.Fidelizacion.Commands.CrearTarjetaFidelizacion;
global using RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerAnalisisFidelizacion;
global using RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos;

// Domain - Inventario
global using RestaurantePro.Domain.Inventario.Services;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;

// Domain - Facturación
global using RestaurantePro.Domain.Comercial.Facturacion.Entities;
global using RestaurantePro.Domain.Comercial.Facturacion.Enums;
global using RestaurantePro.Domain.Comercial.Facturacion.Services;
global using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;

// Domain - Comercial
global using RestaurantePro.Domain.Comercial.Services;
global using RestaurantePro.Domain.Comercial.Clientes.Enums;
global using RestaurantePro.Domain.Comercial.Promociones.Services;

// Domain - Core
global using RestaurantePro.Domain.Core.Usuarios.Entities;
global using RestaurantePro.Domain.Core.Notificaciones.Entities;
global using RestaurantePro.Domain.Core.SharedKernel.Enums;

// Domain - Operaciones
global using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;

// Application - ICommunicationService
global using RestaurantePro.Application.Common.Interfaces; 