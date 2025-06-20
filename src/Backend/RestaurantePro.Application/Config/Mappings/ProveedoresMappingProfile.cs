namespace RestaurantePro.Application.Config.Mappings;

/// <summary>
/// Configuración de AutoMapper para el contexto Proveedores
/// Define mapeos entre entidades del dominio y DTOs de aplicación
/// </summary>
public class ProveedoresMappingProfile : Profile
{
    public ProveedoresMappingProfile()
    {
        // Proveedor mappings
        CreateMap<Proveedor, ProveedorDto>()
            .ForMember(dest => dest.RUT, opt => opt.MapFrom(src => src.RFC)) // Mapear RFC del dominio a RUT del DTO
            .ForMember(dest => dest.Descripcion, opt => opt.Ignore()) // No existe en el dominio actual
            .ForMember(dest => dest.Categoria, opt => opt.Ignore()) // No existe en el dominio actual
            .ForMember(dest => dest.CalificacionPromedio, opt => opt.Ignore()) // No existe en el dominio actual
            .ForMember(dest => dest.CreadoPor, opt => opt.Ignore()) // No existe en el dominio actual
            .ForMember(dest => dest.UltimaCompra, opt => opt.Ignore()) // No existe en el dominio actual
            .ForMember(dest => dest.FechaModificacion, opt => opt.MapFrom(src => src.FechaActualizacion)) // Mapear correctamente
            .ForMember(dest => dest.ModificadoPor, opt => opt.Ignore()); // No existe en el dominio actual

        // TODO: Reactivar cuando existan estas propiedades en Domain y DTOs
        //.ForMember(dest => dest.TipoTexto, opt => opt.MapFrom(src => src.Tipo.ToString()))
        //.ForMember(dest => dest.CondicionesPagoTexto, opt => opt.MapFrom(src => src.CondicionesPago.ToString()))
        //.ForMember(dest => dest.CalificacionTexto, opt => opt.MapFrom(src => src.Calificacion.ToString()));

        // TODO: Reactivar cuando existan estos DTOs
        //CreateMap<ProveedorCreateDto, CrearProveedorCommand>();
        //CreateMap<ProveedorUpdateDto, ActualizarProveedorCommand>();

        // ContactoProveedor mappings  
        CreateMap<ContactoProveedor, RestaurantePro.Application.Proveedores.Proveedores.DTOs.ContactoProveedorDto>()
            .ForMember(dest => dest.EsPrincipal, opt => opt.Ignore()); // No existe en el dominio actual

        // TODO: Reactivar cuando exista la navegación Proveedor en ContactoProveedor
        //.ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src => src.Proveedor != null ? src.Proveedor.Nombre : string.Empty));

        // TODO: Reactivar cuando existan los Commands
        //CreateMap<ContactoProveedorCreateDto, AgregarContactoCommand>();
        //CreateMap<ContactoProveedorUpdateDto, ActualizarContactoCommand>();
    }
} 