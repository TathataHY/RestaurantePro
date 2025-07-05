using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos
{
    [Collection("ApiTestCollection")]
    public class FlujoMonitoreoAlertasTests : ApiIntegrationTestBase
    {
        public FlujoMonitoreoAlertasTests(TestWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact(DisplayName = "Auditoría de acciones - Debe retornar datos de auditoría")]
        public async Task AuditoriaAcciones_DebeRetornarDatos()
        {
            var response = await HttpClient.GetAsync("/api/operaciones/reportes/auditoria");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            // TODO: Validar contenido real de auditoría
        }

        [Fact(DisplayName = "Alertas de inventario - Debe retornar alertas activas")]
        public async Task AlertasInventario_DebeRetornarAlertas()
        {
            var response = await HttpClient.GetAsync("/api/inventario/reportes/alertas");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            // TODO: Validar contenido real de alertas
        }

        [Fact(DisplayName = "Inventario crítico - Debe retornar ingredientes críticos")]
        public async Task InventarioCritico_DebeRetornarIngredientesCriticos()
        {
            var response = await HttpClient.GetAsync("/api/operaciones/reportes/inventario-critico");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            // TODO: Validar contenido real de inventario crítico
        }

        [Fact(DisplayName = "Notificaciones automáticas - Debe enviar alerta correctamente")]
        public async Task NotificacionesAutomaticas_DebeEnviarAlerta()
        {
                    var payload = new 
        { 
            Titulo = "Alerta de prueba",
            Mensaje = "Esta es una alerta de prueba para validar el sistema de monitoreo",
            Tipo = "Advertencia",
            DestinatarioId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        };
            var response = await HttpClient.PostAsJsonAsync("/api/core/notificaciones", payload);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            // TODO: Validar que la notificación fue procesada
        }
    }
} 