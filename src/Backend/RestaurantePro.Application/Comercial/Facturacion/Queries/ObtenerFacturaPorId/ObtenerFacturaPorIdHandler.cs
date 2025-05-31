using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;
using RestaurantePro.Domain.Common;

namespace RestaurantePro.Application.Comercial.Facturacion.Queries.ObtenerFacturaPorId;

public class ObtenerFacturaPorIdHandler : IRequestHandler<ObtenerFacturaPorIdQuery, Result<FacturaDetalladaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ObtenerFacturaPorIdHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public ObtenerFacturaPorIdHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<ObtenerFacturaPorIdHandler> logger,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<Result<FacturaDetalladaDto>> Handle(ObtenerFacturaPorIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando consulta de factura: {FacturaId}, Formato: {Formato}, Usuario: {UsuarioId}",
                request.FacturaId, request.FormatoRespuesta, request.UsuarioConsultaId);

            // 1. Construir query base con includes dinámicos
            var queryBase = ConstruirQueryBase(request);

            // 2. Ejecutar consulta principal
            var factura = await queryBase.FirstOrDefaultAsync(cancellationToken);

            if (factura == null)
            {
                return Result.Failure<FacturaDetalladaDto>("La factura especificada no existe.");
            }

            // 3. Construir DTO base
            var facturaDto = await ConstruirFacturaDetalladaDto(factura, request, cancellationToken);

            // 4. Cargar información adicional según configuración
            await CargarInformacionAdicional(facturaDto, factura, request, cancellationToken);

            // 5. Aplicar filtros de seguridad según formato
            AplicarFiltrosSeguridad(facturaDto, request);

            // 6. Calcular métricas si está solicitado
            if (request.IncluirMetricasRentabilidad)
            {
                await CalcularMetricasRentabilidad(facturaDto, factura, cancellationToken);
            }

            // 7. Registrar consulta para auditoría
            await RegistrarConsultaAuditoria(request, factura);

            _logger.LogInformation("Consulta de factura completada: {NumeroFactura}, Elementos incluidos: {ElementosIncluidos}",
                factura.NumeroFactura, string.Join(", ", request.ObtenerElementosAIncluir()));

            return Result.Success(facturaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar factura {FacturaId}", request.FacturaId);
            return Result.Failure<FacturaDetalladaDto>("Error interno al consultar la factura.");
        }
    }

    private IQueryable<Factura> ConstruirQueryBase(ObtenerFacturaPorIdQuery request)
    {
        var query = _context.Facturas.AsQueryable();

        // Includes condicionales para optimizar la consulta
        if (request.IncluirDetallesProductos)
        {
            query = query.Include(f => f.Detalles)
                        .ThenInclude(d => d.Producto);
        }

        if (request.IncluirInformacionCliente)
        {
            query = query.Include(f => f.Cliente);
        }

        if (request.IncluirDescuentos)
        {
            query = query.Include(f => f.Descuentos);
        }

        if (request.IncluirPagos)
        {
            query = query.Include(f => f.Pagos);
        }

        if (request.IncluirComandas)
        {
            query = query.Include(f => f.Comandas);
        }

        return query.Where(f => f.Id == request.FacturaId);
    }

    private async Task<FacturaDetalladaDto> ConstruirFacturaDetalladaDto(
        Factura factura, 
        ObtenerFacturaPorIdQuery request, 
        CancellationToken cancellationToken)
    {
        var dto = new FacturaDetalladaDto
        {
            // Información básica
            Id = factura.Id,
            NumeroFactura = factura.NumeroFactura,
            TipoFactura = factura.TipoFactura.ToString(),
            Estado = factura.Estado.ToString(),
            FechaEmision = factura.FechaEmision,
            FechaVencimiento = factura.FechaVencimiento,
            
            // Información del cliente (si está incluida)
            NombreCliente = request.IncluirInformacionCliente ? factura.NombreCliente : "INFORMACIÓN RESTRINGIDA",
            IdentificacionFiscal = request.IncluirInformacionCliente ? factura.IdentificacionFiscal : "***",
            DireccionCliente = request.IncluirInformacionCliente ? factura.DireccionCliente : null,
            
            // Totales
            Subtotal = factura.Subtotal,
            TotalImpuestos = factura.TotalImpuestos,
            TotalDescuentos = factura.TotalDescuentos,
            Total = factura.Total,
            TotalPagado = factura.TotalPagado,
            SaldoPendiente = factura.Total - factura.TotalPagado,
            
            // Observaciones
            Observaciones = factura.Observaciones,
            
            // Metadatos de consulta
            FormatoConsulta = request.FormatoRespuesta,
            FechaConsulta = DateTime.UtcNow,
            UsuarioConsulta = request.UsuarioConsultaId,
            MotivoConsulta = request.MotivoConsulta
        };

        // Información de anulación si aplica
        if (factura.Estado == EstadoFactura.Anulada)
        {
            dto.FechaAnulacion = factura.FechaAnulacion;
            dto.MotivoAnulacion = factura.MotivoAnulacion;
            dto.TipoAnulacion = factura.TipoAnulacion;
        }

        return dto;
    }

    private async Task CargarInformacionAdicional(
        FacturaDetalladaDto dto, 
        Factura factura, 
        ObtenerFacturaPorIdQuery request, 
        CancellationToken cancellationToken)
    {
        // Cargar detalles de productos
        if (request.IncluirDetallesProductos)
        {
            dto.Detalles = factura.Detalles.Select(d => new DetalleFacturaDetalladoDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                NombreProducto = d.Producto?.Nombre ?? "Producto no disponible",
                CodigoProducto = d.Producto?.Codigo,
                CategoriaProducto = d.Producto?.Categoria,
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                UnidadMedida = d.UnidadMedida,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal,
                ImporteImpuesto = d.ImporteImpuesto,
                ImporteDescuento = d.ImporteDescuento,
                Total = d.Total,
                PorcentajeImpuesto = d.PorcentajeImpuesto,
                NotasEspeciales = d.NotasEspeciales
            }).ToList();
        }

        // Cargar información completa del cliente
        if (request.IncluirInformacionCliente && factura.Cliente != null)
        {
            dto.InformacionCliente = new ClienteDetalladoDto
            {
                Id = factura.Cliente.Id,
                Nombre = factura.Cliente.Nombre,
                Email = factura.Cliente.Email,
                Telefono = factura.Cliente.Telefono,
                IdentificacionFiscal = factura.Cliente.IdentificacionFiscal,
                Direccion = factura.Cliente.Direccion,
                TipoCliente = factura.Cliente.TipoCliente,
                PuntosFidelizacion = factura.Cliente.PuntosFidelizacion,
                TotalCompras = factura.Cliente.TotalCompras,
                FechaRegistro = factura.Cliente.FechaRegistro
            };
        }

        // Cargar descuentos aplicados
        if (request.IncluirDescuentos)
        {
            dto.Descuentos = factura.Descuentos.Select(desc => new DescuentoDetalladoDto
            {
                Id = desc.Id,
                TipoDescuento = desc.TipoDescuento,
                Concepto = desc.Concepto,
                Monto = desc.Monto,
                Porcentaje = desc.Porcentaje,
                FechaAplicacion = desc.FechaAplicacion,
                UsuarioAutoriza = desc.UsuarioAutoriza,
                CodigoAutorizacion = desc.CodigoAutorizacion,
                Motivo = desc.Motivo,
                AplicadoAntesImpuestos = desc.AplicadoAntesImpuestos
            }).ToList();
        }

        // Cargar historial de pagos
        if (request.IncluirPagos)
        {
            dto.Pagos = factura.Pagos.Select(pago => new PagoFacturaDetalladoDto
            {
                Id = pago.Id,
                Monto = pago.Monto,
                MetodoPago = pago.MetodoPago,
                FechaPago = pago.FechaPago,
                NumeroReferencia = pago.NumeroReferencia,
                Estado = pago.Estado,
                Observaciones = pago.Observaciones,
                MontoDevuelto = pago.MontoDevuelto,
                FechaDevolucion = pago.FechaDevolucion
            }).ToList();
        }

        // Cargar información de comandas
        if (request.IncluirComandas)
        {
            var comandas = await _context.Comandas
                .Where(c => factura.ComandasIds.Contains(c.Id))
                .Select(c => new ComandaResumenDto
                {
                    Id = c.Id,
                    NumeroComanda = c.NumeroComanda,
                    Estado = c.Estado.ToString(),
                    FechaCreacion = c.FechaCreacion,
                    MesaId = c.MesaId,
                    NumeroMesa = c.Mesa != null ? c.Mesa.Numero : null,
                    TotalItems = c.Items.Count(),
                    ObservacionesEspeciales = c.ObservacionesEspeciales
                })
                .ToListAsync(cancellationToken);

            dto.Comandas = comandas;
        }

        // Cargar información tributaria detallada
        if (request.IncluirInformacionTributaria)
        {
            dto.InformacionTributaria = new InformacionTributariaDto
            {
                BaseImponible = factura.Subtotal - factura.TotalDescuentos,
                ImpuestosDetallados = factura.Detalles
                    .GroupBy(d => d.PorcentajeImpuesto)
                    .Select(g => new ImpuestoDetalladoDto
                    {
                        PorcentajeImpuesto = g.Key,
                        BaseImponible = g.Sum(d => d.Subtotal - d.ImporteDescuento),
                        MontoImpuesto = g.Sum(d => d.ImporteImpuesto)
                    })
                    .ToList(),
                ExentosImpuesto = factura.Detalles.Where(d => d.PorcentajeImpuesto == 0).Sum(d => d.Subtotal),
                RetencionesAplicadas = new List<RetencionDto>(),
                NumeroResolucionDian = "Resolución DIAN 123456",
                RangoFacturacion = "Del 1 al 10000"
            };
        }

        // Cargar auditoría si está solicitada
        if (request.IncluirAuditoria)
        {
            var eventosAuditoria = await _context.EventosAuditoria
                .Where(e => e.EntidadId == factura.Id && e.EntidadTipo == "Factura")
                .OrderByDescending(e => e.FechaEvento)
                .Take(50)
                .Select(e => new EventoAuditoriaDto
                {
                    Id = e.Id,
                    TipoEvento = e.TipoEvento,
                    FechaEvento = e.FechaEvento,
                    UsuarioId = e.UsuarioId,
                    Detalles = e.Detalles,
                    DatosAdicionales = e.DatosAdicionales
                })
                .ToListAsync(cancellationToken);

            dto.AuditoriaMovimientos = eventosAuditoria;
        }

        // Cargar documentos adjuntos si está solicitado
        if (request.IncluirDocumentosAdjuntos)
        {
            var documentos = await _context.DocumentosFactura
                .Where(d => d.FacturaId == factura.Id)
                .Select(d => new DocumentoAdjuntoDto
                {
                    Id = d.Id,
                    NombreArchivo = d.NombreArchivo,
                    TipoDocumento = d.TipoDocumento,
                    TamanoArchivo = d.TamanoArchivo,
                    FechaSubida = d.FechaSubida,
                    UsuarioSubida = d.UsuarioSubida,
                    Descripcion = d.Descripcion,
                    RutaArchivo = d.RutaArchivo // Solo metadatos, no el contenido
                })
                .ToListAsync(cancellationToken);

            dto.DocumentosAdjuntos = documentos;
        }
    }

    private void AplicarFiltrosSeguridad(FacturaDetalladaDto dto, ObtenerFacturaPorIdQuery request)
    {
        // Para formato básico, limitar información sensible
        if (request.FormatoRespuesta == "Basico")
        {
            dto.InformacionCliente = null;
            dto.AuditoriaMovimientos = null;
            dto.DocumentosAdjuntos = null;
            dto.MetricasRentabilidad = null;
            
            // Simplificar detalles
            if (dto.Detalles != null)
            {
                foreach (var detalle in dto.Detalles)
                {
                    detalle.NotasEspeciales = null;
                    detalle.CodigoProducto = null;
                }
            }
        }

        // Para usuarios con permisos limitados, ocultar información sensible
        if (request.ValidarPermisos && request.UsuarioConsultaId.HasValue)
        {
            // Esto se podría expandir con lógica específica por rol
            // Por ahora aplicamos filtros básicos
        }
    }

    private async Task CalcularMetricasRentabilidad(
        FacturaDetalladaDto dto, 
        Factura factura, 
        CancellationToken cancellationToken)
    {
        try
        {
            var metricasRentabilidad = new MetricasRentabilidadDto();

            // Calcular costos de productos
            decimal costoTotal = 0;
            foreach (var detalle in factura.Detalles)
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == detalle.ProductoId, cancellationToken);

                if (producto != null)
                {
                    costoTotal += producto.CostoUnitario * detalle.Cantidad;
                }
            }

            metricasRentabilidad.CostoTotalProductos = costoTotal;
            metricasRentabilidad.IngresosBrutos = factura.Subtotal;
            metricasRentabilidad.DescuentosAplicados = factura.TotalDescuentos;
            metricasRentabilidad.IngresosNetos = factura.Total;
            metricasRentabilidad.MargenBruto = metricasRentabilidad.IngresosNetos - metricasRentabilidad.CostoTotalProductos;
            metricasRentabilidad.PorcentajeMargen = metricasRentabilidad.IngresosNetos > 0 
                ? (metricasRentabilidad.MargenBruto / metricasRentabilidad.IngresosNetos) * 100
                : 0;

            // Calcular métricas por categoría
            var metricasPorCategoria = factura.Detalles
                .GroupBy(d => d.Producto?.Categoria ?? "Sin categoría")
                .Select(g => new MetricaCategoriaDto
                {
                    Categoria = g.Key,
                    CantidadItems = g.Sum(d => d.Cantidad),
                    IngresoCategoria = g.Sum(d => d.Total),
                    PorcentajeContribucion = (g.Sum(d => d.Total) / factura.Total) * 100
                })
                .ToList();

            metricasRentabilidad.RentabilidadPorCategoria = metricasPorCategoria;

            // Calcular impacto de descuentos
            metricasRentabilidad.ImpactoDescuentos = new ImpactoDescuentosDto
            {
                TotalDescuentos = factura.TotalDescuentos,
                PorcentajeDescuentoSobreVenta = (factura.TotalDescuentos / factura.Subtotal) * 100,
                DescuentosPorTipo = factura.Descuentos
                    .GroupBy(d => d.TipoDescuento)
                    .ToDictionary(g => g.Key, g => g.Sum(d => d.Monto))
            };

            dto.MetricasRentabilidad = metricasRentabilidad;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al calcular métricas de rentabilidad para factura {FacturaId}", factura.Id);
            dto.MetricasRentabilidad = new MetricasRentabilidadDto
            {
                ErrorCalculoMetricas = "No se pudieron calcular las métricas de rentabilidad"
            };
        }
    }

    private async Task RegistrarConsultaAuditoria(ObtenerFacturaPorIdQuery request, Factura factura)
    {
        try
        {
            var eventoAuditoria = new EventoAuditoria
            {
                Id = Guid.NewGuid(),
                TipoEvento = "ConsultaFactura",
                EntidadId = factura.Id,
                EntidadTipo = "Factura",
                UsuarioId = request.UsuarioConsultaId ?? Guid.Empty,
                Detalles = $"Consulta de factura {factura.NumeroFactura} - Formato: {request.FormatoRespuesta}",
                FechaEvento = DateTime.UtcNow,
                DatosAdicionales = new Dictionary<string, object>
                {
                    { "FormatoConsulta", request.FormatoRespuesta },
                    { "MotivoConsulta", request.MotivoConsulta ?? "No especificado" },
                    { "ElementosIncluidos", request.ObtenerElementosAIncluir() },
                    { "NivelDetalle", request.ObtenerNivelDetalle() },
                    { "ValidarPermisos", request.ValidarPermisos },
                    { "NumeroFactura", factura.NumeroFactura },
                    { "EstadoFactura", factura.Estado.ToString() },
                    { "MontoFactura", factura.Total }
                }
            };

            await _context.EventosAuditoria.AddAsync(eventoAuditoria);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al registrar auditoría de consulta para factura {FacturaId}", factura.Id);
        }
    }
} 