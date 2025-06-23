using Bogus;

namespace RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Operaciones;

/// <summary>
/// Builder para crear datos de prueba de mesas del restaurante
/// </summary>
public class MesaTestDataBuilder
{
    private string _numero = "Mesa 1";
    private int _capacidad = 4;
    private string _ubicacion = "Interior";
    private string _descripcion = "Mesa estándar";
    private RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums.EstadoMesa _estado = RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums.EstadoMesa.Disponible;
    private string _tipo = "Estandar";

    public MesaTestDataBuilder ConNumero(string numero)
    {
        _numero = numero;
        return this;
    }

    public MesaTestDataBuilder ConCapacidad(int capacidad)
    {
        _capacidad = capacidad;
        return this;
    }

    public MesaTestDataBuilder ConUbicacion(string ubicacion)
    {
        _ubicacion = ubicacion;
        return this;
    }

    public MesaTestDataBuilder ConDescripcion(string descripcion)
    {
        _descripcion = descripcion;
        return this;
    }

    public MesaTestDataBuilder ConEstado(RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums.EstadoMesa estado)
    {
        _estado = estado;
        return this;
    }

    public MesaTestDataBuilder ConTipo(string tipo)
    {
        _tipo = tipo;
        return this;
    }

    /// <summary>
    /// Construye un request para crear una mesa
    /// </summary>
    public object BuildCrearMesaRequest()
    {
        // Manejar valores inválidos sin lanzar excepción
        int numero;
        if (string.IsNullOrWhiteSpace(_numero) || !int.TryParse(_numero, out numero))
        {
            numero = 0; // Valor inválido que será capturado por el validador
        }
        
        return new
        {
            Numero = numero,
            Capacidad = _capacidad,
            Zona = _ubicacion,
            Descripcion = _descripcion
        };
    }

    /// <summary>
    /// Construye un request para actualizar una mesa
    /// </summary>
    public object BuildActualizarMesaRequest()
    {
        return new
        {
            Numero = _numero,
            Capacidad = _capacidad,
            Ubicacion = _ubicacion,
            Descripcion = _descripcion,
            Tipo = _tipo
        };
    }

    /// <summary>
    /// Construye un request para cambiar el estado de una mesa
    /// </summary>
    public object BuildCambiarEstadoRequest(string nuevoEstado)
    {
        return new
        {
            Estado = nuevoEstado,
            Observaciones = $"Cambio de estado a {nuevoEstado}"
        };
    }

    /// <summary>
    /// Construye un request para asignar un cliente a una mesa
    /// </summary>
    public object BuildAsignarClienteRequest(Guid clienteId)
    {
        return new
        {
            ClienteId = clienteId,
            NumeroPersonas = _capacidad,
            Observaciones = "Asignación automática"
        };
    }

    /// <summary>
    /// Construye un request para liberar una mesa
    /// </summary>
    public object BuildLiberarMesaRequest()
    {
        return new
        {
            MotivoLiberacion = "Cliente terminó",
            LimpiezaRequerida = true,
            Observaciones = "Mesa liberada automáticamente"
        };
    }

    /// <summary>
    /// Construye un request para reservar una mesa
    /// </summary>
    public object BuildReservarMesaRequest(Guid clienteId, DateTime fechaReservacion, int numeroPersonas)
    {
        return new
        {
            ClienteId = clienteId,
            FechaReservacion = fechaReservacion,
            NumeroPersonas = numeroPersonas,
            Observaciones = "Reserva creada desde tests"
        };
    }

    /// <summary>
    /// Construye una entidad Mesa para tests de dominio
    /// </summary>
    public object BuildMesaEntity()
    {
        return new
        {
            Id = Guid.NewGuid(),
            Numero = _numero,
            Capacidad = _capacidad,
            Ubicacion = _ubicacion,
            Descripcion = _descripcion,
            Estado = _estado.ToString(),
            Tipo = _tipo,
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };
    }
} 