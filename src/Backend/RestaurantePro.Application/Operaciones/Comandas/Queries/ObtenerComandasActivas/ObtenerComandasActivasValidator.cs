namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandasActivas;

/// <summary>
/// Validador para ObtenerComandasActivasQuery
/// Valida parámetros de paginación, filtros y ordenamiento
/// </summary>
public class ObtenerComandasActivasValidator : AbstractValidator<ObtenerComandasActivasQuery>
{
    private static readonly string[] EstadosValidos = 
    {
        "Creada",
        "EnProceso", 
        "Lista",
        "Entregada",
        "Finalizada"
    };

    private static readonly string[] CamposOrdenValidos = 
    {
        "FechaCreacion",
        "TiempoTranscurrido", 
        "Total",
        "Estado",
        "Mesa",
        "Mesero"
    };

    private static readonly string[] DireccionesOrdenValidas = 
    {
        "Asc",
        "Desc"
    };

    public ObtenerComandasActivasValidator()
    {
        // Validación de paginación
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("El número de página debe ser mayor a 0")
            .LessThanOrEqualTo(1000).WithMessage("El número de página no puede exceder 1000");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("El tamaño de página debe ser mayor a 0")
            .LessThanOrEqualTo(100).WithMessage("El tamaño máximo de página es 100");

        // Validación de filtro por estado
        RuleFor(x => x.EstadoFiltro)
            .Must(BeValidEstado).WithMessage($"Estado inválido. Valores válidos: {string.Join(", ", EstadosValidos)}")
            .When(x => !string.IsNullOrEmpty(x.EstadoFiltro));

        // Validación de filtros de ID
        RuleFor(x => x.MesaId)
            .NotEqual(Guid.Empty).WithMessage("El ID de la mesa no puede ser un GUID vacío")
            .When(x => x.MesaId.HasValue);

        RuleFor(x => x.MeseroId)
            .NotEqual(Guid.Empty).WithMessage("El ID del mesero no puede ser un GUID vacío")
            .When(x => x.MeseroId.HasValue);

        RuleFor(x => x.ClienteId)
            .NotEqual(Guid.Empty).WithMessage("El ID del cliente no puede ser un GUID vacío")
            .When(x => x.ClienteId.HasValue);

        // Validación de ordenamiento
        RuleFor(x => x.OrdenarPor)
            .Must(BeValidOrdenCampo).WithMessage($"Campo de ordenamiento inválido. Valores válidos: {string.Join(", ", CamposOrdenValidos)}");

        RuleFor(x => x.DireccionOrden)
            .Must(BeValidOrdenDireccion).WithMessage($"Dirección de ordenamiento inválida. Valores válidos: {string.Join(", ", DireccionesOrdenValidas)}");

        // Validación de fecha específica
        RuleFor(x => x.FechaEspecifica)
            .Must(BeValidDate).WithMessage("La fecha específica no puede ser futura")
            .When(x => x.FechaEspecifica.HasValue);

        // Validación de lógica de fechas
        RuleFor(x => x)
            .Must(HaveValidDateLogic).WithMessage("No se puede especificar FechaEspecifica cuando SoloHoy está habilitado");
    }

    /// <summary>
    /// Valida que el estado esté en la lista de estados válidos
    /// </summary>
    private static bool BeValidEstado(string? estado)
    {
        return string.IsNullOrEmpty(estado) || EstadosValidos.Contains(estado);
    }

    /// <summary>
    /// Valida que el campo de ordenamiento sea válido
    /// </summary>
    private static bool BeValidOrdenCampo(string campo)
    {
        return CamposOrdenValidos.Contains(campo);
    }

    /// <summary>
    /// Valida que la dirección de ordenamiento sea válida
    /// </summary>
    private static bool BeValidOrdenDireccion(string direccion)
    {
        return DireccionesOrdenValidas.Contains(direccion);
    }

    /// <summary>
    /// Valida que la fecha no sea futura
    /// </summary>
    private static bool BeValidDate(DateTime? fecha)
    {
        return !fecha.HasValue || fecha.Value.Date <= DateTime.Now.Date;
    }

    /// <summary>
    /// Valida la lógica de fechas (no SoloHoy y FechaEspecifica al mismo tiempo)
    /// </summary>
    private static bool HaveValidDateLogic(ObtenerComandasActivasQuery query)
    {
        return !(query.SoloHoy && query.FechaEspecifica.HasValue);
    }
} 