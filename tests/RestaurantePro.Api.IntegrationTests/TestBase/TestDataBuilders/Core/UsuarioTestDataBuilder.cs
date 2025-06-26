using Bogus;
using RestaurantePro.Application.Core.Usuarios.Commands.CrearUsuario;
using RestaurantePro.Application.Core.Usuarios.Commands.ActualizarUsuario;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using CrearUsuarioHorarioDto = RestaurantePro.Application.Core.Usuarios.Commands.CrearUsuario.HorarioTrabajoDto;
using RestaurantePro.Application.Core.Usuarios.DTOs;

namespace RestaurantePro.Api.IntegrationTests.TestBase.TestDataBuilders.Core;

/// <summary>
/// Builder para crear datos de prueba de usuarios para tests de integración
/// </summary>
public class UsuarioTestDataBuilder
{
    private readonly Faker _faker = new Faker("es");
    
    private string? _nombreUsuario;
    private string? _nombreCompleto;
    private string? _email;
    private string? _password;
    private string? _confirmarPassword;
    private string? _rol;
    private int? _nivelAcceso;
    private string? _telefono;
    private Guid? _usuarioCreadorId;
    private bool? _requiereAprobacion;
    private Guid? _usuarioCambiadorId;
    private Guid? _usuarioReseteadorId;

    /// <summary>
    /// Genera un nombre de usuario único
    /// </summary>
    private string GenerarNombreUsuarioUnico()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"usuario_{guid.Substring(0, 8)}";
    }

    /// <summary>
    /// Genera un email único
    /// </summary>
    private string GenerarEmailUnico()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"usuario_{guid.Substring(0, 8)}@test.com";
    }

    /// <summary>
    /// Genera un teléfono único válido (formato chileno)
    /// </summary>
    private string GenerarTelefonoUnico()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"+56 9 {guid.Substring(0, 4)} {guid.Substring(4, 4)}";
    }

    /// <summary>
    /// Genera un nombre completo único
    /// </summary>
    private string GenerarNombreCompletoUnico()
    {
        var guid = Guid.NewGuid().ToString("N");
        return $"{_faker.Name.FirstName()}_{guid.Substring(0, 4)} {_faker.Name.LastName()}_{guid.Substring(4, 4)}";
    }

    /// <summary>
    /// Establece el nombre de usuario
    /// </summary>
    public UsuarioTestDataBuilder ConNombreUsuario(string nombreUsuario)
    {
        _nombreUsuario = nombreUsuario;
        return this;
    }

    /// <summary>
    /// Establece el nombre completo
    /// </summary>
    public UsuarioTestDataBuilder ConNombreCompleto(string nombreCompleto)
    {
        _nombreCompleto = nombreCompleto;
        return this;
    }

    /// <summary>
    /// Establece el email
    /// </summary>
    public UsuarioTestDataBuilder ConEmail(string email)
    {
        _email = email;
        return this;
    }

    /// <summary>
    /// Establece la contraseña
    /// </summary>
    public UsuarioTestDataBuilder ConPassword(string password)
    {
        _password = password;
        return this;
    }

    /// <summary>
    /// Establece la confirmación de contraseña
    /// </summary>
    public UsuarioTestDataBuilder ConConfirmarPassword(string confirmarPassword)
    {
        _confirmarPassword = confirmarPassword;
        return this;
    }

    /// <summary>
    /// Establece el rol del usuario
    /// </summary>
    public UsuarioTestDataBuilder ConRol(string rol)
    {
        _rol = rol;
        return this;
    }

    /// <summary>
    /// Establece el nivel de acceso
    /// </summary>
    public UsuarioTestDataBuilder ConNivelAcceso(int nivelAcceso)
    {
        _nivelAcceso = nivelAcceso;
        return this;
    }

    /// <summary>
    /// Establece el teléfono
    /// </summary>
    public UsuarioTestDataBuilder ConTelefono(string telefono)
    {
        _telefono = telefono;
        return this;
    }

    /// <summary>
    /// Establece el ID del usuario creador
    /// </summary>
    public UsuarioTestDataBuilder ConUsuarioCreadorId(Guid usuarioCreadorId)
    {
        _usuarioCreadorId = usuarioCreadorId;
        return this;
    }

    /// <summary>
    /// Establece si la actualización requiere aprobación
    /// </summary>
    public UsuarioTestDataBuilder ConRequiereAprobacion(bool requiereAprobacion)
    {
        _requiereAprobacion = requiereAprobacion;
        return this;
    }

    /// <summary>
    /// Establece el ID del usuario que realiza el cambio de rol
    /// </summary>
    public UsuarioTestDataBuilder ConUsuarioCambiadorId(Guid usuarioCambiadorId)
    {
        _usuarioCambiadorId = usuarioCambiadorId;
        return this;
    }

    /// <summary>
    /// Establece el ID del usuario que realiza el reset de contraseña
    /// </summary>
    public UsuarioTestDataBuilder ConUsuarioReseteadorId(Guid usuarioReseteadorId)
    {
        _usuarioReseteadorId = usuarioReseteadorId;
        return this;
    }

    /// <summary>
    /// Construye el comando para crear un usuario
    /// </summary>
    public CrearUsuarioCommand BuildCrearUsuarioCommand()
    {
        if (!_usuarioCreadorId.HasValue || _usuarioCreadorId.Value == Guid.Empty)
        {
            throw new InvalidOperationException("UsuarioCreadorId es requerido y debe ser un GUID válido. Use ConUsuarioCreadorId() para especificarlo.");
        }

        return new CrearUsuarioCommand
        {
            NombreUsuario = _nombreUsuario ?? GenerarNombreUsuarioUnico(),
            NombreCompleto = _nombreCompleto ?? GenerarNombreCompletoUnico(),
            Email = _email ?? GenerarEmailUnico(),
            Password = _password ?? "Password123!",
            ConfirmarPassword = _confirmarPassword ?? "Password123!",
            Rol = _rol ?? "Empleado",
            NivelAcceso = _nivelAcceso ?? 5,
            Telefono = _telefono ?? GenerarTelefonoUnico(),
            UsuarioCreadorId = _usuarioCreadorId.Value,
            
            // Campos adicionales requeridos por el validador
            RolesAdicionales = new List<string>(),
            PermisosEspecificos = new List<string>(),
            HorariosTrabajo = new List<CrearUsuarioHorarioDto>
            {
                new CrearUsuarioHorarioDto
                {
                    DiaSemana = "Lunes",
                    HoraInicio = TimeSpan.Parse("08:00"),
                    HoraFin = TimeSpan.Parse("17:00"),
                    EsDiaLibre = false
                },
                new CrearUsuarioHorarioDto
                {
                    DiaSemana = "Martes",
                    HoraInicio = TimeSpan.Parse("08:00"),
                    HoraFin = TimeSpan.Parse("17:00"),
                    EsDiaLibre = false
                },
                new CrearUsuarioHorarioDto
                {
                    DiaSemana = "Miércoles",
                    HoraInicio = TimeSpan.Parse("08:00"),
                    HoraFin = TimeSpan.Parse("17:00"),
                    EsDiaLibre = false
                },
                new CrearUsuarioHorarioDto
                {
                    DiaSemana = "Jueves",
                    HoraInicio = TimeSpan.Parse("08:00"),
                    HoraFin = TimeSpan.Parse("17:00"),
                    EsDiaLibre = false
                },
                new CrearUsuarioHorarioDto
                {
                    DiaSemana = "Viernes",
                    HoraInicio = TimeSpan.Parse("08:00"),
                    HoraFin = TimeSpan.Parse("17:00"),
                    EsDiaLibre = false
                }
            },
            Departamento = "Cocina",
            Puesto = "Cocinero",
            SucursalId = null,
            SupervisorId = null
        };
    }

    /// <summary>
    /// Construye el comando para actualizar un usuario
    /// </summary>
    public ActualizarUsuarioCommand BuildActualizarUsuarioCommand(Guid usuarioId, Guid? usuarioAutorizaId = null)
    {
        if (!usuarioAutorizaId.HasValue || usuarioAutorizaId.Value == Guid.Empty)
        {
            throw new InvalidOperationException("UsuarioAutorizaId es requerido y debe ser un GUID válido. Pase un usuario autorizador válido como parámetro.");
        }

        return new ActualizarUsuarioCommand
        {
            UsuarioId = usuarioId,
            Nombre = _nombreCompleto ?? _faker.Name.FullName(),
            Email = _email ?? _faker.Internet.Email(),
            Telefono = _telefono ?? _faker.Phone.PhoneNumber("+569########"),
            Rol = _rol ?? "Gerente",
            NivelAcceso = _nivelAcceso ?? 6,
            UsuarioAutorizaId = usuarioAutorizaId.Value,
            MotivoActualizacion = "Actualización de datos de prueba para tests de integración",
            Prioridad = 2, // Prioridad normal
            RequiereAprobacion = _requiereAprobacion ?? false,
            NotificarUsuario = true,
            NotificarSupervisor = false
        };
    }

    /// <summary>
    /// Construye el objeto de request para cambiar rol
    /// </summary>
    public object BuildCambiarRolRequest(string? nuevoRol = null)
    {
        if (!_usuarioCambiadorId.HasValue || _usuarioCambiadorId.Value == Guid.Empty)
        {
            throw new InvalidOperationException("UsuarioCambiadorId es requerido y debe ser un GUID válido. Use ConUsuarioCambiadorId() para especificarlo.");
        }

        return new
        {
            NuevoRol = nuevoRol ?? _faker.PickRandom("Administrador", "Gerente", "Cajero", "Mesero", "Cocinero", "EncargadoInventario"),
            UsuarioCambiadorId = _usuarioCambiadorId.Value,
            Motivo = _faker.Lorem.Sentence(3, 5)
        };
    }

    /// <summary>
    /// Construye el objeto de request para resetear contraseña
    /// </summary>
    public object BuildResetPasswordRequest()
    {
        if (!_usuarioReseteadorId.HasValue || _usuarioReseteadorId.Value == Guid.Empty)
        {
            throw new InvalidOperationException("UsuarioReseteadorId es requerido y debe ser un GUID válido. Use ConUsuarioReseteadorId() para especificarlo.");
        }

        return new
        {
            UsuarioReseteadorId = _usuarioReseteadorId.Value,
            NuevaPasswordTemporal = "NuevaPassword123!",
            Motivo = _faker.Lorem.Sentence(3, 5)
        };
    }

    /// <summary>
    /// Construye el objeto de request para buscar usuarios
    /// </summary>
    public object BuildBuscarUsuariosRequest(string? termino = null, string? rol = null, bool? activo = null)
    {
        return new
        {
            Termino = termino ?? _faker.Name.FirstName(),
            Rol = rol ?? _faker.PickRandom("Administrador", "Gerente", "Cajero", "Mesero", "Cocinero", "EncargadoInventario"),
            Activo = activo ?? true,
            PageNumber = 1,
            PageSize = 20
        };
    }

    /// <summary>
    /// Construye el objeto de request para activar/desactivar usuario
    /// </summary>
    public object BuildCambiarEstadoRequest(bool? activo = null)
    {
        return new
        {
            Activo = activo ?? true,
            Motivo = _faker.Lorem.Sentence(3, 5)
        };
    }

    /// <summary>
    /// Construye un usuario con datos válidos para tests
    /// </summary>
    public CrearUsuarioCommand BuildUsuarioValido()
    {
        if (!_usuarioCreadorId.HasValue || _usuarioCreadorId.Value == Guid.Empty)
        {
            throw new InvalidOperationException("UsuarioCreadorId es requerido y debe ser un GUID válido. Use ConUsuarioCreadorId() para especificarlo.");
        }

        return new CrearUsuarioCommand
        {
            NombreUsuario = _faker.Internet.UserName(),
            NombreCompleto = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            Password = "Password123!",
            ConfirmarPassword = "Password123!",
            Rol = "Mesero",
            NivelAcceso = 5,
            Telefono = _faker.Phone.PhoneNumber("+569########"),
            UsuarioCreadorId = _usuarioCreadorId.Value
        };
    }

    /// <summary>
    /// Construye un usuario administrador válido
    /// </summary>
    public CrearUsuarioCommand BuildUsuarioAdministrador()
    {
        if (!_usuarioCreadorId.HasValue || _usuarioCreadorId.Value == Guid.Empty)
        {
            throw new InvalidOperationException("UsuarioCreadorId es requerido y debe ser un GUID válido. Use ConUsuarioCreadorId() para especificarlo.");
        }

        return new CrearUsuarioCommand
        {
            NombreUsuario = _faker.Internet.UserName(),
            NombreCompleto = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            Password = "Password123!",
            ConfirmarPassword = "Password123!",
            Rol = "Administrador",
            NivelAcceso = 8,
            Telefono = _faker.Phone.PhoneNumber("+569########"),
            UsuarioCreadorId = _usuarioCreadorId.Value
        };
    }

    /// <summary>
    /// Construye un usuario con datos inválidos para tests de validación
    /// </summary>
    public CrearUsuarioCommand BuildUsuarioInvalido()
    {
        return new CrearUsuarioCommand
        {
            NombreUsuario = "", // Inválido: vacío
            NombreCompleto = "", // Inválido: vacío
            Email = "email-invalido", // Inválido: formato incorrecto
            Password = "123", // Inválido: muy corto
            ConfirmarPassword = "456", // Inválido: no coincide
            Rol = "RolInexistente", // Inválido: rol que no existe
            NivelAcceso = 15, // Inválido: nivel muy alto
            Telefono = "telefono-invalido", // Inválido: formato incorrecto
            UsuarioCreadorId = Guid.Empty // Inválido: ID vacío
        };
    }

    /// <summary>
    /// Resetea el builder a sus valores por defecto
    /// </summary>
    public UsuarioTestDataBuilder Reset()
    {
        _nombreUsuario = null;
        _nombreCompleto = null;
        _email = null;
        _password = null;
        _confirmarPassword = null;
        _rol = null;
        _nivelAcceso = null;
        _telefono = null;
        _usuarioCreadorId = null;
        _requiereAprobacion = null;
        _usuarioCambiadorId = null;
        _usuarioReseteadorId = null;
        return this;
    }

    /// <summary>
    /// Crea una nueva instancia del builder
    /// </summary>
    public static UsuarioTestDataBuilder Nuevo()
    {
        return new UsuarioTestDataBuilder();
    }
} 