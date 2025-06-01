// Testing Frameworks
global using Xunit;
global using FluentAssertions;
global using Moq;
global using Moq.Language;
global using Moq.Language.Flow;

// .NET Base
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Reflection;
global using System.Text.Json;
global using System.Text.RegularExpressions;
global using System.ComponentModel.DataAnnotations;
global using System.Security.Claims;
global using System.Text;
global using System.Collections.ObjectModel;
global using System.Net.Sockets;

// Microsoft Extensions
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Caching.Memory;

// MediatR para tests de handlers
global using MediatR;

// AutoMapper para tests de mapeo
global using AutoMapper;

// FluentValidation para tests de validadores
global using FluentValidation;
global using FluentValidation.Results;

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
global using RestaurantePro.Domain.Core.Productos.Services;

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

// Application - Fidelización DTOs
global using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

// Application - Promociones DTOs
global using RestaurantePro.Application.Comercial.Promociones.DTOs;

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
global using RestaurantePro.Domain.Comercial.Promociones.Services;

// Domain - Core
global using RestaurantePro.Domain.Core.Usuarios.Entities;
global using RestaurantePro.Domain.Core.Notificaciones.Entities;

// Domain - Operaciones
global using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;

// Domain - Reservaciones Enums
global using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;

// Application - Reportes
global using RestaurantePro.Application.Operaciones.Reportes.Commands.GenerarReporte;

// Domain - Inventario Enums
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;

// Application - Reportes DTOs
global using RestaurantePro.Application.Inventario.Reportes.DTOs;

// Domain - Reservaciones Interfaces  
global using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;

// Application - Facturación Queries
global using RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturaPorId;

// Application - Clientes Queries
global using RestaurantePro.Application.Comercial.Clientes.Queries.BuscarClientesPorEmail;

// Application - Common Enums
global using RestaurantePro.Application.Common.Enums;

// Domain - Comercial Promociones
global using RestaurantePro.Domain.Comercial.Promociones.Entities;

// Application - Promociones Commands
global using RestaurantePro.Application.Comercial.Promociones.Commands.AplicarPromocion;

// Application - Facturación Commands
global using RestaurantePro.Application.Comercial.Facturacion.Commands.AplicarDescuento;

// Domain - Core ValueObjects
global using RestaurantePro.Domain.Core.Usuarios.Enums;

// Domain - Inventario Compras
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;

// Domain - Comercial Promociones Enums
global using RestaurantePro.Domain.Comercial.Promociones.Enums;

// Application - Core Usuarios Commands
global using RestaurantePro.Application.Core.Usuarios.Commands.ActualizarUsuario;
global using RestaurantePro.Application.Core.Usuarios.Commands.CrearUsuario;
global using RestaurantePro.Application.Core.Usuarios.Commands.CambiarPasswordUsuario;

// Application - Core Usuarios DTOs
global using RestaurantePro.Application.Core.Usuarios.DTOs;

// Application - Operaciones Comandas Commands
global using RestaurantePro.Application.Operaciones.Comandas.Commands.DividirComanda;
global using RestaurantePro.Application.Operaciones.Comandas.Commands.UnificarComandas;

// Application - Operaciones Mesas Commands
global using RestaurantePro.Application.Operaciones.Mesas.Commands.TransferirMesa;

// Application - Operaciones Mesas Queries
global using RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerEstadoMesas;
global using RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesasDisponibles;

// Application - Operaciones Reservaciones Commands
global using RestaurantePro.Application.Operaciones.Reservaciones.Commands.CancelarReservacion;
global using RestaurantePro.Application.Operaciones.Reservaciones.Commands.ConfirmarReservacion;
global using RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;
global using RestaurantePro.Application.Operaciones.Reservaciones.Commands.ModificarReservacion;

// Application - Operaciones Reservaciones Queries
global using RestaurantePro.Application.Operaciones.Reservaciones.Queries.ConsultarDisponibilidad;
global using RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesCliente;
global using RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesPorFecha;
global using RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionPorId;

// Application - Operaciones Reservaciones DTOs
global using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

// Application - Operaciones Reportes Commands
global using RestaurantePro.Application.Operaciones.Reportes.Commands.ProcesarPedidoCompleto;

// Domain - Core Usuarios Repositories
global using RestaurantePro.Domain.Core.Usuarios.Interfaces;

// Domain - Core Services
global using RestaurantePro.Domain.Core.Services;

// Domain - Core Productos Services
global using RestaurantePro.Domain.Core.Productos.Services;

// Application - Common Interfaces
global using RestaurantePro.Application.Common.Interfaces;

// Domain - Core SharedKernel Validation
global using RestaurantePro.Domain.Core.SharedKernel.Validation;

// Domain - Comercial Services
global using RestaurantePro.Domain.Comercial.Services;

// References for specific missing types found in tests

// Domain - Comercial Clientes Enums (TipoTarjetaFidelizacion, etc.)
global using RestaurantePro.Domain.Comercial.Clientes.Enums;

// Domain - Comercial Promociones Enums (TipoDescuento, etc.)
global using RestaurantePro.Domain.Comercial.Promociones.Enums;

// Domain - Facturación Enums (TipoFactura)
global using RestaurantePro.Domain.Comercial.Facturacion.Enums;

// Domain - Comercial Fidelizacion Enums
// global using RestaurantePro.Domain.Comercial.Fidelizacion.Enums; // Este namespace no existe - usar individual

// Application - Common Interfaces (IAuditingService renamed to IAuditService)
// Note: The interface is actually called IAuditService, not IAuditingService

// Type aliases para compatibilidad con tests que usan nombres incorrectos
// NOTE: Estos types no existen en el proyecto, los tests los usan incorrectamente

// AcumularPuntosResult -> debe ser Result<AcumulacionPuntosDto>
global using AcumularPuntosResult = RestaurantePro.Application.Comercial.Fidelizacion.DTOs.AcumulacionPuntosDto;

// CrearTarjetaFidelizacionResult -> debe ser Result<TarjetaFidelizacionDto>  
global using CrearTarjetaFidelizacionResult = RestaurantePro.Application.Comercial.Fidelizacion.DTOs.TarjetaFidelizacionDto;

// IAuditingService -> should be IAuditService
global using IAuditingService = RestaurantePro.Application.Common.Interfaces.IAuditService;

// TipoDescuento no existe como enum - los tests usan strings, pero necesitan referencia
// global using TipoDescuento = System.String; // No funciona bien para tests

// Domain - Comercial Fidelizacion Enums specific
global using TipoTarjetaFidelizacion = RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTarjetaFidelizacion;

// Additional missing interfaces and services
global using IDateTimeService = RestaurantePro.Domain.Core.Base.Services.IDateTimeService;

// Resolve ValidationException ambiguity
global using ValidationException = RestaurantePro.Application.Common.Exceptions.ValidationException;

// Domain - Comercial Facturacion Entities
global using DescuentoFactura = RestaurantePro.Domain.Comercial.Facturacion.Entities.DescuentoFactura;

// Domain - Operaciones Comandas Enums
global using TipoPersonalizacion = RestaurantePro.Domain.Operaciones.Comandas.Enums.TipoPersonalizacion;

// Alias para entidades de órdenes de compra
global using DetalleOrdenCompra = RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities.ItemOrdenCompra;

// Application - Core Productos DTOs adicionales
global using DisponibilidadProductoDto = RestaurantePro.Application.Core.Productos.DTOs.DisponibilidadProductoDto;
global using AnalisisIngredienteDto = RestaurantePro.Application.Core.Productos.DTOs.AnalisisIngredienteDto;

// Alias para servicios faltantes en pruebas
global using IClienteBusinessService = RestaurantePro.Domain.Comercial.Services.IComercialServiceFacade;

// Domain - Inventario Services
global using IValidacionInventarioService = RestaurantePro.Domain.Inventario.Services.IValidacionInventarioService;
global using IAlertaStockService = RestaurantePro.Domain.Inventario.Services.IAlertaStockService;

// Alias para auditoría de inventario usando el servicio general
global using IInventarioAuditService = RestaurantePro.Application.Common.Interfaces.IAuditService;

// Application - Inventario Commands
global using RestaurantePro.Application.Inventario.Ingredientes.Commands.AjustarInventario;

// Domain - Inventario Interfaces adicionales
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;

// Domain - Core Productos Services
global using RestaurantePro.Domain.Core.Productos.Services;
