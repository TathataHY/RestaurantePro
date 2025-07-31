using AutoMapper;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Entities;

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
        ConfigurarMapeosMesa();
        ConfigurarMapeosPreparacion();
        ConfigurarMapeosReservacion();
    }

    /// <summary>
    /// Configura los mapeos para la entidad Comanda
    /// </summary>
    private void ConfigurarMapeosComanda()
    {
        // Comanda → ComandaDto (mapeo completo con cálculos)
        CreateMap<Comanda, ComandaDto>()
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
            .ForMember(dest => dest.EstadoTexto, opt => opt.MapFrom(src => MapearEstadoTexto(src.Estado)))
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Total != null ? src.Total.Subtotal : 0))
            .ForMember(dest => dest.Impuestos, opt => opt.MapFrom(src => src.Total != null ? src.Total.Impuestos : 0))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total != null ? src.Total.Total : 0))
            .ForMember(dest => dest.CantidadItems, opt => opt.MapFrom(src => src.Items.Count))
            // ✅ ACTIVADOS: Propiedades que SÍ existen en ComandaDto
            .ForMember(dest => dest.FechaApertura, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.FechaCierre, opt => opt.MapFrom(src => src.FechaActualizacion))
            .ForMember(dest => dest.Descuentos, opt => opt.MapFrom(src => src.Total != null ? src.Total.Descuento ?? 0 : 0))
            .ForMember(dest => dest.UsuarioId, opt => opt.MapFrom(src => src.MeseroId))
            .ForMember(dest => dest.MesaId, opt => opt.MapFrom(src => src.MesaId))
            .ForMember(dest => dest.ClienteId, opt => opt.MapFrom(src => src.ClienteId))
            .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones))
            // TODO: Reactivar cuando existan estas propiedades en ComandaDto
            //.ForMember(dest => dest.TieneDescuentoFidelizacion, opt => opt.MapFrom(src => src.TieneDescuentoFidelizacion()))
            //.ForMember(dest => dest.PuedeModificar, opt => opt.MapFrom(src => PuedeModificarComanda(src.Estado)))
            //.ForMember(dest => dest.PuedeCancelar, opt => opt.MapFrom(src => PuedeCancelarComanda(src.Estado)))
            //.ForMember(dest => dest.TiempoTranscurrido, opt => opt.MapFrom(src => DateTime.Now - src.FechaCreacion))
            //.ForMember(dest => dest.TiempoEstimadoPreparacion, opt => opt.MapFrom(src => CalcularTiempoEstimado(src.Items)))
            // Propiedades de BaseDto
            .ForMember(dest => dest.Activo, opt => opt.MapFrom(src => src.Estado != EstadoComanda.Cancelada))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.FechaModificacion, opt => opt.MapFrom(src => src.FechaActualizacion))
            // Campos que requieren datos adicionales (se pueden completar en el handler)
            .ForMember(dest => dest.NumeroMesa, opt => opt.Ignore())
            .ForMember(dest => dest.NombreUsuario, opt => opt.Ignore()) // Se completa en el handler
            .ForMember(dest => dest.NombreCliente, opt => opt.Ignore()); // Se completa en el handler
            // TODO: Reactivar cuando existan propiedades Usuario en Domain
            //.ForMember(dest => dest.NombreMesero, opt => opt.Ignore())
            //.ForMember(dest => dest.NombreCliente, opt => opt.Ignore())
            //.ForMember(dest => dest.NombreUsuario, opt => opt.MapFrom(src => src.Usuario != null ? src.Usuario.Nombre : string.Empty));

        // Comanda → ComandaSummaryDto (mapeo resumido para listas)
        CreateMap<Comanda, ComandaSummaryDto>()
            .ForMember(dest => dest.NumeroComanda, opt => opt.MapFrom(src => $"C-{src.Id.ToString().Substring(0, 8)}"))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
            // TODO: Reactivar cuando existan estas propiedades en ComandaSummaryDto
            //.ForMember(dest => dest.EstadoDisplay, opt => opt.MapFrom(src => MapearEstadoTexto(src.Estado)))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total != null ? src.Total.Total : 0));
            // TODO: Reactivar cuando existan estas propiedades en ComandaSummaryDto
            //.ForMember(dest => dest.CantidadItems, opt => opt.MapFrom(src => src.Items.Count))
            //.ForMember(dest => dest.TiempoTranscurrido, opt => opt.MapFrom(src => DateTime.Now - src.FechaCreacion))
            //.ForMember(dest => dest.TiempoTranscurridoTexto, opt => opt.MapFrom(src => FormatearTiempoTranscurrido(DateTime.Now - src.FechaCreacion)))
            //.ForMember(dest => dest.EstaAtrasada, opt => opt.MapFrom(src => EstaComandaAtrasada(src)))
            //.ForMember(dest => dest.Prioridad, opt => opt.MapFrom(src => CalcularPrioridad(src)))
            //.ForMember(dest => dest.TieneObservaciones, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.Observaciones)))
            //.ForMember(dest => dest.TieneDescuento, opt => opt.MapFrom(src => src.TieneDescuentoFidelizacion()))
            //.ForMember(dest => dest.ColorEstado, opt => opt.MapFrom(src => MapearColorEstado(src.Estado)))
            // Campos que requieren datos adicionales
            //.ForMember(dest => dest.NumeroMesa, opt => opt.Ignore())
            //.ForMember(dest => dest.NombreMesero, opt => opt.Ignore())
            //.ForMember(dest => dest.NombreCliente, opt => opt.Ignore());

        // ComandaCreateDto → CrearComandaCommand (DTO de entrada a comando)
        CreateMap<ComandaCreateDto, CrearComandaCommand>()
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.ProductosIniciales));
    }

    /// <summary>
    /// Configura los mapeos para ItemComanda
    /// </summary>
    private void ConfigurarMapeosItemComanda()
    {
        // ItemComanda → ItemComandaDto
        CreateMap<ItemComanda, ItemComandaDto>()
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
            .ForMember(dest => dest.EstadoTexto, opt => opt.MapFrom(src => MapearEstadoItemTexto(src.Estado)))
            .ForMember(dest => dest.TienePersonalizaciones, opt => opt.MapFrom(src => src.Personalizaciones.Any()))
            .ForMember(dest => dest.PrecioPersonalizaciones, opt => opt.MapFrom(src => src.Personalizaciones.Sum(p => p.PrecioAdicional)))
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Subtotal + src.Personalizaciones.Sum(p => p.PrecioAdicional)))
            .ForMember(dest => dest.Personalizaciones, opt => opt.MapFrom(src => src.Personalizaciones));
            // TODO: Reactivar cuando existan propiedades Producto en Domain
            //.ForMember(dest => dest.NombreProducto, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Nombre : string.Empty))
            //.ForMember(dest => dest.DescripcionProducto, opt => opt.MapFrom(src => src.Producto != null ? src.Producto.Descripcion : null));

        // PersonalizacionItem (Domain) → PersonalizacionDto (Application)
        // Usando namespace completo para evitar ambigüedad
        CreateMap<RestaurantePro.Domain.Operaciones.Comandas.ValueObjects.PersonalizacionItem, PersonalizacionDto>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Los Value Objects no tienen ID, se genera en el DTO
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.Accion.ToString()))
            .ForMember(dest => dest.IngredienteId, opt => opt.MapFrom(src => src.IngredienteId))
            .ForMember(dest => dest.NombreIngrediente, opt => opt.MapFrom(src => src.NombreIngrediente))
            .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.Cantidad))
            .ForMember(dest => dest.PrecioAdicional, opt => opt.MapFrom(src => src.PrecioAdicional))
            .ForMember(dest => dest.Detalles, opt => opt.MapFrom(src => src.ObtenerDescripcion()));

        // TODO: Reactivar cuando existan DTOs
        // ItemComandaCreateDto → AgregarItemComandaCommand (DTO de entrada a comando)
        //CreateMap<ItemComandaCreateDto, AgregarItemComandaCommand>();
    }

    /// <summary>
    /// Configura los mapeos para Mesa
    /// </summary>
    private void ConfigurarMapeosMesa()
    {
        // Mesa → MesaDto
        CreateMap<Mesa, MesaDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
            .ForMember(dest => dest.Zona, opt => opt.MapFrom(src => src.Ubicacion))
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => "Estándar")) // Valor por defecto
            .ForMember(dest => dest.UltimaActualizacion, opt => opt.MapFrom(src => src.FechaActualizacion))
            .ForMember(dest => dest.Numero, opt => opt.MapFrom(src => src.Numero.ToString()));

        // Mesa → MesaDisponibleDto
        CreateMap<Mesa, MesaDisponibleDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.MesaId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Numero, opt => opt.MapFrom(src => src.Numero.ToString()))
            .ForMember(dest => dest.Capacidad, opt => opt.MapFrom(src => src.Capacidad))
            .ForMember(dest => dest.Ubicacion, opt => opt.MapFrom(src => src.Ubicacion))
            .ForMember(dest => dest.Zona, opt => opt.MapFrom(src => src.Ubicacion))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado.ToString()))
            .ForMember(dest => dest.Disponible, opt => opt.MapFrom(src => src.Estado == EstadoMesa.Disponible))
            .ForMember(dest => dest.EsCombinable, opt => opt.MapFrom(src => false)) // Valor por defecto
            .ForMember(dest => dest.Caracteristicas, opt => opt.MapFrom(src => new List<string>())) // Valor por defecto
            .ForMember(dest => dest.PrecioBase, opt => opt.MapFrom(src => 0)) // Valor por defecto
            .ForMember(dest => dest.EsVIP, opt => opt.MapFrom(src => false)) // Valor por defecto
            .ForMember(dest => dest.TieneVentana, opt => opt.MapFrom(src => false)) // Valor por defecto
            .ForMember(dest => dest.ProximaDisponibilidad, opt => opt.Ignore()); // Se asigna en el handler
    }

    /// <summary>
    /// Configura los mapeos para Preparacion
    /// </summary>
    private void ConfigurarMapeosPreparacion()
    {
        // PreparacionDiaria → PreparacionDto
        CreateMap<RestaurantePro.Domain.Operaciones.Preparaciones.Entities.PreparacionDiaria, RestaurantePro.Application.Operaciones.Preparaciones.DTOs.PreparacionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ProductoId, opt => opt.MapFrom(src => src.ProductoId))
            .ForMember(dest => dest.ChefId, opt => opt.MapFrom(src => src.ChefId))
            .ForMember(dest => dest.Cantidad, opt => opt.MapFrom(src => src.CantidadPreparada))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaPreparacion))
            .ForMember(dest => dest.FechaVencimiento, opt => opt.MapFrom(src => src.FechaVencimiento))
            .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones))
            .ForMember(dest => dest.TiempoEstimado, opt => opt.Ignore())
            .ForMember(dest => dest.TiempoReal, opt => opt.Ignore())
            .ForMember(dest => dest.NombreProducto, opt => opt.Ignore())
            .ForMember(dest => dest.NombreChef, opt => opt.Ignore())
            .ForMember(dest => dest.NumeroComanda, opt => opt.Ignore());

        // PreparacionDiaria → PreparacionDiariaDto
        CreateMap<RestaurantePro.Domain.Operaciones.Preparaciones.Entities.PreparacionDiaria, RestaurantePro.Application.Operaciones.Preparaciones.DTOs.PreparacionDiariaDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ProductoId, opt => opt.MapFrom(src => src.ProductoId))
            .ForMember(dest => dest.ChefId, opt => opt.MapFrom(src => src.ChefId))
            .ForMember(dest => dest.CantidadPreparada, opt => opt.MapFrom(src => src.CantidadPreparada))
            .ForMember(dest => dest.CantidadDisponible, opt => opt.MapFrom(src => src.CantidadDisponible))
            .ForMember(dest => dest.FechaVencimiento, opt => opt.MapFrom(src => src.FechaVencimiento))
            .ForMember(dest => dest.Observaciones, opt => opt.MapFrom(src => src.Observaciones))
            .ForMember(dest => dest.FechaPreparacion, opt => opt.MapFrom(src => src.FechaPreparacion))
            .ForMember(dest => dest.Estado, opt => opt.MapFrom(src => src.Estado))
            .ForMember(dest => dest.NombreProducto, opt => opt.Ignore()) // Se asigna en el handler
            .ForMember(dest => dest.NombreChef, opt => opt.Ignore()); // Se asigna en el handler
    }

    /// <summary>
    /// Configura los mapeos para Reservacion
    /// </summary>
    private void ConfigurarMapeosReservacion()
    {
        // Cliente → ClienteReservacionDto
        CreateMap<RestaurantePro.Domain.Comercial.Clientes.Entities.Cliente, ClienteReservacionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.NombreCompleto, opt => opt.MapFrom(src => src.Nombre.NombreCompleto))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono.Value))
            .ForMember(dest => dest.NivelFidelizacion, opt => opt.MapFrom(src => src.Segmento.ToString()))
            .ForMember(dest => dest.ReservacionesPrevias, opt => opt.MapFrom(src => src.CantidadVisitas))
            .ForMember(dest => dest.Preferencias, opt => opt.MapFrom(src => new List<string>())); // Valor por defecto

        // Reservacion → ReservacionDto
        CreateMap<Reservacion, ReservacionDto>()
            .ForMember(dest => dest.NombreCliente, opt => opt.MapFrom(src => 
                src.Cliente != null ? src.Cliente.Nombre.NombreCompleto : string.Empty))
            .ForMember(dest => dest.FechaHoraReservacion, opt => opt.MapFrom(src => src.Fecha.Add(src.Hora)))
            .ForMember(dest => dest.NumeroPersonas, opt => opt.MapFrom(src => src.CantidadPersonas))
            .ForMember(dest => dest.SolicitudesEspeciales, opt => opt.MapFrom(src => src.Observaciones))
            .ForMember(dest => dest.MesaId, opt => opt.MapFrom(src => src.MesaId))
            .ForMember(dest => dest.CodigoReservacion, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.FechaCreacion, opt => opt.MapFrom(src => src.FechaCreacion))
            .ForMember(dest => dest.DuracionEstimada, opt => opt.MapFrom(src => (int?)src.DuracionEstimada.TotalMinutes))
            .ForMember(dest => dest.EstaConfirmada, opt => opt.MapFrom(src => src.Estado == EstadoReservacion.Confirmada))
            .ForMember(dest => dest.PuntosFidelizacionAcumulados, opt => opt.MapFrom(src => 0)) // Valor por defecto
            .ForMember(dest => dest.Canal, opt => opt.MapFrom(src => "Web")) // Valor por defecto
            .ForMember(dest => dest.Prioridad, opt => opt.MapFrom(src => "Normal")) // Valor por defecto
            .ForMember(dest => dest.AceptaListaEspera, opt => opt.MapFrom(src => true)) // Valor por defecto
            .ForMember(dest => dest.RestriccionesAlimentarias, opt => opt.MapFrom(src => new List<string>())) // Valor por defecto
            .ForMember(dest => dest.ServiciosAdicionales, opt => opt.MapFrom(src => new List<ServicioAdicionalDto>())) // Valor por defecto
            .ForMember(dest => dest.HistorialEstados, opt => opt.MapFrom(src => new List<HistorialEstadoDto>())) // Valor por defecto
            .ForMember(dest => dest.Recordatorios, opt => opt.MapFrom(src => new List<RecordatorioDto>())); // Valor por defecto

        // ReservacionCreateDto → CrearReservacionCommand (DTO de entrada a comando)
        CreateMap<ReservacionCreateDto, CrearReservacionCommand>();

        // Mesa → MesaReservacionDto
        CreateMap<RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities.Mesa, MesaReservacionDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Numero, opt => opt.MapFrom(src => src.Numero.ToString()))
            .ForMember(dest => dest.Capacidad, opt => opt.MapFrom(src => src.Capacidad))
            .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Ubicacion))
            .ForMember(dest => dest.Tipo, opt => opt.MapFrom(src => src.GetType().Name)) // Ajustar si hay propiedad específica
            .ForMember(dest => dest.Caracteristicas, opt => opt.MapFrom(src => new List<string>())) // Valor por defecto
            .ForMember(dest => dest.Ubicacion, opt => opt.MapFrom(src => src.Ubicacion));
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

    /// <summary>
    /// Mapea el estado del item a texto amigable
    /// </summary>
    private static string MapearEstadoItemTexto(EstadoItemComanda estado) => estado switch
    {
        EstadoItemComanda.Pendiente => "Pendiente",
        EstadoItemComanda.EnPreparacion => "EnPreparacion",
        EstadoItemComanda.Listo => "Listo",
        EstadoItemComanda.Entregado => "Entregado",
        EstadoItemComanda.Cancelado => "Cancelado",
        _ => estado.ToString()
    };
} 