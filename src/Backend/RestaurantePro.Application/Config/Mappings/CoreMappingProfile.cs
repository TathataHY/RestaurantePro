using RestaurantePro.Application.Core.Productos.Commands.CrearProducto;
using RestaurantePro.Application.Core.Productos.Commands.ActualizarProducto;

namespace RestaurantePro.Application.Config.Mappings;

/// <summary>
/// Profile de AutoMapper para el contexto Core
/// Maneja mapeos entre entidades del Domain y DTOs de Application
/// </summary>
public class CoreMappingProfile : Profile
{
    public CoreMappingProfile()
    {
        ConfigureProductoMappings();
        ConfigureUsuarioMappings();
        ConfigureNotificacionMappings();
        ConfigureRecetaMappings();
    }

    /// <summary>
    /// Configura los mapeos para el agregado Producto
    /// </summary>
    private void ConfigureProductoMappings()
    {
        // 🔄 Entidad → DTO Principal (Response)
        CreateMap<Producto, ProductoDto>()
            .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.Precio != null ? src.Precio.Valor : 0))
            .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.CategoriaNombre ?? "Sin categoría"))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstaActivo))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.FechaModificacion, opt => opt.Ignore())
            .ForMember(dest => dest.CreadoPor, opt => opt.MapFrom(src => "Sistema"))
            .ForMember(dest => dest.ModificadoPor, opt => opt.Ignore());

        // 🔄 Entidad → DTO Resumido (Listas)
        CreateMap<Producto, ProductoSummaryDto>()
            .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.Precio != null ? src.Precio.Valor : 0))
            .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.CategoriaNombre ?? "Sin categoría"))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstaActivo));

        // ➕ DTO Create → Command (Input)
        CreateMap<ProductoCreateDto, CrearProductoCommand>();

        // ✏️ DTO Update → Command (Input)  
        CreateMap<ProductoUpdateDto, ActualizarProductoCommand>();

        // Nota: Los mapeos de Command → ValueObjects se manejan directamente en los handlers
        // usando los builders del domain, no a través de AutoMapper
    }

    /// <summary>
    /// Configura los mapeos para el agregado Usuario
    /// </summary>
    private void ConfigureUsuarioMappings()
    {
        // TODO: Implementar cuando tengamos DTOs de Usuario
        // CreateMap<Usuario, UsuarioDto>()...
    }

    /// <summary>
    /// Configura los mapeos para el agregado Notificacion
    /// </summary>
    private void ConfigureNotificacionMappings()
    {
        // TODO: Implementar cuando tengamos DTOs de Notificacion
        // CreateMap<Notificacion, NotificacionDto>()...
    }

    /// <summary>
    /// Configura los mapeos para el agregado Receta
    /// </summary>
    private void ConfigureRecetaMappings()
    {
        // TODO: Implementar cuando tengamos DTOs de Receta
        // CreateMap<Receta, RecetaDto>()...
    }
} 