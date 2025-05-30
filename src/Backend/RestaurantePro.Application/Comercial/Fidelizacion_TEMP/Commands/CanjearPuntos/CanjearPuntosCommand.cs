using MediatR;
using RestaurantePro.Application.Comercial.Fidelizacion.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Application.Comercial.Fidelizacion.Commands.CanjearPuntos;

/// <summary>
/// Command para canjear puntos por recompensas en el programa de fidelización
/// Gestiona la validación, descuento de puntos y entrega de recompensas
/// </summary>
public class CanjearPuntosCommand : IRequest<Result<CanjeoPuntosDto>>
{
    /// <summary>
    /// ID del cliente que va a canjear puntos
    /// </summary>
    public Guid ClienteId { get; set; }

    /// <summary>
    /// ID de la tarjeta de fidelización a utilizar
    /// </summary>
    public Guid? TarjetaFidelizacionId { get; set; }

    /// <summary>
    /// ID de la recompensa a canjear
    /// </summary>
    public Guid RecompensaId { get; set; }

    /// <summary>
    /// Cantidad de la recompensa a canjear (para recompensas que permiten múltiples unidades)
    /// </summary>
    public int Cantidad { get; set; } = 1;

    /// <summary>
    /// Puntos específicos a utilizar para el canje (opcional)
    /// Si no se especifica, se usarán los puntos requeridos por la recompensa
    /// </summary>
    public int? PuntosEspecificos { get; set; }

    /// <summary>
    /// Tipo de canje 
    /// </summary>
    public TipoCanje TipoCanje { get; set; } = TipoCanje.Inmediato;

    /// <summary>
    /// Método de entrega deseado
    /// </summary>
    public MetodoEntrega MetodoEntrega { get; set; } = MetodoEntrega.Presencial;

    /// <summary>
    /// Sucursal donde se recogerá la recompensa (para entrega presencial)
    /// </summary>
    public string? SucursalRecogida { get; set; }

    /// <summary>
    /// Dirección de entrega (para entrega a domicilio)
    /// </summary>
    public string? DireccionEntrega { get; set; }

    /// <summary>
    /// Fecha preferida para el canje/entrega
    /// </summary>
    public DateTime? FechaPreferida { get; set; }

    /// <summary>
    /// Horario preferido para la entrega
    /// </summary>
    public string? HorarioPreferido { get; set; }

    /// <summary>
    /// Comentarios especiales del cliente
    /// </summary>
    public string? Comentarios { get; set; }

    /// <summary>
    /// ID del empleado que procesa el canje (para canjes presenciales)
    /// </summary>
    public Guid? EmpleadoId { get; set; }

    /// <summary>
    /// Canal a través del cual se realiza el canje
    /// </summary>
    public string Canal { get; set; } = "App";

    /// <summary>
    /// Código de promoción para descuentos adicionales en el canje
    /// </summary>
    public string? CodigoPromocion { get; set; }

    /// <summary>
    /// Indica si se debe enviar notificación al cliente
    /// </summary>
    public bool NotificarCliente { get; set; } = true;

    /// <summary>
    /// Datos adicionales para el canje
    /// </summary>
    public Dictionary<string, object>? DatosAdicionales { get; set; }

    /// <summary>
    /// Preferencias del cliente para la recompensa
    /// </summary>
    public Dictionary<string, string>? PreferenciasRecompensa { get; set; }
}

/// <summary>
/// Tipos de canje disponibles
/// </summary>
public enum TipoCanje
{
    /// <summary>
    /// Canje inmediato - Se procesa al momento
    /// </summary>
    Inmediato = 1,

    /// <summary>
    /// Canje programado - Se ejecuta en una fecha específica
    /// </summary>
    Programado = 2,

    /// <summary>
    /// Reserva - Se reserva la recompensa para canje posterior
    /// </summary>
    Reserva = 3,

    /// <summary>
    /// Canje diferido - Se autoriza pero se entrega después
    /// </summary>
    Diferido = 4
}

/// <summary>
/// Métodos de entrega de recompensas
/// </summary>
public enum MetodoEntrega
{
    /// <summary>
    /// Recogida en sucursal
    /// </summary>
    Presencial = 1,

    /// <summary>
    /// Entrega a domicilio
    /// </summary>
    Domicilio = 2,

    /// <summary>
    /// Envío por correo
    /// </summary>
    Correo = 3,

    /// <summary>
    /// Entrega digital (cupones, códigos, etc.)
    /// </summary>
    Digital = 4,

    /// <summary>
    /// Transferencia bancaria (para recompensas monetarias)
    /// </summary>
    Transferencia = 5,

    /// <summary>
    /// Regalo directo en próxima visita
    /// </summary>
    ProximaVisita = 6
} 