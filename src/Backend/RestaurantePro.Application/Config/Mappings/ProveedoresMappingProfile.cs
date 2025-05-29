using RestaurantePro.Domain.Proveedores.Entities;
using RestaurantePro.Application.Proveedores.Proveedores.DTOs;
using RestaurantePro.Application.Proveedores.Proveedores.Commands.CrearProveedor;

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
            .ForMember(dest => dest.TipoTexto, opt => opt.MapFrom(src => src.Tipo.ToString()))
            .ForMember(dest => dest.CondicionesPagoTexto, opt => opt.MapFrom(src => src.CondicionesPago.ToString()))
            .ForMember(dest => dest.CalificacionTexto, opt => opt.MapFrom(src => src.Calificacion.ToString()));

        CreateMap<ProveedorCreateDto, CrearProveedorCommand>();
        CreateMap<ProveedorUpdateDto, ActualizarProveedorCommand>();

        // ContactoProveedor mappings
        CreateMap<ContactoProveedor, ContactoProveedorDto>()
            .ForMember(dest => dest.NombreProveedor, opt => opt.MapFrom(src => src.Proveedor != null ? src.Proveedor.Nombre : string.Empty));

        CreateMap<ContactoProveedorCreateDto, AgregarContactoCommand>();
        CreateMap<ContactoProveedorUpdateDto, ActualizarContactoCommand>();
    }
} 