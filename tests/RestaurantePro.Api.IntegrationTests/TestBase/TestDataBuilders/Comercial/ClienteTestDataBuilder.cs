using Bogus;

namespace RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Comercial;

/// <summary>
/// Builder para crear datos de prueba de clientes para tests de integración
/// </summary>
public class ClienteTestDataBuilder
{
    private readonly Faker _faker = new Faker("es");
    
    private string? _nombre;
    private string? _apellido;
    private string? _email;
    private string? _telefono;
    private string? _direccion;
    private DateTime? _fechaNacimiento;
    private SegmentoCliente _tipo = SegmentoCliente.Regular;
    private bool _activo = true;
    private string? _observaciones;

    /// <summary>
    /// Genera un nombre único válido (mínimo 2 caracteres)
    /// </summary>
    private string GenerarNombreUnico()
    {
        var guid = Guid.NewGuid().ToString("N");
        var nombre = _faker.Name.FirstName();
        // Asegurar que el nombre tenga al menos 2 caracteres
        if (nombre.Length < 2)
        {
            nombre = "Juan"; // Nombre por defecto válido
        }
        return $"{nombre}_{guid.Substring(0, 4)}";
    }

    /// <summary>
    /// Genera un apellido único válido (mínimo 2 caracteres)
    /// </summary>
    private string GenerarApellidoUnico()
    {
        var guid = Guid.NewGuid().ToString("N");
        var apellido = _faker.Name.LastName();
        // Asegurar que el apellido tenga al menos 2 caracteres
        if (apellido.Length < 2)
        {
            apellido = "Pérez"; // Apellido por defecto válido
        }
        return $"{apellido}_{guid.Substring(4, 4)}";
    }

    /// <summary>
    /// Genera un email único válido
    /// </summary>
    private string GenerarEmailUnico()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"cliente.{guid.Substring(0, 8)}@test.com";
    }

    /// <summary>
    /// Genera un teléfono único válido (formato internacional)
    /// </summary>
    private string GenerarTelefonoUnico()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"+34 91 {guid.Substring(0, 3)} {guid.Substring(3, 3)} {guid.Substring(6, 2)}";
    }

    /// <summary>
    /// Genera una dirección única
    /// </summary>
    private string GenerarDireccionUnica()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"{_faker.Address.StreetAddress()} #{guid.Substring(0, 4)}";
    }

    /// <summary>
    /// Genera una fecha de nacimiento válida (cliente mayor de 18 años)
    /// </summary>
    private DateTime GenerarFechaNacimientoValida()
    {
        // Generar fecha entre 18 y 80 años atrás
        var fechaMinima = DateTime.Now.AddYears(-80);
        var fechaMaxima = DateTime.Now.AddYears(-18);
        return _faker.Date.Between(fechaMinima, fechaMaxima);
    }

    /// <summary>
    /// Establece el nombre del cliente
    /// </summary>
    public ClienteTestDataBuilder ConNombre(string nombre)
    {
        _nombre = nombre;
        return this;
    }

    /// <summary>
    /// Establece el apellido del cliente
    /// </summary>
    public ClienteTestDataBuilder ConApellido(string apellido)
    {
        _apellido = apellido;
        return this;
    }

    /// <summary>
    /// Establece el email del cliente
    /// </summary>
    public ClienteTestDataBuilder ConEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Establece el teléfono del cliente
    /// </summary>
    public ClienteTestDataBuilder ConTelefono(string telefono)
    {
        _telefono = telefono;
        return this;
    }

    /// <summary>
    /// Establece la dirección del cliente
    /// </summary>
    public ClienteTestDataBuilder ConDireccion(string direccion)
    {
        _direccion = direccion;
        return this;
    }

    /// <summary>
    /// Establece la fecha de nacimiento del cliente
    /// </summary>
    public ClienteTestDataBuilder ConFechaNacimiento(DateTime fechaNacimiento)
    {
        _fechaNacimiento = fechaNacimiento;
        return this;
    }

    /// <summary>
    /// Establece el tipo de cliente
    /// </summary>
    public ClienteTestDataBuilder ConTipo(SegmentoCliente tipo)
    {
        _tipo = tipo;
        return this;
    }

    /// <summary>
    /// Establece si el cliente está activo
    /// </summary>
    public ClienteTestDataBuilder Activo(bool activo)
    {
        _activo = activo;
        return this;
    }

    /// <summary>
    /// Establece las observaciones del cliente
    /// </summary>
    public ClienteTestDataBuilder ConObservaciones(string observaciones)
    {
        _observaciones = observaciones;
        return this;
    }

    /// <summary>
    /// Construye el objeto de request para crear un cliente
    /// </summary>
    public object BuildCrearClienteRequest()
    {
        return new
        {
            Nombre = _nombre ?? GenerarNombreUnico(),
            Apellido = _apellido ?? GenerarApellidoUnico(),
            Email = _email ?? GenerarEmailUnico(),
            Telefono = _telefono ?? GenerarTelefonoUnico(),
            Direccion = _direccion ?? GenerarDireccionUnica(),
            FechaNacimiento = _fechaNacimiento ?? GenerarFechaNacimientoValida(),
            Tipo = _tipo,
            Notas = _observaciones ?? _faker.Lorem.Sentence(3, 5),
            CrearTarjetaFidelizacion = true
        };
    }

    /// <summary>
    /// Construye el objeto de request para actualizar un cliente
    /// </summary>
    public object BuildActualizarClienteRequest()
    {
        return new
        {
            Nombre = _nombre ?? GenerarNombreUnico(),
            Apellido = _apellido ?? GenerarApellidoUnico(),
            Email = _email ?? GenerarEmailUnico(),
            Telefono = _telefono ?? GenerarTelefonoUnico(),
            Direccion = _direccion ?? GenerarDireccionUnica(),
            FechaNacimiento = _fechaNacimiento ?? GenerarFechaNacimientoValida(),
            Tipo = _tipo,
            Notas = _observaciones ?? _faker.Lorem.Sentence(3, 5)
        };
    }

    /// <summary>
    /// Construye el objeto de request para buscar clientes
    /// </summary>
    public object BuildBuscarClientesRequest(string? termino = null, string? tipo = null, bool? activo = null)
    {
        return new
        {
            Termino = termino ?? _faker.Name.FirstName(),
            Tipo = tipo ?? _tipo.ToString(),
            Activo = activo ?? _activo,
            PageNumber = 1,
            PageSize = 20
        };
    }

    /// <summary>
    /// Construye el objeto de request para activar cliente
    /// </summary>
    public object BuildActivarClienteRequest()
    {
        return new
        {
            Observaciones = _faker.Lorem.Sentence(3, 5)
        };
    }

    /// <summary>
    /// Construye el objeto de request para agregar comentario al cliente
    /// </summary>
    public object BuildAgregarComentarioRequest(string? comentario = null)
    {
        return new
        {
            Comentario = comentario ?? _faker.Lorem.Sentence(5, 10),
            Tipo = _faker.PickRandom("General", "Preferencias", "Alergias", "Observaciones"),
            Fecha = DateTime.Now
        };
    }

    /// <summary>
    /// Construye el objeto de request para enviar notificación al cliente
    /// </summary>
    public object BuildEnviarNotificacionRequest(string? mensaje = null, string? tipo = null)
    {
        return new
        {
            Mensaje = mensaje ?? _faker.Lorem.Sentence(5, 10),
            Tipo = tipo ?? _faker.PickRandom("Email", "SMS", "Push"),
            Fecha = DateTime.Now
        };
    }

    /// <summary>
    /// Construye el objeto de request para obtener historial del cliente
    /// </summary>
    public object BuildObtenerHistorialRequest(DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        return new
        {
            FechaInicio = fechaInicio ?? DateTime.Now.AddMonths(-1),
            FechaFin = fechaFin ?? DateTime.Now,
            PageNumber = 1,
            PageSize = 20
        };
    }

    /// <summary>
    /// Resetea el builder a su estado inicial
    /// </summary>
    public ClienteTestDataBuilder Reset()
    {
        _nombre = null;
        _apellido = null;
        _email = null;
        _telefono = null;
        _direccion = null;
        _fechaNacimiento = null;
        _tipo = SegmentoCliente.Regular;
        _activo = true;
        _observaciones = null;
        return this;
    }

    /// <summary>
    /// Crea una nueva instancia del builder
    /// </summary>
    public static ClienteTestDataBuilder Nuevo()
    {
        return new ClienteTestDataBuilder();
    }
}

/// <summary>
/// Tipos de cliente para los tests
/// </summary>
public enum SegmentoCliente
{
    Regular,
    VIP,
    Corporativo,
    Frecuente
} 