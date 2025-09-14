using RestaurantePro.Application.Common.DTOs;
using RestaurantePro.Application.Core.Usuarios.Commands.ActualizarUsuario;

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
        // ✏️ DTO Request → Command (Input)  
        CreateMap<ActualizarUsuarioRequest, ActualizarUsuarioCommand>()
            .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.NombreCompleto)) // ✅ MAPEO CORREGIDO
            .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.NombreUsuario))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono))
            .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol))
            .ForMember(dest => dest.NivelAcceso, opt => opt.MapFrom(src => src.NivelAcceso ?? 1))
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.Activo))
            .ForMember(dest => dest.Identificacion, opt => opt.Ignore())
            .ForMember(dest => dest.Direccion, opt => opt.MapFrom(src => src.Direccion))
            .ForMember(dest => dest.UsuarioAutorizaId, opt => opt.MapFrom(src => src.UsuarioAutorizaId))
            .ForMember(dest => dest.MotivoActualizacion, opt => opt.MapFrom(src => src.MotivoActualizacion))
            .ForMember(dest => dest.ObservacionesAdicionales, opt => opt.MapFrom(src => src.ObservacionesAdicionales))
            .ForMember(dest => dest.NotificarUsuario, opt => opt.MapFrom(src => src.NotificarUsuario))
            .ForMember(dest => dest.NotificarSupervisor, opt => opt.MapFrom(src => src.NotificarSupervisor))
            .ForMember(dest => dest.RequiereAprobacion, opt => opt.MapFrom(src => src.RequiereAprobacion))
            .ForMember(dest => dest.CrearBackup, opt => opt.MapFrom(src => src.CrearBackup))
            .ForMember(dest => dest.Prioridad, opt => opt.MapFrom(src => src.Prioridad))
            .ForMember(dest => dest.DocumentosAdjuntos, opt => opt.MapFrom(src => src.DocumentosAdjuntos))
            .ForMember(dest => dest.InvalidarSesionesActivas, opt => opt.MapFrom(src => src.InvalidarSesionesActivas))
            .ForMember(dest => dest.FechaEfectivacambios, opt => opt.MapFrom(src => src.FechaEfectivacambios))
            .ForMember(dest => dest.PermisosEspecificos, opt => opt.Ignore())
            .ForMember(dest => dest.SupervisorId, opt => opt.Ignore())
            .ForMember(dest => dest.Departamento, opt => opt.Ignore())
            .ForMember(dest => dest.Posicion, opt => opt.Ignore())
            .ForMember(dest => dest.FechaIngreso, opt => opt.Ignore())
            .ForMember(dest => dest.SalarioBase, opt => opt.Ignore())
            .ForMember(dest => dest.HorarioTrabajo, opt => opt.Ignore())
            .ForMember(dest => dest.ConfiguracionNotificaciones, opt => opt.Ignore())
            .ForMember(dest => dest.Preferencias, opt => opt.MapFrom(src => new Dictionary<string, object>()));

        // 🔄 Entidad → DTO Principal (Response)
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => src.NombreCompleto))
            .ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.NombreUsuario))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
            .ForMember(dest => dest.TipoUsuario, opt => opt.MapFrom(src => src.TipoUsuario))
            .ForMember(dest => dest.Rol, opt => opt.MapFrom(src => src.Rol))
            .ForMember(dest => dest.NivelAcceso, opt => opt.MapFrom(src => src.NivelAcceso))
            .ForMember(dest => dest.Permisos, opt => opt.MapFrom(src => src.Permisos))
            .ForMember(dest => dest.SupervisorId, opt => opt.MapFrom(src => src.SupervisorId))
            .ForMember(dest => dest.Departamento, opt => opt.MapFrom(src => src.Departamento))
            .ForMember(dest => dest.Posicion, opt => opt.MapFrom(src => src.Posicion))
            .ForMember(dest => dest.Identificacion, opt => opt.MapFrom(src => src.Identificacion))
            .ForMember(dest => dest.UltimoAcceso, opt => opt.MapFrom(src => src.UltimoAcceso))
            .ForMember(dest => dest.MotivoBloqueo, opt => opt.MapFrom(src => src.MotivoBloqueo))
            .ForMember(dest => dest.EsAdministrador, opt => opt.MapFrom(src => src.EsAdministrador))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.Roles.Select(r => r.ToString())))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.FechaModificacion, opt => opt.Ignore()) // No existe en la entidad Usuario
            .ForMember(dest => dest.CreadoPor, opt => opt.Ignore()) // No existe en la entidad Usuario
            .ForMember(dest => dest.ModificadoPor, opt => opt.Ignore()) // No existe en la entidad Usuario
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.Estado == EstadoUsuario.Activo));
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
        // Mapeo de entidad Receta a DTO
        CreateMap<RestaurantePro.Domain.Core.Productos.Entities.Receta, RestaurantePro.Application.Core.Recetas.DTOs.RecetaDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ProductoId, opt => opt.MapFrom(src => src.ProductoId))
            .ForMember(dest => dest.Preparacion, opt => opt.MapFrom(src => src.Preparacion))
            .ForMember(dest => dest.TiempoPreparacionMinutos, opt => opt.MapFrom(src => src.TiempoPreparacionMinutos))
            .ForMember(dest => dest.Ingredientes, opt => opt.MapFrom(src => src.Ingredientes))
            .ForMember(dest => dest.EstaActiva, opt => opt.MapFrom(src => !src.RecetaEliminada))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.FechaModificacion, opt => opt.MapFrom(src => src.FechaActualizacion))
            .ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : string.Empty))
            .ForMember(dest => dest.CostoTotal, opt => opt.Ignore());

        // Mapeo de ingrediente de receta
        CreateMap<RestaurantePro.Domain.Core.Productos.ValueObjects.IngredienteReceta, RestaurantePro.Application.Core.Recetas.DTOs.IngredienteRecetaDto>()
            .ForMember(dest => dest.IngredienteId, opt => opt.MapFrom(src => src.IngredienteId))
            .ForMember(dest => dest.Nombre, opt => opt.MapFrom(src => src.Nombre))
            .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.Cantidad))
            .ForMember(dest => dest.UnidadMedida, opt => opt.MapFrom(src => src.UnidadMedida.ToString()))
            .ForMember(dest => dest.EsOpcional, opt => opt.MapFrom(src => src.EsOpcional))
            .ForMember(dest => dest.CostoUnitario, opt => opt.Ignore())
            .ForMember(dest => dest.CostoTotal, opt => opt.Ignore());
    }
} 