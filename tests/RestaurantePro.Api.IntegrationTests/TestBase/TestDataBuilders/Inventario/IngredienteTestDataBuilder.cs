using RestaurantePro.Domain.Inventario.Ingredientes.Enums;

namespace RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Inventario;

/// <summary>
/// Builder para crear datos de prueba de Ingredientes
/// </summary>
public class IngredienteTestDataBuilder
{
    private string _nombre = "Tomate";
    private string _codigo = "TOM-001";
    private string _descripcion = "Tomate fresco para ensaladas";
    private string _unidadMedida = "Kilogramo";
    private decimal _stockMinimo = 5;
    private decimal _stockInicial = 10;
    private decimal _costoInicial = 1.5m;
    private string _motivoStockInicial = "Carga Inicial";
    private Guid _usuarioId = Guid.Parse("2d4833cb-1190-4829-a789-341a02f59abb"); // Un GUID de prueba consistente

    public IngredienteTestDataBuilder ConNombre(string nombre)
    {
        _nombre = nombre;
        return this;
    }

    public IngredienteTestDataBuilder ConCodigo(string codigo)
    {
        _codigo = codigo;
        return this;
    }
    
    public IngredienteTestDataBuilder ConDescripcion(string descripcion)
    {
        _descripcion = descripcion;
        return this;
    }

    public IngredienteTestDataBuilder ConUnidadMedida(string unidadMedida)
    {
        _unidadMedida = unidadMedida;
        return this;
    }
    
    public IngredienteTestDataBuilder ConStockMinimo(decimal stockMinimo)
    {
        _stockMinimo = stockMinimo;
        return this;
    }

    public IngredienteTestDataBuilder ConStockInicial(decimal stock)
    {
        _stockInicial = stock;
        return this;
    }

    public object BuildCrearIngredienteRequest() => new
    {
        Nombre = _nombre,
        Codigo = _codigo,
        Descripcion = _descripcion,
        UnidadMedida = _unidadMedida,
        StockInicial = _stockInicial,
        StockMinimo = _stockMinimo,
        CostoInicial = _costoInicial,
        MotivoStockInicial = _motivoStockInicial,
        UsuarioId = _usuarioId
    };
    
    public object BuildActualizarIngredienteRequest() => new
    {
        Nombre = _nombre,
        Descripcion = _descripcion,
        UnidadMedida = _unidadMedida,
        StockMinimo = _stockMinimo,
        UsuarioId = _usuarioId
    };
} 