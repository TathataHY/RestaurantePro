using System.Net;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using Xunit;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos;

/// <summary>
/// Tests de integración para el flujo completo de reservaciones inteligentes
/// Este flujo valida la verificación de disponibilidad, confirmaciones automáticas y reprogramación
/// </summary>
public class FlujoReservacionesInteligenteTests : ApiIntegrationTestBase
{
    public FlujoReservacionesInteligenteTests() : base(new TestWebApplicationFactory())
    {
    }

    [Fact]
    public async Task FlujoCompletoReservacionesInteligente_DebeFuncionarCorrectamente()
    {
        // 🎯 ARRANGE - Preparar el escenario completo
        Logger.LogInformation("🚀 Iniciando FLUJO COMPLETO de Reservaciones Inteligentes");
        
        // Limpiar datos de prueba
        await LimpiarDatosPrueba();
        
        // 1. Crear entidades base necesarias
        var cliente1 = await CrearClientePrueba("Cliente Reserva 1", "cliente1.reserva@test.com");
        var cliente2 = await CrearClientePrueba("Cliente Reserva 2", "cliente2.reserva@test.com");
        var mesa1 = await CrearMesaPrueba(1, 4, "Interior");
        var mesa2 = await CrearMesaPrueba(2, 6, "Terraza");
        var mesa3 = await CrearMesaPrueba(3, 8, "VIP");
        
        Logger.LogInformation("✅ Entidades base creadas - Clientes: {Cliente1Id}, {Cliente2Id}", 
            cliente1.Id, cliente2.Id);

        // 2. PASO 1: Verificar disponibilidad en tiempo real
        Logger.LogInformation("🔍 PASO 1: Verificando disponibilidad en tiempo real");
        var fechaReservacion = DateTime.Now.AddHours(2);
        var responseDisponibilidad = await HttpClient.GetAsync($"/api/operaciones/reservaciones/disponibilidad?fecha={fechaReservacion:yyyy-MM-dd}&hora={fechaReservacion:HH:mm}&personas=4");
        responseDisponibilidad.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var disponibilidadResponse = await responseDisponibilidad.Content.ReadFromJsonAsync<ApiResponse<object>>();
        disponibilidadResponse.Should().NotBeNull();
        disponibilidadResponse!.Success.Should().BeTrue();
        
        Logger.LogInformation("✅ Disponibilidad verificada");

        // 3. PASO 2: Crear primera reservación
        Logger.LogInformation("📅 PASO 2: Creando primera reservación");
        var reservacion1Request = new
        {
            ClienteId = cliente1.Id,
            MesaId = mesa1.Id,
            FechaReservacion = fechaReservacion,
            NumeroPersonas = 4,
            Observaciones = "Reservación para flujo de prueba"
        };
        
        var responseReservacion1 = await HttpClient.PostAsJsonAsync("/api/operaciones/reservaciones", reservacion1Request);
        responseReservacion1.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var reservacion1Response = await responseReservacion1.Content.ReadFromJsonAsync<ApiResponse<ReservacionDto>>();
        reservacion1Response.Should().NotBeNull();
        reservacion1Response!.Success.Should().BeTrue();
        var reservacion1 = reservacion1Response.Data;
        
        Logger.LogInformation("✅ Primera reservación creada - ID: {ReservacionId}, Estado: {Estado}", 
            reservacion1.Id, reservacion1.Estado);

        // 4. PASO 3: Verificar que la mesa cambió de estado
        Logger.LogInformation("🪑 PASO 3: Verificando cambio de estado de mesa");
        var mesa1Reservada = await DbContext.Mesas.FindAsync(mesa1.Id);
        mesa1Reservada!.Estado.Should().Be(EstadoMesa.Reservada);
        
        Logger.LogInformation("✅ Mesa reservada - Estado: {Estado}", mesa1Reservada.Estado);

        // 5. PASO 4: Intentar crear reservación conflictiva
        Logger.LogInformation("⚠️ PASO 4: Intentando crear reservación conflictiva");
        var reservacionConflictivaRequest = new
        {
            ClienteId = cliente2.Id,
            MesaId = mesa1.Id, // Misma mesa
            FechaReservacion = fechaReservacion.AddMinutes(30), // Hora similar
            NumeroPersonas = 3,
            Observaciones = "Reservación conflictiva"
        };
        
        var responseReservacionConflictiva = await HttpClient.PostAsJsonAsync("/api/operaciones/reservaciones", reservacionConflictivaRequest);
        responseReservacionConflictiva.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        Logger.LogInformation("✅ Conflicto detectado correctamente");

        // 6. PASO 5: Crear reservación en mesa diferente
        Logger.LogInformation("📅 PASO 5: Creando reservación en mesa diferente");
        var reservacion2Request = new
        {
            ClienteId = cliente2.Id,
            MesaId = mesa2.Id,
            FechaReservacion = fechaReservacion.AddMinutes(30),
            NumeroPersonas = 5,
            Observaciones = "Reservación en mesa diferente"
        };
        
        var responseReservacion2 = await HttpClient.PostAsJsonAsync("/api/operaciones/reservaciones", reservacion2Request);
        responseReservacion2.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var reservacion2Response = await responseReservacion2.Content.ReadFromJsonAsync<ApiResponse<ReservacionDto>>();
        var reservacion2 = reservacion2Response!.Data;
        
        Logger.LogInformation("✅ Segunda reservación creada - ID: {ReservacionId}", reservacion2.Id);

        // 7. PASO 6: Confirmar reservación automáticamente
        Logger.LogInformation("✅ PASO 6: Confirmando reservación automáticamente");
        var responseConfirmar = await HttpClient.PostAsync($"/api/operaciones/reservaciones/{reservacion1.Id}/confirmar", null);
        responseConfirmar.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que la reservación cambió de estado
        var reservacionConfirmada = await DbContext.Reservaciones.FindAsync(reservacion1.Id);
        reservacionConfirmada!.Estado.Should().Be(EstadoReservacion.Confirmada);
        
        Logger.LogInformation("✅ Reservación confirmada");

        // 8. PASO 7: Enviar notificación automática
        Logger.LogInformation("📧 PASO 7: Enviando notificación automática");
        var notificacionRequest = new
        {
            Tipo = "Email",
            Destinatario = cliente1.Email,
            Asunto = "Confirmación de Reservación",
            Mensaje = $"Su reservación para {fechaReservacion:dd/MM/yyyy} a las {fechaReservacion:HH:mm} ha sido confirmada.",
            Prioridad = "Normal"
        };
        
        var responseNotificacion = await HttpClient.PostAsJsonAsync("/api/core/notificaciones", notificacionRequest);
        responseNotificacion.StatusCode.Should().Be(HttpStatusCode.Created);
        
        Logger.LogInformation("✅ Notificación enviada");

        // 9. PASO 8: Reprogramar reservación
        Logger.LogInformation("🔄 PASO 8: Reprogramando reservación");
        var nuevaFecha = fechaReservacion.AddHours(1);
        var reprogramarRequest = new
        {
            NuevaFechaReservacion = nuevaFecha,
            Motivo = "Ajuste de horario",
            Observaciones = "Reprogramación solicitada por el cliente"
        };
        
        var responseReprogramar = await HttpClient.PostAsJsonAsync($"/api/operaciones/reservaciones/{reservacion2.Id}/reprogramar", reprogramarRequest);
        responseReprogramar.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que la fecha se actualizó
        var reservacionReprogramada = await DbContext.Reservaciones.FindAsync(reservacion2.Id);
        reservacionReprogramada!.FechaReservacion.Should().Be(nuevaFecha);
        
        Logger.LogInformation("✅ Reservación reprogramada");

        // 10. PASO 9: Obtener plano de mesas actualizado
        Logger.LogInformation("🗺️ PASO 9: Obteniendo plano de mesas actualizado");
        var responsePlano = await HttpClient.GetAsync("/api/operaciones/mesas/plano");
        responsePlano.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var planoResponse = await responsePlano.Content.ReadFromJsonAsync<ApiResponse<PlanoMesasDto>>();
        planoResponse.Should().NotBeNull();
        planoResponse!.Success.Should().BeTrue();
        
        // Verificar que las mesas reservadas aparecen correctamente
        var mesasEnPlano = planoResponse.Data.Mesas;
        mesasEnPlano.Should().Contain(m => m.Numero == 1 && m.Estado == EstadoMesa.Reservada.ToString());
        mesasEnPlano.Should().Contain(m => m.Numero == 2 && m.Estado == EstadoMesa.Reservada.ToString());
        mesasEnPlano.Should().Contain(m => m.Numero == 3 && m.Estado == EstadoMesa.Disponible.ToString());
        
        Logger.LogInformation("✅ Plano de mesas actualizado");

        // 11. PASO 10: Cancelar una reservación
        Logger.LogInformation("❌ PASO 10: Cancelando reservación");
        var responseCancelar = await HttpClient.DeleteAsync($"/api/operaciones/reservaciones/{reservacion1.Id}");
        responseCancelar.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que la mesa se liberó
        var mesa1Liberada = await DbContext.Mesas.FindAsync(mesa1.Id);
        mesa1Liberada!.Estado.Should().Be(EstadoMesa.Disponible);
        
        Logger.LogInformation("✅ Reservación cancelada y mesa liberada");

        // 🎯 ASSERT - Validaciones finales del flujo completo
        Logger.LogInformation("🔍 Validando resultados del flujo completo");
        
        // Verificar que solo queda una reservación activa
        var reservacionesActivas = await DbContext.Reservaciones
            .Where(r => r.Estado == EstadoReservacion.Confirmada || r.Estado == EstadoReservacion.Pendiente)
            .ToListAsync();
        reservacionesActivas.Should().HaveCount(1);
        
        // Verificar que la mesa cancelada está disponible
        mesa1Liberada.Estado.Should().Be(EstadoMesa.Disponible);
        
        Logger.LogInformation("🎉 FLUJO COMPLETO de Reservaciones Inteligentes EXITOSO");
        Logger.LogInformation("📊 Resumen del flujo:");
        Logger.LogInformation("   - Disponibilidad verificada en tiempo real");
        Logger.LogInformation("   - Reservaciones creadas: {Reservacion1Id}, {Reservacion2Id}", 
            reservacion1.Id, reservacion2.Id);
        Logger.LogInformation("   - Conflicto detectado y manejado correctamente");
        Logger.LogInformation("   - Notificación automática enviada");
        Logger.LogInformation("   - Reservación reprogramada exitosamente");
        Logger.LogInformation("   - Cancelación procesada y mesa liberada");
    }

    [Fact]
    public async Task FlujoReservacionesConCapacidad_DebeFuncionarCorrectamente()
    {
        // 🎯 ARRANGE - Flujo con validación de capacidad
        Logger.LogInformation("🚀 Iniciando FLUJO de Reservaciones con Validación de Capacidad");
        
        await LimpiarDatosPrueba();
        
        var cliente = await CrearClientePrueba("Cliente Capacidad", "cliente.capacidad@test.com");
        var mesaPequena = await CrearMesaPrueba(4, 2, "Interior"); // Mesa de 2 personas
        var mesaGrande = await CrearMesaPrueba(5, 8, "Terraza");   // Mesa de 8 personas
        
        Logger.LogInformation("✅ Entidades base creadas para flujo de capacidad");

        // 1. PASO 1: Intentar reservar mesa pequeña para muchas personas
        Logger.LogInformation("⚠️ PASO 1: Intentando reservar mesa pequeña para muchas personas");
        var reservacionInvalidaRequest = new
        {
            ClienteId = cliente.Id,
            MesaId = mesaPequena.Id,
            FechaReservacion = DateTime.Now.AddHours(1),
            NumeroPersonas = 6, // Más personas que la capacidad de la mesa
            Observaciones = "Reservación que excede capacidad"
        };
        
        var responseReservacionInvalida = await HttpClient.PostAsJsonAsync("/api/operaciones/reservaciones", reservacionInvalidaRequest);
        responseReservacionInvalida.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        Logger.LogInformation("✅ Validación de capacidad funcionando");

        // 2. PASO 2: Reservar mesa apropiada
        Logger.LogInformation("📅 PASO 2: Reservando mesa apropiada");
        var reservacionValidaRequest = new
        {
            ClienteId = cliente.Id,
            MesaId = mesaGrande.Id,
            FechaReservacion = DateTime.Now.AddHours(1),
            NumeroPersonas = 6,
            Observaciones = "Reservación válida"
        };
        
        var responseReservacionValida = await HttpClient.PostAsJsonAsync("/api/operaciones/reservaciones", reservacionValidaRequest);
        responseReservacionValida.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var reservacionValidaResponse = await responseReservacionValida.Content.ReadFromJsonAsync<ApiResponse<ReservacionDto>>();
        var reservacionValida = reservacionValidaResponse!.Data;
        
        Logger.LogInformation("✅ Reservación válida creada - ID: {ReservacionId}", reservacionValida.Id);

        // 3. PASO 3: Verificar que la mesa se reservó correctamente
        Logger.LogInformation("🪑 PASO 3: Verificando reservación de mesa");
        var mesaReservada = await DbContext.Mesas.FindAsync(mesaGrande.Id);
        mesaReservada!.Estado.Should().Be(EstadoMesa.Reservada);
        
        Logger.LogInformation("✅ Mesa reservada correctamente");
        Logger.LogInformation("🎉 FLUJO de Capacidad EXITOSO");
    }

    [Fact]
    public async Task FlujoReservacionesConHorarios_DebeFuncionarCorrectamente()
    {
        // 🎯 ARRANGE - Flujo con validación de horarios
        Logger.LogInformation("🚀 Iniciando FLUJO de Reservaciones con Validación de Horarios");
        
        await LimpiarDatosPrueba();
        
        var cliente = await CrearClientePrueba("Cliente Horarios", "cliente.horarios@test.com");
        var mesa = await CrearMesaPrueba(6, 4, "Interior");
        
        Logger.LogInformation("✅ Entidades base creadas para flujo de horarios");

        // 1. PASO 1: Intentar reservar en horario no disponible
        Logger.LogInformation("⚠️ PASO 1: Intentando reservar en horario no disponible");
        var horarioNoDisponible = DateTime.Today.AddHours(3); // 3 AM
        var reservacionHorarioInvalidoRequest = new
        {
            ClienteId = cliente.Id,
            MesaId = mesa.Id,
            FechaReservacion = horarioNoDisponible,
            NumeroPersonas = 3,
            Observaciones = "Reservación en horario no disponible"
        };
        
        var responseHorarioInvalido = await HttpClient.PostAsJsonAsync("/api/operaciones/reservaciones", reservacionHorarioInvalidoRequest);
        responseHorarioInvalido.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        Logger.LogInformation("✅ Validación de horarios funcionando");

        // 2. PASO 2: Reservar en horario válido
        Logger.LogInformation("📅 PASO 2: Reservando en horario válido");
        var horarioValido = DateTime.Today.AddHours(19); // 7 PM
        var reservacionHorarioValidoRequest = new
        {
            ClienteId = cliente.Id,
            MesaId = mesa.Id,
            FechaReservacion = horarioValido,
            NumeroPersonas = 3,
            Observaciones = "Reservación en horario válido"
        };
        
        var responseHorarioValido = await HttpClient.PostAsJsonAsync("/api/operaciones/reservaciones", reservacionHorarioValidoRequest);
        responseHorarioValido.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var reservacionHorarioValidoResponse = await responseHorarioValido.Content.ReadFromJsonAsync<ApiResponse<ReservacionDto>>();
        var reservacionHorarioValido = reservacionHorarioValidoResponse!.Data;
        
        Logger.LogInformation("✅ Reservación en horario válido creada - ID: {ReservacionId}", reservacionHorarioValido.Id);

        // 3. PASO 3: Verificar disponibilidad en horario específico
        Logger.LogInformation("🔍 PASO 3: Verificando disponibilidad en horario específico");
        var responseDisponibilidadEspecifica = await HttpClient.GetAsync($"/api/operaciones/reservaciones/disponibilidad?fecha={horarioValido:yyyy-MM-dd}&hora={horarioValido:HH:mm}&personas=4");
        responseDisponibilidadEspecifica.StatusCode.Should().Be(HttpStatusCode.OK);
        
        Logger.LogInformation("✅ Disponibilidad verificada en horario específico");
        Logger.LogInformation("🎉 FLUJO de Horarios EXITOSO");
    }

    [Fact]
    public async Task FlujoReservacionesConNotificaciones_DebeFuncionarCorrectamente()
    {
        // 🎯 ARRANGE - Flujo con sistema de notificaciones
        Logger.LogInformation("🚀 Iniciando FLUJO de Reservaciones con Notificaciones");
        
        await LimpiarDatosPrueba();
        
        var cliente = await CrearClientePrueba("Cliente Notificaciones", "cliente.notificaciones@test.com");
        var mesa = await CrearMesaPrueba(7, 4, "Interior");
        
        Logger.LogInformation("✅ Entidades base creadas para flujo de notificaciones");

        // 1. PASO 1: Crear reservación
        Logger.LogInformation("📅 PASO 1: Creando reservación");
        var reservacionRequest = new
        {
            ClienteId = cliente.Id,
            MesaId = mesa.Id,
            FechaReservacion = DateTime.Now.AddHours(1),
            NumeroPersonas = 3,
            Observaciones = "Reservación con notificaciones"
        };
        
        var responseReservacion = await HttpClient.PostAsJsonAsync("/api/operaciones/reservaciones", reservacionRequest);
        responseReservacion.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var reservacionResponse = await responseReservacion.Content.ReadFromJsonAsync<ApiResponse<ReservacionDto>>();
        var reservacion = reservacionResponse!.Data;
        
        Logger.LogInformation("✅ Reservación creada - ID: {ReservacionId}", reservacion.Id);

        // 2. PASO 2: Enviar notificación de confirmación
        Logger.LogInformation("📧 PASO 2: Enviando notificación de confirmación");
        var notificacionConfirmacionRequest = new
        {
            Tipo = "Email",
            Destinatario = cliente.Email,
            Asunto = "Reservación Creada",
            Mensaje = "Su reservación ha sido creada exitosamente. Le enviaremos una confirmación pronto.",
            Prioridad = "Normal"
        };
        
        var responseNotificacionConfirmacion = await HttpClient.PostAsJsonAsync("/api/core/notificaciones", notificacionConfirmacionRequest);
        responseNotificacionConfirmacion.StatusCode.Should().Be(HttpStatusCode.Created);
        
        Logger.LogInformation("✅ Notificación de confirmación enviada");

        // 3. PASO 3: Enviar notificación de recordatorio
        Logger.LogInformation("⏰ PASO 3: Enviando notificación de recordatorio");
        var notificacionRecordatorioRequest = new
        {
            Tipo = "SMS",
            Destinatario = cliente.Telefono,
            Asunto = "Recordatorio de Reservación",
            Mensaje = "Recordatorio: Su reservación es en 1 hora.",
            Prioridad = "Alta"
        };
        
        var responseNotificacionRecordatorio = await HttpClient.PostAsJsonAsync("/api/core/notificaciones", notificacionRecordatorioRequest);
        responseNotificacionRecordatorio.StatusCode.Should().Be(HttpStatusCode.Created);
        
        Logger.LogInformation("✅ Notificación de recordatorio enviada");

        // 4. PASO 4: Verificar historial de notificaciones
        Logger.LogInformation("📋 PASO 4: Verificando historial de notificaciones");
        var responseHistorial = await HttpClient.GetAsync($"/api/core/notificaciones/cliente/{cliente.Id}");
        responseHistorial.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var historialResponse = await responseHistorial.Content.ReadFromJsonAsync<ApiResponse<object>>();
        historialResponse.Should().NotBeNull();
        historialResponse!.Success.Should().BeTrue();
        
        Logger.LogInformation("✅ Historial de notificaciones verificado");
        Logger.LogInformation("🎉 FLUJO de Notificaciones EXITOSO");
    }

    private async Task LimpiarDatosPrueba()
    {
        // Limpiar datos en orden para evitar problemas de FK
        await LimpiarTablaReservaciones();
        await LimpiarTablaMesas();
        await LimpiarTablaClientes();
        await LimpiarTablaNotificaciones();
        
        Logger.LogInformation("🧹 Datos de prueba limpiados");
    }

    private async Task LimpiarTablaReservaciones()
    {
        var reservaciones = await DbContext.Reservaciones.ToListAsync();
        DbContext.Reservaciones.RemoveRange(reservaciones);
        await DbContext.SaveChangesAsync();
    }

    private async Task LimpiarTablaNotificaciones()
    {
        var notificaciones = await DbContext.Notificaciones.ToListAsync();
        DbContext.Notificaciones.RemoveRange(notificaciones);
        await DbContext.SaveChangesAsync();
    }

    private async Task LimpiarTablaMesas()
    {
        var mesas = await DbContext.Mesas.ToListAsync();
        DbContext.Mesas.RemoveRange(mesas);
        await DbContext.SaveChangesAsync();
    }

    private async Task LimpiarTablaClientes()
    {
        var clientes = await DbContext.Clientes.ToListAsync();
        DbContext.Clientes.RemoveRange(clientes);
        await DbContext.SaveChangesAsync();
    }
} 