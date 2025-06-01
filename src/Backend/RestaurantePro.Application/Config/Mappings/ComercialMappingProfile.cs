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
            .ForMember(dest => dest.Numero, opt => opt.MapFrom(src => src.NumeroFactura))
            .ForMember(dest => dest.FechaEmision, opt => opt.MapFrom(src => src.FechaEmision))
            .ForMember(dest => dest.FechaVencimiento, opt => opt.MapFrom(src => src.FechaVencimiento))
            .ForMember(dest => dest.EstadoTexto, opt => opt.MapFrom(src => src.Estado.ToString()))
            .ForMember(dest => dest.TipoTexto, opt => opt.MapFrom(src => src.TipoFactura.ToString()))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal))
            .ForMember(dest => dest.Impuestos, opt => opt.MapFrom(src => src.TotalImpuestos))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.TipoFactura))
            .ForMember(dest => dest.NombreCliente, opt => opt.MapFrom(src => src.NombreCliente))
            .ForMember(dest => dest.ClienteId, opt => opt.MapFrom(src => src.ClienteId))
            .ForMember(dest => dest.MontoPagado, opt => opt.MapFrom(src => src.TotalPagado))
            .ForMember(dest => dest.FechaPago, opt => opt.MapFrom(src => src.FechaPago));

        // TODO: Implementar cuando los DTOs estén disponibles
        // DetalleFacturaDto y DescuentoFacturaDto no existen actualmente
        
        /*
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
        */
    }
} 