namespace RestaurantePro.Application.Comercial.Clientes.Queries.ObtenerClientesPaginados;

/// <summary>
/// Validador para ObtenerClientesPaginadosQuery
/// Valida parámetros de paginación, filtros y ordenamiento
/// </summary>
public class ObtenerClientesPaginadosValidator : AbstractValidator<ObtenerClientesPaginadosQuery>
{
    public ObtenerClientesPaginadosValidator()
    {
        ConfigurarValidacionesPaginacion();
        ConfigurarValidacionesFiltros();
        ConfigurarValidacionesOrdenamiento();
        ConfigurarValidacionesFechas();
    }

    /// <summary>
    /// Configura validaciones para paginación
    /// </summary>
    private void ConfigurarValidacionesPaginacion()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("El número de página debe ser mayor a 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor a 0")
            .LessThanOrEqualTo(100)
            .WithMessage("El tamaño de página no puede exceder 100 registros");
    }

    /// <summary>
    /// Configura validaciones para filtros
    /// </summary>
    private void ConfigurarValidacionesFiltros()
    {
        RuleFor(x => x.FiltroTexto)
            .MaximumLength(100)
            .WithMessage("El filtro de texto no puede exceder 100 caracteres")
            .When(x => !string.IsNullOrEmpty(x.FiltroTexto));

        RuleFor(x => x.Segmento)
            .Must(BeValidSegmento)
            .WithMessage("El segmento debe ser uno de: SinClasificar, Nuevo, Regular, Premium, VIP")
            .When(x => !string.IsNullOrEmpty(x.Segmento));
    }

    /// <summary>
    /// Configura validaciones para ordenamiento
    /// </summary>
    private void ConfigurarValidacionesOrdenamiento()
    {
        var camposValidos = new[] { "FechaCreacion", "FechaRegistro", "Nombre", "NombreCompleto", "Email", "PuntosAcumulados", "CantidadVisitas", "Segmento" };

        RuleFor(x => x.OrdenarPor)
            .Must(campo => string.IsNullOrEmpty(campo) || camposValidos.Contains(campo, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El campo de ordenamiento debe ser uno de: {string.Join(", ", camposValidos)}");

        RuleFor(x => x.DireccionOrden)
            .Must(direccion => string.IsNullOrEmpty(direccion) || 
                              direccion.Equals("asc", StringComparison.OrdinalIgnoreCase) || 
                              direccion.Equals("desc", StringComparison.OrdinalIgnoreCase))
            .WithMessage("La dirección de ordenamiento debe ser 'asc' o 'desc'");
    }

    /// <summary>
    /// Configura validaciones para fechas
    /// </summary>
    private void ConfigurarValidacionesFechas()
    {
        RuleFor(x => x.FechaRegistroDesde)
            .LessThan(DateTime.Now.AddDays(1))
            .WithMessage("La fecha desde no puede ser futura")
            .When(x => x.FechaRegistroDesde.HasValue);

        RuleFor(x => x.FechaRegistroHasta)
            .LessThan(DateTime.Now.AddDays(1))
            .WithMessage("La fecha hasta no puede ser futura")
            .When(x => x.FechaRegistroHasta.HasValue);

        RuleFor(x => x)
            .Must(x => !x.FechaRegistroDesde.HasValue || !x.FechaRegistroHasta.HasValue || 
                      x.FechaRegistroDesde.Value <= x.FechaRegistroHasta.Value)
            .WithMessage("La fecha desde debe ser menor o igual a la fecha hasta")
            .When(x => x.FechaRegistroDesde.HasValue && x.FechaRegistroHasta.HasValue);
    }

    /// <summary>
    /// Valida si el segmento es válido
    /// </summary>
    private bool BeValidSegmento(string? segmento)
    {
        if (string.IsNullOrEmpty(segmento))
            return true;

        var segmentosValidos = new[] { "SinClasificar", "Nuevo", "Regular", "Premium", "VIP" };
        return segmentosValidos.Contains(segmento, StringComparer.OrdinalIgnoreCase);
    }
} 