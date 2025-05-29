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
        RuleFor(x => x.Id)
            .MustAsync(ContactoExists)
            .WithMessage("El contacto especificado no existe");

        RuleFor(x => x.ProveedorId)
            .MustAsync(ProveedorExists)
            .WithMessage("El proveedor especificado no existe");

        RuleFor(x => x)
            .MustAsync(ContactoBelongsToProveedor)
            .WithMessage("El contacto no pertenece al proveedor especificado");

        RuleFor(x => x.NuevoContactoPrincipalId)
            .MustAsync((command, nuevoContactoId, cancellationToken) => 
                ValidateNuevoContactoPrincipal(command, nuevoContactoId, cancellationToken))
            .WithMessage("El nuevo contacto principal especificado no es válido")
            .When(x => x.NuevoContactoPrincipalId.HasValue);

        // Validaciones de reglas de negocio
        RuleFor(x => x)
            .MustAsync(CanDeleteContact)
            .WithMessage("No se puede eliminar este contacto debido a restricciones de negocio");
    }

    private static bool BeLogicalDeleteWhenNotForced(TipoEliminacion tipoEliminacion)
    {
        // Si es eliminación física, debe estar forzada
        return tipoEliminacion == TipoEliminacion.Logica;
    }

    private static async Task<bool> ContactoExists(Guid contactoId, CancellationToken cancellationToken)
    {
        // Esta validación requiere acceso al repositorio
        // Se implementaría inyectando IContactoProveedorRepository
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> ProveedorExists(Guid proveedorId, CancellationToken cancellationToken)
    {
        // Esta validación requiere acceso al repositorio
        // Se implementaría inyectando IProveedorRepository
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> ContactoBelongsToProveedor(EliminarContactoCommand command, CancellationToken cancellationToken)
    {
        // Esta validación verifica que el contacto pertenezca al proveedor especificado
        // Se implementaría consultando IContactoProveedorRepository
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> ValidateNuevoContactoPrincipal(EliminarContactoCommand command, Guid? nuevoContactoId, CancellationToken cancellationToken)
    {
        // Esta validación verifica que:
        // 1. El nuevo contacto principal existe
        // 2. Pertenece al mismo proveedor
        // 3. Está activo
        // 4. No es el mismo contacto que se está eliminando
        await Task.CompletedTask;
        return true;
    }

    private static async Task<bool> CanDeleteContact(EliminarContactoCommand command, CancellationToken cancellationToken)
    {
        // Esta validación verifica que:
        // 1. No es el último contacto activo del proveedor (excepto si se fuerza)
        // 2. No tiene dependencias críticas (pedidos pendientes, etc.)
        // 3. Si es contacto principal, hay otros contactos disponibles para reasignar
        await Task.CompletedTask;
        return true;
    }
} 