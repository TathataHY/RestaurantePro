namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerHistorialComandas;

/// <summary>
/// 🔍 Validador para ObtenerHistorialComandasQuery
/// </summary>
public class ObtenerHistorialComandasValidator : AbstractValidator<ObtenerHistorialComandasQuery>
{
    public ObtenerHistorialComandasValidator()
    {
        // Validación PageNumber
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("📄 El número de página debe ser mayor a 0")
            .WithErrorCode("HISTORIAL_PAGINA_INVALIDA");

        // Validación PageSize
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 200)
            .WithMessage("📊 El tamaño de página debe estar entre 1 y 200")
            .WithErrorCode("HISTORIAL_TAMANO_PAGINA_INVALIDO");

        // Validación rango de fechas
        RuleFor(x => x)
            .Must(HaveValidDateRange)
            .WithMessage("📅 La fecha desde debe ser anterior a la fecha hasta")
            .WithErrorCode("HISTORIAL_RANGO_FECHAS_INVALIDO");

        // Validación fecha desde (no muy en el pasado)
        RuleFor(x => x.FechaDesde)
            .GreaterThanOrEqualTo(DateTime.Today.AddYears(-2))
            .When(x => x.FechaDesde.HasValue)
            .WithMessage("📆 La fecha desde no puede ser mayor a 2 años en el pasado")
            .WithErrorCode("HISTORIAL_FECHA_DESDE_MUY_ANTIGUA");

        // Validación fecha hasta (no en el futuro)
        RuleFor(x => x.FechaHasta)
            .LessThanOrEqualTo(DateTime.Today.AddDays(1))
            .When(x => x.FechaHasta.HasValue)
            .WithMessage("📅 La fecha hasta no puede ser en el futuro")
            .WithErrorCode("HISTORIAL_FECHA_HASTA_FUTURA");

        // Validación IDs (no vacíos si se proporcionan)
        RuleFor(x => x.MesaId)
            .NotEqual(Guid.Empty)
            .When(x => x.MesaId.HasValue)
            .WithMessage("🪑 MesaId no puede estar vacío")
            .WithErrorCode("HISTORIAL_MESA_ID_VACIO");

        RuleFor(x => x.MeseroId)
            .NotEqual(Guid.Empty)
            .When(x => x.MeseroId.HasValue)
            .WithMessage("👨‍🍳 MeseroId no puede estar vacío")
            .WithErrorCode("HISTORIAL_MESERO_ID_VACIO");

        RuleFor(x => x.ClienteId)
            .NotEqual(Guid.Empty)
            .When(x => x.ClienteId.HasValue)
            .WithMessage("👤 ClienteId no puede estar vacío")
            .WithErrorCode("HISTORIAL_CLIENTE_ID_VACIO");

        // Validación rango de montos
        RuleFor(x => x.MontoMinimo)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MontoMinimo.HasValue)
            .WithMessage("💰 El monto mínimo debe ser mayor o igual a 0")
            .WithErrorCode("HISTORIAL_MONTO_MINIMO_INVALIDO");

        RuleFor(x => x.MontoMaximo)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MontoMaximo.HasValue)
            .WithMessage("💰 El monto máximo debe ser mayor o igual a 0")
            .WithErrorCode("HISTORIAL_MONTO_MAXIMO_INVALIDO");

        RuleFor(x => x)
            .Must(HaveValidMontoRange)
            .WithMessage("💰 El monto mínimo debe ser menor o igual al monto máximo")
            .WithErrorCode("HISTORIAL_RANGO_MONTOS_INVALIDO");

        // Validación término de búsqueda
        RuleFor(x => x.TerminoBusqueda)
            .MinimumLength(2)
            .When(x => !string.IsNullOrWhiteSpace(x.TerminoBusqueda))
            .WithMessage("🔍 El término de búsqueda debe tener al menos 2 caracteres")
            .WithErrorCode("HISTORIAL_TERMINO_BUSQUEDA_CORTO");

        RuleFor(x => x.TerminoBusqueda)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.TerminoBusqueda))
            .WithMessage("🔍 El término de búsqueda no puede exceder 100 caracteres")
            .WithErrorCode("HISTORIAL_TERMINO_BUSQUEDA_LARGO");

        // Validación enum OrdenHistorial
        RuleFor(x => x.OrdenarPor)
            .IsInEnum()
            .WithMessage("📊 El criterio de ordenamiento no es válido")
            .WithErrorCode("HISTORIAL_ORDEN_INVALIDO");
    }

    /// <summary>
    /// Valida que el rango de fechas sea consistente
    /// </summary>
    private static bool HaveValidDateRange(ObtenerHistorialComandasQuery query)
    {
        if (!query.FechaDesde.HasValue || !query.FechaHasta.HasValue)
            return true; // Si no hay ambas fechas, no validamos

        return query.FechaDesde.Value <= query.FechaHasta.Value;
    }

    /// <summary>
    /// Valida que el rango de montos sea consistente
    /// </summary>
    private static bool HaveValidMontoRange(ObtenerHistorialComandasQuery query)
    {
        if (!query.MontoMinimo.HasValue || !query.MontoMaximo.HasValue)
            return true; // Si no hay ambos montos, no validamos

        return query.MontoMinimo.Value <= query.MontoMaximo.Value;
    }
} 