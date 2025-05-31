namespace RestaurantePro.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando una entidad no es encontrada
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Nombre de la entidad que no fue encontrada
    /// </summary>
    public string? EntityName { get; }

    /// <summary>
    /// Clave/ID de la entidad que no fue encontrada
    /// </summary>
    public object? EntityKey { get; }

    /// <summary>
    /// Contexto adicional sobre la búsqueda
    /// </summary>
    public object? SearchContext { get; }

    public NotFoundException()
        : base("La entidad solicitada no fue encontrada.")
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NotFoundException(string name, object key)
        : base($"Entidad \"{name}\" ({key}) no fue encontrada.")
    {
        EntityName = name;
        EntityKey = key;
    }

    public NotFoundException(string name, object key, object? searchContext)
        : base($"Entidad \"{name}\" ({key}) no fue encontrada.")
    {
        EntityName = name;
        EntityKey = key;
        SearchContext = searchContext;
    }

    /// <summary>
    /// Crea una NotFoundException para una entidad por ID
    /// </summary>
    public static NotFoundException ForEntity<T>(object id) where T : class
    {
        var entityName = typeof(T).Name;
        return new NotFoundException(
            $"{entityName} con ID {id} no fue encontrado.",
            entityName,
            id);
    }

    /// <summary>
    /// Crea una NotFoundException para una entidad con criterios específicos
    /// </summary>
    public static NotFoundException ForEntityWithCriteria(string entityName, object criteria)
    {
        return new NotFoundException(
            $"{entityName} no encontrado con los criterios especificados.",
            entityName,
            null,
            criteria);
    }

    /// <summary>
    /// Crea una NotFoundException para un Cliente
    /// </summary>
    public static NotFoundException ForCliente(Guid clienteId)
    {
        return new NotFoundException(
            $"Cliente con ID {clienteId} no fue encontrado.",
            "Cliente",
            clienteId);
    }

    /// <summary>
    /// Crea una NotFoundException para un Producto
    /// </summary>
    public static NotFoundException ForProducto(Guid productoId)
    {
        return new NotFoundException(
            $"Producto con ID {productoId} no fue encontrado.",
            "Producto",
            productoId);
    }

    /// <summary>
    /// Crea una NotFoundException para una Comanda
    /// </summary>
    public static NotFoundException ForComanda(Guid comandaId)
    {
        return new NotFoundException(
            $"Comanda con ID {comandaId} no fue encontrada.",
            "Comanda",
            comandaId);
    }

    /// <summary>
    /// Crea una NotFoundException para una Mesa
    /// </summary>
    public static NotFoundException ForMesa(Guid mesaId)
    {
        return new NotFoundException(
            $"Mesa con ID {mesaId} no fue encontrada.",
            "Mesa",
            mesaId);
    }

    /// <summary>
    /// Crea una NotFoundException para una Mesa por número
    /// </summary>
    public static NotFoundException ForMesaPorNumero(int numeroMesa)
    {
        return new NotFoundException(
            $"Mesa número {numeroMesa} no fue encontrada.",
            "Mesa",
            numeroMesa,
            new { SearchBy = "Numero", Value = numeroMesa });
    }

    /// <summary>
    /// Crea una NotFoundException para una Reservación
    /// </summary>
    public static NotFoundException ForReservacion(Guid reservacionId)
    {
        return new NotFoundException(
            $"Reservación con ID {reservacionId} no fue encontrada.",
            "Reservacion",
            reservacionId);
    }

    /// <summary>
    /// Crea una NotFoundException para una Factura
    /// </summary>
    public static NotFoundException ForFactura(Guid facturaId)
    {
        return new NotFoundException(
            $"Factura con ID {facturaId} no fue encontrada.",
            "Factura",
            facturaId);
    }

    /// <summary>
    /// Crea una NotFoundException para un Ingrediente
    /// </summary>
    public static NotFoundException ForIngrediente(Guid ingredienteId)
    {
        return new NotFoundException(
            $"Ingrediente con ID {ingredienteId} no fue encontrado.",
            "Ingrediente",
            ingredienteId);
    }

    /// <summary>
    /// Crea una NotFoundException para un Proveedor
    /// </summary>
    public static NotFoundException ForProveedor(Guid proveedorId)
    {
        return new NotFoundException(
            $"Proveedor con ID {proveedorId} no fue encontrado.",
            "Proveedor",
            proveedorId);
    }

    /// <summary>
    /// Crea una NotFoundException para un Usuario
    /// </summary>
    public static NotFoundException ForUsuario(Guid usuarioId)
    {
        return new NotFoundException(
            $"Usuario con ID {usuarioId} no fue encontrado.",
            "Usuario",
            usuarioId);
    }

    /// <summary>
    /// Crea una NotFoundException para un Usuario por email
    /// </summary>
    public static NotFoundException ForUsuarioPorEmail(string email)
    {
        return new NotFoundException(
            $"Usuario con email {email} no fue encontrado.",
            "Usuario",
            email,
            new { SearchBy = "Email", Value = email });
    }
} 