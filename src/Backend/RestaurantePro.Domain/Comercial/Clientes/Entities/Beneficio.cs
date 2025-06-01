namespace RestaurantePro.Domain.Comercial.Clientes.Entities;

/// <summary>
/// Entidad para representar un beneficio de fidelización
/// </summary>
public class Beneficio : EntityBase
{
    /// <summary>
    /// Nombre del beneficio
    /// </summary>
    public string Nombre { get; private set; } = string.Empty;

    /// <summary>
    /// Descripción del beneficio
    /// </summary>
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>
    /// Tipo de beneficio
    /// </summary>
    public string Tipo { get; private set; } = string.Empty;

    /// <summary>
    /// Valor del beneficio (descuento, puntos, etc.)
    /// </summary>
    public decimal Valor { get; private set; }

    /// <summary>
    /// Tipo de tarjeta aplicable
    /// </summary>
    public string TipoTarjetaAplicable { get; private set; } = string.Empty;

    /// <summary>
    /// Condiciones para aplicar el beneficio
    /// </summary>
    public string? Condiciones { get; private set; }

    /// <summary>
    /// Fecha de inicio de vigencia
    /// </summary>
    public DateTime FechaInicio { get; private set; }

    /// <summary>
    /// Fecha de fin de vigencia
    /// </summary>
    public DateTime? FechaFin { get; private set; }

    /// <summary>
    /// Indica si el beneficio está activo
    /// </summary>
    public bool Activo { get; private set; } = true;

    /// <summary>
    /// Metadatos adicionales en formato JSON
    /// </summary>
    public string? Metadatos { get; private set; }

    // Constructor privado para EF Core
    private Beneficio() { }

    /// <summary>
    /// Constructor para crear un nuevo beneficio
    /// </summary>
    public Beneficio(
        string nombre,
        string descripcion,
        string tipo,
        decimal valor,
        string tipoTarjetaAplicable,
        DateTime fechaInicio,
        DateTime? fechaFin = null,
        string? condiciones = null,
        string? metadatos = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del beneficio es requerido", nameof(nombre));
        
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException("La descripción del beneficio es requerida", nameof(descripcion));
            
        if (string.IsNullOrWhiteSpace(tipo))
            throw new ArgumentException("El tipo del beneficio es requerido", nameof(tipo));
            
        if (valor < 0)
            throw new ArgumentException("El valor del beneficio no puede ser negativo", nameof(valor));
            
        if (string.IsNullOrWhiteSpace(tipoTarjetaAplicable))
            throw new ArgumentException("El tipo de tarjeta aplicable es requerido", nameof(tipoTarjetaAplicable));
            
        if (fechaFin.HasValue && fechaFin <= fechaInicio)
            throw new ArgumentException("La fecha de fin debe ser posterior a la fecha de inicio", nameof(fechaFin));

        Nombre = nombre;
        Descripcion = descripcion;
        Tipo = tipo;
        Valor = valor;
        TipoTarjetaAplicable = tipoTarjetaAplicable;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Condiciones = condiciones;
        Metadatos = metadatos;
        Activo = true;
    }

    /// <summary>
    /// Actualiza los datos del beneficio
    /// </summary>
    public void Actualizar(
        string nombre,
        string descripcion,
        string tipo,
        decimal valor,
        string tipoTarjetaAplicable,
        DateTime fechaInicio,
        DateTime? fechaFin = null,
        string? condiciones = null,
        string? metadatos = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del beneficio es requerido", nameof(nombre));
        
        if (string.IsNullOrWhiteSpace(descripcion))
            throw new ArgumentException("La descripción del beneficio es requerida", nameof(descripcion));
            
        if (string.IsNullOrWhiteSpace(tipo))
            throw new ArgumentException("El tipo del beneficio es requerido", nameof(tipo));
            
        if (valor < 0)
            throw new ArgumentException("El valor del beneficio no puede ser negativo", nameof(valor));
            
        if (string.IsNullOrWhiteSpace(tipoTarjetaAplicable))
            throw new ArgumentException("El tipo de tarjeta aplicable es requerido", nameof(tipoTarjetaAplicable));
            
        if (fechaFin.HasValue && fechaFin <= fechaInicio)
            throw new ArgumentException("La fecha de fin debe ser posterior a la fecha de inicio", nameof(fechaFin));

        Nombre = nombre;
        Descripcion = descripcion;
        Tipo = tipo;
        Valor = valor;
        TipoTarjetaAplicable = tipoTarjetaAplicable;
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
        Condiciones = condiciones;
        Metadatos = metadatos;
    }

    /// <summary>
    /// Activa el beneficio
    /// </summary>
    public void Activar()
    {
        Activo = true;
    }

    /// <summary>
    /// Desactiva el beneficio
    /// </summary>
    public void Desactivar()
    {
        Activo = false;
    }

    /// <summary>
    /// Verifica si el beneficio está vigente en una fecha específica
    /// </summary>
    public bool EstaVigente(DateTime fecha)
    {
        if (!Activo) return false;
        
        if (fecha < FechaInicio) return false;
        
        if (FechaFin.HasValue && fecha > FechaFin.Value) return false;
        
        return true;
    }

    /// <summary>
    /// Verifica si el beneficio aplica para un tipo de tarjeta específico
    /// </summary>
    public bool AplicaPara(string tipoTarjeta)
    {
        return TipoTarjetaAplicable.Equals(tipoTarjeta, StringComparison.OrdinalIgnoreCase);
    }
} 