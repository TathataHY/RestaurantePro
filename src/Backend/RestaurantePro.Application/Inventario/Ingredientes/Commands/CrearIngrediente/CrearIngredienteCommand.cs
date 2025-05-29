using RestaurantePro.Application.Inventario.Ingredientes.DTOs;

namespace RestaurantePro.Application.Inventario.Ingredientes.Commands.CrearIngrediente;

/// <summary>
/// Comando para crear un nuevo ingrediente en el inventario
/// Incluye stock inicial y configuración completa
/// </summary>
public class CrearIngredienteCommand : IRequest<Result<IngredienteDto>>
{
    /// <summary>
    /// Nombre del ingrediente
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Código único del ingrediente
    /// </summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del ingrediente
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Unidad de medida
    /// </summary>
    public string UnidadMedida { get; set; } = string.Empty;

    /// <summary>
    /// Stock inicial
    /// </summary>
    public decimal StockInicial { get; set; } = 0;

    /// <summary>
    /// Stock mínimo permitido
    /// </summary>
    public decimal StockMinimo { get; set; } = 0;

    /// <summary>
    /// Nivel de rotación del ingrediente
    /// </summary>
    public string Rotacion { get; set; } = "Media";

    /// <summary>
    /// Temporada principal del ingrediente
    /// </summary>
    public string Temporada { get; set; } = "TodoElAño";

    /// <summary>
    /// ID del proveedor principal (opcional)
    /// </summary>
    public Guid? ProveedorPrincipalId { get; set; }

    /// <summary>
    /// Costo inicial del ingrediente
    /// </summary>
    public decimal CostoInicial { get; set; } = 0;

    /// <summary>
    /// Indica si el ingrediente debe estar activo al crearse
    /// </summary>
    public bool EstaActivo { get; set; } = true;

    /// <summary>
    /// ID del usuario que crea el ingrediente
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Motivo del stock inicial (para el movimiento de inventario)
    /// </summary>
    public string MotivoStockInicial { get; set; } = "Stock inicial al crear ingrediente";

    /// <summary>
    /// Constructor sin parámetros para model binding
    /// </summary>
    public CrearIngredienteCommand() { }

    /// <summary>
    /// Constructor con parámetros básicos
    /// </summary>
    public CrearIngredienteCommand(string nombre, string codigo, string unidadMedida, decimal stockInicial, decimal stockMinimo)
    {
        Nombre = nombre;
        Codigo = codigo;
        UnidadMedida = unidadMedida;
        StockInicial = stockInicial;
        StockMinimo = stockMinimo;
    }

    /// <summary>
    /// Factory method para crear con información completa
    /// </summary>
    public static CrearIngredienteCommand Crear(
        string nombre, 
        string codigo, 
        string descripcion,
        string unidadMedida, 
        decimal stockInicial, 
        decimal stockMinimo,
        decimal costoInicial,
        Guid usuarioId)
    {
        return new CrearIngredienteCommand
        {
            Nombre = nombre,
            Codigo = codigo,
            Descripcion = descripcion,
            UnidadMedida = unidadMedida,
            StockInicial = stockInicial,
            StockMinimo = stockMinimo,
            CostoInicial = costoInicial,
            UsuarioId = usuarioId
        };
    }
} 