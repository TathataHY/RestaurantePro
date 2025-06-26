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
        ConfigurarMapeosPromociones();
        // TODO: Agregar otros mapeos cuando estén implementados:
        // ConfigurarMapeosFidelizacion();
        // ConfigurarMapeosPagos();
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
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstaActivo))
            // Mapeos de propiedades faltantes 
            .ForMember(dest => dest.Direccion, opt => opt.Ignore()) // TODO: Implementar en entidad
            .ForMember(dest => dest.Tipo, opt => opt.Ignore()) // TODO: Implementar en entidad
            .ForMember(dest => dest.Notas, opt => opt.Ignore()) // TODO: Implementar en entidad
            .ForMember(dest => dest.TarjetaFidelizacionId, opt => opt.Ignore()) // TODO: Implementar en entidad
            .ForMember(dest => dest.PuntosFidelizacion, opt => opt.MapFrom(src => 0)) // Default value
            .ForMember(dest => dest.NivelFidelizacion, opt => opt.MapFrom(src => NivelFidelizacion.Basico)) // Default value
            .ForMember(dest => dest.TotalVisitas, opt => opt.MapFrom(src => 0)) // TODO: Calcular desde histórico
            .ForMember(dest => dest.TotalGastado, opt => opt.MapFrom(src => 0)) // TODO: Calcular desde histórico
            .ForMember(dest => dest.UltimaVisita, opt => opt.MapFrom(src => (DateTime?)null)) // TODO: Calcular desde histórico
            .ForMember(dest => dest.PromedioGasto, opt => opt.MapFrom(src => 0)) // TODO: Calcular desde histórico
            // Heredadas de BaseDto
            .ForMember(dest => dest.FechaModificacion, opt => opt.Ignore()) // TODO: Implementar en entidad
            .ForMember(dest => dest.CreadoPor, opt => opt.Ignore()) // TODO: Implementar en entidad
            .ForMember(dest => dest.ModificadoPor, opt => opt.Ignore()); // TODO: Implementar en entidad

        // Cliente Entity -> ClienteSummaryDto
        CreateMap<Cliente, ClienteSummaryDto>()
            .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => $"{src.Nombre.Nombre} {src.Nombre.Apellido}".Trim()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono.Value))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstaActivo))
            // Mapeos de propiedades faltantes
            .ForMember(dest => dest.TipoCliente, opt => opt.MapFrom(src => "Regular")) // TODO: Implementar en entidad
            .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.RegistradoPor, opt => opt.MapFrom(src => "Sistema")) // TODO: Implementar en entidad
            .ForMember(dest => dest.FechaNacimiento, opt => opt.Ignore()) // TODO: Implementar en entidad
            .ForMember(dest => dest.Ciudad, opt => opt.MapFrom(src => "")) // TODO: Implementar en entidad
            .ForMember(dest => dest.Pais, opt => opt.MapFrom(src => "")) // TODO: Implementar en entidad
            .ForMember(dest => dest.PuntosFidelizacion, opt => opt.MapFrom(src => 0)) // TODO: Calcular desde fidelización
            .ForMember(dest => dest.NivelFidelizacion, opt => opt.MapFrom(src => "Bronce")) // TODO: Calcular desde fidelización
            .ForMember(dest => dest.TotalOrdenes, opt => opt.MapFrom(src => 0)) // TODO: Calcular desde histórico
            .ForMember(dest => dest.MontoTotalCompras, opt => opt.MapFrom(src => 0)) // TODO: Calcular desde histórico
            .ForMember(dest => dest.FechaUltimaOrden, opt => opt.MapFrom(src => (DateTime?)null)) // TODO: Calcular desde histórico
            .ForMember(dest => dest.PromedioCompra, opt => opt.MapFrom(src => 0)) // TODO: Calcular desde histórico
            .ForMember(dest => dest.EsFrecuente, opt => opt.MapFrom(src => false)) // TODO: Calcular desde histórico
            .ForMember(dest => dest.DiasSinVisitar, opt => opt.MapFrom(src => 0)) // TODO: Calcular desde histórico
            .ForMember(dest => dest.EsVIP, opt => opt.MapFrom(src => false)); // TODO: Calcular desde lógica de negocio

        // ClienteCreateDto -> CrearClienteCommand
        CreateMap<ClienteCreateDto, CrearClienteCommand>()
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => $"{src.Nombre} {src.Apellido}".Trim()))
            .ForMember(dest => dest.EstaActivo, opt => opt.MapFrom(src => true));

        // ClienteUpdateDto -> ActualizarClienteCommand
        CreateMap<ClienteUpdateDto, ActualizarClienteCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // El ID viene por separado
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => $"{src.Nombre} {src.Apellido}".Trim()))
            .ForMember(dest => dest.EstaActivo, opt => opt.Ignore()); // Mapear si existe en UpdateDto

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
            .ForMember(dest => dest.FechaPago, opt => opt.MapFrom(src => src.FechaPago))
            // Mapeos de propiedades faltantes identificadas por AutoMapper
            .ForMember(dest => dest.ComandaId, opt => opt.Ignore()) // TODO: Implementar en entidad
            .ForMember(dest => dest.NumeroComanda, opt => opt.Ignore()) // TODO: Implementar en entidad
            .ForMember(dest => dest.Descuentos, opt => opt.MapFrom(src => 0)) // TODO: Calcular desde descuentos aplicados
            .ForMember(dest => dest.MetodoPago, opt => opt.Ignore()) // TODO: Implementar en entidad
            .ForMember(dest => dest.ReferenciaPago, opt => opt.Ignore()) // TODO: Implementar en entidad
            // Heredadas de BaseDto
            .ForMember(dest => dest.FechaModificacion, opt => opt.Ignore()) // TODO: Implementar en entidad
            .ForMember(dest => dest.CreadoPor, opt => opt.Ignore()) // TODO: Implementar en entidad
            .ForMember(dest => dest.ModificadoPor, opt => opt.Ignore()) // TODO: Implementar en entidad
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => true)); // Default value

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

    /// <summary>
    /// Configura los mapeos específicos para Promociones
    /// </summary>
    private void ConfigurarMapeosPromociones()
    {
        // Promocion Entity -> PromocionDto
        CreateMap<RestaurantePro.Domain.Comercial.Promociones.Entities.Promocion, RestaurantePro.Application.Comercial.Promociones.DTOs.PromocionDto>()
            .ForMember(dest => dest.ProductosAplicablesIds, opt => opt.MapFrom(src => src.ProductosAplicablesIds != null ? src.ProductosAplicablesIds.ToList() : new List<Guid>()))
            .ForMember(dest => dest.CategoriasAplicablesIds, opt => opt.MapFrom(src => src.CategoriasAplicablesIds != null ? src.CategoriasAplicablesIds.ToList() : new List<Guid>()))
            .ForMember(dest => dest.ClientesQueUsaronIds, opt => opt.MapFrom(src => src.ClientesQueUsaronIds != null ? src.ClientesQueUsaronIds.ToList() : new List<Guid>()))
            .ForMember(dest => dest.EstaVigente, opt => opt.MapFrom(src => src.EstaVigente()))
            .ForMember(dest => dest.DescuentoCalculado, opt => opt.Ignore()) // Se calcula en el handler si aplica
            .ForMember(dest => dest.Condiciones, opt => opt.MapFrom(src => src.Condiciones ?? string.Empty))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.FechaUltimaActualizacion, opt => opt.MapFrom(src => src.FechaActualizacion))
            .ForMember(dest => dest.DiasValidos, opt => opt.Ignore()); // Si existe lógica específica, mapear aquí
        // Si necesitas el mapeo inverso:
        // CreateMap<PromocionDto, Promocion>()... (no recomendado para entidades de dominio)
    }
} 