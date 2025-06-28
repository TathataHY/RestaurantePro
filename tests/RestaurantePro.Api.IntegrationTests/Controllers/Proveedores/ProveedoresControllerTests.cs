using RestaurantePro.Application.Proveedores.Proveedores.Commands.CrearProveedor;
using RestaurantePro.Application.Proveedores.Proveedores.Commands.ActualizarProveedor;
using RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.AgregarContacto;
using RestaurantePro.Application.Proveedores.ContactosProveedor.Commands.ActualizarContacto;
using RestaurantePro.Application.Proveedores.Proveedores.DTOs;
using RestaurantePro.Api.Common;
using System.Text.Json;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Proveedores;

/// <summary>
/// Tests de integración COMPLETOS para ProveedoresController
/// Valida todos los endpoints REST con interacción real de BD y lógica de negocio
/// </summary>
[Collection("Sequential")]
public class ProveedoresControllerTests : ApiIntegrationTestBase, IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public ProveedoresControllerTests() : base(new TestWebApplicationFactory())
    {
        _factory = (TestWebApplicationFactory)Factory;
    }

    #region Obtener Proveedores (GET /api/proveedores)

    [Fact]
    public async Task ObtenerProveedores_SinDatos_DebeRetornarListaVacia()
    {
        // Arrange
        await LimpiarProveedores();
        var url = "/api/proveedores?pageSize=10";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ProveedorDto>>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Items.Should().BeEmpty();
        apiResponse.Data.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task ObtenerProveedores_ConProveedoresEnBD_DebeRetornarListaCompleta()
    {
        // Arrange
        await LimpiarProveedores();
        
        // Debug: Verificar si hay datos en la BD
        var debugResponse = await HttpClient.GetAsync("/api/proveedores?pageSize=100");
        if (debugResponse.IsSuccessStatusCode)
        {
            var debugApiResponse = await debugResponse.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ProveedorDto>>>();
            Console.WriteLine($"🔍 DEBUG: Hay {debugApiResponse!.Data.TotalCount} proveedores en la BD antes del test");
            if (debugApiResponse.Data.TotalCount > 0)
            {
                foreach (var p in debugApiResponse.Data.Items.Take(3))
                {
                    Console.WriteLine($"🔍 DEBUG: Proveedor existente: {p.Nombre} - RUT: {p.RUT}");
                }
            }
        }
        
        var proveedor1 = await CrearProveedorTestAsync("Proveedor 1", "proveedor1@test.cl", "Santiago");
        Console.WriteLine($"🔍 DEBUG: Primer proveedor creado: {proveedor1?.Nombre} - ID: {proveedor1?.Id} - RUT: {proveedor1?.RUT}");
        
        // Debug: Verificar si el primer proveedor se creó
        var debugResponse1 = await HttpClient.GetAsync("/api/proveedores?pageSize=100");
        if (debugResponse1.IsSuccessStatusCode)
        {
            var debugApiResponse1 = await debugResponse1.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ProveedorDto>>>();
            Console.WriteLine($"🔍 DEBUG: Después del primer proveedor: {debugApiResponse1!.Data.TotalCount} proveedores en la BD");
            if (debugApiResponse1.Data.TotalCount > 0)
            {
                foreach (var p in debugApiResponse1.Data.Items)
                {
                    Console.WriteLine($"🔍 DEBUG: Proveedor en BD: {p.Nombre} - RUT: {p.RUT}");
                }
            }
        }
        
        var proveedor2 = await CrearProveedorTestAsync("Proveedor 2", "proveedor2@test.cl", "Valparaíso");
        var url = "/api/proveedores?pageSize=10";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ProveedorDto>>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Items.Should().HaveCount(2);
        apiResponse.Data.TotalCount.Should().Be(2);
        
        var proveedorIds = apiResponse.Data.Items.Select(p => p.Id).ToList();
        proveedorIds.Should().Contain(proveedor1.Id);
        proveedorIds.Should().Contain(proveedor2.Id);
    }

    [Fact]
    public async Task ObtenerProveedores_ConFiltros_DebeRetornarSoloCoincidencias()
    {
        // Arrange
        await LimpiarProveedores();
        var proveedorSantiago = await CrearProveedorTestAsync("Proveedor Santiago", "santiago@test.cl", "Santiago");
        var proveedorValparaiso = await CrearProveedorTestAsync("Proveedor Valparaíso", "valparaiso@test.cl", "Valparaíso");
        
        var url = "/api/proveedores?ciudad=Santiago&pageSize=10";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ProveedorDto>>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Items.Should().HaveCount(1);
        apiResponse.Data.Items.First().Ciudad.Should().Be("Santiago");
        apiResponse.Data.Items.First().Id.Should().Be(proveedorSantiago.Id);
    }

    [Fact]
    public async Task ObtenerProveedores_ConBusqueda_DebeRetornarCoincidenciasPorNombre()
    {
        // Arrange
        await LimpiarProveedores();
        var proveedorAlimentos = await CrearProveedorTestAsync("Alimentos Frescos S.A.", "alimentos@test.cl", "Santiago");
        var proveedorLacteos = await CrearProveedorTestAsync("Lácteos del Sur", "lacteos@test.cl", "Temuco");
        
        var url = "/api/proveedores?terminoBusqueda=Alimentos&pageSize=10";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ProveedorDto>>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Items.Should().HaveCount(1);
        apiResponse.Data.Items.First().Nombre.Should().Contain("Alimentos");
    }

    [Fact]
    public async Task ObtenerProveedores_ConPaginacion_DebeRespetarParametros()
    {
        // Arrange
        await LimpiarProveedores();
        for (int i = 1; i <= 5; i++)
        {
            await CrearProveedorTestAsync($"Proveedor {i:D2}", $"proveedor{i:D2}@test.cl", "Santiago");
        }
        
        var url = "/api/proveedores?pageNumber=2&pageSize=2";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<PaginatedList<ProveedorDto>>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Items.Should().HaveCount(2); // Página 2 con 2 elementos
        apiResponse.Data.TotalCount.Should().Be(5);
        apiResponse.Data.PageNumber.Should().Be(2);
        apiResponse.Data.PageSize.Should().Be(2);
    }

    #endregion

    #region Obtener Proveedor Por ID (GET /api/proveedores/{id})

    [Fact]
    public async Task ObtenerProveedor_ConIdExistente_DebeRetornarProveedor()
    {
        // Arrange
        var proveedorCreado = await CrearProveedorTestAsync("Proveedor Test", "test@provider.cl", "Santiago");
        var url = $"/api/proveedores/{proveedorCreado.Id}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProveedorDto>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().Be(proveedorCreado.Id);
        apiResponse.Data.Nombre.Should().Be("Proveedor Test");
        apiResponse.Data.Email.Should().Be("test@provider.cl");
        apiResponse.Data.Ciudad.Should().Be("Santiago");
    }

    [Fact]
    public async Task ObtenerProveedor_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var url = $"/api/proveedores/{idInexistente}";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().Contain(e => e.Contains("no fue encontrado"));
    }

    [Fact]
    public async Task ObtenerProveedor_ConContactos_DebeIncluirContactos()
    {
        // Arrange
        var proveedorCreado = await CrearProveedorTestAsync("Proveedor Con Contactos", "contactos@provider.cl", "Santiago");
        await AgregarContactoTestAsync(proveedorCreado.Id, "Juan Pérez", "Gerente", "juan@provider.cl");
        
        var url = $"/api/proveedores/{proveedorCreado.Id}?incluirContactos=true";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProveedorDto>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Contactos.Should().NotBeNull();
        apiResponse.Data.Contactos.Should().HaveCount(1);
        apiResponse.Data.Contactos.First().Nombre.Should().Be("Juan Pérez");
        apiResponse.Data.Contactos.First().Email.Should().Be("juan@provider.cl");
    }

    #endregion

    #region Crear Proveedor (POST /api/proveedores)

    [Fact]
    public async Task CrearProveedor_ConDatosValidos_DebeCrearEnBDYRetornarCreated()
    {
        // Arrange
        var url = "/api/proveedores";
        var command = new CrearProveedorCommand
        {
            Nombre = "Nuevo Proveedor S.A.",
            NombreContacto = "Pedro González",
            Email = "pedro@nuevoproveedor.cl",
            Telefono = "+56 2 2345 6789",
            Direccion = "Av. Providencia 1234",
            Ciudad = "Santiago",
            Pais = "Chile",
            RFC = "12345678-90",
            RUT = "12345678-9",
            InformacionBancaria = "Banco de Chile, Cuenta Corriente 12345678",
            DiasCredito = 30,
            UsuarioId = Guid.NewGuid()
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Debug: Si hay error, mostrar detalles
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error 500 detected. Response content: {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProveedorDto>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().NotBeEmpty();
        apiResponse.Data.Nombre.Should().Be("Nuevo Proveedor S.A.");
        apiResponse.Data.Email.Should().Be("pedro@nuevoproveedor.cl");
        apiResponse.Data.DiasCredito.Should().Be(30);
        
        // Verificar que se creó en BD
        var verifyResponse = await HttpClient.GetAsync($"/api/proveedores/{apiResponse.Data.Id}");
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CrearProveedor_ConEmailDuplicado_DebeRetornarBadRequest()
    {
        // Arrange
        await LimpiarProveedores();
        var proveedorExistente = await CrearProveedorTestAsync("Proveedor Existente", "duplicado@test.cl", "Santiago");
        
        var url = "/api/proveedores";
        var command = new CrearProveedorCommand
        {
            Nombre = "Otro Proveedor",
            NombreContacto = "Ana María",
            Email = "duplicado@test.cl", // Email duplicado
            Telefono = "+56 2 9876 5432",
            Direccion = "Otra dirección",
            Ciudad = "Valparaíso",
            RFC = "98765432-1"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Errors.Should().Contain(e => e.Contains("email"));
    }

    [Fact]
    public async Task CrearProveedor_ConDatosIncompletos_DebeRetornarBadRequest()
    {
        // Arrange
        var url = "/api/proveedores";
        var command = new CrearProveedorCommand
        {
            // Faltan campos obligatorios como Nombre, Email, etc.
            Ciudad = "Santiago"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Actualizar Proveedor (PUT /api/proveedores/{id})

    [Fact]
    public async Task ActualizarProveedor_ConDatosValidos_DebeActualizarEnBD()
    {
        // Arrange
        var proveedorCreado = await CrearProveedorTestAsync("Proveedor Original", "original@test.cl", "Santiago");
        var url = $"/api/proveedores/{proveedorCreado.Id}";
        var command = new ActualizarProveedorCommand
        {
            Nombre = "Proveedor Actualizado",
            Email = "actualizado@test.cl",
            Telefono = "+56 2 9999 8888",
            Activo = true
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProveedorDto>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Nombre.Should().Be("Proveedor Actualizado");
        apiResponse.Data.Email.Should().Be("actualizado@test.cl");
        
        // Verificar que se actualizó en BD
        var verifyResponse = await HttpClient.GetAsync($"/api/proveedores/{proveedorCreado.Id}");
        var verifyData = await response.Content.ReadFromJsonAsync<ApiResponse<ProveedorDto>>();
        verifyData!.Data.Nombre.Should().Be("Proveedor Actualizado");
    }

    [Fact]
    public async Task ActualizarProveedor_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var url = $"/api/proveedores/{idInexistente}";
        var command = new ActualizarProveedorCommand
        {
            Nombre = "Proveedor Inexistente",
            Email = "inexistente@test.cl"
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Eliminar Proveedor (DELETE /api/proveedores/{id})

    [Fact]
    public async Task EliminarProveedor_ConIdExistente_DebeDesactivarEnBD()
    {
        // Arrange
        var proveedorCreado = await CrearProveedorTestAsync("Proveedor A Eliminar", "eliminar@test.cl", "Santiago");
        var url = $"/api/proveedores/{proveedorCreado.Id}";
        var request = new { RazonDesactivacion = "Test de eliminación" };

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = JsonContent.Create(request)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();
    }

    [Fact]
    public async Task EliminarProveedor_ConIdInexistente_DebeRetornar404()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        var url = $"/api/proveedores/{idInexistente}";

        // Act
        var response = await HttpClient.DeleteAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Gestión de Contactos

    [Fact]
    public async Task ObtenerContactosProveedor_ConProveedorExistente_DebeRetornarContactos()
    {
        // Arrange
        var proveedorCreado = await CrearProveedorTestAsync("Proveedor Con Contactos", "contactos@test.cl", "Santiago");
        await AgregarContactoTestAsync(proveedorCreado.Id, "María López", "Vendedora", "maria@test.cl");
        await AgregarContactoTestAsync(proveedorCreado.Id, "Carlos Ruiz", "Administrador", "carlos@test.cl");
        
        var url = $"/api/proveedores/{proveedorCreado.Id}/contactos";

        // Act
        var response = await HttpClient.GetAsync(url);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ContactoProveedorDto>>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().HaveCount(2);
        apiResponse.Data.Should().Contain(c => c.Nombre == "María López");
        apiResponse.Data.Should().Contain(c => c.Nombre == "Carlos Ruiz");
    }

    [Fact]
    public async Task AgregarContactoProveedor_ConDatosValidos_DebeCrearContactoEnBD()
    {
        // Arrange
        var proveedorCreado = await CrearProveedorTestAsync("Proveedor Test", "test@provider.cl", "Santiago");
        var url = $"/api/proveedores/{proveedorCreado.Id}/contactos";
        var command = new AgregarContactoCommand
        {
            Nombre = "Juan",
            Apellidos = "Pérez",
            Cargo = "Gerente de Ventas",
            Email = "juan.perez@test.cl",
            Telefono = "+56 9 8765 4321",
            EsPrincipal = false
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Debug: Mostrar error si falla
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"🔍 DEBUG: Error al agregar contacto: {response.StatusCode} - {errorContent}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ContactoProveedorDto>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Nombre.Should().Be("Juan");
        apiResponse.Data.Email.Should().Be("juan.perez@test.cl");
    }

    [Fact]
    public async Task AgregarContactoProveedor_ConProveedorInexistente_DebeRetornar404()
    {
        // Arrange
        var proveedorInexistente = Guid.NewGuid();
        var url = $"/api/proveedores/{proveedorInexistente}/contactos";
        var command = new AgregarContactoCommand
        {
            Nombre = "Contacto Inexistente",
            Email = "inexistente@test.cl",
            Cargo = "Test"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActualizarContactoProveedor_ConDatosValidos_DebeActualizarEnBD()
    {
        // Arrange
        var proveedorCreado = await CrearProveedorTestAsync("Proveedor Test", "test@provider.cl", "Santiago");
        var contactoCreado = await AgregarContactoTestAsync(proveedorCreado.Id, "Roberto Silva", "Vendedor", "roberto@test.cl");
        
        var url = $"/api/proveedores/{proveedorCreado.Id}/contactos/{contactoCreado.Id}";
        var command = new ActualizarContactoCommand
        {
            Nombre = "Roberto",
            Apellidos = "Silva Actualizado",
            Cargo = "Gerente de Ventas",
            Email = "roberto.silva@test.cl",
            Activo = true
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync(url, command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ContactoProveedorDto>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Nombre.Should().Be("Roberto");
        apiResponse.Data.Email.Should().Be("roberto.silva@test.cl");
    }

    [Fact]
    public async Task EliminarContactoProveedor_ConIdExistente_DebeEliminarDeDB()
    {
        // Arrange
        var proveedorCreado = await CrearProveedorTestAsync("Proveedor Test", "test@provider.cl", "Santiago");
        var contactoCreado = await AgregarContactoTestAsync(proveedorCreado.Id, "Eliminar Contact", "Test", "eliminar@test.cl");
        
        var url = $"/api/proveedores/{proveedorCreado.Id}/contactos/{contactoCreado.Id}";
        var request = new { MotivoEliminacion = "Test de eliminación" };

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Delete, url)
        {
            Content = JsonContent.Create(request)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();
    }

    #endregion

    #region Activar/Desactivar Proveedor

    [Fact]
    public async Task ActivarProveedor_ConIdExistente_DebeRetornarOK()
    {
        // Arrange
        var proveedorCreado = await CrearProveedorTestAsync("Proveedor A Activar", "activar@test.cl", "Santiago");
        var url = $"/api/proveedores/{proveedorCreado.Id}/activar";

        // Act
        var response = await HttpClient.PatchAsync(url, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();
    }

    [Fact]
    public async Task DesactivarProveedor_ConMotivo_DebeRetornarOK()
    {
        // Arrange
        var proveedorCreado = await CrearProveedorTestAsync("Proveedor A Desactivar", "desactivar@test.cl", "Santiago");
        var url = $"/api/proveedores/{proveedorCreado.Id}/desactivar";
        var request = new { RazonDesactivacion = "Cambio de proveedor" };

        // Act
        var response = await HttpClient.PatchAsJsonAsync(url, request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data.Should().BeTrue();
    }

    #endregion

    #region Métodos de Ayuda

    private async Task<ProveedorDto> CrearProveedorTestAsync(string nombre, string email, string ciudad)
    {
        // Generar un RUT completamente único usando timestamp + GUID
        await Task.Delay(10); // Pequeño delay para asegurar timestamps únicos
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var guid = Guid.NewGuid().ToString("N");
        var digitos = guid.Where(char.IsDigit).ToArray();
        
        // Combinar timestamp (últimos 4 dígitos) + dígitos del GUID
        var timestampDigitos = (timestamp % 10000).ToString("D4");
        var guidDigitos = new string(digitos.Take(4).ToArray());
        var numeroBase = timestampDigitos + guidDigitos;
        
        var digito = digitos.Length > 4 ? digitos[4].ToString() : "K";
        var rut = $"{numeroBase}-{digito}";
        
        // Generar RFC único también para evitar conflictos
        var rfcDigitos = new string(digitos.Skip(5).Take(8).ToArray());
        var rfcDigito = digitos.Length > 13 ? digitos[13].ToString() : "A";
        var rfc = $"{rfcDigitos}-{rfcDigito}";

        var command = new CrearProveedorCommand
        {
            Nombre = nombre,
            NombreContacto = $"Contacto de {nombre}",
            Email = email,
            Telefono = "+56 2 1234 5678",
            Direccion = $"Dirección en {ciudad}",
            Ciudad = ciudad,
            Pais = "Chile",
            RFC = rfc,
            RUT = rut,
            InformacionBancaria = "Banco de Chile, Cuenta Corriente 12345678",
            DiasCredito = 0
        };

        var response = await HttpClient.PostAsJsonAsync("/api/proveedores", command);
        
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al crear proveedor: {response.StatusCode} - {errorContent}");
        }
        var apiResponse = await DeserializarResponse<ProveedorDto>(response);
        return apiResponse.Data;
    }

    private async Task<ContactoProveedorDto> AgregarContactoTestAsync(Guid proveedorId, string nombre, string cargo, string email)
    {
        var command = new AgregarContactoCommand
        {
            Nombre = nombre,
            Apellidos = "Apellido Test",
            Cargo = cargo,
            Email = email,
            Telefono = "+56 9 8765 4321",
            EsPrincipal = false
        };

        var response = await HttpClient.PostAsJsonAsync($"/api/proveedores/{proveedorId}/contactos", command);
        response.EnsureSuccessStatusCode();
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ContactoProveedorDto>>();
        return apiResponse!.Data;
    }

    private async Task LimpiarProveedores()
    {
        // Usar la limpieza completa de la clase base
        await LimpiarBaseDeDatosCompletamente();
    }

    private static async Task<ApiResponse<T>> DeserializarResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new ApiResponse<T> { Success = false, Data = default };
    }

    #endregion

    public new void Dispose()
    {
        _factory?.Dispose();
        base.Dispose();
    }
} 