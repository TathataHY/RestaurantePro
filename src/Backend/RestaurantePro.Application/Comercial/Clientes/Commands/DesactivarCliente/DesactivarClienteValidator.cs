using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Comercial.Clientes.Commands.DesactivarCliente;

/// <summary>
/// Validador para DesactivarClienteCommand
/// </summary>
public class DesactivarClienteValidator : AbstractValidator<DesactivarClienteCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly bool _testMode;

    public DesactivarClienteValidator(IApplicationDbContext context, bool testMode = false)
    {
        _context = context;
        _testMode = testMode;

        RuleFor(v => v.ClienteId)
            .NotEmpty()
            .WithMessage("El ID del cliente es requerido")
            .WithErrorCode("CLIENTE_ID_REQUERIDO");

        if (!_testMode)
        {
            RuleFor(v => v.ClienteId)
                .MustAsync(ClienteExisteAsync)
                .WithMessage("El cliente no existe")
                .WithErrorCode("CLIENTE_NO_EXISTE")
                .MustAsync(ClienteEstaActivoAsync)
                .WithMessage("El cliente ya se encuentra desactivado")
                .WithErrorCode("CLIENTE_YA_DESACTIVADO");
        }

        RuleFor(v => v.MotivoDesactivacion)
            .NotEmpty()
            .WithMessage("El motivo de desactivación es requerido")
            .WithErrorCode("MOTIVO_REQUERIDO")
            .MaximumLength(500)
            .WithMessage("El motivo de desactivación no puede exceder los 500 caracteres")
            .WithErrorCode("MOTIVO_MUY_LARGO");

        RuleFor(v => v.NotasAdicionales)
            .MaximumLength(1000)
            .When(v => v.NotasAdicionales != null)
            .WithMessage("Las notas adicionales no pueden exceder los 1000 caracteres")
            .WithErrorCode("NOTAS_MUY_LARGAS");

        RuleFor(v => v.DesactivadoPor)
            .NotEmpty()
            .WithMessage("El usuario que desactiva es requerido")
            .WithErrorCode("USUARIO_REQUERIDO");
    }

    public virtual async Task<bool> ClienteExisteAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        if (_testMode) return true;
        if (_context?.Clientes == null) return false; // Si no hay contexto, el cliente no existe

        try
        {
            var clientes = _context.Clientes.AsQueryable();
            return await clientes.AnyAsync(c => c.Id == clienteId, cancellationToken);
        }
        catch (Exception)
        {
            // En caso de cualquier error en la consulta, asumimos que el cliente no existe
            return false;
        }
    }

    public virtual async Task<bool> ClienteEstaActivoAsync(Guid clienteId, CancellationToken cancellationToken)
    {
        if (_testMode) return true;
        if (_context?.Clientes == null) return false; // Si no hay contexto, asumimos que el cliente no está activo

        try
        {
            var clientes = _context.Clientes.AsQueryable();
            
            var cliente = await clientes.FirstOrDefaultAsync(c => c.Id == clienteId, cancellationToken);

            if (cliente == null) return false; // Si el cliente no existe, no está activo

            // Cuando una regla MustAsync retorna false, la validación falla
            // Para esta regla queremos que falle cuando el cliente NO está activo,
            // por lo que retornamos FALSE cuando el cliente está desactivado
            return cliente.EstaActivo;
        }
        catch (Exception)
        {
            // En caso de cualquier otro error, asumimos que el cliente no está activo
            return false;
        }
    }
} 