namespace RestaurantePro.Mobile.Core.Models.DTOs;

/// <summary>
/// DTO para información completa de una comanda
/// </summary>
public class ComandaDto
{
    /// <summary>
    /// ID único de la comanda
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Número de la comanda
    /// </summary>
    public string Numero { get; set; } = string.Empty;

    /// <summary>
    /// Número de comanda enviado por backend (alternativo)
    /// </summary>
    public string NumeroComanda { get; set; } = string.Empty;

    /// <summary>
    /// ID de la mesa asociada
    /// </summary>
    public Guid MesaId { get; set; }

    /// <summary>
    /// Número de la mesa (string usado en mobile). Si no viene, usar <see cref="NumeroMesa"/>.
    /// </summary>
    public string MesaNumero { get; set; } = string.Empty;

    /// <summary>
    /// Número de la mesa como entero (propiedad que viene del backend)
    /// </summary>
    public int NumeroMesa { get; set; }

    /// <summary>
    /// Número de mesa para mostrar (fallback a NumeroMesa si MesaNumero está vacío)
    /// </summary>
    public string MesaNumeroDisplay => !string.IsNullOrWhiteSpace(MesaNumero)
        ? MesaNumero
        : (NumeroMesa > 0 ? NumeroMesa.ToString() : string.Empty);

    /// <summary>
    /// Número de comanda para mostrar con fallback seguro
    /// </summary>
    public string NumeroDisplay => !string.IsNullOrWhiteSpace(Numero)
        ? Numero
        : (!string.IsNullOrWhiteSpace(NumeroComanda) ? NumeroComanda : Id.ToString().Substring(0, 8).ToUpper());

    /// <summary>
    /// Estado actual de la comanda (texto si está disponible)
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Estado en texto amigable (propiedad enviada por backend)
    /// </summary>
    public string EstadoTexto { get; set; } = string.Empty;

    /// <summary>
    /// ID del cliente (si está disponible)
    /// </summary>
    public Guid? ClienteId { get; set; }

    /// <summary>
    /// Nombre del cliente (mobile)
    /// </summary>
    public string ClienteNombre { get; set; } = string.Empty;

    /// <summary>
    /// Nombre del cliente (propiedad enviada por backend)
    /// </summary>
    public string? NombreCliente { get; set; }

    /// <summary>
    /// Fecha y hora de creación
    /// </summary>
    public DateTime FechaCreacion { get; set; }

    /// <summary>
    /// Fecha y hora de última actualización
    /// </summary>
    public DateTime UltimaActualizacion { get; set; }

    /// <summary>
    /// Observaciones de la comanda
    /// </summary>
    public string? Observaciones { get; set; }

    /// <summary>
    /// Productos incluidos en la comanda
    /// </summary>
    public List<ComandaProductoDto> Productos { get; set; } = new List<ComandaProductoDto>();
    
    /// <summary>
    /// Items de la comanda (propiedad que viene del backend)
    /// </summary>
    public List<ComandaProductoDto> Items { get; set; } = new List<ComandaProductoDto>();

    /// <summary>
    /// Total de la comanda
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Subtotal de la comanda
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// IVA de la comanda
    /// </summary>
    public decimal Iva { get; set; }

    /// <summary>
    /// Descuentos aplicados
    /// </summary>
    public decimal Descuentos { get; set; }

    /// <summary>
    /// Cantidad total de productos
    /// </summary>
    public int CantidadProductos => Productos.Sum(p => p.Cantidad);

    /// <summary>
    /// Indica si la comanda está activa
    /// </summary>
    private static string Normalize(string? value)
    {
        return new string((value ?? string.Empty).ToLowerInvariant().Where(char.IsLetter).ToArray());
    }

    public bool EstaActiva
    {
        get
        {
            var estado = Normalize(Estado);
            var estadoTexto = Normalize(EstadoTexto);
            return estado is "pendiente" or "enpreparacion" or "preparando" or "lista" or "enproceso" or "creada"
                || estadoTexto is "pendiente" or "enpreparacion" or "preparando" or "lista" or "enproceso" or "creada";
        }
    }

    /// <summary>
    /// Indica si la comanda puede ser editada
    /// </summary>
    public bool PuedeSerEditada
    {
        get
        {
            var estado = Normalize(Estado);
            return estado is "pendiente" or "creada";
        }
    }

    /// <summary>
    /// Color para mostrar en la UI según el estado
    /// </summary>
    public string ColorEstado
    {
        get
        {
            var estado = Normalize(Estado);
            if (string.IsNullOrWhiteSpace(estado)) estado = Normalize(EstadoTexto);
            return estado switch
            {
                "pendiente" => "#FFC107",
                "enpreparacion" => "#FF9800",
                "preparando" => "#FF9800",
                "lista" => "#4CAF50",
                "entregada" => "#2196F3",
                "cancelada" => "#F44336",
                "finalizada" => "#9E9E9E",
                "enproceso" => "#FF9800",
                _ => "#607D8B"
            };
        }
    }

    /// <summary>
    /// Descripción amigable del estado
    /// </summary>
    public string EstadoDescripcion
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(EstadoTexto)) return EstadoTexto;
            var estado = Normalize(Estado);
            return estado switch
            {
                "pendiente" => "Pendiente",
                "enpreparacion" => "En Preparación",
                "preparando" => "En Preparación",
                "lista" => "Lista",
                "entregada" => "Entregada",
                "cancelada" => "Cancelada",
                "finalizada" => "Finalizada",
                "enproceso" => "En Proceso",
                "creada" => "Creada",
                _ => string.IsNullOrWhiteSpace(Estado) ? "-" : Estado
            };
        }
    }

    /// <summary>
    /// Información de la mesa asociada
    /// </summary>
    public MesaDto? Mesa { get; set; }

    /// <summary>
    /// Resumen de productos para mostrar en la lista
    /// </summary>
    public string ProductosResumen
    {
        get
        {
            // Usar Items si está disponible, sino usar Productos
            var productosLista = Items.Any() ? Items : Productos;
            
            if (!productosLista.Any()) return "Sin productos";
            
            var productos = productosLista.Take(3).Select(p => $"{p.Cantidad}x {p.Nombre}");
            var resumen = string.Join(", ", productos);
            
            if (productosLista.Count > 3)
                resumen += $" y {productosLista.Count - 3} más";
                
            return resumen;
        }
    }

    /// <summary>
    /// Tiempo transcurrido desde la creación
    /// </summary>
    public string TiempoTranscurrido
    {
        get
        {
            var timeSpan = DateTime.Now - FechaCreacion;
            
            if (timeSpan.TotalMinutes < 1)
                return "un momento";
            else if (timeSpan.TotalMinutes < 60)
                return $"{timeSpan.Minutes} min";
            else if (timeSpan.TotalHours < 24)
                return $"{timeSpan.Hours}h {timeSpan.Minutes}m";
            else
                return $"{timeSpan.Days} días";
        }
    }

    /// <summary>
    /// Indica si la comanda puede ser tomada por cocina
    /// </summary>
    public bool PuedeTomar => Estado.ToLowerInvariant() == "creada";

    /// <summary>
    /// Indica si la comanda puede ser marcada como lista
    /// </summary>
    public bool PuedeMarcarLista => Estado.ToLowerInvariant() == "enproceso";

    /// <summary>
    /// Indica si la comanda puede ser entregada al cliente
    /// </summary>
    public bool PuedeEntregar => Estado.ToLowerInvariant() == "lista";

    /// <summary>
    /// Indica si la comanda puede ser cobrada
    /// </summary>
    public bool PuedeCobrar => Estado.ToLowerInvariant() == "entregada";

    /// <summary>
    /// Indica si la comanda tiene observaciones
    /// </summary>
    public bool TieneObservaciones => !string.IsNullOrWhiteSpace(Observaciones);
} 