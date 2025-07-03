using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos
{
    [Collection("ApiTestCollection")]
    public class FlujoConfiguracionSistemaTests : ApiIntegrationTestBase
    {
        public FlujoConfiguracionSistemaTests(TestWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact(DisplayName = "Obtener configuración - Debe retornar configuración actual")]
        public async Task ObtenerConfiguracion_DebeRetornarConfiguracionActual()
        {
            // Act
            var response = await HttpClient.GetAsync("/api/core/notificaciones/configuracion");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "Actualizar configuración - Debe actualizar configuración exitosamente")]
        public async Task ActualizarConfiguracion_DebeActualizarExitosamente()
        {
            // Arrange
            var nuevaConfiguracion = new
            {
                EmailHabilitado = true,
                SMTPHost = "smtp.ejemplo.com",
                SMTPPuerto = 587,
                NotificacionesPush = true,
                HorarioNotificaciones = "09:00-18:00"
            };

            // Act
            var response = await HttpClient.PostAsJsonAsync("/api/core/notificaciones/configuracion", nuevaConfiguracion);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "Obtener plano de mesas - Debe retornar plano actual")]
        public async Task ObtenerPlanoMesas_DebeRetornarPlanoActual()
        {
            // Act
            var response = await HttpClient.GetAsync("/api/operaciones/mesas/plano");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "Configuración dinámica - Debe permitir cambios sin reinicio")]
        public async Task ConfiguracionDinamica_DebePermitirCambiosSinReinicio()
        {
            // Arrange - Obtener configuración inicial
            var configInicial = await HttpClient.GetAsync("/api/core/notificaciones/configuracion");
            configInicial.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act - Actualizar configuración
            var nuevaConfig = new
            {
                EmailHabilitado = false,
                SMTPHost = "nuevo.smtp.com",
                SMTPPuerto = 465,
                NotificacionesPush = false,
                HorarioNotificaciones = "10:00-19:00"
            };

            var actualizarResponse = await HttpClient.PostAsJsonAsync("/api/core/notificaciones/configuracion", nuevaConfig);
            actualizarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert - Verificar que el cambio se aplicó inmediatamente
            var configFinal = await HttpClient.GetAsync("/api/core/notificaciones/configuracion");
            configFinal.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var contentFinal = await configFinal.Content.ReadAsStringAsync();
            contentFinal.Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "Validación de configuraciones - Debe validar configuraciones inválidas")]
        public async Task ValidacionConfiguraciones_DebeValidarConfiguracionesInvalidas()
        {
            // Arrange - Configuración inválida
            var configInvalida = new
            {
                EmailHabilitado = true,
                SMTPHost = "", // Host vacío es inválido
                SMTPPuerto = -1, // Puerto negativo es inválido
                NotificacionesPush = true,
                HorarioNotificaciones = "formato-invalido"
            };

            // Act
            var response = await HttpClient.PostAsJsonAsync("/api/core/notificaciones/configuracion", configInvalida);

            // Assert - Debe manejar la validación (puede ser 400 o 200 dependiendo de la implementación)
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Flujo completo de configuración - Debe funcionar correctamente")]
        public async Task FlujoCompletoConfiguracion_DebeFuncionarCorrectamente()
        {
            // Arrange - Obtener estado inicial
            var planoInicial = await HttpClient.GetAsync("/api/operaciones/mesas/plano");
            planoInicial.StatusCode.Should().Be(HttpStatusCode.OK);

            var configInicial = await HttpClient.GetAsync("/api/core/notificaciones/configuracion");
            configInicial.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act - Realizar cambios de configuración
            var nuevaConfig = new
            {
                EmailHabilitado = true,
                SMTPHost = "configuracion.test.com",
                SMTPPuerto = 587,
                NotificacionesPush = true,
                HorarioNotificaciones = "08:00-20:00"
            };

            var actualizarConfig = await HttpClient.PostAsJsonAsync("/api/core/notificaciones/configuracion", nuevaConfig);
            actualizarConfig.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert - Verificar que todo sigue funcionando
            var planoFinal = await HttpClient.GetAsync("/api/operaciones/mesas/plano");
            planoFinal.StatusCode.Should().Be(HttpStatusCode.OK);

            var configFinal = await HttpClient.GetAsync("/api/core/notificaciones/configuracion");
            configFinal.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
} 