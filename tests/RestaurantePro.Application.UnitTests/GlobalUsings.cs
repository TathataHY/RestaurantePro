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
global using RestaurantePro.Domain.Core.SharedKernel.Exceptions;
global using RestaurantePro.Domain.Core.Productos.Entities;
global using RestaurantePro.Domain.Core.Productos.ValueObjects;
global using RestaurantePro.Domain.Core.Productos.Interfaces;
global using RestaurantePro.Domain.Core.Productos.Builders;
global using RestaurantePro.Domain.Core.Productos.Services;

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
global using RestaurantePro.Application.Common.Enums;

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
global using RestaurantePro.Application.Operaciones.Mesas.Commands.TransferirMesa;
global using RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerEstadoMesas;
global using RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesasDisponibles;
global using RestaurantePro.Application.Operaciones.Mesas.DTOs;

// Application - Operaciones Reservaciones
global using RestaurantePro.Application.Operaciones.Reservaciones.EventHandlers.ReservacionCreada;
global using RestaurantePro.Application.Operaciones.Reservaciones.Commands.CancelarReservacion;
global using RestaurantePro.Application.Operaciones.Reservaciones.Commands.ConfirmarReservacion;
global using RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;
global using RestaurantePro.Application.Operaciones.Reservaciones.Commands.ModificarReservacion;
global using RestaurantePro.Application.Operaciones.Reservaciones.Queries.ConsultarDisponibilidad;
global using RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesCliente;
global using RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesPorFecha;
global using RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionPorId;
global using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;

// Application - Inventario
global using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientePorId;
global using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesPaginados;
global using RestaurantePro.Application.Inventario.Ingredientes.Queries.ObtenerIngredientesBajoStock;
global using RestaurantePro.Application.Inventario.Ingredientes.Commands.ActualizarStock;
global using RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;
global using RestaurantePro.Application.Inventario.Ingredientes.Commands.AjustarInventario;
global using RestaurantePro.Application.Inventario.Ingredientes.DTOs;
global using RestaurantePro.Application.Inventario.Reportes.Queries.ObtenerAnalisisInventario;
global using RestaurantePro.Application.Inventario.Reportes.DTOs;

// Domain - Proveedores
global using RestaurantePro.Domain.Proveedores.Entities;
global using RestaurantePro.Domain.Proveedores.Interfaces;
global using RestaurantePro.Domain.Proveedores.Enums;

// Domain - Operaciones
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
global using RestaurantePro.Domain.Operaciones.Services;
global using IDisponibilidadService = RestaurantePro.Domain.Operaciones.Services.IDisponibilidadService;
global using IValidacionReservacionService = RestaurantePro.Domain.Operaciones.Services.IValidacionReservacionService;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Events.Reservacion;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
global using RestaurantePro.Domain.Operaciones.Reservaciones.Interfaces;

// Domain - Inventario
global using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
global using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Entities;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Interfaces;
global using RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;
global using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Enums;
global using RestaurantePro.Domain.Inventario.Services;

// Domain - Notificaciones
global using RestaurantePro.Domain.Core.Notificaciones.Services;
global using RestaurantePro.Domain.Core.Notificaciones.Enums;
global using RestaurantePro.Domain.Core.Notificaciones.Entities;

// Domain - Clientes
global using RestaurantePro.Domain.Comercial.Clientes.Entities;
global using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
global using RestaurantePro.Domain.Comercial.Clientes.Builders;
global using RestaurantePro.Domain.Comercial.Clientes.Enums;
global using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;

// Application - Comandas
global using RestaurantePro.Application.Operaciones.Comandas.DTOs;
global using RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerHistorialComandas;
global using RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandasActivas;
global using RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandaPorId;
global using RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarEstadoComanda;
global using RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarItemComanda;
global using RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;
global using RestaurantePro.Application.Operaciones.Comandas.Commands.FinalizarComanda;
global using RestaurantePro.Application.Operaciones.Comandas.Commands.DividirComanda;
global using RestaurantePro.Application.Operaciones.Comandas.Commands.UnificarComandas;
global using RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaCreada;
global using RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaFinalizada;

// Domain - Comandas
global using RestaurantePro.Domain.Operaciones.Comandas.Entities;
global using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
global using RestaurantePro.Domain.Operaciones.Comandas.Enums;
global using RestaurantePro.Domain.Operaciones.Comandas.ValueObjects;

// Application - Common Testing
global using RestaurantePro.Application.UnitTests.Common;

// Application - Clientes
global using RestaurantePro.Application.Comercial.Clientes.Commands.ActualizarCliente;
global using RestaurantePro.Application.Comercial.Clientes.Commands.CrearCliente;
global using RestaurantePro.Application.Comercial.Clientes.Commands.DesactivarCliente;
global using RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientePorId;
global using RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientesPaginados;
global using RestaurantePro.Application.Comercial.Clientes.Queries.BuscarClientesPorEmail;
global using RestaurantePro.Application.Comercial.Clientes.DTOs;

// Application - Facturación
global using RestaurantePro.Application.Comercial.Facturacion.Commands.AnularFactura;
global using RestaurantePro.Application.Comercial.Facturacion.Commands.CrearFactura;
global using RestaurantePro.Application.Comercial.Facturacion.Commands.AplicarDescuento;
global using RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturaPorId;
global using RestaurantePro.Application.Comercial.Facturacion.DTOs;
global using RestaurantePro.Application.Comercial.Facturacion.EventHandlers.FacturaCreada;

// Application - Fidelización
global using RestaurantePro.Application.Comercial.Fidelizacion.Commands.CanjearPuntos;
global using RestaurantePro.Application.Comercial.Fidelizacion.Commands.CrearTarjetaFidelizacion;
global using RestaurantePro.Application.Comercial.Fidelizacion.Commands.AcumularPuntos;
global using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;

// Application - Promociones
global using RestaurantePro.Application.Comercial.Promociones.DTOs;
global using RestaurantePro.Application.Comercial.Promociones.Commands.AplicarPromocion;

// Application - Reportes
global using RestaurantePro.Application.Operaciones.Reportes.Commands.GenerarReporte;
global using RestaurantePro.Application.Operaciones.Reportes.Commands.ProcesarPedidoCompleto;
global using RestaurantePro.Application.Operaciones.Reportes.Queries.ObtenerReporteVentasDiaria;
global using RestaurantePro.Application.Comercial.Reportes.Queries.ObtenerAnalisisFidelizacion;

// Domain - Facturación
global using RestaurantePro.Domain.Comercial.Facturacion.Entities;
global using RestaurantePro.Domain.Comercial.Facturacion.Enums;
global using RestaurantePro.Domain.Comercial.Facturacion.Services;
global using RestaurantePro.Domain.Comercial.Facturacion.Interfaces;

// Domain - Comercial
global using RestaurantePro.Domain.Comercial.Services;
global using RestaurantePro.Domain.Comercial.Promociones.Services;
global using RestaurantePro.Domain.Comercial.Promociones.Entities;
global using RestaurantePro.Domain.Comercial.Promociones.Enums;

// Domain - Core
global using RestaurantePro.Domain.Core.Usuarios.Entities;
global using RestaurantePro.Domain.Core.Usuarios.Interfaces;
global using RestaurantePro.Domain.Core.Usuarios.Enums;
global using RestaurantePro.Domain.Core.Services;

// Application - Core Usuarios
global using RestaurantePro.Application.Core.Usuarios.Commands.ActualizarUsuario;
global using RestaurantePro.Application.Core.Usuarios.Commands.CrearUsuario;
global using RestaurantePro.Application.Core.Usuarios.Commands.CambiarPasswordUsuario;
global using RestaurantePro.Application.Core.Usuarios.DTOs;

// Resolve ValidationException ambiguity
global using ValidationException = RestaurantePro.Application.Common.Exceptions.ValidationException;

// Type aliases and missing types
global using AcumularPuntosResult = RestaurantePro.Application.Comercial.Fidelizacion.DTOs.AcumulacionPuntosDto;
global using CrearTarjetaFidelizacionResult = RestaurantePro.Application.Comercial.Fidelizacion.DTOs.TarjetaFidelizacionDto;
global using IAuditingService = RestaurantePro.Application.Common.Interfaces.IAuditService;
global using TipoTarjetaFidelizacion = RestaurantePro.Domain.Comercial.Clientes.Enums.TipoTarjetaFidelizacion;
global using IDateTimeService = RestaurantePro.Domain.Core.Base.Services.IDateTimeService;
global using DescuentoFactura = RestaurantePro.Domain.Comercial.Facturacion.Entities.DescuentoFactura;
global using TipoPersonalizacion = RestaurantePro.Domain.Operaciones.Comandas.Enums.TipoPersonalizacion;
global using DetalleOrdenCompra = RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities.ItemOrdenCompra;
global using DisponibilidadProductoDto = RestaurantePro.Application.Core.Productos.DTOs.DisponibilidadProductoDto;
global using AnalisisIngredienteDto = RestaurantePro.Application.Core.Productos.DTOs.AnalisisIngredienteDto;
global using IClienteBusinessService = RestaurantePro.Domain.Comercial.Services.IComercialServiceFacade;
global using IValidacionInventarioService = RestaurantePro.Domain.Inventario.Services.IValidacionInventarioService;
global using IAlertaStockService = RestaurantePro.Domain.Inventario.Services.IAlertaStockService;
global using IInventarioAuditService = RestaurantePro.Application.Common.Interfaces.IAuditService;

// Missing interfaces and services for tests
global using ISecurityValidationService = RestaurantePro.Application.Common.Interfaces.IAuditService;

// Type aliases for missing services (commented out as they don't exist in the actual project)
// Note: These types are referenced in tests but don't exist in the actual codebase
// Tests should be updated to mock these interfaces or use existing ones

// global using IPagoService = RestaurantePro.Application.Common.Interfaces.IPagoService;
// global using IFacturacionService = RestaurantePro.Application.Common.Interfaces.IFacturacionService;
// global using IFidelizacionService = RestaurantePro.Application.Common.Interfaces.IFidelizacionService;
// global using IDisponibilidadService = RestaurantePro.Application.Common.Interfaces.IDisponibilidadService;
// global using IValidacionReservacionService = RestaurantePro.Application.Common.Interfaces.IValidacionReservacionService;
// global using IOrquestadorWorkflowService = RestaurantePro.Application.Common.Interfaces.IOrquestadorWorkflowService;

// global using LiberarMesaResult = RestaurantePro.Application.Operaciones.Mesas.DTOs.LiberarMesaDto;
// global using ProcesarPedidoCompletoDto = RestaurantePro.Application.Operaciones.Reportes.DTOs.ProcesarPedidoCompletoDto;
// global using CrearItemComandaDto = RestaurantePro.Application.Operaciones.Comandas.DTOs.CrearItemComandaDto;
// global using CrearPersonalizacionItemDto = RestaurantePro.Application.Operaciones.Comandas.DTOs.CrearPersonalizacionItemDto;
// global using ItemPedidoDto = RestaurantePro.Application.Operaciones.Reportes.DTOs.ItemPedidoDto;
// global using PrediccionSemanal = RestaurantePro.Application.Inventario.Reportes.DTOs.PrediccionSemanal;
// global using StockOptimoIngrediente = RestaurantePro.Application.Inventario.Reportes.DTOs.StockOptimoIngrediente;

// global using TipoProveedor = RestaurantePro.Domain.Proveedores.Enums.TipoProveedor;
// global using TipoMesa = RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums.TipoMesa;

// global using VerificarDisponibilidadProductoHandler = RestaurantePro.Application.Core.Productos.Queries.VerificarDisponibilidadProducto.VerificarDisponibilidadProductoHandler;
// global using CrearTarjetaFidelizacionValidator = RestaurantePro.Application.Comercial.Fidelizacion.Validators.CrearTarjetaFidelizacionValidator;
