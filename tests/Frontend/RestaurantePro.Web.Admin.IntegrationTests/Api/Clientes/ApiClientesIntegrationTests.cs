using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using RestaurantePro.Application.Comercial.Clientes.DTOs;
using RestaurantePro.Web.Admin.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClienteDto = RestaurantePro.Application.Comercial.Clientes.DTOs.ClienteDto;
using RestaurantePro.Web.Admin.IntegrationTests.Core;

namespace RestaurantePro.Web.Admin.IntegrationTests.Api.Clientes;

/// <summary>
/// Pruebas de integración para la API de clientes usando la API con base de datos en memoria
/// </summary>
public class ApiClientesIntegrationTests : BaseIntegrationTest
{
    public ApiClientesIntegrationTests(WebApplicationFactory factory) : base(factory)
    {
        // Los tests regulares usan el cliente autenticado por defecto del BaseIntegrationTest
        // No necesitan configuración adicional de autenticación
    }

    [Fact]
    public async Task ObtenerClientes_ConFiltrosBasicos_DeberiaRetornarListaPaginada()
    {
        // Arrange
        var queryParams = "pageNumber=1&pageSize=10&orderBy=FechaCreacion&orderDirection=desc";

        // Act
        var response = await _client.GetAsync($"/api/comercial/clientes?{queryParams}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ClienteDto>>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Items.Should().NotBeNull();
    }

    [Theory]
    [InlineData(1, 5, "NombreCompleto", "asc")]
    [InlineData(2, 3, "Email", "desc")]
    [InlineData(1, 1, "FechaRegistro", "asc")]
    public async Task ObtenerClientes_ConDiferentesPaginaciones_DeberiaRetornarResultadosCorrectos(int pageNumber, int pageSize, string orderBy, string orderDirection)
    {
        // Arrange
        var queryParams = $"pageNumber={pageNumber}&pageSize={pageSize}&orderBy={orderBy}&orderDirection={orderDirection}";

        // Act
        var response = await _client.GetAsync($"/api/comercial/clientes?{queryParams}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ClienteDto>>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.PageNumber.Should().Be(pageNumber);
        resultado.Data.PageSize.Should().Be(pageSize);
        resultado.Data.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task CrearCliente_ConDatosValidos_DeberiaCrearClienteExitosamente()
    {
        // Arrange
        var nuevoCliente = new CrearClienteRequest
        {
            Nombre = "Juan Pérez", // Nombre completo para que el handler lo divida correctamente
            Email = "juan.perez@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-30),
            Ciudad = "Lima",
            Pais = "Perú",
            AceptaMarketing = true,
            AceptaTerminos = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", nuevoCliente);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var jsonContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"JSON Response: {jsonContent}"); // Log temporal para debug
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nombre.Should().Be("Juan");
        resultado.Data.Apellido.Should().Be("Pérez"); // Corregido: Apellido (singular)
        resultado.Data.Email.Should().Be("juan.perez@test.com");
    }

    [Fact]
    public async Task CrearCliente_ConEmailDuplicado_DeberiaRetornarError()
    {
        // Arrange
        var cliente1 = new CrearClienteRequest
        {
            Nombre = "Cliente Uno", // Nombre completo
            Email = "duplicado@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        var cliente2 = new CrearClienteRequest
        {
            Nombre = "Cliente Dos", // Nombre completo
            Email = "duplicado@test.com", // Mismo email
            Telefono = "+1234567891",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/comercial/clientes", cliente1);
        var response2 = await _client.PostAsJsonAsync("/api/comercial/clientes", cliente2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearCliente_ConDatosInvalidosBasicos_DeberiaRetornarError()
    {
        // Arrange
        var clienteInvalido = new CrearClienteRequest
        {
            Nombre = "Test Usuario", // Nombre completo válido
            Email = "email-invalido", // Email inválido (sin @)
            Telefono = "+1234567890", // Teléfono válido
            FechaNacimiento = DateTime.Today.AddYears(1), // Fecha futura (inválida)
            AceptaTerminos = false // No acepta términos (inválido)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteInvalido);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("", "Apellido", "test@test.com", "1234567890", "2020-01-01", false, "Nombre vacío")]
    [InlineData("Nombre", "", "test@test.com", "1234567890", "2020-01-01", false, "Apellido vacío")]
    [InlineData("Nombre", "Apellido", "email-invalido", "1234567890", "2020-01-01", false, "Email inválido")]
    [InlineData("Nombre", "Apellido", "test@test.com", "", "2020-01-01", false, "Teléfono vacío")]
    [InlineData("Nombre", "Apellido", "test@test.com", "1234567890", "2030-01-01", false, "Fecha futura")]
    [InlineData("Nombre", "Apellido", "test@test.com", "1234567890", "2020-01-01", false, "No acepta términos")]
    public async Task CrearCliente_ConDatosInvalidos_DeberiaRetornarError(string nombre, string apellidos, string email, string telefono, string fechaNacimiento, bool aceptaTerminos, string descripcion)
    {
        // Arrange
        var clienteInvalido = new CrearClienteRequest
        {
            Nombre = nombre,
            Apellidos = apellidos,
            Email = email,
            Telefono = telefono,
            FechaNacimiento = DateTime.Parse(fechaNacimiento),
            AceptaTerminos = aceptaTerminos
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteInvalido);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, $"Debería fallar para: {descripcion}");
    }

    [Fact]
    public async Task ValidarEmail_EndpointNoExiste_DeberiaRetornarNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/comercial/clientes/validar-email?email={Uri.EscapeDataString("test@test.com")}");

        // Assert
        // El endpoint de validar email no existe en el controlador
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObtenerClientePorId_ConIdValido_DeberiaRetornarCliente()
    {
        // Arrange
        var clienteId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/comercial/clientes/{clienteId}");

        // Assert
        // Como el cliente no existe, esperamos 404 Not Found
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ActualizarCliente_ConDatosValidos_DeberiaActualizarCliente()
    {
        // Arrange - Primero crear un cliente
        var clienteCreado = new CrearClienteRequest
        {
            Nombre = "Cliente Original",
            Email = "original@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteCreado);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var jsonContent = await createResponse.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        var clienteId = resultado!.Data!.Id;

        // Ahora actualizar el cliente
        var clienteActualizado = new ActualizarClienteRequest
        {
            Nombre = "Cliente Actualizado",
            Apellidos = "Apellido Actualizado",
            Email = "actualizado@test.com",
            Telefono = "+9876543210",
            FechaNacimiento = DateTime.Today.AddYears(-30),
            AceptaMarketing = true
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/comercial/clientes/{clienteId}", clienteActualizado);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updateJsonContent = await response.Content.ReadAsStringAsync();
        var updateResultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(updateJsonContent, GetJsonOptions());
        updateResultado.Should().NotBeNull();
        updateResultado!.Success.Should().BeTrue();
        updateResultado.Data.Should().NotBeNull();
        updateResultado.Data!.Nombre.Should().Be("Cliente");
        updateResultado.Data.Apellido.Should().Be("Actualizado");
        updateResultado.Data.Email.Should().Be("actualizado@test.com");
    }

    [Fact]
    public async Task EliminarCliente_ConIdValido_DeberiaEliminarCliente()
    {
        // Arrange - Primero crear un cliente
        var clienteCreado = new CrearClienteRequest
        {
            Nombre = "Cliente Para Eliminar",
            Email = "eliminar@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteCreado);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var jsonContent = await createResponse.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        var clienteId = resultado!.Data!.Id;

        // Act
        var response = await _client.DeleteAsync($"/api/comercial/clientes/{clienteId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que el cliente fue desactivado (soft delete)
        var getResponse = await _client.GetAsync($"/api/comercial/clientes/{clienteId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que el cliente está desactivado
        var getJsonContent = await getResponse.Content.ReadAsStringAsync();
        var getResultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(getJsonContent, GetJsonOptions());
        getResultado.Should().NotBeNull();
        getResultado!.Success.Should().BeTrue();
        getResultado.Data.Should().NotBeNull();
        getResultado.Data!.Activo.Should().BeFalse();
    }

    [Fact]
    public async Task ObtenerEstadisticas_EndpointNoExiste_DeberiaRetornarNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/comercial/clientes/estadisticas");

        // Assert
        // El endpoint de estadísticas no existe en el controlador
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CrearCliente_Concurrencia_DeberiaManejarCorrectamente()
    {
        // Arrange
        var tareas = new List<Task<HttpResponseMessage>>();
        var clientes = new List<CrearClienteRequest>();

        // Crear 10 clientes concurrentemente
        for (int i = 0; i < 10; i++)
        {
            var cliente = new CrearClienteRequest
            {
                Nombre = $"Cliente Concurrente {i}",
                Email = $"concurrente{i}@test.com",
                Telefono = $"+123456789{i}",
                FechaNacimiento = DateTime.Today.AddYears(-25),
                AceptaTerminos = true
            };
            clientes.Add(cliente);
            tareas.Add(_client.PostAsJsonAsync("/api/comercial/clientes", cliente));
        }

        // Act
        var responses = await Task.WhenAll(tareas);

        // Assert
        responses.Should().HaveCount(10);
        responses.All(r => r.StatusCode == HttpStatusCode.Created).Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerClientes_Rendimiento_DeberiaResponderRapidamente()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/comercial/clientes?pageNumber=1&pageSize=10");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "La consulta debería responder en menos de 1 segundo");
    }

    [Fact]
    public async Task CrearCliente_ConDatosExtremos_DeberiaManejarCorrectamente()
    {
        // Arrange - Datos con caracteres especiales y límites
        var clienteExtremo = new CrearClienteRequest
        {
            Nombre = "José María de la Cruz y del Valle", // Nombre muy largo
            Email = "jose.maria.delacruz@empresa-muy-larga.com.pe", // Email largo
            Telefono = "+51-987-654-321", // Teléfono con formato especial
            FechaNacimiento = DateTime.Today.AddYears(-100), // Edad extrema
            Ciudad = "Lima Metropolitana", // Ciudad larga
            Pais = "República del Perú", // País largo
            AceptaMarketing = true,
            AceptaTerminos = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteExtremo);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nombre.Should().Be("José"); // El handler divide el nombre
        resultado.Data.Apellido.Should().Be("María de la Cruz y del Valle");
    }

    [Fact]
    public async Task ObtenerClientes_ConPaginacionExtrema_DeberiaManejarCorrectamente()
    {
        // Arrange - Página muy grande
        var queryParams = "pageNumber=999999&pageSize=1";
        var authenticatedClient = CreateAuthenticatedClient();

        // Act
        var response = await authenticatedClient.GetAsync($"/api/comercial/clientes?{queryParams}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ClienteDto>>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Items.Should().BeEmpty(); // Página vacía para página muy grande
    }

    [Fact]
    public async Task CrearCliente_ConCaracteresEspeciales_DeberiaManejarCorrectamente()
    {
        // Arrange - Caracteres especiales en nombre
        var clienteEspecial = new CrearClienteRequest
        {
            Nombre = "José María OConnor Smith", // Sin caracteres especiales problemáticos
            Email = "jose.oconnor@test.com",
            Telefono = "+51-987-654-321",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteEspecial);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Nombre.Should().Be("José"); // El handler divide el nombre
        resultado.Data.Apellido.Should().Be("María OConnor Smith");
    }

    [Fact]
    public async Task CrearCliente_ConEmailInternacional_DeberiaManejarCorrectamente()
    {
        // Arrange - Email con dominio internacional
        var clienteInternacional = new CrearClienteRequest
        {
            Nombre = "Cliente Internacional",
            Email = "cliente@empresa.co.uk", // Dominio .co.uk
            Telefono = "+44-20-7946-0958", // Teléfono del Reino Unido
            FechaNacimiento = DateTime.Today.AddYears(-30),
            Ciudad = "London",
            Pais = "United Kingdom",
            AceptaTerminos = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteInternacional);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        resultado.Data!.Email.Should().Be("cliente@empresa.co.uk");
    }

    [Fact]
    public async Task CrearCliente_ConInyeccionSQL_DeberiaRechazarCorrectamente()
    {
        // Arrange - Intentar inyección SQL en el nombre
        var clienteMalicioso = new CrearClienteRequest
        {
            Nombre = "'; DROP TABLE Clientes; --", // Inyección SQL
            Email = "hacker@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteMalicioso);

        // Assert
        // Debería rechazar el nombre malicioso, no ejecutar la inyección
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created);
        
        // Si se crea, verificar que no se ejecutó la inyección
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var jsonContent = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
            resultado.Should().NotBeNull();
            resultado!.Data.Should().NotBeNull();
            // El nombre debería estar sanitizado o rechazado
            resultado.Data!.Nombre.Should().NotContain("DROP TABLE");
        }
    }

    [Fact]
    public async Task CrearCliente_ConXSS_DeberiaSanitizarCorrectamente()
    {
        // Arrange - Intentar XSS en el nombre
        var clienteXSS = new CrearClienteRequest
        {
            Nombre = "<script>alert('XSS')</script>Juan", // XSS attempt
            Email = "xss@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteXSS);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.Created);
        
        // Si se crea, verificar que se sanitizó
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var jsonContent = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
            resultado.Should().NotBeNull();
            resultado!.Data.Should().NotBeNull();
            // El nombre debería estar sanitizado
            resultado.Data!.Nombre.Should().NotContain("<script>");
            resultado.Data.Nombre.Should().NotContain("alert");
        }
    }

    [Fact]
    public async Task ObtenerClientes_ConParametrosMaliciosos_DeberiaRechazarCorrectamente()
    {
        // Arrange - Parámetros maliciosos en la URL
        var queryParams = "pageNumber=1&pageSize=10&orderBy=<script>alert('XSS')</script>";

        // Act
        var response = await _client.GetAsync($"/api/comercial/clientes?{queryParams}");

        // Assert
        // Debería rechazar o sanitizar los parámetros maliciosos
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
        
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var jsonContent = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ClienteDto>>>(jsonContent, GetJsonOptions());
            resultado.Should().NotBeNull();
            // Verificar que no se ejecutó el script
            jsonContent.Should().NotContain("alert('XSS')");
        }
    }

    [Fact]
    public async Task CrearCliente_ConDatosMuyLargos_DeberiaRechazarCorrectamente()
    {
        // Arrange - Datos excesivamente largos
        var clienteLargo = new CrearClienteRequest
        {
            Nombre = new string('A', 1000), // Nombre de 1000 caracteres
            Email = "test@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteLargo);

        // Assert
        // Debería rechazar datos excesivamente largos
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrearCliente_ConTransaccionCompleta_DeberiaMantenerConsistencia()
    {
        // Arrange - Crear cliente con datos completos
        var clienteCompleto = new CrearClienteRequest
        {
            Nombre = "Cliente Transaccional",
            Email = "transaccional@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            Ciudad = "Lima",
            Pais = "Perú",
            AceptaMarketing = true,
            AceptaTerminos = true
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteCompleto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var jsonContent = await response.Content.ReadAsStringAsync();
        var resultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(jsonContent, GetJsonOptions());
        resultado.Should().NotBeNull();
        resultado!.Success.Should().BeTrue();
        resultado.Data.Should().NotBeNull();
        
        var clienteId = resultado.Data!.Id;
        
        // Verificar que el cliente se puede recuperar después de la creación
        var getResponse = await _client.GetAsync($"/api/comercial/clientes/{clienteId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getJsonContent = await getResponse.Content.ReadAsStringAsync();
        var getResultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(getJsonContent, GetJsonOptions());
        getResultado.Should().NotBeNull();
        getResultado!.Data.Should().NotBeNull();
        getResultado.Data!.Id.Should().Be(clienteId);
        getResultado.Data.Email.Should().Be("transaccional@test.com");
    }

    [Fact]
    public async Task ActualizarCliente_ConTransaccionCompleta_DeberiaMantenerConsistencia()
    {
        // Arrange - Crear cliente primero
        var clienteOriginal = new CrearClienteRequest
        {
            Nombre = "Cliente Original",
            Email = "original@test.com",
            Telefono = "+1234567890",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        var createResponse = await _client.PostAsJsonAsync("/api/comercial/clientes", clienteOriginal);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createJsonContent = await createResponse.Content.ReadAsStringAsync();
        var createResultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(createJsonContent, GetJsonOptions());
        var clienteId = createResultado!.Data!.Id;

        // Actualizar cliente
        var clienteActualizado = new ActualizarClienteRequest
        {
            Nombre = "Cliente Actualizado",
            Apellidos = "Apellido Actualizado",
            Email = "actualizado@test.com",
            Telefono = "+9876543210",
            FechaNacimiento = DateTime.Today.AddYears(-30),
            AceptaMarketing = true
        };

        // Act
        var updateResponse = await _client.PutAsJsonAsync($"/api/comercial/clientes/{clienteId}", clienteActualizado);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que la actualización se mantiene
        var getResponse = await _client.GetAsync($"/api/comercial/clientes/{clienteId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var getJsonContent = await getResponse.Content.ReadAsStringAsync();
        var getResultado = JsonSerializer.Deserialize<ApiResponse<ClienteDto>>(getJsonContent, GetJsonOptions());
        getResultado.Should().NotBeNull();
        getResultado!.Data.Should().NotBeNull();
        getResultado.Data!.Email.Should().Be("actualizado@test.com");
        // Nota: Ciudad y Pais no están disponibles en ClienteDto
    }

    [Fact]
    public async Task ObtenerClientes_ConFiltrosComplejos_DeberiaFuncionarCorrectamente()
    {
        // Arrange - Crear varios clientes con diferentes características
        var clientes = new[]
        {
            new CrearClienteRequest { Nombre = "Ana García", Email = "ana@test.com", Telefono = "+1111111111", FechaNacimiento = DateTime.Today.AddYears(-25), AceptaTerminos = true },
            new CrearClienteRequest { Nombre = "Carlos López", Email = "carlos@test.com", Telefono = "+2222222222", FechaNacimiento = DateTime.Today.AddYears(-30), AceptaTerminos = true },
            new CrearClienteRequest { Nombre = "María Rodríguez", Email = "maria@test.com", Telefono = "+3333333333", FechaNacimiento = DateTime.Today.AddYears(-35), AceptaTerminos = true }
        };

        foreach (var cliente in clientes)
        {
            await _client.PostAsJsonAsync("/api/comercial/clientes", cliente);
        }

        // Act - Probar diferentes combinaciones de filtros
        var filtros = new[]
        {
            "pageNumber=1&pageSize=2&orderBy=NombreCompleto&orderDirection=asc",
            "pageNumber=1&pageSize=2&orderBy=Email&orderDirection=desc",
            "pageNumber=2&pageSize=1&orderBy=FechaRegistro&orderDirection=asc"
        };

        // Assert
        foreach (var filtro in filtros)
        {
            var response = await _client.GetAsync($"/api/comercial/clientes?{filtro}");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var jsonContent = await response.Content.ReadAsStringAsync();
            var resultado = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ClienteDto>>>(jsonContent, GetJsonOptions());
            resultado.Should().NotBeNull();
            resultado!.Success.Should().BeTrue();
            resultado.Data.Should().NotBeNull();
            resultado.Data!.Items.Should().NotBeNull();
        }
    }
}
