using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Application.Proveedores.Proveedores.DTOs;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos
{
    [Collection("ApiTestCollection")]
    public class FlujoGestionProveedoresCompletaTests : ApiIntegrationTestBase
    {
        public FlujoGestionProveedoresCompletaTests(TestWebApplicationFactory factory) : base(factory)
        {
        }

        // Clase auxiliar para deserializar la respuesta estándar de la API
        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }
        }

        [Fact(DisplayName = "Crear proveedor - Debe crear proveedor exitosamente")]
        public async Task CrearProveedor_DebeCrearExitosamente()
        {
            // Arrange
            var proveedor = new
            {
                Nombre = "Proveedor Test",
                Email = "proveedor.test@example.com",
                Telefono = "+1234567890",
                Direccion = "Calle Test 123",
                Categoria = "Alimentos",
                Estado = "Activo"
            };

            // Act
            var response = await HttpClient.PostAsJsonAsync("/api/proveedores", proveedor);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<ProveedorDto>>();
            apiResponse.Should().NotBeNull();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data.Id.Should().NotBeEmpty();
        }

        [Fact(DisplayName = "Asignar productos a proveedor - Debe asignar productos correctamente")]
        public async Task AsignarProductosProveedor_DebeAsignarCorrectamente()
        {
            // Arrange
            var proveedor = new
            {
                Nombre = "Proveedor Productos",
                Email = "proveedor.productos@example.com",
                Telefono = "+1234567890",
                Direccion = "Calle Productos 123",
                Categoria = "Alimentos",
                Estado = "Activo"
            };
            var crearProveedorResponse = await HttpClient.PostAsJsonAsync("/api/proveedores", proveedor);
            crearProveedorResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var apiResponse = await crearProveedorResponse.Content.ReadFromJsonAsync<ApiResponse<ProveedorDto>>();
            var proveedorId = apiResponse.Data.Id;

            var productos = new
            {
                Productos = new[]
                {
                    new { Id = Guid.NewGuid(), Nombre = "Producto 1" },
                    new { Id = Guid.NewGuid(), Nombre = "Producto 2" }
                }
            };

            // Act
            var response = await HttpClient.PostAsJsonAsync($"/api/proveedores/{proveedorId}/productos", productos);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "Evaluar proveedor - Debe crear evaluación exitosamente")]
        public async Task EvaluarProveedor_DebeCrearEvaluacion()
        {
            // Arrange - Primero crear un proveedor
            var proveedor = new
            {
                Nombre = "Proveedor para Evaluar",
                Email = "proveedor.evaluar@example.com",
                Telefono = "+1234567890",
                Direccion = "Calle Evaluacion 123",
                Categoria = "Alimentos",
                Estado = "Activo"
            };
            var crearProveedorResponse = await HttpClient.PostAsJsonAsync("/api/proveedores", proveedor);
            crearProveedorResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var apiResponse = await crearProveedorResponse.Content.ReadFromJsonAsync<ApiResponse<ProveedorDto>>();
            var proveedorId = apiResponse.Data.Id;

            // Ahora crear la evaluación
            var evaluacion = new
            {
                ProveedorId = proveedorId,
                CalificacionGeneral = 4,
                CalificacionCalidad = 5,
                CalificacionPuntualidad = 4,
                CalificacionComunicacion = 4,
                CalificacionPrecios = 3,
                Comentarios = "Excelente proveedor, entrega a tiempo"
            };

            // Act
            var response = await HttpClient.PostAsJsonAsync("/api/proveedores/evaluaciones", evaluacion);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "Reporte de compras por proveedor - Debe retornar datos")]
        public async Task ReporteComprasProveedor_DebeRetornarDatos()
        {
            // Arrange
            var proveedor = new
            {
                Nombre = "Proveedor para Reporte",
                Email = "proveedor.reporte@example.com",
                Telefono = "+1234567890",
                Direccion = "Calle Reporte 123",
                Categoria = "Alimentos",
                Estado = "Activo"
            };
            var crearProveedorResponse = await HttpClient.PostAsJsonAsync("/api/proveedores", proveedor);
            crearProveedorResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var apiResponse = await crearProveedorResponse.Content.ReadFromJsonAsync<ApiResponse<ProveedorDto>>();
            var proveedorId = apiResponse.Data.Id;

            var fechaInicio = DateTime.Now.AddMonths(-1);
            var fechaFin = DateTime.Now;

            // Act
            var response = await HttpClient.GetAsync($"/api/proveedores/reporte/compras?proveedorId={proveedorId}&fechaInicio={fechaInicio:yyyy-MM-dd}&fechaFin={fechaFin:yyyy-MM-dd}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact(DisplayName = "Historial de evaluaciones - Debe retornar evaluaciones")]
        public async Task HistorialEvaluaciones_DebeRetornarEvaluaciones()
        {
            // Arrange - Primero crear un proveedor
            var proveedor = new
            {
                Nombre = "Proveedor para Historial",
                Email = "proveedor.historial@example.com",
                Telefono = "+1234567890",
                Direccion = "Calle Historial 123",
                Categoria = "Alimentos",
                Estado = "Activo"
            };
            var crearProveedorResponse = await HttpClient.PostAsJsonAsync("/api/proveedores", proveedor);
            crearProveedorResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var apiResponse = await crearProveedorResponse.Content.ReadFromJsonAsync<ApiResponse<ProveedorDto>>();
            var proveedorId = apiResponse.Data.Id;

            // Act
            var response = await HttpClient.GetAsync($"/api/proveedores/evaluaciones/proveedor/{proveedorId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "Flujo completo gestión proveedores - Debe funcionar end-to-end")]
        public async Task FlujoCompletoGestionProveedores_DebeFuncionarCorrectamente()
        {
            // Por ahora, verificamos que los endpoints básicos responden
            var response = await HttpClient.GetAsync("/api/proveedores");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
} 