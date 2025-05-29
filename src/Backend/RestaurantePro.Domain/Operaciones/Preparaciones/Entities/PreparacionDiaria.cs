namespace RestaurantePro.Domain.Operaciones.Preparaciones.Entities;

/// <summary>
/// Representa una preparación diaria de un producto específico
/// </summary>
public class PreparacionDiaria : EntityBase
{
    /// <summary>
    /// ID del producto que se preparó
    /// </summary>
    public Guid ProductoId { get; private set; }
    
    /// <summary>
    /// Cantidad total que se preparó inicialmente
    /// </summary>
    public int CantidadPreparada { get; private set; }
    
    /// <summary>
    /// Cantidad disponible actualmente (se reduce al ser consumida)
    /// </summary>
    public int CantidadDisponible { get; private set; }
    
    /// <summary>
    /// Fecha y hora de preparación
    /// </summary>
    public DateTime FechaPreparacion { get; private set; }
    
    /// <summary>
    /// Fecha y hora de vencimiento (opcional, depende del producto)
    /// </summary>
    public DateTime? FechaVencimiento { get; private set; }
    
    /// <summary>
    /// Estado actual de la preparación
    /// </summary>
    public EstadoPreparacion Estado { get; private set; }
    
    /// <summary>
    /// ID del chef que realizó la preparación
    /// </summary>
    public Guid ChefId { get; private set; }
    
    /// <summary>
    /// Observaciones adicionales sobre la preparación
    /// </summary>
    public string? Observaciones { get; private set; }

    // Constructor privado para EF Core
    private PreparacionDiaria() { }

    /// <summary>
    /// Crea una nueva preparación diaria
    /// </summary>
    public static PreparacionDiaria Crear(
        Guid productoId,
        int cantidadPreparada,
        Guid chefId,
        DateTime? fechaVencimiento = null,
        string? observaciones = null)
    {
        if (productoId == Guid.Empty)
            throw new ArgumentException("El ID del producto no puede estar vacío", nameof(productoId));
        
        if (cantidadPreparada <= 0)
            throw new ArgumentException("La cantidad preparada debe ser mayor que cero", nameof(cantidadPreparada));
        
        if (chefId == Guid.Empty)
            throw new ArgumentException("El ID del chef no puede estar vacío", nameof(chefId));

        if (fechaVencimiento.HasValue && fechaVencimiento.Value <= DateTime.Now)
            throw new ArgumentException("La fecha de vencimiento debe ser futura", nameof(fechaVencimiento));

        var preparacion = new PreparacionDiaria
        {
            Id = Guid.NewGuid(),
            ProductoId = productoId,
            CantidadPreparada = cantidadPreparada,
            CantidadDisponible = cantidadPreparada,
            FechaPreparacion = DateTime.Now,
            FechaVencimiento = fechaVencimiento,
            Estado = EstadoPreparacion.Preparando,
            ChefId = chefId,
            Observaciones = observaciones?.Trim()
        };

        // Agregar evento de dominio
        preparacion.AddDomainEvent(new PreparacionCreada(
            preparacion.Id,
            preparacion.ProductoId,
            preparacion.CantidadPreparada,
            preparacion.ChefId));

        return preparacion;
    }

    /// <summary>
    /// Marca la preparación como disponible para consumo
    /// </summary>
    public Result MarcarComoDisponible()
    {
        if (Estado != EstadoPreparacion.Preparando)
            return Result.Failure("Solo se puede marcar como disponible una preparación en estado 'Preparando'");

        Estado = EstadoPreparacion.Disponible;

        AddDomainEvent(new PreparacionDisponible(Id, ProductoId));

        return Result.Success();
    }

    /// <summary>
    /// Consume una cantidad específica de la preparación
    /// </summary>
    public Result ConsumirCantidad(int cantidad)
    {
        if (cantidad <= 0)
            return Result.Failure("La cantidad a consumir debe ser mayor que cero");

        if (Estado == EstadoPreparacion.Vencida)
            return Result.Failure("No se puede consumir una preparación vencida");

        if (Estado == EstadoPreparacion.Agotada)
            return Result.Failure("No se puede consumir una preparación agotada");

        if (cantidad > CantidadDisponible)
            return Result.Failure($"No hay suficiente cantidad disponible. Disponible: {CantidadDisponible}, Solicitada: {cantidad}");

        CantidadDisponible -= cantidad;

        // Si se agotó, cambiar estado
        if (CantidadDisponible == 0)
        {
            Estado = EstadoPreparacion.Agotada;
            AddDomainEvent(new PreparacionAgotada(Id, ProductoId));
        }
        else
        {
            AddDomainEvent(new PreparacionConsumida(Id, ProductoId, cantidad, CantidadDisponible));
        }

        return Result.Success();
    }

    /// <summary>
    /// Agrega cantidad adicional a la preparación (preparación adicional)
    /// </summary>
    public Result AgregarCantidad(int cantidadAdicional)
    {
        if (cantidadAdicional <= 0)
            return Result.Failure("La cantidad adicional debe ser mayor que cero");

        if (Estado == EstadoPreparacion.Vencida)
            return Result.Failure("No se puede agregar cantidad a una preparación vencida");

        var cantidadAnterior = CantidadDisponible;
        CantidadPreparada += cantidadAdicional;
        CantidadDisponible += cantidadAdicional;

        // Si estaba agotada, cambiar a disponible
        if (Estado == EstadoPreparacion.Agotada)
        {
            Estado = EstadoPreparacion.Disponible;
        }

        AddDomainEvent(new CantidadAgregadaAPreparacion(Id, ProductoId, cantidadAdicional, cantidadAnterior, CantidadDisponible));

        return Result.Success();
    }

    /// <summary>
    /// Marca la preparación como vencida
    /// </summary>
    public Result MarcarComoVencida()
    {
        if (Estado == EstadoPreparacion.Vencida)
            return Result.Failure("La preparación ya está marcada como vencida");

        var cantidadDesperdiciada = CantidadDisponible;
        Estado = EstadoPreparacion.Vencida;
        CantidadDisponible = 0;

        AddDomainEvent(new PreparacionVencida(Id, ProductoId, cantidadDesperdiciada));

        return Result.Success();
    }

    /// <summary>
    /// Marca la preparación como por vencer (alerta)
    /// </summary>
    public Result MarcarComoPorVencer()
    {
        if (Estado != EstadoPreparacion.Disponible)
            return Result.Failure("Solo se puede marcar como 'por vencer' una preparación disponible");

        Estado = EstadoPreparacion.PorVencer;

        AddDomainEvent(new PreparacionPorVencer(Id, ProductoId, FechaVencimiento));

        return Result.Success();
    }

    /// <summary>
    /// Verifica si hay cantidad disponible suficiente
    /// </summary>
    public bool EstaDisponible(int cantidadRequerida)
    {
        return Estado == EstadoPreparacion.Disponible && 
               CantidadDisponible >= cantidadRequerida;
    }

    /// <summary>
    /// Verifica si la preparación ha vencido
    /// </summary>
    public bool HaVencido()
    {
        return Estado == EstadoPreparacion.Vencida ||
               (FechaVencimiento.HasValue && DateTime.Now > FechaVencimiento.Value);
    }

    /// <summary>
    /// Verifica si está por vencer en las próximas horas
    /// </summary>
    public bool EstaPorVencer(int horasAnticipacion = 2)
    {
        if (!FechaVencimiento.HasValue) return false;
        
        return DateTime.Now.AddHours(horasAnticipacion) >= FechaVencimiento.Value &&
               Estado == EstadoPreparacion.Disponible;
    }

    /// <summary>
    /// Actualiza observaciones
    /// </summary>
    public void ActualizarObservaciones(string? observaciones)
    {
        Observaciones = observaciones?.Trim();
    }
}