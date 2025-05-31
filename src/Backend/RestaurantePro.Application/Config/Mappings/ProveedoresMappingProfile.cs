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
        CreateMap<Proveedor, ProveedorDto>();
            // TODO: Reactivar cuando existan estas propiedades en Domain y DTOs
            //.ForMember(dest => dest.TipoTexto, opt => opt.MapFrom(src => src.Tipo.ToString()))
            //.ForMember(dest => dest.CondicionesPagoTexto, opt => opt.MapFrom(src => src.CondicionesPago.ToString()))
            //.ForMember(dest => dest.CalificacionTexto, opt => opt.MapFrom(src => src.Calificacion.ToString()));

        // TODO: Reactivar cuando existan estos DTOs
        //CreateMap<ProveedorCreateDto, CrearProveedorCommand>();
        //CreateMap<ProveedorUpdateDto, ActualizarProveedorCommand>();

        // ContactoProveedor mappings  
        CreateMap<ContactoProveedor, RestaurantePro.Application.Proveedores.ContactosProveedor.DTOs.ContactoProveedorDto>();
            // TODO: Reactivar cuando exista la navegación Proveedor en ContactoProveedor
            //.ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src => src.Proveedor != null ? src.Proveedor.Nombre : string.Empty));

        // TODO: Reactivar cuando existan los Commands
        //CreateMap<ContactoProveedorCreateDto, AgregarContactoCommand>();
        //CreateMap<ContactoProveedorUpdateDto, ActualizarContactoCommand>();
    }
} 