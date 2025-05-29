using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Application.Operaciones.Comandas.Commands.CrearComanda;

namespace RestaurantePro.Application.Config.Mappings;

/// <summary>
/// Perfil de AutoMapper para el contexto Operaciones
/// Configura los mapeos entre entidades de dominio y DTOs del contexto operativo
/// </summary>
public class OperacionesMappingProfile : Profile
{
    public OperacionesMappingProfile()
    {
        ConfigurarMapeosComanda();
        ConfigurarMapeosItemComanda();
        // TODO: Agregar otros mapeos cuando estén implementados
        // ConfigurarMapeosReservacion();
        // ConfigurarMapeosMesa();
    }

    /// <summary>
    /// Configura los mapeos para la entidad Comanda
    /// </summary>
    private void ConfigurarMapeosComanda()
    {
        // Comanda → ComandaDto (mapeo completo con cálculos)
        CreateMap<Comanda, ComandaDto>()
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
            .ForMember(dest => dest.EstadoTexto, opt => opt.MapFrom(src => MapearEstadoTexto(src.Estado)))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Total != null ? src.Total.Subtotal : 0))
            .ForMember(dest => dest.Impuestos, opt => opt.MapFrom(src => src.Total != null ? src.Total.Impuestos : 0))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total != null ? src.Total.Total : 0))
            .ForMember(dest => dest.CantidadItems, opt => opt.MapFrom(src => src.Items.Count))
            .ForMember(dest => dest.TieneDescuentoFidelizacion, opt => opt.MapFrom(src => src.TieneDescuentoFidelizacion()))
            .ForMember(dest => dest.PuedeModificar, opt => opt.MapFrom(src => PuedeModificarComanda(src.Estado)))
            .ForMember(dest => dest.PuedeCancelar, opt => opt.MapFrom(src => PuedeCancelarComanda(src.Estado)))
            .ForMember(dest => dest.TiempoTranscurrido, opt => opt.MapFrom(src => DateTime.Now - src.FechaCreacion))
            .ForMember(dest => dest.TiempoEstimadoPreparacion, opt => opt.MapFrom(src => CalcularTiempoEstimado(src.Items)))
            // Propiedades de BaseDto
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.Estado != EstadoComanda.Cancelada))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.FechaModificacion, opt => opt.MapFrom(src => src.FechaActualizacion))
            // Campos que requieren datos adicionales (se pueden completar en el handler)
            .ForMember(dest => dest.NumeroMesa, opt => opt.Ignore())
            .ForMember(dest => dest.NombreMesero, opt => opt.Ignore())
            .ForMember(dest => dest.NombreCliente, opt => opt.Ignore());

        // Comanda → ComandaSummaryDto (mapeo resumido para listas)
        CreateMap<Comanda, ComandaSummaryDto>()
            .ForMember(dest => dest.NumeroComanda, opt => opt.MapFrom(src => $"C-{src.Id.ToString().Substring(0, 8)}"))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
            .ForMember(dest => dest.EstadoDisplay, opt => opt.MapFrom(src => MapearEstadoTexto(src.Estado)))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total != null ? src.Total.Total : 0))
            .ForMember(dest => dest.CantidadItems, opt => opt.MapFrom(src => src.Items.Count))
            .ForMember(dest => dest.TiempoTranscurrido, opt => opt.MapFrom(src => DateTime.Now - src.FechaCreacion))
            .ForMember(dest => dest.TiempoTranscurridoTexto, opt => opt.MapFrom(src => FormatearTiempoTranscurrido(DateTime.Now - src.FechaCreacion)))
            .ForMember(dest => dest.EstaAtrasada, opt => opt.MapFrom(src => EstaComandaAtrasada(src)))
            .ForMember(dest => dest.Prioridad, opt => opt.MapFrom(src => CalcularPrioridad(src)))
            .ForMember(dest => dest.TieneObservaciones, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.Observaciones)))
            .ForMember(dest => dest.TieneDescuento, opt => opt.MapFrom(src => src.TieneDescuentoFidelizacion()))
            .ForMember(dest => dest.ColorEstado, opt => opt.MapFrom(src => MapearColorEstado(src.Estado)))
            // Campos que requieren datos adicionales
            .ForMember(dest => dest.NumeroMesa, opt => opt.Ignore())
            .ForMember(dest => dest.NombreMesero, opt => opt.Ignore())
            .ForMember(dest => dest.NombreCliente, opt => opt.Ignore());

        // ComandaCreateDto → CrearComandaCommand (DTO de entrada a comando)
        CreateMap<ComandaCreateDto, CrearComandaCommand>();
    }

    /// <summary>
    /// Configura los mapeos para ItemComanda
    /// </summary>
    private void ConfigurarMapeosItemComanda()
    {
        // ItemComanda → ItemComandaDto
        CreateMap<ItemComanda, ItemComandaDto>()
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
            .ForMember(dest => dest.TienePersonalizaciones, opt => opt.MapFrom(src => src.Personalizaciones.Any()))
            .ForMember(dest => dest.PrecioPersonalizaciones, opt => opt.MapFrom(src => src.Personalizaciones.Sum(p => p.PrecioAdicional)))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Subtotal + src.Personalizaciones.Sum(p => p.PrecioAdicional)))
            .ForMember(dest => dest.Personalizaciones, opt => opt.MapFrom(src => src.Personalizaciones))
            // Campos que requieren datos adicionales del catálogo
            .ForMember(dest => dest.NombreProducto, opt => opt.Ignore());

        // TODO: Mapear personalizaciones cuando estén disponibles en el dominio
        // CreateMap<PersonalizacionItem, PersonalizacionDto>()
        //     .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Tipo.ToString()));
    }

    /// <summary>
    /// Mapea el estado de la comanda a texto amigable
    /// </summary>
    private static string MapearEstadoTexto(EstadoComanda estado) => estado switch
    {
        EstadoComanda.Creada => "Nueva",
        EstadoComanda.EnProceso => "En Preparación",
        EstadoComanda.Lista => "Lista",
        EstadoComanda.Entregada => "Entregada",
        EstadoComanda.Finalizada => "Finalizada",
        EstadoComanda.Cancelada => "Cancelada",
        _ => estado.ToString()
    };

    /// <summary>
    /// Determina si una comanda puede ser modificada
    /// </summary>
    private static bool PuedeModificarComanda(EstadoComanda estado) => estado switch
    {
        EstadoComanda.Creada => true,
        EstadoComanda.EnProceso => true,
        _ => false
    };

    /// <summary>
    /// Determina si una comanda puede ser cancelada
    /// </summary>
    private static bool PuedeCancelarComanda(EstadoComanda estado) => estado switch
    {
        EstadoComanda.Creada => true,
        EstadoComanda.EnProceso => true,
        EstadoComanda.Lista => true,
        _ => false
    };

    /// <summary>
    /// Calcula el tiempo estimado de preparación basado en los items
    /// </summary>
    private static TimeSpan? CalcularTiempoEstimado(IReadOnlyCollection<ItemComanda> items)
    {
        if (!items.Any()) return null;

        // Estimación simple: 5 minutos base + 2 minutos por item + tiempo por personalizaciones
        var minutosBase = 5;
        var minutosPorItem = items.Count * 2;
        var minutosPersonalizaciones = items.Sum(item => item.Personalizaciones.Count * 1);

        return TimeSpan.FromMinutes(minutosBase + minutosPorItem + minutosPersonalizaciones);
    }

    /// <summary>
    /// Formatea el tiempo transcurrido en texto legible
    /// </summary>
    private static string FormatearTiempoTranscurrido(TimeSpan tiempo)
    {
        if (tiempo.TotalMinutes < 1)
            return "< 1 min";
        if (tiempo.TotalHours < 1)
            return $"{(int)tiempo.TotalMinutes} min";
        if (tiempo.TotalDays < 1)
            return $"{(int)tiempo.TotalHours}h {tiempo.Minutes}m";
        
        return $"{(int)tiempo.TotalDays}d {tiempo.Hours}h";
    }

    /// <summary>
    /// Determina si una comanda está atrasada
    /// </summary>
    private static bool EstaComandaAtrasada(Comanda comanda)
    {
        var tiempoTranscurrido = DateTime.Now - comanda.FechaCreacion;
        var tiempoEstimado = CalcularTiempoEstimado(comanda.Items);
        
        if (tiempoEstimado == null) return false;
        
        // Considerar atrasada si excede el tiempo estimado + 10 minutos de margen
        return tiempoTranscurrido > tiempoEstimado.Value.Add(TimeSpan.FromMinutes(10));
    }

    /// <summary>
    /// Calcula la prioridad de una comanda
    /// </summary>
    private static string CalcularPrioridad(Comanda comanda)
    {
        var tiempoTranscurrido = DateTime.Now - comanda.FechaCreacion;
        
        // Prioridad alta para comandas con más de 30 minutos
        if (tiempoTranscurrido.TotalMinutes > 30)
            return "Alta";
        
        // Prioridad media para comandas con más de 15 minutos
        if (tiempoTranscurrido.TotalMinutes > 15)
            return "Media";
        
        return "Normal";
    }

    /// <summary>
    /// Mapea el estado a color para UI
    /// </summary>
    private static string MapearColorEstado(EstadoComanda estado) => estado switch
    {
        EstadoComanda.Creada => "#3B82F6",      // Azul
        EstadoComanda.EnProceso => "#F59E0B",   // Amarillo
        EstadoComanda.Lista => "#10B981",       // Verde
        EstadoComanda.Entregada => "#6B7280",   // Gris
        EstadoComanda.Finalizada => "#6B7280",  // Gris
        EstadoComanda.Cancelada => "#EF4444",   // Rojo
        _ => "#6B7280"                          // Gris por defecto
    };
} 