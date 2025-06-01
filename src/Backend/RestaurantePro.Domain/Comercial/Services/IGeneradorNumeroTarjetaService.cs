namespace RestaurantePro.Domain.Comercial.Services;

/// <summary>
/// Servicio de dominio para generar números únicos de tarjetas de fidelización
/// </summary>
public interface IGeneradorNumeroTarjetaService
{
    /// <summary>
    /// Genera un número único para tarjeta de fidelización
    /// </summary>
    /// <param name="tipoTarjeta">Tipo de tarjeta</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número generado</returns>
    Task<string> GenerarNumeroTarjetaAsync(string tipoTarjeta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida si un número de tarjeta es válido según las reglas de negocio
    /// </summary>
    /// <param name="numeroTarjeta">Número a validar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si es válido</returns>
    Task<bool> ValidarNumeroTarjetaAsync(string numeroTarjeta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica si un número ya existe
    /// </summary>
    /// <param name="numeroTarjeta">Número a verificar</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>True si existe</returns>
    Task<bool> ExisteNumeroTarjetaAsync(string numeroTarjeta, CancellationToken cancellationToken = default);

    /// <summary>
    /// Genera un número de tarjeta con formato específico
    /// </summary>
    /// <param name="tipoTarjeta">Tipo de tarjeta</param>
    /// <param name="clienteId">ID del cliente</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Número generado con formato específico</returns>
    Task<string> GenerarNumeroTarjetaConFormatoAsync(string tipoTarjeta, Guid clienteId, CancellationToken cancellationToken = default);
} 