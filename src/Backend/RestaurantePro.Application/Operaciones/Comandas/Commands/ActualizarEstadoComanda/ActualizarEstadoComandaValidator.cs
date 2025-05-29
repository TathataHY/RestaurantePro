namespace RestaurantePro.Application.Operaciones.Comandas.Commands.ActualizarEstadoComanda;

/// <summary>
/// Validador para ActualizarEstadoComandaCommand
/// Valida transiciones de estado y reglas de negocio operativas
/// </summary>
public class ActualizarEstadoComandaValidator : AbstractValidator<ActualizarEstadoComandaCommand>
{
    private static readonly string[] EstadosValidos = 
    {
        "Creada",
        "EnProceso", 
        "Lista",
        "Entregada",
        "Finalizada",
        "Cancelada"
    };

    public ActualizarEstadoComandaValidator()
    {
        RuleFor(x => x.ComandaId)
            .NotEmpty().WithMessage("El ID de la comanda es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID de la comanda no puede ser un GUID vacío");

        RuleFor(x => x.NuevoEstado)
            .NotEmpty().WithMessage("El nuevo estado es obligatorio")
            .Must(estado => EstadosValidos.Contains(estado))
            .WithMessage($"El estado debe ser uno de: {string.Join(", ", EstadosValidos)}");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("El ID del usuario es obligatorio")
            .NotEqual(Guid.Empty).WithMessage("El ID del usuario no puede ser un GUID vacío");

        RuleFor(x => x.Observaciones)
            .MaximumLength(500).WithMessage("Las observaciones no pueden exceder 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.Observaciones));

        // Validaciones específicas por estado
        RuleFor(x => x)
            .Must(ValidarTransicionEstado)
            .WithMessage("La transición de estado solicitada no es válida según las reglas de negocio")
            .When(x => EstadosValidos.Contains(x.NuevoEstado));
    }

    /// <summary>
    /// Valida que la transición de estado sea lógica
    /// Nota: Esta validación es básica, la validación completa se hace en el handler con el estado actual
    /// </summary>
    private static bool ValidarTransicionEstado(ActualizarEstadoComandaCommand command)
    {
        // Validaciones básicas que se pueden hacer sin conocer el estado actual
        
        // No se puede volver al estado "Creada" desde ningún otro estado
        if (command.NuevoEstado == "Creada")
        {
            return false;
        }

        // Las transiciones específicas se validarán en el handler con el estado actual
        return true;
    }
} 