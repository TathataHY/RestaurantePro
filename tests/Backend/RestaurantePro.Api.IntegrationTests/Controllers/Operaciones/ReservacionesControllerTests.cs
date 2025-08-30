using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Domain.Operaciones.Reservaciones.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Comercial.Clientes;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands.CrearReservacion;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands.ActualizarReservacion;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands.ReprogramarReservacion;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands.CancelarReservacion;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands.ConfirmarReservacion;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Application.Operaciones.Reservaciones.Queries.VerificarDisponibilidad;
using RestaurantePro.Application.Common.Models;
using System.Text.Json;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.ValueObjects;
using System.Threading;

namespace RestaurantePro.Api.IntegrationTests.Controllers.Operaciones;

/// <summary>
/// Tests de integración completos para el controlador de reservaciones.
/// Valida interacción real con BD y reglas de negocio específicas.
/// </summary>
[Collection("Sequential")]
public class ReservacionesControllerTests : ApiIntegrationTestBase
{
    private static int _contadorMesas = 0;

    public ReservacionesControllerTests(TestWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetReservaciones_ConDatosExistentes_RetornaListaPaginada()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        // Crear datos de prueba
        var cliente = CrearClienteTest("reservaciones");
        var mesa = CrearMesaTest("reservaciones");
        var reservacion = CrearReservacionTest(cliente.Id, mesa.Id, "reservaciones");
        
        context.Clientes.Add(cliente);
        context.Mesas.Add(mesa);
        context.Reservaciones.Add(reservacion);
        await context.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync("/api/operaciones/reservaciones");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await DeserializarResponse<PaginatedList<ReservacionDto>>(response);
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Items.Should().NotBeEmpty();
        apiResponse.Data.Items.Should().Contain(r => r.Id == reservacion.Id);
    }

    [Fact]
    public async Task GetReservacionPorId_ConReservacionExistente_RetornaReservacion()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        var cliente = CrearClienteTest("get");
        var mesa = CrearMesaTest("get");
        var reservacion = CrearReservacionTest(cliente.Id, mesa.Id, "get");
        
        context.Clientes.Add(cliente);
        context.Mesas.Add(mesa);
        context.Reservaciones.Add(reservacion);
        await context.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/reservaciones/{reservacion.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await DeserializarResponse<ReservacionDto>(response);
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(reservacion.Id);
        apiResponse.Data.ClienteId.Should().Be(cliente.Id);
        apiResponse.Data.MesaId.Should().Be(mesa.Id);
    }

    [Fact]
    public async Task GetReservacionPorId_ConReservacionInexistente_RetornaNotFound()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        // Crear datos reales para establecer contexto
        var cliente = CrearClienteTest("notfound");
        var mesa = CrearMesaTest("notfound");
        var reservacion = CrearReservacionTest(cliente.Id, mesa.Id, "notfound");
        
        context.Clientes.Add(cliente);
        context.Mesas.Add(mesa);
        context.Reservaciones.Add(reservacion);
        await context.SaveChangesAsync();

        // Generar un ID que realmente no existe en la BD
        var idInexistente = Guid.NewGuid();
        
        // Verificar que el ID realmente no existe en la BD
        context.ChangeTracker.Clear();
        var reservacionEnBD = await context.Reservaciones.FindAsync(idInexistente);
        reservacionEnBD.Should().BeNull();

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/reservaciones/{idInexistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var apiResponse = await DeserializarResponse<ApiResponse<object>>(response);
        apiResponse.Should().NotBeNull();
        apiResponse.Success.Should().BeFalse();
        
        // Verificar que la reservación real sigue existiendo en la BD
        var reservacionReal = await context.Reservaciones.FindAsync(reservacion.Id);
        reservacionReal.Should().NotBeNull();
        reservacionReal!.Id.Should().Be(reservacion.Id);
    }

    [Fact]
    public async Task CrearReservacion_ConDatosValidos_RetornaReservacionCreada()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        var cliente = CrearClienteTest("crear");
        var mesa = CrearMesaTest("crear");
        
        context.Clientes.Add(cliente);
        context.Mesas.Add(mesa);
        await context.SaveChangesAsync();

        var command = new CrearReservacionCommand
        {
            ClienteId = cliente.Id,
            MesaId = mesa.Id,
            FechaHoraReservacion = DateTime.Now.AddDays(1).Date.AddHours(19), // 7:00 PM mañana
            NumeroPersonas = 4,
            NombreCliente = "Cliente Test",
            TelefonoContacto = "1234567890",
            Email = "cliente@test.com",
            Observaciones = "Mesa junto a la ventana",
            Canal = "Web"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/reservaciones", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var apiResponse = await DeserializarResponse<ReservacionDto>(response);
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().NotBeEmpty();
        apiResponse.Data.FechaHoraReservacion.Should().Be(command.FechaHoraReservacion);
        apiResponse.Data.NumeroPersonas.Should().Be(command.NumeroPersonas);
    }

    [Fact]
    public async Task ActualizarReservacion_ConDatosValidos_RetornaReservacionActualizada()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        var cliente = CrearClienteTest("actualizar");
        var mesa = CrearMesaTest("actualizar");
        var mesaNueva = CrearMesaTest("actualizar-nueva");
        var reservacion = CrearReservacionTest(cliente.Id, mesa.Id, "actualizar");
        
        context.Clientes.Add(cliente);
        context.Mesas.Add(mesa);
        context.Mesas.Add(mesaNueva);
        context.Reservaciones.Add(reservacion);
        await context.SaveChangesAsync();

        var command = new ActualizarReservacionCommand
        {
            Id = reservacion.Id,
            MesaId = mesaNueva.Id,
            Observaciones = "Reservación actualizada para mesa más grande"
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/operaciones/reservaciones/{reservacion.Id}", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await DeserializarResponse<ReservacionDto>(response);
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(reservacion.Id);
        apiResponse.Data.MesaId.Should().Be(mesaNueva.Id);
        apiResponse.Data.Observaciones.Should().Be("Reservación actualizada para mesa más grande");
    }

    [Fact]
    public async Task CancelarReservacion_ConReservacionExistente_RetornaReservacionCancelada()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        var cliente = CrearClienteTest("cancelar");
        var mesa = CrearMesaTest("cancelar");
        var reservacion = CrearReservacionTest(cliente.Id, mesa.Id, "cancelar");
        
        context.Clientes.Add(cliente);
        context.Mesas.Add(mesa);
        context.Reservaciones.Add(reservacion);
        await context.SaveChangesAsync();

        // Crear comando con los datos requeridos por el validador
        var command = new CancelarReservacionCommand
        {
            Id = reservacion.Id,
            ReservacionId = reservacion.Id,
            Motivo = MotivoCancelacion.ClienteSolicita,
            MotivoDetalle = "Cliente canceló por cambio de planes",
            NotificarCliente = true,
            LiberarMesaInmediatamente = true
        };

        // Act - Usar POST en lugar de DELETE para enviar el comando con datos
        var response = await HttpClient.PostAsJsonAsync($"/api/operaciones/reservaciones/{reservacion.Id}/cancelar", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await DeserializarResponse<ReservacionDto>(response);
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(reservacion.Id);
        apiResponse.Data.Estado.Should().Be(EstadoReservacion.Cancelada);
    }

    [Fact]
    public async Task ConfirmarReservacion_ConReservacionPendiente_RetornaReservacionConfirmada()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        var cliente = CrearClienteTest("confirmar");
        var mesa = CrearMesaTest("confirmar");
        var reservacion = CrearReservacionTest(cliente.Id, mesa.Id, "confirmar");
        
        context.Clientes.Add(cliente);
        context.Mesas.Add(mesa);
        context.Reservaciones.Add(reservacion);
        await context.SaveChangesAsync();

        var command = new ConfirmarReservacionCommand
        {
            ReservacionId = reservacion.Id,
            MetodoConfirmacion = "Manual",
            ConfirmadoPor = "Empleado Test",
            NotasConfirmacion = "Confirmación realizada por empleado"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/operaciones/reservaciones/{reservacion.Id}/confirmar", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await DeserializarResponse<ReservacionDto>(response);
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data!.Id.Should().Be(reservacion.Id);
        apiResponse.Data.Estado.Should().Be(EstadoReservacion.Confirmada);

        // Verificar en la BD
        context.ChangeTracker.Clear();
        var reservacionConfirmada = await context.Reservaciones.FindAsync(reservacion.Id);
        reservacionConfirmada.Should().NotBeNull();
        reservacionConfirmada!.Estado.Should().Be(EstadoReservacion.Confirmada);
    }

    [Fact]
    public async Task ReprogramarReservacion_ConReservacionExistente_RetornaReservacionReprogramada()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        var cliente = CrearClienteTest("reprogramar");
        var mesa = CrearMesaTest("reprogramar");
        var reservacion = CrearReservacionTest(cliente.Id, mesa.Id, "reprogramar");
        
        context.Clientes.Add(cliente);
        context.Mesas.Add(mesa);
        context.Reservaciones.Add(reservacion);
        await context.SaveChangesAsync();

        var nuevaFecha = DateTime.Now.AddDays(3);
        var nuevaHora = TimeSpan.FromHours(21);
        var command = new ReprogramarReservacionCommand
        {
            Id = reservacion.Id,
            NuevaFechaReservacion = nuevaFecha,
            NuevaHoraReservacion = nuevaHora,
            NuevoNumeroPersonas = 8,
            MotivoReprogramacion = "Cliente solicitó cambio"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/operaciones/reservaciones/{reservacion.Id}/reprogramar", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await DeserializarResponse<ReservacionDto>(response);
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        apiResponse.Data.Id.Should().NotBe(reservacion.Id); // Nueva reservación con ID diferente
        apiResponse.Data.FechaHoraReservacion.Should().Be(nuevaFecha.Date.Add(nuevaHora));
        apiResponse.Data.NumeroPersonas.Should().Be(8);

        // Verificar que la reservación original fue cancelada
        context.ChangeTracker.Clear();
        var reservacionOriginal = await context.Reservaciones.FindAsync(reservacion.Id);
        reservacionOriginal.Should().NotBeNull();
        reservacionOriginal!.Estado.Should().Be(EstadoReservacion.Cancelada);
        reservacionOriginal.MotivoCancelacion.Should().Contain("Reprogramada");

        // Verificar que se creó la nueva reservación
        var nuevaReservacion = await context.Reservaciones.FindAsync(apiResponse.Data.Id);
        nuevaReservacion.Should().NotBeNull();
        nuevaReservacion!.Fecha.Should().Be(nuevaFecha.Date);
        nuevaReservacion.Hora.Should().Be(nuevaHora);
        nuevaReservacion.CantidadPersonas.Should().Be(8);
    }

    [Fact]
    public async Task VerificarDisponibilidad_ConParametrosValidos_RetornaDisponibilidad()
    {
        // Arrange
        await using var context = CreateNewDbContext();
        
        // Crear datos reales para el test
        var mesa1 = CrearMesaTest("disponibilidad-1");
        var mesa2 = CrearMesaTest("disponibilidad-2");
        var mesa3 = CrearMesaTest("disponibilidad-3");
        var cliente = CrearClienteTest("disponibilidad");
        
        var fecha = DateTime.Now.AddDays(1).Date;
        var hora = TimeSpan.FromHours(19);
        
        // Crear una reservación existente para la misma fecha/hora que se consultará
        var reservacionExistente = Reservacion.Crear(
            mesa1.Id,
            cliente.Id,
            fecha.Add(hora), // Usar la misma fecha y hora que se consultará
            TimeSpan.FromHours(2), // Duración estimada
            4,
            "+1234567890",
            "reservacion-disponibilidad@test.com",
            "Observaciones disponibilidad-existente"
        );
        
        context.Mesas.AddRange(mesa1, mesa2, mesa3);
        context.Clientes.Add(cliente);
        context.Reservaciones.Add(reservacionExistente);
        await context.SaveChangesAsync();

        // --- LOGS TEMPORALES DE DEPURACIÓN ---
        // var reservacionesEnBD = context.Reservaciones.ToList();
        // Console.WriteLine("\n=== RESERVACIONES EN BD ===");
        // foreach (var r in reservacionesEnBD)
        // {
        //     Console.WriteLine($"Id: {r.Id}, MesaId: {r.MesaId}, Fecha: {r.Fecha:yyyy-MM-dd}, Hora: {r.Hora}, Duración: {r.DuracionEstimada}, Estado: {r.Estado}");
        // }
        // Console.WriteLine("===========================\n");
        
        // LOG: Mesas disponibles devueltas por el endpoint
        // Console.WriteLine("\n=== MESAS DISPONIBLES DEVUELTAS POR EL ENDPOINT ===");
        // foreach (var m in apiResponse.Data!.MesasDisponibles)
        // {
        //     Console.WriteLine($"Id: {m.Id}, Numero: {m.Numero}, Capacidad: {m.Capacidad}, Ubicacion: {m.Ubicacion}");
        // }
        // Console.WriteLine("===============================================\n");
        
        var numeroPersonas = 4;

        // Act
        var response = await HttpClient.GetAsync($"/api/operaciones/reservaciones/disponibilidad?fecha={fecha:yyyy-MM-dd}&hora={hora}&numeroPersonas={numeroPersonas}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var apiResponse = await DeserializarResponse<DisponibilidadDto>(response);
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data.Should().NotBeNull();
        
        // Verificar que hay disponibilidad (debería haber al menos 2 mesas disponibles)
        apiResponse.Data!.Disponible.Should().BeTrue();
        apiResponse.Data.MesasDisponibles.Should().NotBeEmpty();
        apiResponse.Data.MesasDisponibles.Should().HaveCount(c => c >= 2); // mesa2 y mesa3 deberían estar disponibles
        
        // Verificar que la mesa ocupada (mesa1) no esté en las disponibles
        apiResponse.Data.MesasDisponibles.Should().NotContain(m => m.Id == mesa1.Id);
        
        // Verificar que las mesas disponibles realmente existen en la BD
        context.ChangeTracker.Clear();
        foreach (var mesaDisponible in apiResponse.Data.MesasDisponibles)
        {
            var mesaEnBD = await context.Mesas.FindAsync(mesaDisponible.Id);
            mesaEnBD.Should().NotBeNull();
            mesaEnBD!.Capacidad.Should().BeGreaterThanOrEqualTo(numeroPersonas);
        }
        
        // Verificar que la reservación existente realmente está en la BD
        var reservacionEnBD = await context.Reservaciones.FindAsync(reservacionExistente.Id);
        reservacionEnBD.Should().NotBeNull();
        reservacionEnBD!.Fecha.Should().Be(fecha.Date);
        reservacionEnBD.Hora.Should().Be(hora);
    }

    #region Métodos Helper

    private static Cliente CrearClienteTest(string? sufijo = null)
    {
        var guid = Guid.NewGuid();
        var sufijoUnico = sufijo ?? guid.ToString().Substring(0, 8);
        var nombre = ClienteNombre.Crear($"Cliente{sufijoUnico}", $"Test{sufijoUnico}");
        var email = Email.Create($"cliente{sufijoUnico}@test.com");
        var telefono = PhoneNumber.Create("+1234567890");
        var fechaNacimiento = DateTime.Now.AddYears(-30);
        
        return Cliente.Crear(guid, nombre, email, telefono, fechaNacimiento);
    }

    private static Mesa CrearMesaTest(string? sufijo = null)
    {
        var guid = Guid.NewGuid();
        var sufijoUnico = sufijo ?? guid.ToString().Substring(0, 8);
        
        // Generar un número de mesa único usando contador estático
        var numeroMesa = Interlocked.Increment(ref _contadorMesas) + 1000; // Números únicos desde 1001
        
        return Mesa.Crear(
            numeroMesa,
            4,
            $"Ubicación {sufijoUnico}"
        );
    }

    private static Reservacion CrearReservacionTest(Guid clienteId, Guid mesaId, string? sufijo = null)
    {
        var guid = Guid.NewGuid();
        var sufijoUnico = sufijo ?? guid.ToString().Substring(0, 8);
        
        return Reservacion.Crear(
            mesaId,
            clienteId,
            DateTime.Now.AddDays(1),
            TimeSpan.FromHours(2),
            4,
            "+1234567890",
            $"reservacion{sufijoUnico}@test.com",
            $"Observaciones {sufijoUnico}"
        );
    }

    private static async Task<ApiResponse<T>> DeserializarResponse<T>(HttpResponseMessage response)
    {
        return await response.Content.ReadFromJsonAsyncApiResponse<T>() ?? new ApiResponse<T> { Success = false, Data = default };
    }

    #endregion
} 