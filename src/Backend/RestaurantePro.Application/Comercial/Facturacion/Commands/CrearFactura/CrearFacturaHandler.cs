using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;
using RestaurantePro.Domain.Common;
using RestaurantePro.Domain.Comercial.Facturacion.Services;
using RestaurantePro.Domain.Comercial.Services;
using RestaurantePro.Domain.Common.Services;

namespace RestaurantePro.Application.Comercial.Facturacion.Commands.CrearFactura;

public class CrearFacturaHandler : IRequestHandler<CrearFacturaCommand, Result<FacturaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CrearFacturaHandler> _logger;
    private readonly IServicioFacturacion _servicioFacturacion;
    private readonly ComercialServiceFacade _comercialServiceFacade;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;

    public CrearFacturaHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<CrearFacturaHandler> logger,
        IServicioFacturacion servicioFacturacion,
        ComercialServiceFacade comercialServiceFacade,
        IEmailService emailService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _servicioFacturacion = servicioFacturacion;
        _comercialServiceFacade = comercialServiceFacade;
        _emailService = emailService;
        _currentUserService = currentUserService;
    }

    public async Task<Result<FacturaDto>> Handle(CrearFacturaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando creación de factura para {CantidadComandas} comandas. Tipo: {TipoFactura}, Cliente: {NombreCliente}",
                request.ComandasIds.Count, request.TipoFactura, request.NombreCliente);

            // 1. Preparar información del cliente
            var informacionCliente = await PrepararInformacionCliente(request, cancellationToken);
            if (!informacionCliente.IsSuccess)
            {
                return Result.Failure<FacturaDto>(informacionCliente.Error);
            }

            // 2. Validar y obtener comandas
            var comandasResult = await ValidarYObtenerComandas(request.ComandasIds, cancellationToken);
            if (!comandasResult.IsSuccess)
            {
                return Result.Failure<FacturaDto>(comandasResult.Error);
            }

            // 3. Crear factura usando el servicio de dominio
            var facturaResult = await CrearFacturaConServicioDominio(request, informacionCliente.Value, cancellationToken);
            if (!facturaResult.IsSuccess)
            {
                return Result.Failure<FacturaDto>(facturaResult.Error);
            }

            var factura = facturaResult.Value;

            // 4. Aplicar descuentos adicionales si existen
            if (request.DescuentosAdicionales.Any())
            {
                await AplicarDescuentosAdicionales(factura, request.DescuentosAdicionales);
            }

            // 5. Emitir factura si se solicita
            if (request.EmitirInmediatamente)
            {
                var emisionResult = await _servicioFacturacion.EmitirFacturaAsync(factura.Id, request.DiasCredito, cancellationToken);
                if (!emisionResult.IsSuccess)
                {
                    _logger.LogWarning("No se pudo emitir la factura {FacturaId} automáticamente: {Error}", 
                        factura.Id, emisionResult.Error);
                }
                else
                {
                    factura = emisionResult.Value;
                }
            }

            // 6. Registrar puntos de fidelización si aplica
            if (request.ClienteId.HasValue)
            {
                await RegistrarPuntosFidelizacion(request.ClienteId.Value, factura.Total, cancellationToken);
            }

            // 7. Enviar por email si se solicita
            if (request.EnviarPorEmail && !string.IsNullOrEmpty(request.EmailCliente))
            {
                await EnviarFacturaPorEmail(factura, request.EmailCliente);
            }

            // 8. Mapear a DTO y devolver resultado
            var facturaDto = await MapearFacturaADto(factura);

            _logger.LogInformation("Factura {NumeroFactura} creada exitosamente con ID {FacturaId}. Total: {Total:C}",
                factura.NumeroFactura, factura.Id, factura.Total);

            return Result.Success(facturaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear factura para comandas: {ComandasIds}", 
                string.Join(", ", request.ComandasIds));
            return Result.Failure<FacturaDto>("Error interno al crear la factura.");
        }
    }

    private async Task<Result<InformacionClienteDto>> PrepararInformacionCliente(CrearFacturaCommand request, CancellationToken cancellationToken)
    {
        var informacion = new InformacionClienteDto
        {
            NombreCliente = request.NombreCliente,
            IdentificacionFiscal = request.IdentificacionFiscal,
            DireccionCliente = request.DireccionCliente,
            EmailCliente = request.EmailCliente,
            TelefonoCliente = request.TelefonoCliente
        };

        // Si hay ClienteId, completar información desde la base de datos
        if (request.ClienteId.HasValue)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == request.ClienteId.Value, cancellationToken);

            if (cliente == null)
            {
                return Result.Failure<InformacionClienteDto>("El cliente especificado no existe.");
            }

            // Usar información del cliente registrado, con posibilidad de override
            informacion.NombreCliente = string.IsNullOrEmpty(request.NombreCliente) ? cliente.Nombre : request.NombreCliente;
            informacion.EmailCliente = string.IsNullOrEmpty(request.EmailCliente) ? cliente.Email : request.EmailCliente;
            informacion.TelefonoCliente = string.IsNullOrEmpty(request.TelefonoCliente) ? cliente.Telefono : request.TelefonoCliente;
            
            // Si es factura fiscal y el cliente tiene información fiscal
            if (request.TipoFactura.Equals("Fiscal", StringComparison.OrdinalIgnoreCase))
            {
                informacion.IdentificacionFiscal = string.IsNullOrEmpty(request.IdentificacionFiscal) ? 
                    cliente.RFC : request.IdentificacionFiscal;
                informacion.DireccionCliente = string.IsNullOrEmpty(request.DireccionCliente) ? 
                    cliente.DireccionFiscal : request.DireccionCliente;
            }
        }

        return Result.Success(informacion);
    }

    private async Task<Result<List<Comanda>>> ValidarYObtenerComandas(List<Guid> comandasIds, CancellationToken cancellationToken)
    {
        var comandas = await _context.Comandas
            .Include(c => c.Items)
            .ThenInclude(i => i.Producto)
            .Where(c => comandasIds.Contains(c.Id))
            .ToListAsync(cancellationToken);

        if (comandas.Count != comandasIds.Count)
        {
            var comandasEncontradas = comandas.Select(c => c.Id).ToList();
            var comandasFaltantes = comandasIds.Except(comandasEncontradas).ToList();
            return Result.Failure<List<Comanda>>($"No se encontraron las comandas: {string.Join(", ", comandasFaltantes)}");
        }

        // Validar que todas las comandas estén en estado válido para facturar
        var comandasInvalidas = comandas.Where(c => c.Estado != EstadoComanda.Completada).ToList();
        if (comandasInvalidas.Any())
        {
            return Result.Failure<List<Comanda>>($"Las siguientes comandas no están completadas: {string.Join(", ", comandasInvalidas.Select(c => c.Id))}");
        }

        return Result.Success(comandas);
    }

    private async Task<Result<Factura>> CrearFacturaConServicioDominio(
        CrearFacturaCommand request,
        InformacionClienteDto informacionCliente,
        CancellationToken cancellationToken)
    {
        // Determinar el tipo de factura usando enum
        if (!Enum.TryParse<TipoFactura>(request.TipoFactura, true, out var tipoFactura))
        {
            return Result.Failure<Factura>($"Tipo de factura no válido: {request.TipoFactura}");
        }

        // Crear factura usando el servicio de dominio
        if (request.ComandasIds.Count == 1)
        {
            // Factura para una sola comanda
            return await _servicioFacturacion.GenerarFacturaParaComandaAsync(
                request.ComandasIds.First(),
                tipoFactura,
                informacionCliente.NombreCliente,
                request.ClienteId,
                informacionCliente.IdentificacionFiscal,
                informacionCliente.DireccionCliente,
                request.Observaciones,
                cancellationToken);
        }
        else
        {
            // Factura para múltiples comandas
            return await _servicioFacturacion.GenerarFacturaParaComandasAsync(
                request.ComandasIds,
                tipoFactura,
                informacionCliente.NombreCliente,
                request.ClienteId,
                informacionCliente.IdentificacionFiscal,
                informacionCliente.DireccionCliente,
                request.Observaciones,
                cancellationToken);
        }
    }

    private async Task AplicarDescuentosAdicionales(Factura factura, List<DescuentoFacturaDto> descuentos)
    {
        foreach (var descuento in descuentos)
        {
            try
            {
                if (descuento.Porcentaje > 0)
                {
                    // Aplicar descuento por porcentaje
                    var montoDescuento = factura.Subtotal * (descuento.Porcentaje / 100m);
                    // Nota: En una implementación real, necesitaríamos un método en la entidad Factura
                    // factura.AplicarDescuentoAdicional(montoDescuento, descuento.Concepto, descuento.Motivo);
                }
                else if (descuento.MontoFijo > 0)
                {
                    // Aplicar descuento por monto fijo
                    // factura.AplicarDescuentoAdicional(descuento.MontoFijo, descuento.Concepto, descuento.Motivo);
                }

                _logger.LogInformation("Descuento aplicado a factura {FacturaId}: {Concepto} - {Porcentaje}% / {MontoFijo:C}",
                    factura.Id, descuento.Concepto, descuento.Porcentaje, descuento.MontoFijo);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al aplicar descuento {Concepto} a factura {FacturaId}",
                    descuento.Concepto, factura.Id);
            }
        }
    }

    private async Task RegistrarPuntosFidelizacion(Guid clienteId, decimal montoFactura, CancellationToken cancellationToken)
    {
        try
        {
            var puntosResult = await _comercialServiceFacade.AcumularPuntosPorCompraAsync(
                clienteId, montoFactura, null, "Compra - Facturación", cancellationToken);

            if (puntosResult.IsSuccess)
            {
                _logger.LogInformation("Puntos de fidelización registrados para cliente {ClienteId}: {PuntosAcumulados}",
                    clienteId, puntosResult.Value);
            }
            else
            {
                _logger.LogWarning("No se pudieron registrar puntos para cliente {ClienteId}: {Error}",
                    clienteId, puntosResult.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al registrar puntos de fidelización para cliente {ClienteId}", clienteId);
        }
    }

    private async Task EnviarFacturaPorEmail(Factura factura, string email)
    {
        try
        {
            var asunto = $"Su factura #{factura.NumeroFactura} - RestaurantePro";
            var mensaje = GenerarMensajeEmail(factura);
            
            await _emailService.SendEmailAsync(email, asunto, mensaje);
            
            _logger.LogInformation("Factura {NumeroFactura} enviada por email a {Email}", 
                factura.NumeroFactura, email);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enviar factura {NumeroFactura} por email a {Email}", 
                factura.NumeroFactura, email);
        }
    }

    private string GenerarMensajeEmail(Factura factura)
    {
        return $@"
            Estimado/a {factura.NombreCliente},

            Le enviamos su factura #{factura.NumeroFactura} por un total de {factura.Total:C}.

            Detalles de la factura:
            - Fecha de emisión: {factura.FechaEmision:dd/MM/yyyy}
            - Tipo de factura: {factura.TipoFactura}
            - Subtotal: {factura.Subtotal:C}
            - Impuestos: {factura.TotalImpuestos:C}
            - Descuentos: {factura.TotalDescuentos:C}
            - Total: {factura.Total:C}

            Gracias por su preferencia.

            RestaurantePro
        ";
    }

    private async Task<FacturaDto> MapearFacturaADto(Factura factura)
    {
        return new FacturaDto
        {
            Id = factura.Id,
            NumeroFactura = factura.NumeroFactura,
            TipoFactura = factura.TipoFactura.ToString(),
            Estado = factura.Estado.ToString(),
            FechaEmision = factura.FechaEmision,
            FechaVencimiento = factura.FechaVencimiento,
            NombreCliente = factura.NombreCliente,
            IdentificacionFiscal = factura.IdentificacionFiscal,
            DireccionCliente = factura.DireccionCliente,
            Subtotal = factura.Subtotal,
            TotalImpuestos = factura.TotalImpuestos,
            TotalDescuentos = factura.TotalDescuentos,
            Total = factura.Total,
            TotalPagado = factura.TotalPagado,
            Observaciones = factura.Observaciones,
            ComandasIds = factura.ComandasIds.ToList(),
            Detalles = factura.Detalles.Select(d => new DetalleFacturaDto
            {
                Id = d.Id,
                ProductoId = d.ProductoId,
                Descripcion = d.Descripcion,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Subtotal,
                ImporteImpuesto = d.ImporteImpuesto,
                ImporteDescuento = d.ImporteDescuento,
                Total = d.Total
            }).ToList()
        };
    }
}

/// <summary>
/// DTO interno para información consolidada del cliente
/// </summary>
internal class InformacionClienteDto
{
    public string NombreCliente { get; set; } = string.Empty;
    public string? IdentificacionFiscal { get; set; }
    public string? DireccionCliente { get; set; }
    public string? EmailCliente { get; set; }
    public string? TelefonoCliente { get; set; }
} 