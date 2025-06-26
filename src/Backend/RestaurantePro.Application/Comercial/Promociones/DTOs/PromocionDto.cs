using RestaurantePro.Domain.Comercial.Promociones.Enums;

namespace RestaurantePro.Application.Comercial.Promociones.DTOs;

/// <summary>
/// DTO para representar una promoción
/// </summary>
public class PromocionDto
{
    public Guid Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public TipoPromocion Tipo { get; set; }
    public decimal ValorDescuento { get; set; }
    public decimal MontoMinimo { get; set; }
    public int PuntosRequeridos { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int? MaximoUsos { get; set; }
    public int VecesUsada { get; set; }
    public EstadoPromocion Estado { get; set; }
    public bool EsAcumulable { get; set; }
    public int? DiasValidos { get; set; }
    public int Prioridad { get; set; }
    public string Condiciones { get; set; } = string.Empty;
    public List<Guid> ProductosAplicablesIds { get; set; } = new();
    public List<Guid> CategoriasAplicablesIds { get; set; } = new();
    public List<Guid> ClientesQueUsaronIds { get; set; } = new();
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaUltimaActualizacion { get; set; }
    public bool EstaVigente { get; set; }
    public decimal DescuentoCalculado { get; set; }
} 