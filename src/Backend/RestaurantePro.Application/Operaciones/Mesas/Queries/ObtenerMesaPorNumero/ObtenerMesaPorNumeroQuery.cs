using RestaurantePro.Application.Operaciones.Mesas.DTOs;

namespace RestaurantePro.Application.Operaciones.Mesas.Queries.ObtenerMesaPorNumero;

/// <summary>
/// Query para obtener una mesa específica por su número
/// </summary>
public class ObtenerMesaPorNumeroQuery : IRequest<Result<MesaDto>>
{
    /// <summary>
    /// Número de la mesa a buscar
    /// </summary>
    public int Numero { get; set; }

    /// <summary>
    /// Zona específica donde buscar (opcional, mejora la precisión)
    /// </summary>
    public string? Zona { get; set; }

    /// <summary>
    /// Incluir solo mesas activas
    /// </summary>
    public bool SoloActivas { get; set; } = true;

    /// <summary>
    /// Incluir información de comanda actual si está ocupada
    /// </summary>
    public bool IncluirComandaActual { get; set; } = false;

    /// <summary>
    /// Factory method para búsqueda básica por número
    /// </summary>
    public static ObtenerMesaPorNumeroQuery PorNumero(int numero, bool soloActivas = true)
    {
        return new ObtenerMesaPorNumeroQuery
        {
            Numero = numero,
            SoloActivas = soloActivas,
            IncluirComandaActual = false
        };
    }

    /// <summary>
    /// Factory method para búsqueda específica con zona
    /// </summary>
    public static ObtenerMesaPorNumeroQuery PorNumeroYZona(int numero, string zona, bool soloActivas = true)
    {
        return new ObtenerMesaPorNumeroQuery
        {
            Numero = numero,
            Zona = zona,
            SoloActivas = soloActivas,
            IncluirComandaActual = false
        };
    }

    /// <summary>
    /// Factory method para búsqueda detallada con información de comanda
    /// </summary>
    public static ObtenerMesaPorNumeroQuery Detallada(int numero, string? zona = null)
    {
        return new ObtenerMesaPorNumeroQuery
        {
            Numero = numero,
            Zona = zona,
            SoloActivas = true,
            IncluirComandaActual = true
        };
    }
} 