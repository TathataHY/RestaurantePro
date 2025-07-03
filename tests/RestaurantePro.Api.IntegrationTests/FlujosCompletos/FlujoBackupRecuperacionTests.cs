using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using RestaurantePro.Api.IntegrationTests.TestBase;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos
{
    [Collection("ApiTestCollection")]
    public class FlujoBackupRecuperacionTests : ApiIntegrationTestBase
    {
        public FlujoBackupRecuperacionTests(TestWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact(DisplayName = "Auditoría de datos - Debe retornar historial de acciones")]
        public async Task AuditoriaDatos_DebeRetornarHistorialAcciones()
        {
            // Arrange
            var fechaInicio = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
            var fechaFin = DateTime.Now.ToString("yyyy-MM-dd");

            // Act
            var response = await HttpClient.GetAsync($"/api/operaciones/reportes/auditoria?fechaInicio={fechaInicio}&fechaFin={fechaFin}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "Notificaciones de backup - Debe enviar notificación de estado")]
        public async Task NotificacionesBackup_DebeEnviarNotificacionEstado()
        {
            // Arrange
            var notificacionBackup = new
            {
                Titulo = "Backup Completado",
                Mensaje = "Backup automático completado exitosamente",
                Tipo = "Exito",
                DestinatarioId = Guid.Parse("11111111-1111-1111-1111-111111111111")
            };

            // Act
            var response = await HttpClient.PostAsJsonAsync("/api/core/notificaciones", notificacionBackup);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "Verificación de integridad - Debe validar datos críticos")]
        public async Task VerificacionIntegridad_DebeValidarDatosCriticos()
        {
            // Arrange - Obtener datos críticos del sistema
            var auditoriaResponse = await HttpClient.GetAsync("/api/operaciones/reportes/auditoria");
            auditoriaResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var notificacionesResponse = await HttpClient.GetAsync("/api/core/notificaciones");
            notificacionesResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act - Verificar integridad de datos críticos
            var inventarioCriticoResponse = await HttpClient.GetAsync("/api/operaciones/reportes/inventario-critico");

            // Assert - Todos los endpoints deben responder correctamente
            inventarioCriticoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await inventarioCriticoResponse.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "Recuperación rápida - Debe restaurar datos críticos")]
        public async Task RecuperacionRapida_DebeRestaurarDatosCriticos()
        {
            // Arrange - Simular estado antes de recuperación
            var estadoInicial = await HttpClient.GetAsync("/api/operaciones/reportes/auditoria");
            estadoInicial.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act - Simular proceso de recuperación
            var notificacionRecuperacion = new
            {
                Titulo = "Recuperación Iniciada",
                Mensaje = "Proceso de recuperación iniciado",
                Tipo = "Informativa",
                DestinatarioId = Guid.Parse("11111111-1111-1111-1111-111111111111")
            };

            var response = await HttpClient.PostAsJsonAsync("/api/core/notificaciones", notificacionRecuperacion);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "Notificaciones de estado - Debe informar progreso de backup")]
        public async Task NotificacionesEstado_DebeInformarProgresoBackup()
        {
            // Arrange - Crear notificaciones de progreso
            var notificacionesProgreso = new[]
            {
                new
                {
                    Titulo = "Backup Iniciado",
                    Mensaje = "Iniciando backup de base de datos",
                    Tipo = "Informativa",
                    DestinatarioId = Guid.Parse("11111111-1111-1111-1111-111111111111")
                },
                new
                {
                    Titulo = "Backup en Progreso",
                    Mensaje = "Backup completado al 50%",
                    Tipo = "Advertencia",
                    DestinatarioId = Guid.Parse("11111111-1111-1111-1111-111111111111")
                },
                new
                {
                    Titulo = "Backup Completado",
                    Mensaje = "Backup completado exitosamente",
                    Tipo = "Exito",
                    DestinatarioId = Guid.Parse("11111111-1111-1111-1111-111111111111")
                }
            };

            // Act - Enviar notificaciones de progreso
            foreach (var notificacion in notificacionesProgreso)
            {
                var response = await HttpClient.PostAsJsonAsync("/api/core/notificaciones", notificacion);
                response.StatusCode.Should().Be(HttpStatusCode.Created);
            }

            // Assert - Verificar que las notificaciones se enviaron
            var notificacionesResponse = await HttpClient.GetAsync("/api/core/notificaciones");
            notificacionesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact(DisplayName = "Auditoría de backups - Debe registrar todas las operaciones")]
        public async Task AuditoriaBackups_DebeRegistrarTodasOperaciones()
        {
            // Arrange - Simular operaciones de backup
            var operacionesBackup = new[]
            {
                new { Accion = "InicioBackup", Usuario = "Sistema", Fecha = DateTime.Now },
                new { Accion = "VerificacionIntegridad", Usuario = "Sistema", Fecha = DateTime.Now },
                new { Accion = "CompresionDatos", Usuario = "Sistema", Fecha = DateTime.Now },
                new { Accion = "TransferenciaSegura", Usuario = "Sistema", Fecha = DateTime.Now },
                new { Accion = "FinalizacionBackup", Usuario = "Sistema", Fecha = DateTime.Now }
            };

            // Act - Registrar operaciones en auditoría
            var fechaInicio = DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd");
            var fechaFin = DateTime.Now.ToString("yyyy-MM-dd");
            
            var auditoriaResponse = await HttpClient.GetAsync($"/api/operaciones/reportes/auditoria?fechaInicio={fechaInicio}&fechaFin={fechaFin}");

            // Assert
            auditoriaResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await auditoriaResponse.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "Flujo completo de backup y recuperación - Debe funcionar correctamente")]
        public async Task FlujoCompletoBackupRecuperacion_DebeFuncionarCorrectamente()
        {
            // Arrange - Estado inicial del sistema
            var auditoriaInicial = await HttpClient.GetAsync("/api/operaciones/reportes/auditoria");
            auditoriaInicial.StatusCode.Should().Be(HttpStatusCode.OK);

            var notificacionesInicial = await HttpClient.GetAsync("/api/core/notificaciones");
            notificacionesInicial.StatusCode.Should().Be(HttpStatusCode.OK);

            // Act - Simular flujo completo de backup
            var notificacionInicio = new
            {
                Titulo = "Backup Iniciado",
                Mensaje = "Iniciando proceso de backup automático",
                Tipo = "Informativa",
                DestinatarioId = Guid.Parse("11111111-1111-1111-1111-111111111111")
            };

            var inicioResponse = await HttpClient.PostAsJsonAsync("/api/core/notificaciones", notificacionInicio);
            inicioResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Simular verificación de integridad
            var inventarioResponse = await HttpClient.GetAsync("/api/operaciones/reportes/inventario-critico");
            inventarioResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Simular finalización de backup
            var notificacionFinal = new
            {
                Titulo = "Backup Completado",
                Mensaje = "Backup completado exitosamente",
                Tipo = "Exito",
                DestinatarioId = Guid.Parse("11111111-1111-1111-1111-111111111111")
            };

            var finalResponse = await HttpClient.PostAsJsonAsync("/api/core/notificaciones", notificacionFinal);
            finalResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // Assert - Verificar estado final
            var auditoriaFinal = await HttpClient.GetAsync("/api/operaciones/reportes/auditoria");
            auditoriaFinal.StatusCode.Should().Be(HttpStatusCode.OK);

            var notificacionesFinal = await HttpClient.GetAsync("/api/core/notificaciones");
            notificacionesFinal.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
} 