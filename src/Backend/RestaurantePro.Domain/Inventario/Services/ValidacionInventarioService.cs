namespace RestaurantePro.Domain.Inventario.Services;

/// <summary>
/// Implementación del servicio de dominio para validación de operaciones de inventario
/// </summary>
public class ValidacionInventarioService : IValidacionInventarioService
{
    private readonly ILogger<ValidacionInventarioService> _logger;

    public ValidacionInventarioService(ILogger<ValidacionInventarioService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ResultadoValidacionInventario> ValidarAjusteInventarioAsync(
        Guid ingredienteId,
        RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario tipoMovimiento,
        decimal cantidad,
        decimal stockActual)
    {
        try
        {
            var errores = new List<string>();
            var advertencias = new List<string>();

            // Validación básica de parámetros
            if (ingredienteId == Guid.Empty)
                errores.Add("El ID del ingrediente no puede estar vacío");

            if (cantidad <= 0)
                errores.Add("La cantidad debe ser mayor a cero");

            if (stockActual < 0)
                errores.Add("El stock actual no puede ser negativo");

            // Validaciones específicas por tipo de movimiento
            switch (tipoMovimiento)
            {
                case RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Egreso:
                case RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Decremento:
                    if (cantidad > stockActual)
                        errores.Add($"No se puede retirar {cantidad} unidades. Stock disponible: {stockActual}");
                    else if (stockActual - cantidad <= 0)
                        advertencias.Add("El ajuste dejará el stock en cero o negativo");
                    break;

                case RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Merma:
                    if (cantidad > stockActual)
                        errores.Add($"La merma no puede ser mayor al stock disponible: {stockActual}");
                    break;
            }

            // Validaciones de límites máximos
            if (tipoMovimiento == RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Ingreso ||
                tipoMovimiento == RestaurantePro.Domain.Inventario.Ingredientes.Movimientos.Enums.TipoMovimientoInventario.Incremento)
            {
                var stockResultante = stockActual + cantidad;
                if (stockResultante > 999999) // Límite máximo arbitrario
                    advertencias.Add($"El stock resultante ({stockResultante}) podría ser excesivo");
            }

            if (errores.Any())
                return ResultadoValidacionInventario.ConErrores(errores.ToArray());

            var resultado = ResultadoValidacionInventario.Exitoso();
            if (advertencias.Any())
                return ResultadoValidacionInventario.ConAdvertencias(advertencias.ToArray());

            return resultado;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar ajuste de inventario para ingrediente {IngredienteId}", ingredienteId);
            return ResultadoValidacionInventario.ConErrores($"Error interno en validación: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<bool> ValidarSalidaInventarioAsync(
        Guid ingredienteId,
        decimal cantidadSalida,
        decimal stockActual)
    {
        try
        {
            if (ingredienteId == Guid.Empty) return false;
            if (cantidadSalida <= 0) return false;
            if (stockActual < 0) return false;
            if (cantidadSalida > stockActual) return false;

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar salida de inventario para ingrediente {IngredienteId}", ingredienteId);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<ResultadoValidacionInventario> ValidarReglasNegocioAsync(
        DatosMovimientoInventario movimiento)
    {
        try
        {
            var errores = new List<string>();
            var advertencias = new List<string>();

            // Validaciones de datos requeridos
            if (movimiento.IngredienteId == Guid.Empty)
                errores.Add("El ID del ingrediente es requerido");

            if (string.IsNullOrWhiteSpace(movimiento.TipoMovimiento))
                errores.Add("El tipo de movimiento es requerido");

            if (movimiento.Cantidad <= 0)
                errores.Add("La cantidad debe ser mayor a cero");

            if (movimiento.UsuarioId == Guid.Empty)
                errores.Add("El ID del usuario es requerido");

            if (string.IsNullOrWhiteSpace(movimiento.Motivo))
                errores.Add("El motivo del movimiento es requerido");

            // Validaciones de fechas
            if (movimiento.FechaMovimiento > DateTime.UtcNow.AddHours(1))
                errores.Add("La fecha del movimiento no puede ser futura");

            if (movimiento.FechaMovimiento < DateTime.UtcNow.AddYears(-2))
                advertencias.Add("La fecha del movimiento es muy antigua");

            // Validaciones de stock
            if (movimiento.StockAnterior < 0)
                errores.Add("El stock anterior no puede ser negativo");

            if (errores.Any())
                return ResultadoValidacionInventario.ConErrores(errores.ToArray());

            if (advertencias.Any())
                return ResultadoValidacionInventario.ConAdvertencias(advertencias.ToArray());

            return ResultadoValidacionInventario.Exitoso();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar reglas de negocio para movimiento de inventario");
            return ResultadoValidacionInventario.ConErrores($"Error interno en validación: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<ResultadoValidacionInventario> VerificarIntegridadDatosAsync(Guid ingredienteId)
    {
        try
        {
            var errores = new List<string>();
            var advertencias = new List<string>();

            if (ingredienteId == Guid.Empty)
                errores.Add("El ID del ingrediente no puede estar vacío");

            // Aquí podrían ir validaciones más complejas como:
            // - Verificar que el ingrediente existe
            // - Verificar consistencia de movimientos
            // - Verificar que el stock calculado coincide con el registrado
            // Por ahora mantenemos validaciones básicas

            if (errores.Any())
                return ResultadoValidacionInventario.ConErrores(errores.ToArray());

            if (advertencias.Any())
                return ResultadoValidacionInventario.ConAdvertencias(advertencias.ToArray());

            return ResultadoValidacionInventario.Exitoso();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar integridad de datos para ingrediente {IngredienteId}", ingredienteId);
            return ResultadoValidacionInventario.ConErrores($"Error interno en verificación: {ex.Message}");
        }
    }
} 