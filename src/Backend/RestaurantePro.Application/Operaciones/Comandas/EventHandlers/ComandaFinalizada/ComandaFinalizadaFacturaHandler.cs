using RestaurantePro.Domain.Operaciones.Comandas.Events.Comanda;
using RestaurantePro.Application.Comercial.Facturacion.Commands;
using RestaurantePro.Application.Common.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Core.Usuarios.Interfaces;
using RestaurantePro.Domain.Core.Usuarios.Entities;

namespace RestaurantePro.Application.Operaciones.Comandas.EventHandlers.ComandaFinalizada;

/// <summary>
/// Handler para generar automáticamente factura cuando se finaliza una comanda
/// </summary>
public class ComandaFinalizadaFacturaHandler : Domain.Core.Base.Events.Handlers.IDomainEventHandler<Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IMediator _mediator;
    private readonly ILogger<ComandaFinalizadaFacturaHandler> _logger;

    public ComandaFinalizadaFacturaHandler(
        IComandaRepository comandaRepository,
        IClienteRepository clienteRepository,
        IMediator mediator,
        ILogger<ComandaFinalizadaFacturaHandler> logger)
    {
        _comandaRepository = comandaRepository;
        _clienteRepository = clienteRepository;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(Domain.Operaciones.Comandas.Events.Comanda.ComandaFinalizada evento, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🧾 [FACTURA HANDLER] Iniciando generación de factura automática para comanda finalizada {ComandaId}", evento.ComandaId);

        try
        {
            // Obtener la comanda con sus datos completos
            var comanda = await _comandaRepository.ObtenerPorIdAsync(evento.ComandaId, cancellationToken);
            if (comanda == null)
            {
                _logger.LogWarning("❌ Comanda {ComandaId} no encontrada para generar factura", evento.ComandaId);
                return;
            }

            // Obtener datos del cliente
            Cliente? cliente = null;
            if (comanda.ClienteId.HasValue)
            {
                cliente = await _clienteRepository.ObtenerPorIdAsync(comanda.ClienteId.Value, cancellationToken);
            }

            // Crear comando para generar factura
            var crearFacturaCommand = new CrearFacturaCommand
            {
                ComandasIds = new List<Guid> { evento.ComandaId },
                TipoFactura = "Normal",
                NombreCliente = cliente?.Nombre?.NombreCompleto ?? "Cliente General",
                MetodoPagoPreferido = "Efectivo", // Por defecto
                Observaciones = $"Factura generada automáticamente al finalizar comanda {evento.ComandaId}"
            };

            // Ejecutar comando de creación de factura
            var resultado = await _mediator.Send(crearFacturaCommand, cancellationToken);
            
            if (resultado.IsSuccess())
            {
                _logger.LogInformation("✅ Factura generada automáticamente para comanda {ComandaId} - Factura ID: {FacturaId}", 
                    evento.ComandaId, resultado.Value?.Id);
            }
            else
            {
                _logger.LogError("❌ Error al generar factura automática para comanda {ComandaId}: {Error}", 
                    evento.ComandaId, resultado.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💥 Error inesperado al generar factura automática para comanda {ComandaId}", 
                evento.ComandaId);
        }
    }
} 