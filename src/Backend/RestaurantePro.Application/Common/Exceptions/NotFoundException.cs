namespace RestaurantePro.Application.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando una entidad no se encuentra
/// Proporciona información detallada sobre la entidad faltante
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Tipo de entidad que no se encontró
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Identificador de la entidad que no se encontró
    /// </summary>
    public object EntityId { get; }

    /// <summary>
    /// Constructor principal
    /// </summary>
    /// <param name="entityType">Tipo de entidad</param>
    /// <param name="entityId">Identificador de la entidad</param>
    public NotFoundException(string entityType, object entityId)
        : base($"La entidad '{entityType}' con ID '{entityId}' no fue encontrada.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    /// <summary>
    /// Constructor con mensaje personalizado
    /// </summary>
    /// <param name="entityType">Tipo de entidad</param>
    /// <param name="entityId">Identificador de la entidad</param>
    /// <param name="message">Mensaje personalizado</param>
    public NotFoundException(string entityType, object entityId, string message)
        : base(message)
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    /// <summary>
    /// Constructor con excepción interna
    /// </summary>
    /// <param name="entityType">Tipo de entidad</param>
    /// <param name="entityId">Identificador de la entidad</param>
    /// <param name="message">Mensaje personalizado</param>
    /// <param name="innerException">Excepción interna</param>
    public NotFoundException(string entityType, object entityId, string message, Exception innerException)
        : base(message, innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    /// <summary>
    /// Constructor simple con solo mensaje
    /// </summary>
    /// <param name="message">Mensaje de error</param>
    public NotFoundException(string message)
        : base(message)
    {
        EntityType = "Unknown";
        EntityId = "Unknown";
    }

    /// <summary>
    /// Constructor de fábrica para casos comunes
    /// </summary>
    /// <typeparam name="T">Tipo de entidad</typeparam>
    /// <param name="entityId">Identificador de la entidad</param>
    /// <returns>Nueva instancia de NotFoundException</returns>
    public static NotFoundException For<T>(object entityId)
    {
        return new NotFoundException(typeof(T).Name, entityId);
    }

    /// <summary>
    /// Constructor de fábrica con mensaje personalizado
    /// </summary>
    /// <typeparam name="T">Tipo de entidad</typeparam>
    /// <param name="entityId">Identificador de la entidad</param>
    /// <param name="customMessage">Mensaje personalizado</param>
    /// <returns>Nueva instancia de NotFoundException</returns>
    public static NotFoundException For<T>(object entityId, string customMessage)
    {
        return new NotFoundException(typeof(T).Name, entityId, customMessage);
    }

    /// <summary>
    /// Crea una NotFoundException para una entidad por ID
    /// </summary>
    public static NotFoundException ForEntity<T>(object id) where T : class
    {
        var entityName = typeof(T).Name;
        return new NotFoundException(
            entityName,
            id,
            $"{entityName} con ID {id} no fue encontrado.");
    }

    /// <summary>
    /// Crea una NotFoundException para una entidad con criterios específicos
    /// </summary>
    public static NotFoundException ForEntityWithCriteria(string entityName, object criteria)
    {
        return new NotFoundException(
            entityName,
            criteria,
            $"{entityName} no encontrado con los criterios especificados.");
    }

    /// <summary>
    /// Crea una NotFoundException para un Cliente
    /// </summary>
    public static NotFoundException ForCliente(Guid clienteId)
    {
        return new NotFoundException(
            "Cliente",
            clienteId,
            $"Cliente con ID {clienteId} no fue encontrado.");
    }

    /// <summary>
    /// Crea una NotFoundException para un Producto
    /// </summary>
    public static NotFoundException ForProducto(Guid productoId)
    {
        return new NotFoundException(
            "Producto",
            productoId,
            $"Producto con ID {productoId} no fue encontrado.");
    }

    /// <summary>
    /// Crea una NotFoundException para una Comanda
    /// </summary>
    public static NotFoundException ForComanda(Guid comandaId)
    {
        return new NotFoundException(
            "Comanda",
            comandaId,
            $"Comanda con ID {comandaId} no fue encontrada.");
    }

    /// <summary>
    /// Crea una NotFoundException para una Mesa
    /// </summary>
    public static NotFoundException ForMesa(Guid mesaId)
    {
        return new NotFoundException(
            "Mesa",
            mesaId,
            $"Mesa con ID {mesaId} no fue encontrada.");
    }

    /// <summary>
    /// Crea una NotFoundException para una Mesa por número
    /// </summary>
    public static NotFoundException ForMesaPorNumero(int numeroMesa)
    {
        return new NotFoundException(
            "Mesa",
            numeroMesa,
            $"Mesa número {numeroMesa} no fue encontrada.");
    }

    /// <summary>
    /// Crea una NotFoundException para una Reservación
    /// </summary>
    public static NotFoundException ForReservacion(Guid reservacionId)
    {
        return new NotFoundException(
            "Reservacion",
            reservacionId,
            $"Reservación con ID {reservacionId} no fue encontrada.");
    }

    /// <summary>
    /// Crea una NotFoundException para una Factura
    /// </summary>
    public static NotFoundException ForFactura(Guid facturaId)
    {
        return new NotFoundException(
            "Factura",
            facturaId,
            $"Factura con ID {facturaId} no fue encontrada.");
    }

    /// <summary>
    /// Crea una NotFoundException para un Ingrediente
    /// </summary>
    public static NotFoundException ForIngrediente(Guid ingredienteId)
    {
        return new NotFoundException(
            "Ingrediente",
            ingredienteId,
            $"Ingrediente con ID {ingredienteId} no fue encontrado.");
    }

    /// <summary>
    /// Crea una NotFoundException para un Proveedor
    /// </summary>
    public static NotFoundException ForProveedor(Guid proveedorId)
    {
        return new NotFoundException(
            "Proveedor",
            proveedorId,
            $"Proveedor con ID {proveedorId} no fue encontrado.");
    }

    /// <summary>
    /// Crea una NotFoundException para un Usuario
    /// </summary>
    public static NotFoundException ForUsuario(Guid usuarioId)
    {
        return new NotFoundException(
            "Usuario",
            usuarioId,
            $"Usuario con ID {usuarioId} no fue encontrado.");
    }

    /// <summary>
    /// Crea una NotFoundException para un Usuario por email
    /// </summary>
    public static NotFoundException ForUsuarioPorEmail(string email)
    {
        return new NotFoundException(
            "Usuario",
            email,
            $"Usuario con email {email} no fue encontrado.");
    }
} 