namespace RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Comercial;

/// <summary>
/// Builder para crear datos de prueba de promociones comerciales
/// </summary>
public class PromocionTestDataBuilder
{
    private string _nombre = "Promo Test";
    private string _descripcion = "Promoción de prueba";
    private decimal _descuento = 10.0m;
    private DateTime _fechaInicio = DateTime.UtcNow.Date;
    private DateTime _fechaFin = DateTime.UtcNow.Date.AddDays(7);
    private bool _activa = true;
    private List<Guid> _productos = new();

    public PromocionTestDataBuilder ConNombre(string nombre)
    {
        _nombre = nombre;
        return this;
    }

    public PromocionTestDataBuilder ConDescripcion(string descripcion)
    {
        _descripcion = descripcion;
        return this;
    }

    public PromocionTestDataBuilder ConDescuento(decimal descuento)
    {
        _descuento = descuento;
        return this;
    }

    public PromocionTestDataBuilder ConFechas(DateTime inicio, DateTime fin)
    {
        _fechaInicio = inicio;
        _fechaFin = fin;
        return this;
    }

    public PromocionTestDataBuilder ConActiva(bool activa)
    {
        _activa = activa;
        return this;
    }

    public PromocionTestDataBuilder ConProductos(params Guid[] productos)
    {
        _productos = productos.ToList();
        return this;
    }

    public object BuildCrearPromocionRequest() => new
    {
        Nombre = _nombre,
        Descripcion = _descripcion,
        Descuento = _descuento,
        FechaInicio = _fechaInicio,
        FechaFin = _fechaFin,
        Activa = _activa,
        Productos = _productos
    };
} 