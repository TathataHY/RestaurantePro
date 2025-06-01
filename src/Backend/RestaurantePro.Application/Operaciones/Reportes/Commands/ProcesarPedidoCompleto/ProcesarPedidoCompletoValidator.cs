namespace RestaurantePro.Application.Operaciones.Reportes.Commands.ProcesarPedidoCompleto;

/// <summary>
/// Validator para ProcesarPedidoCompletoCommand
/// </summary>
public class ProcesarPedidoCompletoValidator : AbstractValidator<ProcesarPedidoCompletoCommand>
{
    private readonly List<string> _tiposPagoValidos = new() 
    { 
        "Efectivo", "Tarjeta", "Transferencia", "Cheque", "Mixto" 
    };

    private readonly List<string> _tiposFacturaValidos = new() 
    { 
        "Consumidor Final", "Crédito Fiscal", "Exportación" 
    };

    public ProcesarPedidoCompletoValidator()
    {
        RuleFor(x => x.ComandaId)
            .NotEmpty()
            .WithMessage("El ID de la comanda es requerido");

        RuleFor(x => x.TipoPago)
            .NotEmpty()
            .WithMessage("El tipo de pago es requerido")
            .Must(BeValidPaymentType)
            .WithMessage($"El tipo de pago debe ser uno de: {string.Join(", ", _tiposPagoValidos)}");

        RuleFor(x => x.UsuarioId)
            .NotEmpty()
            .WithMessage("El ID del usuario es requerido");

        // Validaciones cuando requiere pago
        When(x => x.RequierePago, () =>
        {
            RuleFor(x => x.InfoPago)
                .NotNull()
                .WithMessage("La información de pago es requerida cuando se requiere procesamiento de pago");

            RuleFor(x => x.InfoPago!.MontoTotal)
                .GreaterThan(0)
                .WithMessage("El monto total debe ser mayor a 0")
                .When(x => x.InfoPago != null);

            RuleFor(x => x.InfoPago!.Moneda)
                .NotEmpty()
                .WithMessage("La moneda es requerida")
                .Must(BeValidCurrency)
                .WithMessage("La moneda debe ser USD, EUR, o la moneda local")
                .When(x => x.InfoPago != null);
        });

        // Validaciones para pago con tarjeta
        When(x => x.TipoPago == "Tarjeta" && x.InfoPago != null, () =>
        {
            RuleFor(x => x.InfoPago!.NumeroTarjeta)
                .NotEmpty()
                .WithMessage("El número de tarjeta es requerido para pagos con tarjeta")
                .Must(BeValidCardNumber)
                .WithMessage("El número de tarjeta no es válido");

            RuleFor(x => x.InfoPago!.NombreTitular)
                .NotEmpty()
                .WithMessage("El nombre del titular es requerido para pagos con tarjeta")
                .MaximumLength(100)
                .WithMessage("El nombre del titular no puede exceder 100 caracteres");
        });

        // Validaciones de facturación
        RuleFor(x => x.TipoFactura)
            .Must(BeValidInvoiceType)
            .WithMessage($"El tipo de factura debe ser uno de: {string.Join(", ", _tiposFacturaValidos)}")
            .When(x => !string.IsNullOrEmpty(x.TipoFactura));

        RuleFor(x => x.NombreCliente)
            .NotEmpty()
            .WithMessage("El nombre del cliente es requerido para la factura")
            .MaximumLength(200)
            .WithMessage("El nombre del cliente no puede exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.TipoFactura));

        RuleFor(x => x.IdentificacionCliente)
            .NotEmpty()
            .WithMessage("La identificación del cliente es requerida")
            .MaximumLength(50)
            .WithMessage("La identificación no puede exceder 50 caracteres")
            .When(x => x.TipoFactura == "Crédito Fiscal");

        RuleFor(x => x.EmailCliente)
            .EmailAddress()
            .WithMessage("El email del cliente no es válido")
            .When(x => !string.IsNullOrEmpty(x.EmailCliente));

        RuleFor(x => x.TelefonoCliente)
            .Matches(@"^\+?[\d\s\-\(\)]{7,15}$")
            .WithMessage("El teléfono del cliente no es válido")
            .When(x => !string.IsNullOrEmpty(x.TelefonoCliente));

        RuleFor(x => x.ObservacionesFactura)
            .MaximumLength(1000)
            .WithMessage("Las observaciones de la factura no pueden exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.ObservacionesFactura));

        // Validaciones de InfoPago cuando está presente
        RuleFor(x => x.InfoPago!.ReferenciaPago)
            .MaximumLength(100)
            .WithMessage("La referencia de pago no puede exceder 100 caracteres")
            .When(x => x.InfoPago != null && !string.IsNullOrEmpty(x.InfoPago.ReferenciaPago));

        RuleFor(x => x.InfoPago!.ObservacionesPago)
            .MaximumLength(500)
            .WithMessage("Las observaciones de pago no pueden exceder 500 caracteres")
            .When(x => x.InfoPago != null && !string.IsNullOrEmpty(x.InfoPago.ObservacionesPago));
    }

    private bool BeValidPaymentType(string tipoPago)
    {
        return _tiposPagoValidos.Contains(tipoPago);
    }

    private bool BeValidInvoiceType(string? tipoFactura)
    {
        return string.IsNullOrEmpty(tipoFactura) || _tiposFacturaValidos.Contains(tipoFactura);
    }

    private static bool BeValidCurrency(string? moneda)
    {
        var monedasValidas = new[] { "USD", "EUR", "CRC", "GTQ", "HNL", "NIO", "PAB" };
        return !string.IsNullOrEmpty(moneda) && monedasValidas.Contains(moneda.ToUpper());
    }

    private static bool BeValidCardNumber(string? numeroTarjeta)
    {
        if (string.IsNullOrEmpty(numeroTarjeta))
            return false;

        // Remover espacios y guiones
        var numero = numeroTarjeta.Replace(" ", "").Replace("-", "");
        
        // Verificar que solo contenga dígitos y tenga longitud válida
        return numero.All(char.IsDigit) && numero.Length >= 13 && numero.Length <= 19;
    }
} 