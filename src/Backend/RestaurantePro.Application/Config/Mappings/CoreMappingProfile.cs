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

        // 🔄 Entidad → DTO Resumido (Listas) - ARREGLADO
        CreateMap<Producto, ProductoSummaryDto>()
            .ForMember(dest => dest.Precio, opt => opt.MapFrom(src => src.Precio != null ? src.Precio.Valor : 0))
            .ForMember(dest => dest.CategoriaNombre, opt => opt.MapFrom(src => src.CategoriaNombre ?? "Sin categoría"))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.EstaActivo))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CreadoPor, opt => opt.MapFrom(src => "Sistema"))
            .ForMember(dest => dest.DescripcionCorta, opt => opt.MapFrom(src => 
                !string.IsNullOrEmpty(src.Descripcion) && src.Descripcion.Length > 100 
                    ? src.Descripcion.Substring(0, 100) + "..." 
                    : src.Descripcion ?? "Sin descripción"))
            .ForMember(dest => dest.Disponible, opt => opt.MapFrom(src => src.EstaActivo))
            .ForMember(dest => dest.TotalIngredientes, opt => opt.MapFrom(src => 0)) // TODO: Calcular desde recetas cuando esté implementado
            .ForMember(dest => dest.CostoEstimado, opt => opt.MapFrom(src => (decimal?)null)); // TODO: Calcular desde ingredientes cuando esté implementado

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
        // 🔄 Entidad → DTO Principal (Response)
        CreateMap<Usuario, UsuarioDto>()
            // Propiedades básicas
            .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.NombreUsuario))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
            .ForMember(dest => dest.TipoUsuario, opt => opt.MapFrom(src => src.TipoUsuario))
            
            // Propiedades organizacionales nuevas
            .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol))
            .ForMember(dest => dest.NivelAcceso, opt => opt.MapFrom(src => src.NivelAcceso))
            .ForMember(dest => dest.Permisos, opt => opt.MapFrom(src => src.Permisos.ToList()))
            .ForMember(dest => dest.SupervisorId, opt => opt.MapFrom(src => src.SupervisorId))
            .ForMember(dest => dest.NombreSupervisor, opt => opt.MapFrom(src => string.Empty))
            .ForMember(dest => dest.Departamento, opt => opt.MapFrom(src => src.Departamento))
            .ForMember(dest => dest.Posicion, opt => opt.MapFrom(src => src.Posicion))
            .ForMember(dest => dest.Identificacion, opt => opt.MapFrom(src => src.Identificacion))
            
            // Estado y acceso
            .ForMember(dest => dest.UltimoAcceso, opt => opt.MapFrom(src => src.UltimoAcceso))
            .ForMember(dest => dest.MotivoBloqueo, opt => opt.MapFrom(src => src.MotivoBloqueo))
            .ForMember(dest => dest.EsAdministrador, opt => opt.MapFrom(src => src.EsAdministrador))
            
            // Mapear NombreCompleto a Nombre y Apellido (split básico)
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => 
                MapearPrimerNombre(src.NombreCompleto)))
            .ForMember(dest => dest.Apellido, opt => opt.MapFrom(src => 
                MapearApellidos(src.NombreCompleto)))
                    
            // Propiedades calculadas y de compatibilidad
            .ForMember(dest => dest.NumeroIdentificacion, opt => opt.MapFrom(src => src.Identificacion))
            .ForMember(dest => dest.Cargo, opt => opt.MapFrom(src => src.Posicion))
            .ForMember(dest => dest.FechaUltimaConexion, opt => opt.MapFrom(src => src.UltimoAcceso))
            .ForMember(dest => dest.Verificado, opt => opt.MapFrom(src => src.Estado == EstadoUsuario.Activo))
            
            // Propiedades por implementar (valores por defecto)
            .ForMember(dest => dest.Telefono, opt => opt.Ignore())
            .ForMember(dest => dest.FechaNacimiento, opt => opt.Ignore())
            .ForMember(dest => dest.Direccion, opt => opt.Ignore())
            .ForMember(dest => dest.Ciudad, opt => opt.Ignore())
            .ForMember(dest => dest.Pais, opt => opt.Ignore())
            .ForMember(dest => dest.FechaContratacion, opt => opt.Ignore())
            .ForMember(dest => dest.SalarioBase, opt => opt.Ignore())
            .ForMember(dest => dest.EsTemporal, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.IntentosFallidos, opt => opt.MapFrom(src => 0))
            .ForMember(dest => dest.FechaBloqueado, opt => opt.MapFrom(src => 
                src.Estado == EstadoUsuario.Bloqueado ? DateTime.UtcNow : (DateTime?)null))
            .ForMember(dest => dest.FechaExpiracionPassword, opt => opt.Ignore())
            .ForMember(dest => dest.DebeResetearPassword, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.SucursalesAcceso, opt => opt.MapFrom(src => new List<Guid>()))
            .ForMember(dest => dest.HorariosTrabajo, opt => opt.Ignore())
            .ForMember(dest => dest.ConfiguracionNotificaciones, opt => opt.Ignore())
            .ForMember(dest => dest.Observaciones, opt => opt.Ignore())
            .ForMember(dest => dest.Avatar, opt => opt.Ignore())
            .ForMember(dest => dest.Preferencias, opt => opt.Ignore())
            .ForMember(dest => dest.ZonaHoraria, opt => opt.MapFrom(src => "America/Santiago"))
            .ForMember(dest => dest.Idioma, opt => opt.MapFrom(src => "es-CL"))
            .ForMember(dest => dest.RolesAdicionales, opt => opt.MapFrom(src => new List<string>()))
            .ForMember(dest => dest.FechaUltimaActualizacion, opt => opt.Ignore())
            .ForMember(dest => dest.UsuarioUltimaActualizacion, opt => opt.Ignore())
            .ForMember(dest => dest.CantidadSubordinados, opt => opt.MapFrom(src => 0))
            
            // Heredadas de BaseDto - Usuario hereda de EntityBase
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.Estado == EstadoUsuario.Activo))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.FechaModificacion, opt => opt.Ignore())
            .ForMember(dest => dest.CreadoPor, opt => opt.Ignore())
            .ForMember(dest => dest.ModificadoPor, opt => opt.Ignore());
    }

    /// <summary>
    /// Extrae el primer nombre de un nombre completo
    /// </summary>
    private static string MapearPrimerNombre(string? nombreCompleto)
    {
        if (string.IsNullOrEmpty(nombreCompleto))
            return string.Empty;
            
        var partes = nombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return partes.Length > 0 ? partes[0] : string.Empty;
    }

    /// <summary>
    /// Extrae los apellidos de un nombre completo
    /// </summary>
    private static string MapearApellidos(string? nombreCompleto)
    {
        if (string.IsNullOrEmpty(nombreCompleto))
            return string.Empty;
            
        var partes = nombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return partes.Length > 1 ? string.Join(" ", partes.Skip(1)) : string.Empty;
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