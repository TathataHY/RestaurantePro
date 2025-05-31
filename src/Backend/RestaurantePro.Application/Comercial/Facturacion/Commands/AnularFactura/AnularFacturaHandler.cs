using RestaurantePro.Domain.Comercial.Facturacion.Services;

namespace RestaurantePro.Application.Comercial.Facturacion.Commands.AnularFactura;

public class AnularFacturaHandler : IRequestHandler<AnularFacturaCommand, Result<FacturaDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AnularFacturaHandler> _logger;
    private readonly IServicioFacturacion _servicioFacturacion;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;

    public AnularFacturaHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ILogger<AnularFacturaHandler> logger,
        IServicioFacturacion servicioFacturacion,
        ICurrentUserService currentUserService,
        IEmailService emailService,
        INotificationService notificationService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _servicioFacturacion = servicioFacturacion;
        _currentUserService = currentUserService;
        _emailService = emailService;
        _notificationService = notificationService;
    }

    public async Task<Result<FacturaDto>> Handle(AnularFacturaCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Iniciando anulación de factura: {FacturaId}, Tipo: {TipoAnulacion}, Usuario: {UsuarioId}",
                request.FacturaId, request.TipoAnulacion, request.UsuarioAutorizaId);

            // 1. Obtener y validar la factura
            var facturaResult = await ObtenerFactura(request.FacturaId, cancellationToken);
            if (!facturaResult.Succeeded)
            {
                return Result.Failure<FacturaDto>(facturaResult.Error);
            }

            var factura = facturaResult.Value;

            // 2. Validar aprobaciones necesarias
            var aprobacionResult = await ValidarAprobacionesNecesarias(request, factura);
            if (!aprobacionResult.Succeeded)
            {
                return Result.Failure<FacturaDto>(aprobacionResult.Error);
            }

            // 3. Pre-validaciones de negocio
            var preValidacionResult = await ValidarPrecondicionesAnulacion(factura, request);
            if (!preValidacionResult.Succeeded)
            {
                return Result.Failure<FacturaDto>(preValidacionResult.Error);
            }

            // 4. Ejecutar anulación
            var anulacionResult = await EjecutarAnulacion(factura, request);
            if (!anulacionResult.Succeeded)
            {
                return Result.Failure<FacturaDto>(anulacionResult.Error);
            }

            // 5. Procesar reversión de inventario
            if (request.RevertirInventario)
            {
                await ProcesarReversionInventario(factura);
            }

            // 6. Procesar cancelación de puntos de fidelización
            if (request.CancelarPuntosFidelizacion)
            {
                await ProcesarCancelacionPuntosFidelizacion(factura);
            }

            // 7. Procesar devolución de pagos
            if (request.ProcesarDevolucionPago)
            {
                await ProcesarDevolucionPagos(request, factura);
            }

            // 8. Generar nota de crédito si es necesario
            if (request.GenerarNotaCredito)
            {
                await GenerarNotaCredito(request, factura);
            }

            // 9. Guardar cambios en base de datos
            await _context.SaveChangesAsync(cancellationToken);

            // 10. Registrar auditoría completa
            await RegistrarAuditoriaAnulacion(request, factura);

            // 11. Procesar notificaciones
            await ProcesarNotificaciones(request, factura);

            // 12. Mapear resultado a DTO
            var facturaDto = await MapearFacturaADto(factura);

            _logger.LogInformation("Factura anulada exitosamente: {NumeroFactura}, Tipo: {TipoAnulacion}",
                factura.NumeroFactura, request.TipoAnulacion);

            return Result.Success(facturaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular factura {FacturaId}, Tipo: {TipoAnulacion}",
                request.FacturaId, request.TipoAnulacion);
            return Result.Failure<FacturaDto>("Error interno al anular la factura.");
        }
    }

    private async Task<Result<Factura>> ObtenerFactura(Guid facturaId, CancellationToken cancellationToken)
    {
        var factura = await _context.Facturas
            .Include(f => f.Detalles)
            .ThenInclude(d => d.Producto)
            .Include(f => f.Descuentos)
            .Include(f => f.Pagos)
            .Include(f => f.Cliente)
            .FirstOrDefaultAsync(f => f.Id == facturaId, cancellationToken);

        if (factura == null)
        {
            return Result.Failure<Factura>("La factura especificada no existe.");
        }

        if (factura.Estado == EstadoFactura.Anulada)
        {
            return Result.Failure<Factura>("La factura ya está anulada.");
        }

        return Result.Success(factura);
    }

    private async Task<Result<bool>> ValidarAprobacionesNecesarias(AnularFacturaCommand request, Factura factura)
    {
        if (!request.RequiereAprobacionGerencia)
        {
            return Result.Success(true);
        }

        if (request.GerenteAprobadorId == null)
        {
            // Crear solicitud de aprobación
            var solicitudAprobacion = new SolicitudAprobacionAnulacion
            {
                Id = Guid.NewGuid(),
                FacturaId = request.FacturaId,
                UsuarioSolicita = request.UsuarioAutorizaId,
                TipoAnulacion = request.TipoAnulacion,
                Motivo = request.Motivo,
                MontoFactura = factura.Total,
                FechaSolicitud = DateTime.UtcNow,
                Estado = "Pendiente",
                Prioridad = request.Prioridad
            };

            await _context.SolicitudesAprobacionAnulacion.AddAsync(solicitudAprobacion);

            // Notificar a gerentes
            await NotificarSolicitudAprobacion(solicitudAprobacion, factura);

            return Result.Failure<bool>("La anulación requiere aprobación de gerencia. Se ha enviado solicitud.");
        }

        // Verificar que el gerente especificado sea válido
        var gerente = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == request.GerenteAprobadorId.Value);

        if (gerente?.Rol != "Gerente" && gerente?.Rol != "Administrador")
        {
            return Result.Failure<bool>("El gerente aprobador especificado no es válido.");
        }

        return Result.Success(true);
    }

    private async Task<Result<bool>> ValidarPrecondicionesAnulacion(Factura factura, AnularFacturaCommand request)
    {
        // Verificar que no haya cambios concurrentes
        var facturaActual = await _context.Facturas
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.FacturaId);

        if (facturaActual?.Estado == EstadoFactura.Anulada)
        {
            return Result.Failure<bool>("La factura fue anulada por otro usuario.");
        }

        // Verificar límites de anulación por usuario/día
        var anulacionesHoy = await _context.Facturas
            .Where(f => f.UsuarioAnulaId == request.UsuarioAutorizaId &&
                       f.FechaAnulacion.HasValue &&
                       f.FechaAnulacion.Value.Date == DateTime.UtcNow.Date)
            .CountAsync();

        if (anulacionesHoy >= 5 && request.TipoAnulacion != "Emergencia")
        {
            return Result.Failure<bool>("Ha excedido el límite diario de anulaciones permitidas.");
        }

        return Result.Success(true);
    }

    private async Task<Result<bool>> EjecutarAnulacion(Factura factura, AnularFacturaCommand request)
    {
        // Usar servicio de dominio para anular
        var anulacionResult = await _servicioFacturacion.AnularFacturaAsync(
            factura.Id,
            request.Motivo,
            request.UsuarioAutorizaId,
            request.TipoAnulacion,
            default);

        if (!anulacionResult.Succeeded)
        {
            return Result.Failure<bool>(anulacionResult.Error);
        }

        // Actualizar propiedades adicionales
        factura.MotivoAnulacion = request.Motivo;
        factura.DescripcionAnulacion = request.DescripcionDetallada;
        factura.CodigoAutorizacionAnulacion = request.CodigoAutorizacion;
        factura.TipoAnulacion = request.TipoAnulacion;
        factura.ObservacionesAnulacion = request.ObservacionesAdicionales;
        factura.DocumentosAnulacion = request.DocumentosAdjuntos;
        factura.PrioridadAnulacion = request.Prioridad;

        return Result.Success(true);
    }

    private async Task ProcesarReversionInventario(Factura factura)
    {
        foreach (var detalle in factura.Detalles)
        {
            // Revertir movimientos de inventario
            var movimientoReversion = new MovimientoInventario
            {
                Id = Guid.NewGuid(),
                IngredienteId = detalle.ProductoId, // Asumiendo relación directa
                TipoMovimiento = "Entrada",
                Cantidad = detalle.Cantidad,
                Motivo = $"Reversión por anulación de factura {factura.NumeroFactura}",
                FacturaRelacionadaId = factura.Id,
                FechaMovimiento = DateTime.UtcNow,
                UsuarioId = factura.UsuarioAnulaId!.Value
            };

            await _context.MovimientosInventario.AddAsync(movimientoReversion);

            // Actualizar stock disponible
            var ingrediente = await _context.Ingredientes
                .FirstOrDefaultAsync(i => i.ProductoId == detalle.ProductoId);

            if (ingrediente != null)
            {
                ingrediente.CantidadDisponible += detalle.Cantidad;
                ingrediente.FechaUltimaActualizacion = DateTime.UtcNow;
            }
        }

        _logger.LogInformation("Inventario revertido para factura {NumeroFactura}", factura.NumeroFactura);
    }

    private async Task ProcesarCancelacionPuntosFidelizacion(Factura factura)
    {
        if (!factura.ClienteId.HasValue) return;

        // Buscar puntos otorgados por esta factura
        var puntosOtorgados = await _context.MovimientosPuntos
            .Where(m => m.FacturaId == factura.Id && m.Tipo == "Credito")
            .ToListAsync();

        foreach (var movimiento in puntosOtorgados)
        {
            // Crear movimiento de cancelación
            var cancelacion = new MovimientoPuntos
            {
                Id = Guid.NewGuid(),
                ClienteId = factura.ClienteId.Value,
                FacturaId = factura.Id,
                Tipo = "Debito",
                Puntos = movimiento.Puntos,
                Concepto = $"Cancelación por anulación de factura {factura.NumeroFactura}",
                FechaMovimiento = DateTime.UtcNow,
                UsuarioId = factura.UsuarioAnulaId!.Value
            };

            await _context.MovimientosPuntos.AddAsync(cancelacion);
        }

        // Actualizar saldo del cliente
        var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.Id == factura.ClienteId.Value);
        if (cliente != null)
        {
            var totalPuntosACancelar = puntosOtorgados.Sum(p => p.Puntos);
            cliente.PuntosFidelizacion = Math.Max(0, cliente.PuntosFidelizacion - totalPuntosACancelar);
        }

        _logger.LogInformation("Puntos de fidelización cancelados para factura {NumeroFactura}", factura.NumeroFactura);
    }

    private async Task ProcesarDevolucionPagos(AnularFacturaCommand request, Factura factura)
    {
        var pagos = await _context.PagosFactura
            .Where(p => p.FacturaId == factura.Id)
            .ToListAsync();

        foreach (var pago in pagos)
        {
            var devolucion = new DevolucionPago
            {
                Id = Guid.NewGuid(),
                PagoOriginalId = pago.Id,
                FacturaId = factura.Id,
                MontoDevolucion = pago.Monto,
                MetodoDevolucion = request.MetodoDevolucion!,
                ReferenciaDevolucion = request.ReferenciaDevolucion,
                FechaDevolucion = DateTime.UtcNow,
                FechaLimiteDevolucion = request.FechaLimiteDevolucion,
                Estado = "Procesando",
                UsuarioAutoriza = request.UsuarioAutorizaId,
                Motivo = $"Devolución por anulación: {request.Motivo}"
            };

            await _context.DevolucionesPagos.AddAsync(devolucion);

            // Marcar pago original como devuelto
            pago.Estado = "Devuelto";
            pago.FechaDevolucion = DateTime.UtcNow;
            pago.MontoDevuelto = pago.Monto;
        }

        // Actualizar total pagado de la factura
        factura.TotalPagado = 0;

        _logger.LogInformation("Devoluciones procesadas para factura {NumeroFactura}, Método: {MetodoDevolucion}",
            factura.NumeroFactura, request.MetodoDevolucion);
    }

    private async Task GenerarNotaCredito(AnularFacturaCommand request, Factura factura)
    {
        var notaCredito = new NotaCredito
        {
            Id = Guid.NewGuid(),
            NumeroNota = await GenerarNumeroNotaCredito(),
            FacturaOriginalId = factura.Id,
            ClienteId = factura.ClienteId,
            NombreCliente = factura.NombreCliente,
            IdentificacionFiscal = factura.IdentificacionFiscal,
            Concepto = $"Nota de crédito por anulación de factura {factura.NumeroFactura}",
            Motivo = request.Motivo,
            MontoCredito = factura.Total,
            FechaEmision = DateTime.UtcNow,
            FechaVencimiento = DateTime.UtcNow.AddDays(90),
            Estado = "Emitida",
            UsuarioEmite = request.UsuarioAutorizaId
        };

        await _context.NotasCredito.AddAsync(notaCredito);

        _logger.LogInformation("Nota de crédito generada: {NumeroNota} para factura {NumeroFactura}",
            notaCredito.NumeroNota, factura.NumeroFactura);
    }

    private async Task<string> GenerarNumeroNotaCredito()
    {
        var ultimaNotaCredito = await _context.NotasCredito
            .OrderByDescending(n => n.NumeroNota)
            .FirstOrDefaultAsync();

        if (ultimaNotaCredito == null)
        {
            return "NC-000001";
        }

        var ultimoNumero = int.Parse(ultimaNotaCredito.NumeroNota.Split('-')[1]);
        return $"NC-{(ultimoNumero + 1):D6}";
    }

    private async Task RegistrarAuditoriaAnulacion(AnularFacturaCommand request, Factura factura)
    {
        var eventoAuditoria = new EventoAuditoria
        {
            Id = Guid.NewGuid(),
            TipoEvento = "FacturaAnulada",
            EntidadId = factura.Id,
            EntidadTipo = "Factura",
            UsuarioId = request.UsuarioAutorizaId,
            Detalles = $"Factura {factura.NumeroFactura} anulada - Tipo: {request.TipoAnulacion} - Motivo: {request.Motivo}",
            FechaEvento = DateTime.UtcNow,
            DatosAdicionales = new Dictionary<string, object>
            {
                { "TipoAnulacion", request.TipoAnulacion },
                { "Motivo", request.Motivo },
                { "DescripcionDetallada", request.DescripcionDetallada ?? "N/A" },
                { "CodigoAutorizacion", request.CodigoAutorizacion ?? "N/A" },
                { "MontoFactura", factura.Total },
                { "GenerarNotaCredito", request.GenerarNotaCredito },
                { "ProcesarDevolucionPago", request.ProcesarDevolucionPago },
                { "RevertirInventario", request.RevertirInventario },
                { "CancelarPuntosFidelizacion", request.CancelarPuntosFidelizacion },
                { "NumeroFactura", factura.NumeroFactura },
                { "Prioridad", request.Prioridad }
            }
        };

        await _context.EventosAuditoria.AddAsync(eventoAuditoria);
        
        _logger.LogInformation("Auditoría registrada para anulación de factura {NumeroFactura}", factura.NumeroFactura);
    }

    private async Task ProcesarNotificaciones(AnularFacturaCommand request, Factura factura)
    {
        try
        {
            // Notificar a administración
            await NotificarAnulacionAdministracion(request, factura);

            // Notificar al cliente si corresponde
            if (request.NotificarCliente && !string.IsNullOrEmpty(factura.Cliente?.Email))
            {
                await NotificarAnulacionCliente(request, factura);
            }

            // Notificar a gerencia para anulaciones importantes
            if (factura.Total > 5000 || request.TipoAnulacion == "Emergencia")
            {
                await NotificarAnulacionGerencia(request, factura);
            }

            _logger.LogInformation("Notificaciones enviadas para anulación de factura {NumeroFactura}", factura.NumeroFactura);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al enviar notificaciones para anulación de factura {NumeroFactura}", factura.NumeroFactura);
        }
    }

    private async Task NotificarSolicitudAprobacion(SolicitudAprobacionAnulacion solicitud, Factura factura)
    {
        var gerentes = await _context.Usuarios
            .Where(u => (u.Rol == "Gerente" || u.Rol == "Administrador") && u.Activo)
            .ToListAsync();

        foreach (var gerente in gerentes)
        {
            if (!string.IsNullOrEmpty(gerente.Email))
            {
                var asunto = $"Solicitud de Aprobación - Anulación Factura #{factura.NumeroFactura}";
                var mensaje = $@"
                    Se requiere su aprobación para anular la siguiente factura:

                    Factura: #{factura.NumeroFactura}
                    Monto: {factura.Total:C}
                    Tipo de Anulación: {solicitud.TipoAnulacion}
                    Motivo: {solicitud.Motivo}
                    Solicitado por: Usuario ID {solicitud.UsuarioSolicita}
                    Fecha: {solicitud.FechaSolicitud:dd/MM/yyyy HH:mm}

                    Por favor, revise y apruebe o rechace esta solicitud en el sistema.

                    RestaurantePro - Sistema de Gestión
                ";

                await _emailService.SendEmailAsync(gerente.Email, asunto, mensaje);
            }
        }
    }

    private async Task NotificarAnulacionAdministracion(AnularFacturaCommand request, Factura factura)
    {
        var asunto = $"Factura Anulada - {request.TipoAnulacion}";
        var mensaje = $@"
            Se ha anulado una factura:

            Factura: #{factura.NumeroFactura}
            Tipo de Anulación: {request.TipoAnulacion}
            Monto: {factura.Total:C}
            Motivo: {request.Motivo}
            Usuario: {request.UsuarioAutorizaId}
            Fecha: {DateTime.UtcNow:dd/MM/yyyy HH:mm}

            Detalles adicionales:
            - Generar Nota Crédito: {(request.GenerarNotaCredito ? "Sí" : "No")}
            - Procesar Devolución: {(request.ProcesarDevolucionPago ? "Sí" : "No")}
            - Revertir Inventario: {(request.RevertirInventario ? "Sí" : "No")}
            - Cancelar Puntos: {(request.CancelarPuntosFidelizacion ? "Sí" : "No")}

            RestaurantePro - Sistema de Auditoría
        ";

        await _emailService.SendEmailAsync("admin@restaurantepro.com", asunto, mensaje);
    }

    private async Task NotificarAnulacionCliente(AnularFacturaCommand request, Factura factura)
    {
        if (request.TipoAnulacion == "Administrativa") return; // No notificar anulaciones administrativas

        var asunto = $"Anulación de Factura #{factura.NumeroFactura}";
        var mensaje = $@"
            Estimado/a {factura.NombreCliente},

            Le informamos que su factura ha sido anulada:

            Factura: #{factura.NumeroFactura}
            Fecha Original: {factura.FechaEmision:dd/MM/yyyy}
            Monto: {factura.Total:C}
            Motivo: {request.Motivo}

            {(request.ProcesarDevolucionPago ? $"Su devolución será procesada mediante: {request.MetodoDevolucion}" : "")}
            {(request.GenerarNotaCredito ? "Se generará una nota de crédito a su favor." : "")}

            Para cualquier consulta, contáctenos.

            RestaurantePro
        ";

        await _emailService.SendEmailAsync(factura.Cliente.Email, asunto, mensaje);
    }

    private async Task NotificarAnulacionGerencia(AnularFacturaCommand request, Factura factura)
    {
        var asunto = $"Anulación Importante - Factura #{factura.NumeroFactura}";
        var mensaje = $@"
            ANULACIÓN DE ALTO IMPACTO

            Factura: #{factura.NumeroFactura}
            Monto: {factura.Total:C}
            Tipo: {request.TipoAnulacion}
            Motivo: {request.Motivo}
            Usuario: {request.UsuarioAutorizaId}
            Prioridad: {request.Prioridad}

            Requiere seguimiento gerencial.

            RestaurantePro - Alertas Gerenciales
        ";

        await _emailService.SendEmailAsync("gerencia@restaurantepro.com", asunto, mensaje);
    }

    private string SerializarConfiguracionAnulacion(AnularFacturaCommand request)
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            request.TipoAnulacion,
            request.Motivo,
            request.DescripcionDetallada,
            request.GenerarNotaCredito,
            request.NotificarCliente,
            request.ProcesarDevolucionPago,
            request.MetodoDevolucion,
            request.RevertirInventario,
            request.CancelarPuntosFidelizacion,
            request.ObservacionesAdicionales,
            request.Prioridad
        });
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
            FechaAnulacion = factura.FechaAnulacion,
            MotivoAnulacion = factura.MotivoAnulacion,
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