namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesCliente;

/// <summary>
/// Validator para ObtenerReservacionesClienteQuery
/// </summary>
public class ObtenerReservacionesClienteValidator : AbstractValidator<ObtenerReservacionesClienteQuery>
{
    public ObtenerReservacionesClienteValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty()
            .WithMessage("El ID del cliente es requerido")
            .NotEqual(Guid.Empty)
            .WithMessage("El ID del cliente no puede ser un GUID vacío");

        RuleFor(x => x.Pagina)
            .GreaterThan(0)
            .WithMessage("La página debe ser mayor a 0");

        RuleFor(x => x.TamanoPagina)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor a 0")
            .LessThanOrEqualTo(100)
            .WithMessage("El tamaño de página no puede exceder 100");

        // Validación de fechas
        RuleFor(x => x.FechaDesde)
            .LessThanOrEqualTo(x => x.FechaHasta)
            .WithMessage("La fecha desde debe ser anterior o igual a la fecha hasta")
            .When(x => x.FechaDesde.HasValue && x.FechaHasta.HasValue);

        RuleFor(x => x.FechaHasta)
            .GreaterThanOrEqualTo(x => x.FechaDesde)
            .WithMessage("La fecha hasta debe ser posterior o igual a la fecha desde")
            .When(x => x.FechaDesde.HasValue && x.FechaHasta.HasValue);

        // Validación de rango de fechas no superior a 2 años
        RuleFor(x => x)
            .Must(x => !x.FechaDesde.HasValue || !x.FechaHasta.HasValue || 
                      (x.FechaHasta.Value - x.FechaDesde.Value).TotalDays <= 730)
            .WithMessage("El rango de fechas no puede exceder 2 años")
            .When(x => x.FechaDesde.HasValue && x.FechaHasta.HasValue);

        // Validación de estado válido
        RuleFor(x => x.Estado)
            .IsInEnum()
            .WithMessage("El estado de reservación no es válido")
            .When(x => x.Estado.HasValue);

        // Validación condicional para fechas futuras
        RuleFor(x => x.FechaDesde)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Para filtrar por reservaciones futuras, la fecha desde debe ser igual o posterior a hoy")
            .When(x => x.SoloFuturas && x.FechaDesde.HasValue);

        // Validación condicional para historial (fechas pasadas)
        RuleFor(x => x.FechaHasta)
            .LessThan(DateTime.Today)
            .WithMessage("Para filtrar solo historial, la fecha hasta debe ser anterior a hoy")
            .When(x => x.SoloHistorial && x.FechaHasta.HasValue);

        // Validación de lógica de filtros mutuamente excluyentes
        RuleFor(x => x)
            .Must(x => !(x.SoloFuturas && x.SoloHistorial))
            .WithMessage("No se puede filtrar por futuras e historial al mismo tiempo");

        RuleFor(x => x)
            .Must(x => !(x.SoloFuturas && x.SoloActivas && x.SoloHistorial))
            .WithMessage("Solo se puede aplicar uno de los filtros: Futuras, Activas o Historial");
    }
} 