using RestaurantePro.Domain.Comercial.Facturacion.Services;

namespace RestaurantePro.Application.Comercial.Facturacion.Commands.AplicarDescuento;

public class AplicarDescuentoHandler : IRequestHandler<AplicarDescuentoCommand, Result<FacturaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AplicarDescuentoHandler> _logger;
    private readonly IServicioFacturacion _servicioFacturacion;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public AplicarDescuentoHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<AplicarDescuentoHandler> logger,
        IServicioFacturacion servicioFacturacion,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _servicioFacturacion = servicioFacturacion;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<Result<FacturaDto>> Handle(AplicarDescuentoCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando aplicación de descuento: Tipo {TipoDescuento}, Factura {FacturaId}, Usuario {UsuarioId}",
                request.TipoDescuento, request.FacturaId, request.UsuarioAutorizaId);

            // 1. Obtener factura completa
            var facturaResult = await ObtenerFacturaCompleta(request.FacturaId, cancellationToken);
            if (!facturaResult.Succeeded)
            {
                return Result.Failure<FacturaDto>(facturaResult.Error);
            }

            var factura = facturaResult.Value;

            // 2. Calcular el monto del descuento
            var montoDescuentoResult = await CalcularMontoDescuento(request, factura);
            if (!montoDescuentoResult.Succeeded)
            {
                return Result.Failure<FacturaDto>(montoDescuentoResult.Error);
            }

            var montoDescuento = montoDescuentoResult.Value;

            // 3. Verificar límites finales
            var verificacionLimites = await VerificarLimitesFinales(factura, montoDescuento, request);
            if (!verificacionLimites.Succeeded)
            {
                return Result.Failure<FacturaDto>(verificacionLimites.Error);
            }

            // 4. Aplicar descuento usando servicio de dominio
            var aplicacionResult = await AplicarDescuentoConServicioDominio(
                factura, request, montoDescuento, cancellationToken);
            if (!aplicacionResult.Succeeded)
            {
                return Result.Failure<FacturaDto>(aplicacionResult.Error);
            }

            // 5. Guardar cambios en base de datos
            await _context.SaveChangesAsync(cancellationToken);

            // 6. Registrar auditoría del descuento
            await RegistrarAuditoriaDescuento(factura, request, montoDescuento);

            // 7. Procesar lógica específica por tipo de descuento
            await ProcesarLogicaEspecificaPorTipo(factura, request, montoDescuento);

            // 8. Notificar sobre la aplicación del descuento
            await EnviarNotificacionesDescuento(factura, request, montoDescuento);

            // 9. Mapear resultado a DTO
            var facturaDto = await MapearFacturaADto(factura);

            _logger.LogInformation("Descuento aplicado exitosamente: {TipoDescuento} por {MontoDescuento:C} en factura {NumeroFactura}",
                request.TipoDescuento, montoDescuento, factura.NumeroFactura);

            return Result.Success(facturaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al aplicar descuento {TipoDescuento} en factura {FacturaId}",
                request.TipoDescuento, request.FacturaId);
            return Result.Failure<FacturaDto>("Error interno al aplicar el descuento.");
        }
    }

    private async Task<Result<Factura>> ObtenerFacturaCompleta(Guid facturaId, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .Include(f => f.Detalles)
            .ThenInclude(d => d.Producto)
            .Include(f => f.Descuentos)
            .Include(f => f.Cliente)
            .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

        if (factura == null)
        {
            return Result.Failure<Factura>("La factura especificada no existe.");
        }

        if (factura.Estado == EstadoFactura.Anulada)
        {
            return Result.Failure<Factura>("No se pueden aplicar descuentos a facturas anuladas.");
        }

        if (factura.Estado == EstadoFactura.Pagada)
        {
            return Result.Failure<Factura>("No se pueden aplicar descuentos a facturas ya pagadas.");
        }

        return Result.Success(factura);
    }

    private async Task<Result<decimal>> CalcularMontoDescuento(AplicarDescuentoCommand request, Factura factura)
    {
        decimal montoDescuento = 0;

        if (request.Porcentaje > 0)
        {
            // Calcular base para el descuento
            var baseCalculo = await ObtenerBaseCalculoDescuento(request, factura);
            montoDescuento = baseCalculo * (request.Porcentaje / 100m);
        }
        else if (request.MontoFijo > 0)
        {
            montoDescuento = request.MontoFijo;
        }

        // Verificar que no exceda el máximo permitido
        if (request.MontoMaximoDescuento.HasValue && montoDescuento > request.MontoMaximoDescuento.Value)
        {
            montoDescuento = request.MontoMaximoDescuento.Value;
            
            _logger.LogInformation("Descuento ajustado al máximo permitido: {MontoMaximo:C} para factura {FacturaId}",
                request.MontoMaximoDescuento.Value, request.FacturaId);
        }

        // Verificar que no exceda el total de la factura
        if (montoDescuento > factura.Total)
        {
            return Result.Failure<decimal>("El descuento no puede exceder el total de la factura.");
        }

        return Result.Success(montoDescuento);
    }

    private async Task<decimal> ObtenerBaseCalculoDescuento(AplicarDescuentoCommand request, Factura factura)
    {
        // Para descuentos en productos específicos
        if (request.ProductosEspecificos.Any())
        {
            return factura.Detalles
                .Where(d => request.ProductosEspecificos.Contains(d.ProductoId))
                .Sum(d => request.AplicarAntesDeImpuestos ? d.Subtotal : d.Total);
        }

        // Para descuentos en categorías específicas
        if (request.CategoriasAplicables.Any())
        {
            return factura.Detalles
                .Where(d => request.CategoriasAplicables.Contains(d.Producto.Categoria, StringComparer.OrdinalIgnoreCase))
                .Sum(d => request.AplicarAntesDeImpuestos ? d.Subtotal : d.Total);
        }

        // Para descuentos generales
        return request.AplicarAntesDeImpuestos ? factura.Subtotal : factura.Total;
    }

    private async Task<Result<bool>> VerificarLimitesFinales(Factura factura, decimal montoDescuento, AplicarDescuentoCommand request)
    {
        // Verificar descuentos acumulados
        var totalDescuentosActuales = factura.TotalDescuentos;
        var nuevoTotalDescuentos = totalDescuentosActuales + montoDescuento;
        var porcentajeDescuentoTotal = (nuevoTotalDescuentos / factura.Subtotal) * 100;

        if (porcentajeDescuentoTotal > 50m && request.TipoDescuento != "Cortesia")
        {
            return Result.Failure<bool>("El total de descuentos no puede exceder el 50% de la factura (excepto descuentos de cortesía).");
        }

        // Verificar monto mínimo de factura si aplica
        if (request.MontoMinimoFactura.HasValue)
        {
            var totalDespuesDescuento = factura.Total - montoDescuento;
            if (totalDespuesDescuento < 0)
            {
                return Result.Failure<bool>("El descuento resultaría en un total negativo.");
            }
        }

        return Result.Success(true);
    }

    private async Task<Result<bool>> AplicarDescuentoConServicioDominio(
        Factura factura, 
        AplicarDescuentoCommand request, 
        decimal montoDescuento, 
        CancellationToken cancellationToken)
    {
        // Crear el descuento usando el servicio de dominio
        var descuentoResult = await _servicioFacturacion.AplicarDescuentoAsync(
            factura.Id,
            request.TipoDescuento,
            montoDescuento,
            request.Concepto,
            request.Motivo,
            request.UsuarioAutorizaId,
            request.AplicarAntesDeImpuestos,
            cancellationToken);

        if (!descuentoResult.IsSuccess)
        {
            return Result.Failure<bool>(descuentoResult.Error);
        }

        // Configurar propiedades adicionales del descuento
        var descuento = descuentoResult.Value;
        descuento.CodigoAutorizacion = request.CodigoAutorizacion;
        descuento.FechaExpiracion = request.FechaExpiracion;
        descuento.EsAcumulable = request.EsAcumulable;
        descuento.Prioridad = request.Prioridad;
        descuento.NotasAdicionales = request.NotasAdicionales;

        // Si hay productos específicos, crear registros de relación
        if (request.ProductosEspecificos.Any())
        {
            await CrearRelacionesProductosDescuento(descuento.Id, request.ProductosEspecificos);
        }

        // Si hay categorías específicas, registrarlas
        if (request.CategoriasAplicables.Any())
        {
            await CrearRelacionesCategoriasDescuento(descuento.Id, request.CategoriasAplicables);
        }

        return Result.Success(true);
    }

    private async Task CrearRelacionesProductosDescuento(Guid descuentoId, List<Guid> productosIds)
    {
        var relaciones = productosIds.Select(productoId => new DescuentoProducto
        {
            Id = Guid.NewGuid(),
            DescuentoId = descuentoId,
            ProductoId = productoId,
            FechaCreacion = DateTime.UtcNow
        });

        await _context.DescuentoProductos.AddRangeAsync(relaciones);
    }

    private async Task CrearRelacionesCategoriasDescuento(Guid descuentoId, List<string> categorias)
    {
        var relaciones = categorias.Select(categoria => new DescuentoCategoria
        {
            Id = Guid.NewGuid(),
            DescuentoId = descuentoId,
            Categoria = categoria,
            FechaCreacion = DateTime.UtcNow
        });

        await _context.DescuentoCategorias.AddRangeAsync(relaciones);
    }

    private async Task RegistrarAuditoriaDescuento(Factura factura, AplicarDescuentoCommand request, decimal montoDescuento)
    {
        var eventoAuditoria = new EventoAuditoria
        {
            Id = Guid.NewGuid(),
            TipoEvento = "DescuentoAplicado",
            EntidadId = factura.Id,
            EntidadTipo = "Factura",
            UsuarioId = request.UsuarioAutorizaId,
            Detalles = $"Descuento {request.TipoDescuento} aplicado por {montoDescuento:C}: {request.Concepto}",
            FechaEvento = DateTime.UtcNow,
            DatosAdicionales = new Dictionary<string, object>
            {
                { "TipoDescuento", request.TipoDescuento },
                { "MontoDescuento", montoDescuento },
                { "Porcentaje", request.Porcentaje },
                { "MontoFijo", request.MontoFijo },
                { "Concepto", request.Concepto },
                { "Motivo", request.Motivo },
                { "CodigoAutorizacion", request.CodigoAutorizacion ?? "N/A" },
                { "NumeroFactura", factura.NumeroFactura }
            }
        };

        await _context.EventosAuditoria.AddAsync(eventoAuditoria);
        
        _logger.LogInformation("Auditoría registrada para descuento {TipoDescuento} en factura {NumeroFactura}",
            request.TipoDescuento, factura.NumeroFactura);
    }

    private async Task ProcesarLogicaEspecificaPorTipo(Factura factura, AplicarDescuentoCommand request, decimal montoDescuento)
    {
        switch (request.TipoDescuento.ToLower())
        {
            case "promocional":
                await ProcesarDescuentoPromocional(factura, request, montoDescuento);
                break;
            case "empleado":
                await ProcesarDescuentoEmpleado(factura, request, montoDescuento);
                break;
            case "volumen":
                await ProcesarDescuentoVolumen(factura, request, montoDescuento);
                break;
            case "cortesia":
                await ProcesarDescuentoCortesia(factura, request, montoDescuento);
                break;
        }
    }

    private async Task ProcesarDescuentoPromocional(Factura factura, AplicarDescuentoCommand request, decimal montoDescuento)
    {
        // Registrar uso del código promocional
        if (!string.IsNullOrEmpty(request.CodigoAutorizacion))
        {
            var usoPromocion = new UsoPromocion
            {
                Id = Guid.NewGuid(),
                CodigoPromocion = request.CodigoAutorizacion,
                FacturaId = factura.Id,
                ClienteId = factura.ClienteId,
                MontoDescuento = montoDescuento,
                FechaUso = DateTime.UtcNow
            };

            await _context.UsoPromociones.AddAsync(usoPromocion);
        }
    }

    private async Task ProcesarDescuentoEmpleado(Factura factura, AplicarDescuentoCommand request, decimal montoDescuento)
    {
        // Registrar estadística de descuentos a empleados
        var estadistica = new EstadisticaDescuentoEmpleado
        {
            Id = Guid.NewGuid(),
            UsuarioEmpleadoId = request.UsuarioAutorizaId,
            FacturaId = factura.Id,
            MontoDescuento = montoDescuento,
            FechaDescuento = DateTime.UtcNow
        };

        await _context.EstadisticasDescuentosEmpleados.AddAsync(estadistica);
    }

    private async Task ProcesarDescuentoVolumen(Factura factura, AplicarDescuentoCommand request, decimal montoDescuento)
    {
        // Actualizar estadísticas de cliente para descuentos por volumen
        if (factura.ClienteId.HasValue)
        {
            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == factura.ClienteId.Value);
            if (cliente != null)
            {
                cliente.TotalCompras += factura.Total;
                cliente.CantidadDescuentosVolumen += 1;
            }
        }
    }

    private async Task ProcesarDescuentoCortesia(Factura factura, AplicarDescuentoCommand request, decimal montoDescuento)
    {
        // Los descuentos de cortesía requieren aprobación adicional para montos altos
        if (montoDescuento > 1000)
        {
            var aprobacion = new AprobacionDescuentoCortesia
            {
                Id = Guid.NewGuid(),
                FacturaId = factura.Id,
                UsuarioSolicita = request.UsuarioAutorizaId,
                MontoDescuento = montoDescuento,
                Motivo = request.Motivo,
                CodigoAutorizacion = request.CodigoAutorizacion,
                EstadoAprobacion = "Aplicado",
                FechaAprobacion = DateTime.UtcNow
            };

            await _context.AprobacionesDescuentosCortesia.AddAsync(aprobacion);
        }
    }

    private async Task EnviarNotificacionesDescuento(Factura factura, AplicarDescuentoCommand request, decimal montoDescuento)
    {
        try
        {
            // Notificar a administración para descuentos significativos
            if (montoDescuento > 2000 || request.TipoDescuento == "Cortesia")
            {
                await NotificarDescuentoSignificativo(factura, request, montoDescuento);
            }

            // Notificar al cliente si tiene email
            if (!string.IsNullOrEmpty(factura.Cliente?.Email))
            {
                await NotificarClienteDescuento(factura, request, montoDescuento);
            }

            _logger.LogInformation("Notificaciones de descuento enviadas para factura {NumeroFactura}",
                factura.NumeroFactura);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enviar notificaciones de descuento para factura {NumeroFactura}",
                factura.NumeroFactura);
        }
    }

    private async Task NotificarDescuentoSignificativo(Factura factura, AplicarDescuentoCommand request, decimal montoDescuento)
    {
        var asunto = $"Descuento Significativo Aplicado - {request.TipoDescuento}";
        var mensaje = $@"
            Se ha aplicado un descuento significativo:

            Factura: {factura.NumeroFactura}
            Tipo de Descuento: {request.TipoDescuento}
            Monto del Descuento: {montoDescuento:C}
            Concepto: {request.Concepto}
            Motivo: {request.Motivo}
            Autorizado por: Usuario ID {request.UsuarioAutorizaId}
            Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm}

            Total Original: {factura.Total + montoDescuento:C}
            Total con Descuento: {factura.Total:C}

            RestaurantePro - Sistema de Auditoría
        ";

        // Enviar a administradores
        await _emailService.SendEmailAsync("admin@restaurantepro.com", asunto, mensaje);
    }

    private async Task NotificarClienteDescuento(Factura factura, AplicarDescuentoCommand request, decimal montoDescuento)
    {
        if (request.TipoDescuento == "Cortesia") return; // No notificar descuentos de cortesía al cliente

        var asunto = $"Descuento Aplicado - Factura #{factura.NumeroFactura}";
        var mensaje = $@"
            Estimado/a {factura.NombreCliente},

            ¡Buenas noticias! Se ha aplicado un descuento a su factura:

            Factura: #{factura.NumeroFactura}
            Descuento: {request.Concepto}
            Monto del Descuento: {montoDescuento:C}

            Total Original: {factura.Total + montoDescuento:C}
            Total con Descuento: {factura.Total:C}
            Ahorro: {montoDescuento:C}

            Gracias por su preferencia.

            RestaurantePro
        ";

        await _emailService.SendEmailAsync(factura.Cliente.Email, asunto, mensaje);
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
            }).ToList(),
            Descuentos = factura.Descuentos.Select(desc => new DescuentoDto
            {
                Id = desc.Id,
                TipoDescuento = desc.TipoDescuento,
                Concepto = desc.Concepto,
                Monto = desc.Monto,
                Porcentaje = desc.Porcentaje,
                FechaAplicacion = desc.FechaAplicacion
            }).ToList()
        };
    }
} 