using FluentValidation;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;

namespace RestaurantePro.Application.Operaciones.Comandas.Queries.ObtenerComandasPaginadas;

/// <summary>
/// Validator para ObtenerComandasPaginadasQuery
/// </summary>
public class ObtenerComandasPaginadasQueryValidator : AbstractValidator<ObtenerComandasPaginadasQuery>
{
    public ObtenerComandasPaginadasQueryValidator()
    {
        // 🎯 Validar paginación
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("🚫 El número de página debe ser mayor a 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("🚫 El tamaño de página debe ser mayor a 0")
            .LessThanOrEqualTo(100)
            .WithMessage("🚫 El tamaño de página no puede exceder 100");

        // 🎯 Validar estado si se especifica
        RuleFor(x => x.Estado)
            .Must(estado =>
            {
                if (string.IsNullOrEmpty(estado)) return true;
                return Enum.TryParse<EstadoComanda>(estado, true, out _);
            })
            .WithMessage("🚫 El estado especificado no es válido")
            .When(x => !string.IsNullOrEmpty(x.Estado));

        // 🎯 Validar fechas
        RuleFor(x => x.FechaDesde)
            .LessThanOrEqualTo(x => x.FechaHasta)
            .WithMessage("🚫 La fecha desde debe ser menor o igual a la fecha hasta")
            .When(x => x.FechaDesde.HasValue && x.FechaHasta.HasValue);

        RuleFor(x => x.FechaDesde)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("🚫 La fecha desde no puede ser futura")
            .When(x => x.FechaDesde.HasValue);

        RuleFor(x => x.FechaHasta)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("🚫 La fecha hasta no puede ser futura")
            .When(x => x.FechaHasta.HasValue);

        // 🎯 Validar ordenamiento
        RuleFor(x => x.OrdenarPor)
            .Must(ordenarPor =>
            {
                if (string.IsNullOrEmpty(ordenarPor)) return true;
                var camposValidos = new[] { "fechacreacion", "numerocomanda", "estado", "mesa", "mesero", "cliente", "total" };
                return camposValidos.Contains(ordenarPor.ToLower());
            })
            .WithMessage("🚫 El campo de ordenamiento no es válido")
            .When(x => !string.IsNullOrEmpty(x.OrdenarPor));

        RuleFor(x => x.DireccionOrdenamiento)
            .Must(direccion =>
            {
                if (string.IsNullOrEmpty(direccion)) return true;
                return direccion.ToLower() == "asc" || direccion.ToLower() == "desc";
            })
            .WithMessage("🚫 La dirección de ordenamiento debe ser 'asc' o 'desc'")
            .When(x => !string.IsNullOrEmpty(x.DireccionOrdenamiento));
    }
} 