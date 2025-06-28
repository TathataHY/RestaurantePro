namespace RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.EliminarContacto;

/// <summary>
/// Validador para EliminarContactoCommand
/// Valida reglas de negocio para la eliminación de contactos de proveedores
/// </summary>
public class EliminarContactoValidator : AbstractValidator<EliminarContactoCommand>
{
    public EliminarContactoValidator()
    {
        // Validaciones básicas requeridas
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("El ID del contacto es obligatorio");

        RuleFor(x => x.ProveedorId)
            .NotEmpty()
            .WithMessage("El ID del proveedor es obligatorio");

        RuleFor(x => x.MotivoEliminacion)
            .NotEmpty()
            .WithMessage("El motivo de eliminación es obligatorio")
            .MaximumLength(300)
            .WithMessage("El motivo de eliminación no puede exceder 300 caracteres");

        RuleFor(x => x.TipoEliminacion)
            .IsInEnum()
            .WithMessage("El tipo de eliminación debe ser válido (Logica o Fisica)");

        // Validaciones condicionales
        RuleFor(x => x.NuevoContactoPrincipalId)
            .NotEqual(x => x.Id)
            .WithMessage("El nuevo contacto principal no puede ser el mismo que se está eliminando")
            .When(x => x.NuevoContactoPrincipalId.HasValue);

        // Validación de eliminación física
        RuleFor(x => x.TipoEliminacion)
            .Must(BeLogicalDeleteWhenNotForced)
            .WithMessage("La eliminación física requiere confirmación forzada")
            .When(x => x.TipoEliminacion == TipoEliminacion.Fisica && !x.ForzarEliminacion);

        // Validaciones de integridad referencial que se realizarán en el handler
        // TODO: Implementar validación real con repositorio
        // RuleFor(x => x.Id)
        //     .MustAsync(ContactoExists)
        //     .WithMessage("El contacto especificado no existe");

        // RuleFor(x => x.ProveedorId)
        //     .MustAsync(ProveedorExists)
        //     .WithMessage("El proveedor especificado no existe");

        // RuleFor(x => x)
        //     .MustAsync(ContactoBelongsToProveedor)
        //     .WithMessage("El contacto no pertenece al proveedor especificado");

        // RuleFor(x => x.NuevoContactoPrincipalId)
        //     .MustAsync((command, nuevoContactoId, cancellationToken) => 
        //         ValidateNuevoContactoPrincipal(command, nuevoContactoId, cancellationToken))
        //     .WithMessage("El nuevo contacto principal especificado no es válido")
        //     .When(x => x.NuevoContactoPrincipalId.HasValue);

        // RuleFor(x => x)
        //     .MustAsync(CanDeleteContact)
        //     .WithMessage("No se puede eliminar este contacto debido a restricciones de negocio");

        // Métodos async simulados eliminados para evitar errores 500
    }

    private static bool BeLogicalDeleteWhenNotForced(TipoEliminacion tipoEliminacion)
    {
        // Si es eliminación física, debe estar forzada
        return tipoEliminacion == TipoEliminacion.Logica;
    }

    // Métodos async simulados eliminados para evitar errores 500
} 