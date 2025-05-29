namespace RestaurantePro.Application.Comercial.Clientes.Commands.DesactivarCliente;

/// <summary>
/// Handler para desactivar un cliente
/// Implementa eliminación lógica (soft delete) manteniendo integridad
/// </summary>
public class DesactivarClienteHandler : IRequestHandler<DesactivarClienteCommand, Result<bool>>
{
    private readonly IClienteRepository _repository;
    private readonly ILogger<DesactivarClienteHandler> _logger;

    public DesactivarClienteHandler(
        IClienteRepository repository,
        ILogger<DesactivarClienteHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(
        DesactivarClienteCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🗑️ Iniciando desactivación de cliente: {ClienteId}", request.ClienteId);

        try
        {
            // 1. Buscar el cliente existente
            var cliente = await _repository.ObtenerPorIdAsync(request.ClienteId, cancellationToken);
            if (cliente == null)
            {
                _logger.LogWarning("⚠️ Cliente no encontrado para desactivar: {ClienteId}", request.ClienteId);
                return Result.Failure<bool>($"No se encontró un cliente con el ID {request.ClienteId}");
            }

            // 2. Verificar si ya está desactivado
            if (cliente.EstaEliminado)
            {
                _logger.LogWarning("⚠️ Cliente ya estaba desactivado: {ClienteId}", request.ClienteId);
                return Result.Failure<bool>("El cliente ya está desactivado");
            }

            if (!cliente.EstaActivo)
            {
                _logger.LogWarning("⚠️ Cliente ya estaba inactivo: {ClienteId}", request.ClienteId);
                return Result.Failure<bool>("El cliente ya está inactivo");
            }

            // 3. TODO: Verificar reglas de negocio antes de desactivar
            // Por ejemplo: verificar que no tenga comandas activas, facturas pendientes, etc.
            // Esto se podría implementar como un domain service

            // 4. Desactivar usando el método del dominio
            cliente.Desactivar();

            // 5. Persistir los cambios
            await _repository.GuardarAsync(cliente, cancellationToken);

            _logger.LogInformation("✅ Cliente desactivado exitosamente: {ClienteId} - Motivo: {Motivo}", 
                request.ClienteId, 
                request.MotivoDesactivacion ?? "No especificado");

            return Result.Success(true);
        }
        catch (BusinessRuleViolationException ex)
        {
            _logger.LogWarning("📋 Violación de regla de negocio al desactivar cliente: {Message}", ex.Message);
            return Result.Failure<bool>(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error inesperado al desactivar cliente: {ClienteId}", request.ClienteId);
            return Result.Failure<bool>("Error interno del servidor al desactivar el cliente");
        }
    }
} 