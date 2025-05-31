namespace RestaurantePro.Application.Operaciones.Reservaciones.Queries.ObtenerReservacionesPorFecha;

/// <summary>
/// Validator para ObtenerReservacionesPorFechaQuery
/// </summary>
public class ObtenerReservacionesPorFechaValidator : AbstractValidator<ObtenerReservacionesPorFechaQuery>
{
    public ObtenerReservacionesPorFechaValidator()
    {
        RuleFor(x => x.Fecha)
            .NotEmpty()
            .WithMessage("La fecha es requerida")
            .GreaterThanOrEqualTo(new DateTime(2020, 1, 1))
            .WithMessage("La fecha no puede ser anterior al año 2020")
            .LessThanOrEqualTo(DateTime.Now.AddYears(2))
            .WithMessage("La fecha no puede ser superior a 2 años en el futuro");

        RuleFor(x => x.Pagina)
            .GreaterThan(0)
            .WithMessage("La página debe ser mayor a 0");

        RuleFor(x => x.TamanoPagina)
            .GreaterThan(0)
            .WithMessage("El tamaño de página debe ser mayor a 0")
            .LessThanOrEqualTo(100)
            .WithMessage("El tamaño de página no puede exceder 100");

        RuleFor(x => x.MesaId)
            .NotEqual(Guid.Empty)
            .WithMessage("El ID de mesa no puede ser vacío")
            .When(x => x.MesaId.HasValue);

        // Validación condicional para fechas futuras
        RuleFor(x => x.Fecha)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Para filtrar por reservaciones futuras, la fecha debe ser igual o posterior a hoy")
            .When(x => x.SoloFuturas);

        // Validación de estado válido
        RuleFor(x => x.Estado)
            .IsInEnum()
            .WithMessage("El estado de reservación no es válido")
            .When(x => x.Estado.HasValue);
    }
} 