namespace RestaurantePro.Application.Config.Mappings;

/// <summary>
/// Profile de AutoMapper para el contexto Comercial
/// Configura los mapeos entre entidades de dominio y DTOs para Clientes
/// </summary>
public class ComercialMappingProfile : Profile
{
    public ComercialMappingProfile()
    {
        ConfigurarMapeosClientes();
        ConfigurarMapeosFacturacion();
        // TODO: Agregar otros mapeos cuando estén implementados:
        // ConfigurarMapeosFidelizacion();
        // ConfigurarMapeosPagos();
        // ConfigurarMapeosPromociones();
    }

    /// <summary>
    /// Configura los mapeos específicos para Clientes
    /// </summary>
    private void ConfigurarMapeosClientes()
    {
        // Cliente Entity -> ClienteDto
        CreateMap<Cliente, ClienteDto>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre.Nombre))
            .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => src.Nombre.Apellido))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono.Value))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstaActivo));

        // Cliente Entity -> ClienteSummaryDto
        CreateMap<Cliente, ClienteSummaryDto>()
            .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => $"{src.Nombre.Nombre} {src.Nombre.Apellido}".Trim()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono.Value))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstaActivo));

        // ClienteCreateDto -> CrearClienteCommand
        CreateMap<ClienteCreateDto, CrearClienteCommand>();

        // ClienteUpdateDto -> ActualizarClienteCommand
        CreateMap<ClienteUpdateDto, ActualizarClienteCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // El ID viene por separado

        // TODO: Reactivar cuando existan estos DTOs y entidades en Domain
        // TarjetaFidelizacion mappings
        //CreateMap<TarjetaFidelizacion, TarjetaFidelizacionDto>()
        //    .ForMember(dest => dest.NivelTexto, opt => opt.MapFrom(src => src.Nivel.ToString()))
        //    .ForMember(dest => dest.NombreCliente, opt => opt.MapFrom(src => src.Cliente != null ? $"{src.Cliente.Nombre} {src.Cliente.Apellido}".Trim() : string.Empty));

        // Factura mappings
        //CreateMap<Factura, FacturaDto>()
        //    .ForMember(dest => dest.EstadoTexto, opt => opt.MapFrom(src => src.Estado.ToString()))
        //    .ForMember(dest => dest.TipoTexto, opt => opt.MapFrom(src => src.Tipo.ToString()))
        //    .ForMember(dest => dest.NombreCliente, opt => opt.MapFrom(src => src.Cliente != null ? $"{src.Cliente.Nombre} {src.Cliente.Apellido}".Trim() : "Cliente Anónimo"));
    }

    /// <summary>
    /// Configura los mapeos específicos para Facturación
    /// </summary>
    private void ConfigurarMapeosFacturacion()
    {
        // Factura Entity -> FacturaDto
        CreateMap<Factura, FacturaDto>()
            .ForMember(dest => dest.NumeroFactura, opt => opt.MapFrom(src => src.Numero.Value))
            .ForMember(dest => dest.FechaEmision, opt => opt.MapFrom(src => src.FechaEmision))
            .ForMember(dest => dest.FechaVencimiento, opt => opt.MapFrom(src => src.FechaVencimiento))
            .ForMember(dest => dest.EstadoTexto, opt => opt.MapFrom(src => src.Estado.ToString()))
            .ForMember(dest => dest.TipoFacturaTexto, opt => opt.MapFrom(src => src.TipoFactura.ToString()))
            .ForMember(dest => dest.SubTotal, opt => opt.MapFrom(src => src.SubTotal.Amount))
            .ForMember(dest => dest.TotalImpuestos, opt => opt.MapFrom(src => src.TotalImpuestos.Amount))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total.Amount))
            .ForMember(dest => dest.EstaPagada, opt => opt.MapFrom(src => src.EstaPagada))
            .ForMember(dest => dest.EstaPendiente, opt => opt.MapFrom(src => src.EstaPendiente))
            .ForMember(dest => dest.EstaVencida, opt => opt.MapFrom(src => src.EstaVencida))
            .ForMember(dest => dest.TieneSaldo, opt => opt.MapFrom(src => src.TieneSaldo))
            .ForMember(dest => dest.Saldo, opt => opt.MapFrom(src => src.Saldo.Amount));

        // DetalleFactura Entity -> DetalleFacturaDto
        CreateMap<DetalleFactura, DetalleFacturaDto>()
            .ForMember(dest => dest.ProductoNombre, opt => opt.MapFrom(src => src.ProductoNombre))
            .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.Cantidad))
            .ForMember(dest => dest.PrecioUnitario, opt => opt.MapFrom(src => src.PrecioUnitario.Amount))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal.Amount));

        // DescuentoFactura Entity -> DescuentoFacturaDto
        CreateMap<DescuentoFactura, DescuentoFacturaDto>()
            .ForMember(dest => dest.TipoDescuento, opt => opt.MapFrom(src => src.TipoDescuento.ToString()))
            .ForMember(dest => dest.Valor, opt => opt.MapFrom(src => src.Valor.Amount))
            .ForMember(dest => dest.Concepto, opt => opt.MapFrom(src => src.Concepto))
            .ForMember(dest => dest.FechaAplicacion, opt => opt.MapFrom(src => src.FechaAplicacion));
    }
} 