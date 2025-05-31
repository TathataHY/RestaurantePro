using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Application.Common.Interfaces;

namespace RestaurantePro.Application.Comercial.Facturacion.Commands.CrearFactura;

public class CrearFacturaValidator : AbstractValidator<CrearFacturaCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly string[] _tiposFacturaValidos = { "Normal", "Fiscal", "Global", "NotaCredito", "NotaDebito" };
    private readonly string[] _monedasValidas = { "MXN", "USD", "EUR", "CAD" };
    private readonly string[] _metodosPagoValidos = { "Efectivo", "TarjetaCredito", "TarjetaDebito", "Transferencia", "Cheque" };

    public CrearFacturaValidator(IApplicationDbContext context)
    {
        _context = context;

        ConfigurarValidacionesBasicas();
        ConfigurarValidacionesComandas();
        ConfigurarValidacionesCliente();
        ConfigurarValidacionesFiscales();
        ConfigurarValidacionesFinancieras();
        ConfigurarValidacionesDescuentos();
        ConfigurarValidacionesNegocio();
    }

    private void ConfigurarValidacionesBasicas()
    {
        RuleFor(v => v.ComandasIds)
            .NotEmpty()
            .WithMessage("Debe especificar al menos una comanda para facturar.")
            .Must(TenerComandasValidas)
            .WithMessage("Todas las comandas deben tener IDs válidos.");

        RuleFor(v => v.TipoFactura)
            .NotEmpty()
            .WithMessage("El tipo de factura es requerido.")
            .Must(tipoFactura => _tiposFacturaValidos.Contains(tipoFactura, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El tipo de factura debe ser uno de: {string.Join(", ", _tiposFacturaValidos)}.");

        RuleFor(v => v.NombreCliente)
            .NotEmpty()
            .WithMessage("El nombre del cliente es requerido.")
            .MaximumLength(200)
            .WithMessage("El nombre del cliente no puede exceder 200 caracteres.")
            .MinimumLength(2)
            .WithMessage("El nombre del cliente debe tener al menos 2 caracteres.");

        RuleFor(v => v.Moneda)
            .Must(moneda => _monedasValidas.Contains(moneda, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"La moneda debe ser una de: {string.Join(", ", _monedasValidas)}.");
    }

    private void ConfigurarValidacionesComandas()
    {
        RuleFor(v => v.ComandasIds)
            .Must(comandasIds => comandasIds.Count <= 10)
            .WithMessage("No se pueden facturar más de 10 comandas a la vez.")
            .MustAsync(TodasLasComandasExisten)
            .WithMessage("Una o más comandas especificadas no existen.")
            .MustAsync(TodasLasComandasEstanCompletas)
            .WithMessage("Solo se pueden facturar comandas completadas.")
            .MustAsync(NingunaCamandaYaFacturada)
            .WithMessage("Una o más comandas ya han sido facturadas.");

        RuleForEach(v => v.ComandasIds)
            .NotEqual(Guid.Empty)
            .WithMessage("Los IDs de comanda no pueden estar vacíos.");
    }

    private void ConfigurarValidacionesCliente()
    {
        RuleFor(v => v.ClienteId)
            .MustAsync(ClienteExiste)
            .WithMessage("El cliente especificado no existe.")
            .When(v => v.ClienteId.HasValue);

        RuleFor(v => v.EmailCliente)
            .EmailAddress()
            .WithMessage("El formato del email no es válido.")
            .MaximumLength(320)
            .WithMessage("El email no puede exceder 320 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.EmailCliente));

        RuleFor(v => v.TelefonoCliente)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("El formato del teléfono no es válido.")
            .When(v => !string.IsNullOrEmpty(v.TelefonoCliente));

        RuleFor(v => v.DireccionCliente)
            .MaximumLength(500)
            .WithMessage("La dirección no puede exceder 500 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.DireccionCliente));
    }

    private void ConfigurarValidacionesFiscales()
    {
        // Para facturas fiscales, el RFC es obligatorio
        RuleFor(v => v.IdentificacionFiscal)
            .NotEmpty()
            .WithMessage("El RFC es obligatorio para facturas fiscales.")
            .Length(12, 13)
            .WithMessage("El RFC debe tener 12 o 13 caracteres.")
            .Must(BeValidRFC)
            .WithMessage("El RFC no tiene un formato válido.")
            .When(v => v.TipoFactura.Equals("Fiscal", StringComparison.OrdinalIgnoreCase));

        // Para facturas fiscales, la dirección es obligatoria
        RuleFor(v => v.DireccionCliente)
            .NotEmpty()
            .WithMessage("La dirección fiscal es obligatoria para facturas fiscales.")
            .When(v => v.TipoFactura.Equals("Fiscal", StringComparison.OrdinalIgnoreCase));

        // Para facturas fiscales con envío por email, el email es obligatorio
        RuleFor(v => v.EmailCliente)
            .NotEmpty()
            .WithMessage("El email es obligatorio para facturas fiscales con envío electrónico.")
            .When(v => v.TipoFactura.Equals("Fiscal", StringComparison.OrdinalIgnoreCase) && v.EnviarPorEmail);
    }

    private void ConfigurarValidacionesFinancieras()
    {
        RuleFor(v => v.DiasCredito)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Los días de crédito no pueden ser negativos.")
            .LessThanOrEqualTo(365)
            .WithMessage("Los días de crédito no pueden exceder 365 días.");

        RuleFor(v => v.TipoCambio)
            .GreaterThan(0)
            .WithMessage("El tipo de cambio debe ser mayor a cero.")
            .LessThanOrEqualTo(100)
            .WithMessage("El tipo de cambio no puede exceder 100.")
            .When(v => v.TipoCambio.HasValue);

        RuleFor(v => v.MetodoPagoPreferido)
            .Must(metodo => string.IsNullOrEmpty(metodo) || _metodosPagoValidos.Contains(metodo, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"El método de pago debe ser uno de: {string.Join(", ", _metodosPagoValidos)}.");
    }

    private void ConfigurarValidacionesDescuentos()
    {
        RuleFor(v => v.DescuentosAdicionales)
            .Must(descuentos => descuentos.Count <= 5)
            .WithMessage("No se pueden aplicar más de 5 descuentos adicionales.");

        RuleForEach(v => v.DescuentosAdicionales)
            .ChildRules(descuento =>
            {
                descuento.RuleFor(d => d.Concepto)
                    .NotEmpty()
                    .WithMessage("El concepto del descuento es requerido.")
                    .MaximumLength(100)
                    .WithMessage("El concepto no puede exceder 100 caracteres.");

                descuento.RuleFor(d => d.Porcentaje)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("El porcentaje de descuento no puede ser negativo.")
                    .LessThanOrEqualTo(100)
                    .WithMessage("El porcentaje de descuento no puede exceder 100%.")
                    .When(d => d.MontoFijo == 0);

                descuento.RuleFor(d => d.MontoFijo)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("El monto fijo de descuento no puede ser negativo.")
                    .When(d => d.Porcentaje == 0);

                descuento.RuleFor(d => d)
                    .Must(d => d.Porcentaje > 0 || d.MontoFijo > 0)
                    .WithMessage("Debe especificar un porcentaje o un monto fijo para el descuento.");

                descuento.RuleFor(d => d.Motivo)
                    .NotEmpty()
                    .WithMessage("El motivo del descuento es requerido.")
                    .MaximumLength(200)
                    .WithMessage("El motivo no puede exceder 200 caracteres.");
            });
    }

    private void ConfigurarValidacionesNegocio()
    {
        RuleFor(v => v.FechaEmision)
            .GreaterThanOrEqualTo(DateTime.Today.AddDays(-30))
            .WithMessage("La fecha de emisión no puede ser anterior a 30 días.")
            .LessThanOrEqualTo(DateTime.Today.AddDays(1))
            .WithMessage("La fecha de emisión no puede ser futura.")
            .When(v => v.FechaEmision.HasValue);

        RuleFor(v => v.Observaciones)
            .MaximumLength(1000)
            .WithMessage("Las observaciones no pueden exceder 1000 caracteres.")
            .When(v => !string.IsNullOrEmpty(v.Observaciones));

        // Validación de lógica de negocio
        RuleFor(v => v)
            .MustAsync(BeValidBusinessLogic)
            .WithMessage("La configuración de la factura no es válida para las reglas de negocio.")
            .WithName("BusinessLogic");
    }

    // Métodos de validación personalizados
    private static bool TenerComandasValidas(List<Guid> comandasIds)
    {
        return comandasIds.All(id => id != Guid.Empty);
    }

    private async Task<bool> TodasLasComandasExisten(List<Guid> comandasIds, CancellationToken cancellationToken)
    {
        var existenTodas = await _context.Comandas
            .Where(c => comandasIds.Contains(c.Id))
            .CountAsync(cancellationToken) == comandasIds.Count;

        return existenTodas;
    }

    private async Task<bool> TodasLasComandasEstanCompletas(List<Guid> comandasIds, CancellationToken cancellationToken)
    {
        var comandasCompletas = await _context.Comandas
            .Where(c => comandasIds.Contains(c.Id))
            .AllAsync(c => c.Estado == EstadoComanda.Completada, cancellationToken);

        return comandasCompletas;
    }

    private async Task<bool> NingunaCamandaYaFacturada(List<Guid> comandasIds, CancellationToken cancellationToken)
    {
        var comandasFacturadas = await _context.Facturas
            .Where(f => f.ComandasIds.Any(id => comandasIds.Contains(id)))
            .AnyAsync(cancellationToken);

        return !comandasFacturadas;
    }

    private async Task<bool> ClienteExiste(Guid? clienteId, CancellationToken cancellationToken)
    {
        if (!clienteId.HasValue) return true;

        return await _context.Clientes
            .AnyAsync(c => c.Id == clienteId.Value, cancellationToken);
    }

    private static bool BeValidRFC(string? rfc)
    {
        if (string.IsNullOrWhiteSpace(rfc)) return false;

        // Validación básica de RFC mexicano
        var rfcPattern = @"^[A-ZÑ&]{3,4}[0-9]{6}[A-Z0-9]{3}$";
        return System.Text.RegularExpressions.Regex.IsMatch(rfc.ToUpper(), rfcPattern);
    }

    private async Task<bool> BeValidBusinessLogic(CrearFacturaCommand command, CancellationToken cancellationToken)
    {
        // Si se especifica envío por email, debe haber email
        if (command.EnviarPorEmail && string.IsNullOrEmpty(command.EmailCliente))
        {
            return false;
        }

        // Si hay tipo de cambio, la moneda no debe ser MXN
        if (command.TipoCambio.HasValue && command.Moneda.Equals("MXN", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Si hay cliente registrado, validar consistencia
        if (command.ClienteId.HasValue)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == command.ClienteId.Value, cancellationToken);

            if (cliente != null)
            {
                // Si el cliente tiene email registrado y se especifica uno diferente
                if (!string.IsNullOrEmpty(cliente.Email) && 
                    !string.IsNullOrEmpty(command.EmailCliente) &&
                    !cliente.Email.Equals(command.EmailCliente, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }
        }

        return true;
    }
} 