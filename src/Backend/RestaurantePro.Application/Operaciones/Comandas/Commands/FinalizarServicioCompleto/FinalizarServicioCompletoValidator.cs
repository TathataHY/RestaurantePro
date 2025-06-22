using FluentValidation;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Operaciones.Commands.FinalizarServicioCompleto;

/// <summary>
/// 🔍 Validator para FinalizarServicioCompletoCommand con validaciones de negocio complejas
/// Valida el flujo completo de finalización de servicio siguiendo el patrón Saga
/// </summary>
public class FinalizarServicioCompletoValidator : AbstractValidator<FinalizarServicioCompletoCommand>
{
    public FinalizarServicioCompletoValidator(IApplicationDbContext context)
    {
        // 🎯 Validar ComandaId
        RuleFor(x => x.ComandaId)
            .NotEmpty()
            .WithMessage("🚫 El ID de la comanda es obligatorio");

        // 🎯 Validar ClienteId si se especifica
        RuleFor(x => x.ClienteId)
            .MustAsync(async (clienteId, cancellation) =>
            {
                if (!clienteId.HasValue) return true;
                var cliente = await context.Clientes.FindAsync(new object[] { clienteId.Value }, cancellation);
                return cliente != null && cliente.EstaActivo;
            })
            .WithMessage("🚫 El cliente especificado no existe o no está activo")
            .When(x => x.ClienteId.HasValue);

        // 🎯 Validar TipoFinalizacion
        RuleFor(x => x.TipoFinalizacion)
            .IsInEnum()
            .WithMessage("🚫 El tipo de finalización no es válido");

        // 🎯 Validar PropinaSugerida
        RuleFor(x => x.PropinaSugerida)
            .GreaterThanOrEqualTo(0)
            .WithMessage("🚫 La propina sugerida no puede ser negativa")
            .LessThanOrEqualTo(1000)
            .WithMessage("🚫 La propina sugerida no puede exceder $1,000")
            .When(x => x.PropinaSugerida.HasValue);

        // 🎯 Validar ObservacionesFinalizacion
        RuleFor(x => x.ObservacionesFinalizacion)
            .MaximumLength(2000)
            .WithMessage("🚫 Las observaciones de finalización no pueden exceder 2000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.ObservacionesFinalizacion));

        // 🎯 Validaciones de lógica de negocio complejas
        RuleFor(x => x)
            .MustAsync(async (command, cancellation) =>
            {
                // Validar que la comanda existe y está en estado válido para finalizar
                var comanda = await context.Comandas.FindAsync(new object[] { command.ComandaId }, cancellation);
                if (comanda == null) return false;

                // Solo se puede finalizar comandas en estados específicos
                var estadosValidos = new[] { 
                    Domain.Operaciones.Comandas.Enums.EstadoComanda.EnProceso,
                    Domain.Operaciones.Comandas.Enums.EstadoComanda.Lista,
                    Domain.Operaciones.Comandas.Enums.EstadoComanda.Servida
                };

                return estadosValidos.Contains(comanda.Estado);
            })
            .WithMessage("🚫 La comanda no existe o no está en un estado válido para finalizar");

        // 🎯 Validar configuración de finalización según tipo
        RuleFor(x => x)
            .Must(command =>
            {
                return command.TipoFinalizacion switch
                {
                    TipoFinalizacion.Normal => true, // Configuración estándar
                    TipoFinalizacion.Express => !command.AplicarDescuentoFidelizacion, // Sin descuentos
                    TipoFinalizacion.Premium => command.GenerarFacturaInmediata, // Siempre factura
                    TipoFinalizacion.Cortesia => !command.GenerarFacturaInmediata, // Sin factura
                    TipoFinalizacion.Cancelacion => !command.AplicarDescuentoFidelizacion && !command.GenerarFacturaInmediata,
                    _ => false
                };
            })
            .WithMessage("🚫 La configuración no es válida para el tipo de finalización seleccionado");

        // 🎯 Validar que si se genera factura inmediata, debe haber cliente
        RuleFor(x => x.ClienteId)
            .NotEmpty()
            .WithMessage("🚫 Debe especificar un cliente para generar factura inmediata")
            .When(x => x.GenerarFacturaInmediata);

        // 🎯 Validar que si se aplica descuento de fidelización, debe haber cliente
        RuleFor(x => x.ClienteId)
            .NotEmpty()
            .WithMessage("🚫 Debe especificar un cliente para aplicar descuento de fidelización")
            .When(x => x.AplicarDescuentoFidelizacion);

        // 🎯 Validar que si se envía notificación, debe haber cliente
        RuleFor(x => x.ClienteId)
            .NotEmpty()
            .WithMessage("🚫 Debe especificar un cliente para enviar notificación")
            .When(x => x.EnviarNotificacionCliente);

        // 🎯 Validar configuración de propina según tipo de finalización
        RuleFor(x => x.PropinaSugerida)
            .Must((command, propina) =>
            {
                return command.TipoFinalizacion switch
                {
                    TipoFinalizacion.Cortesia => !propina.HasValue || propina.Value == 0,
                    TipoFinalizacion.Cancelacion => !propina.HasValue || propina.Value == 0,
                    _ => true
                };
            })
            .WithMessage("🚫 No se puede sugerir propina para finalización de cortesía o cancelación")
            .When(x => x.PropinaSugerida.HasValue);
    }
} 