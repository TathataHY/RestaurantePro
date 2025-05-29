namespace RestaurantePro.Domain.Core.SharedKernel.Exceptions;

/// <summary>
/// Excepción que se lanza cuando no se encuentra una entidad específica en el dominio.
/// Proporciona información detallada sobre qué entidad se buscaba y con qué criterios.
/// </summary>
public class EntityNotFoundException : DomainException
{
    /// <summary>
    /// Nombre de la entidad que no se encontró
    /// </summary>
    public string EntityName { get; }
    
    /// <summary>
    /// Identificador utilizado para buscar la entidad
    /// </summary>
    public object SearchCriteria { get; }
    
    /// <summary>
    /// Tipo de búsqueda realizada (ej: "Id", "Email", "Codigo")
    /// </summary>
    public string SearchType { get; }

    /// <summary>
    /// Constructor para entidad no encontrada
    /// </summary>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="searchCriteria">Criterio de búsqueda utilizado</param>
    /// <param name="searchType">Tipo de búsqueda</param>
    /// <param name="domainContext">Contexto del dominio</param>
    public EntityNotFoundException(
        string entityName,
        object searchCriteria,
        string searchType,
        string domainContext)
        : base($"{entityName} con {searchType} '{searchCriteria}' no fue encontrado", "ENTITY_NOT_FOUND", domainContext)
    {
        EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName));
        SearchCriteria = searchCriteria ?? throw new ArgumentNullException(nameof(searchCriteria));
        SearchType = searchType ?? throw new ArgumentNullException(nameof(searchType));
        
        // Agregar datos contextuales
        WithData("EntityName", EntityName)
            .WithData("SearchCriteria", SearchCriteria)
            .WithData("SearchType", SearchType);
    }

    /// <summary>
    /// Constructor simplificado para búsqueda por ID
    /// </summary>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="id">ID de la entidad</param>
    /// <param name="domainContext">Contexto del dominio</param>
    public EntityNotFoundException(string entityName, Guid id, string domainContext)
        : this(entityName, id, "Id", domainContext)
    {
    }

    /// <summary>
    /// Crea una excepción para entidad no encontrada por ID
    /// </summary>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="id">ID de la entidad</param>
    /// <param name="domainContext">Contexto del dominio</param>
    /// <returns>Nueva instancia de EntityNotFoundException</returns>
    public static EntityNotFoundException ForId(string entityName, Guid id, string domainContext)
    {
        return new EntityNotFoundException(entityName, id, "Id", domainContext);
    }

    /// <summary>
    /// Crea una excepción para entidad no encontrada por código
    /// </summary>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="codigo">Código de la entidad</param>
    /// <param name="domainContext">Contexto del dominio</param>
    /// <returns>Nueva instancia de EntityNotFoundException</returns>
    public static EntityNotFoundException ForCodigo(string entityName, string codigo, string domainContext)
    {
        return new EntityNotFoundException(entityName, codigo, "Codigo", domainContext);
    }

    /// <summary>
    /// Crea una excepción para entidad no encontrada por email
    /// </summary>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="email">Email de la entidad</param>
    /// <param name="domainContext">Contexto del dominio</param>
    /// <returns>Nueva instancia de EntityNotFoundException</returns>
    public static EntityNotFoundException ForEmail(string entityName, string email, string domainContext)
    {
        return new EntityNotFoundException(entityName, email, "Email", domainContext);
    }

    /// <summary>
    /// Crea una excepción para entidad no encontrada por nombre
    /// </summary>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="nombre">Nombre buscado</param>
    /// <param name="domainContext">Contexto del dominio</param>
    /// <returns>Nueva instancia de EntityNotFoundException</returns>
    public static EntityNotFoundException ForNombre(string entityName, string nombre, string domainContext)
    {
        return new EntityNotFoundException(entityName, nombre, "Nombre", domainContext);
    }

    /// <summary>
    /// Crea una excepción para múltiples entidades no encontradas
    /// </summary>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="ids">Lista de IDs no encontrados</param>
    /// <param name="domainContext">Contexto del dominio</param>
    /// <returns>Nueva instancia de EntityNotFoundException</returns>
    public static EntityNotFoundException ForMultipleIds(string entityName, IEnumerable<Guid> ids, string domainContext)
    {
        var idsArray = ids.ToArray();
        var message = $"No se encontraron {idsArray.Length} {entityName}(s) con los IDs especificados";
        
        return new EntityNotFoundException(entityName, string.Join(", ", idsArray), "MultipleIds", domainContext)
        {
            Data = { ["Count"] = idsArray.Length, ["Ids"] = idsArray }
        };
    }

    /// <summary>
    /// Crea una excepción para entidad no encontrada con criterios complejos
    /// </summary>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <param name="criteria">Diccionario con criterios de búsqueda</param>
    /// <param name="domainContext">Contexto del dominio</param>
    /// <returns>Nueva instancia de EntityNotFoundException</returns>
    public static EntityNotFoundException ForCriteria(
        string entityName, 
        Dictionary<string, object> criteria, 
        string domainContext)
    {
        var criteriaString = string.Join(", ", criteria.Select(kvp => $"{kvp.Key}='{kvp.Value}'"));
        var message = $"{entityName} no encontrado con criterios: {criteriaString}";
        
        return new EntityNotFoundException(entityName, criteriaString, "ComplexCriteria", domainContext)
            .WithData(criteria) as EntityNotFoundException;
    }

    /// <summary>
    /// Crea una excepción específica para Cliente no encontrado
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <returns>Nueva instancia de EntityNotFoundException</returns>
    public static EntityNotFoundException ClienteNotFound(Guid clienteId)
    {
        return ForId("Cliente", clienteId, "Comercial");
    }

    /// <summary>
    /// Crea una excepción específica para Producto no encontrado
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>Nueva instancia de EntityNotFoundException</returns>
    public static EntityNotFoundException ProductoNotFound(Guid productoId)
    {
        return ForId("Producto", productoId, "Core");
    }

    /// <summary>
    /// Crea una excepción específica para Ingrediente no encontrado
    /// </summary>
    /// <param name="ingredienteId">ID del ingrediente</param>
    /// <returns>Nueva instancia de EntityNotFoundException</returns>
    public static EntityNotFoundException IngredienteNotFound(Guid ingredienteId)
    {
        return ForId("Ingrediente", ingredienteId, "Inventario");
    }

    /// <summary>
    /// Crea una excepción específica para Mesa no encontrada
    /// </summary>
    /// <param name="mesaId">ID de la mesa</param>
    /// <returns>Nueva instancia de EntityNotFoundException</returns>
    public static EntityNotFoundException MesaNotFound(Guid mesaId)
    {
        return ForId("Mesa", mesaId, "Operaciones");
    }
} 