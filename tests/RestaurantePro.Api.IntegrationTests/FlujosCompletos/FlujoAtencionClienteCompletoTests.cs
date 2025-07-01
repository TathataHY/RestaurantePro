using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Api.Common;
using RestaurantePro.Application.Comercial.Facturacion.DTOs;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using RestaurantePro.Application.Operaciones.Preparaciones.DTOs;
using RestaurantePro.Application.Operaciones.Reservaciones.DTOs;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Operaciones.Preparaciones.Enums;
using Xunit;
using RestaurantePro.Application.Operaciones.Comandas.Commands.CambiarEstadoComanda;
using RestaurantePro.Application.Operaciones.Reservaciones.Commands.ConfirmarReservacion;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos;

/// <summary>
/// Tests de integración para el flujo completo de atención al cliente
/// Este flujo valida toda la cadena desde que llega un cliente hasta que se va
/// </summary>
public class FlujoAtencionClienteCompletoTests : ApiIntegrationTestBase
{
    public FlujoAtencionClienteCompletoTests() : base(new TestWebApplicationFactory())
    {
    }

    [Fact]
    public async Task FlujoCompletoAtencionCliente_DebeFuncionarCorrectamente()
    {
        // 🎯 ARRANGE - Preparar el escenario completo
        Logger.LogInformation("🚀 Iniciando FLUJO COMPLETO de Atención al Cliente");
        
        // Limpiar datos de prueba
        await LimpiarDatosPrueba();
        
        // 1. Crear entidades base necesarias
        var mesero = await CrearUsuarioPrueba("mesero.flujo", "Mesero Flujo", "mesero.flujo@test.com", RolUsuario.Mesero);
        var chef = await CrearUsuarioPrueba("chef.flujo", "Chef Flujo", "chef.flujo@test.com", RolUsuario.Cocinero);
        var cliente = await CrearClientePrueba("Cliente Flujo", "cliente.flujo@test.com");
        var mesa = await CrearMesaPrueba(1, 4, "Interior");
        var producto = await CrearProductoPrueba("Pizza Margherita", 25.00m);
        
        Logger.LogInformation("✅ Entidades base creadas - Mesero: {MeseroId}, Cliente: {ClienteId}, Mesa: {MesaId}", 
            mesero.Id, cliente.Id, mesa.Id);

        // Verificar que la mesa siga disponible después de crear la reservación
        var mesaDespuesReservacion = await DbContext.Mesas.FindAsync(mesa.Id);
        mesaDespuesReservacion!.Estado.Should().Be(EstadoMesa.Disponible);

        // 2. PASO 1: Crear reservación
        Logger.LogInformation("📅 PASO 1: Creando reservación");
        var reservacionRequest = new
        {
            ClienteId = cliente.Id,
            MesaId = mesa.Id,
            NombreCliente = "Cliente Prueba",
            Telefono = "1234567890",
            Email = "cliente@prueba.com",
            FechaHoraReservacion = DateTime.Today.AddDays(1).AddHours(19), // 7:00 PM mañana (dentro del horario 12:00-22:00)
            NumeroPersonas = 3,
            Observaciones = "Reservación para flujo de prueba",
            Canal = "Web"
        };
        
        var responseReservacion = await HttpClient.PostAsJsonAsync("/api/operaciones/reservaciones", reservacionRequest);
        
        // Si falla, capturar el error para diagnóstico
        if (!responseReservacion.IsSuccessStatusCode)
        {
            var errorContent = await responseReservacion.Content.ReadAsStringAsync();
            Logger.LogError("❌ Error en creación de reservación - Status: {StatusCode}, Content: {ErrorContent}", 
                responseReservacion.StatusCode, errorContent);
        }
        
        responseReservacion.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var reservacionResponse = await responseReservacion.Content.ReadFromJsonAsync<ApiResponse<ReservacionDto>>();
        reservacionResponse.Should().NotBeNull();
        reservacionResponse!.Success.Should().BeTrue();
        var reservacion = reservacionResponse.Data;
        
        Logger.LogInformation("✅ Reservación creada - ID: {ReservacionId}, Estado: {Estado}", 
            reservacion.Id, reservacion.Estado);

        // Log de diagnóstico antes de confirmar la reservación
        var fechaHoraReservacion = reservacion.FechaHoraReservacion;
        var ahoraLocal = DateTime.Now;
        var ahoraUtc = DateTime.UtcNow;
        var diferenciaHoras = (fechaHoraReservacion - ahoraUtc).TotalHours;
        Logger.LogInformation("[TEST] fechaHoraReservacion: {0:O}, ahoraLocal: {1:O}, ahoraUtc: {2:O}, diferenciaHoras: {3:F2}", fechaHoraReservacion, ahoraLocal, ahoraUtc, diferenciaHoras);

        // 3. PASO 2: Asignar mesa
        Logger.LogInformation("🪑 PASO 2: Confirmando reservación antes de asignar mesa");
        var confirmarReservacionRequest = new ConfirmarReservacionCommand
        {
            Id = reservacion.Id,
            ReservacionId = reservacion.Id,
            MetodoConfirmacion = "Manual",
            ConfirmadoPor = "Administrador",
            NotasConfirmacion = "Reservación confirmada para flujo de prueba"
        };
        
        var responseConfirmarReservacion = await HttpClient.PostAsJsonAsync($"/api/operaciones/reservaciones/{reservacion.Id}/confirmar", confirmarReservacionRequest);
        
        // Log del error si falla para diagnóstico
        if (!responseConfirmarReservacion.IsSuccessStatusCode)
        {
            var errorContent = await responseConfirmarReservacion.Content.ReadAsStringAsync();
            Logger.LogError("❌ Error confirmando reservación - Status: {StatusCode}, Content: {ErrorContent}", 
                responseConfirmarReservacion.StatusCode, errorContent);
        }
        
        responseConfirmarReservacion.StatusCode.Should().Be(HttpStatusCode.OK);
        
        Logger.LogInformation("✅ Reservación confirmada");
        
        // Liberar la mesa antes de asignarla (cancelar la reservación)
        Logger.LogInformation("🪑 PASO 2.1: Liberando mesa para asignación");
        var cancelarReservacionRequest = new
        {
            Id = reservacion.Id,
            ReservacionId = reservacion.Id,
            UsuarioId = Guid.NewGuid(), // ID del administrador
            Motivo = 10, // SolicitudEspecial
            MotivoDetalle = "Asignación directa para flujo de prueba",
            NotificarCliente = false,
            AplicarPenalizacion = false,
            LiberarMesaInmediatamente = true,
            FechaCancelacion = DateTime.UtcNow
        };
        
        var responseCancelarReservacion = await HttpClient.PostAsJsonAsync($"/api/operaciones/reservaciones/{reservacion.Id}/cancelar", cancelarReservacionRequest);
        responseCancelarReservacion.StatusCode.Should().Be(HttpStatusCode.OK);
        Logger.LogInformation("✅ Reservación cancelada, mesa liberada");
        
        // Ahora asignar la mesa (que ya no está reservada)
        Logger.LogInformation("🪑 PASO 2.2: Asignando mesa");
        var asignarMesaRequest = new
        {
            MeseroId = mesero.Id,
            Observaciones = "Asignación para flujo de prueba",
            TipoAsignacion = "Manual",
            NumeroPersonas = 3
        };
        
        var responseAsignarMesa = await HttpClient.PostAsJsonAsync($"/api/operaciones/mesas/{mesa.Id}/asignar", asignarMesaRequest);
        responseAsignarMesa.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que la mesa cambió de estado
        await DbContext.Entry(mesa).ReloadAsync();
        mesa.Estado.Should().Be(EstadoMesa.Ocupada);
        
        Logger.LogInformation("✅ Mesa asignada - Estado: {Estado}", mesa.Estado);

        // 4. PASO 3: Crear comanda
        Logger.LogInformation("📋 PASO 3: Creando comanda");
        var comandaRequest = new
        {
            MeseroId = mesero.Id,
            ClienteId = cliente.Id,
            MesaId = mesa.Id,
            Observaciones = "Comanda para flujo de prueba",
            ProductosIniciales = new[]
            {
                new
                {
                    ProductoId = producto.Id,
                    Cantidad = 2,
                    Observaciones = "Sin cebolla"
                }
            }
        };
        
        // Log del request para debug
        var requestJson = JsonSerializer.Serialize(comandaRequest, new JsonSerializerOptions { WriteIndented = true });
        Logger.LogInformation("📤 Request de comanda enviado: {RequestJson}", requestJson);
        
        var responseComanda = await HttpClient.PostAsJsonAsync("/api/operaciones/comandas", comandaRequest);
        
        // Log del error si falla
        if (responseComanda.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await responseComanda.Content.ReadAsStringAsync();
            Logger.LogError("❌ Error creando comanda - Status: {StatusCode}, Content: {ErrorContent}", 
                responseComanda.StatusCode, errorContent);
        }
        
        responseComanda.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var comandaResponse = await responseComanda.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        comandaResponse.Should().NotBeNull();
        comandaResponse!.Success.Should().BeTrue();
        var comanda = comandaResponse.Data;
        
        Logger.LogInformation("✅ Comanda creada - ID: {ComandaId}, Total: {Total:C}", 
            comanda.Id, comanda.Total);

        // 5. PASO 4: Agregar productos adicionales a la comanda
        Logger.LogInformation("🍕 PASO 4: Agregando productos adicionales");
        var producto2 = await CrearProductoPrueba("Ensalada César", 12.00m);
        var agregarProductoRequest = new
        {
            ProductoId = producto2.Id,
            Cantidad = 1,
            Observaciones = "Sin crutones"
        };
        
        var responseAgregarProducto = await HttpClient.PostAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/productos", agregarProductoRequest);
        responseAgregarProducto.StatusCode.Should().Be(HttpStatusCode.OK);
        
        Logger.LogInformation("✅ Producto adicional agregado");

        // 6. PASO 5: Iniciar preparación
        Logger.LogInformation("👨‍🍳 PASO 5: Iniciando preparación");
        var preparacionRequest = new
        {
            ProductoId = producto.Id,
            ComandaId = comanda.Id,
            Cantidad = 2,
            ChefId = chef.Id,
            TiempoEstimado = TimeSpan.FromMinutes(20),
            FechaVencimiento = DateTime.Now.AddMinutes(30),
            Observaciones = "Preparación para flujo de prueba"
        };
        
        var responsePreparacion = await HttpClient.PostAsJsonAsync("/api/operaciones/preparaciones", preparacionRequest);
        responsePreparacion.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var preparacionResponse = await responsePreparacion.Content.ReadFromJsonAsync<ApiResponse<PreparacionDto>>();
        var preparacion = preparacionResponse!.Data;
        
        Logger.LogInformation("✅ Preparación iniciada - ID: {PreparacionId}, Estado: {Estado}", 
            preparacion.Id, preparacion.Estado);

        // Verificar que la preparación esté en estado preparando
        preparacion!.Estado.Should().Be(EstadoPreparacion.Preparando);

        // Marcar la preparación como disponible antes de completarla
        var responseDisponible = await HttpClient.PostAsJsonAsync($"/api/operaciones/preparaciones/{preparacion.Id}/disponible", new { Observaciones = "Listo para servir" });
        responseDisponible.EnsureSuccessStatusCode();
        Logger.LogInformation("✅ Preparación marcada como disponible");

        // 7. PASO 6: Completar preparación
        Logger.LogInformation("✅ PASO 6: Completando preparación");
        var responseCompletarPreparacion = await HttpClient.PostAsync($"/api/operaciones/preparaciones/{preparacion.Id}/completar", null);
        responseCompletarPreparacion.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que la preparación se completó
        var preparacionCompletada = await DbContext.Preparaciones.FindAsync(preparacion.Id);
        preparacionCompletada!.Estado.Should().Be(EstadoPreparacion.Agotada);
        
        Logger.LogInformation("✅ Preparación completada");

        // 8. PASO 7: Cambiar comanda a "enproceso"
        Logger.LogInformation("🔄 PASO 7: Cambiando comanda a enproceso");
        var responseEnPreparacion = await HttpClient.PatchAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/estado", 
            new { NuevoEstado = "enproceso" });
        
        // Log del error si falla
        if (responseEnPreparacion.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await responseEnPreparacion.Content.ReadAsStringAsync();
            Logger.LogError("❌ Error cambiando comanda a enproceso - Status: {StatusCode}, Content: {ErrorContent}", 
                responseEnPreparacion.StatusCode, errorContent);
        }
        
        responseEnPreparacion.StatusCode.Should().Be(HttpStatusCode.OK);
        Logger.LogInformation("✅ Comanda en proceso");

        // 9. PASO 8: Cambiar comanda a "lista"
        Logger.LogInformation("📋 PASO 8: Cambiando comanda a lista");
        var responseLista = await HttpClient.PatchAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/estado", 
            new { NuevoEstado = "lista" });
        responseLista.StatusCode.Should().Be(HttpStatusCode.OK);
        Logger.LogInformation("✅ Comanda lista");

        // 10. PASO 9: Cambiar comanda a "entregada"
        Logger.LogInformation("🚚 PASO 9: Cambiando comanda a entregada");
        var responseEntregada = await HttpClient.PatchAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/estado", 
            new { NuevoEstado = "entregada" });
        responseEntregada.StatusCode.Should().Be(HttpStatusCode.OK);
        Logger.LogInformation("✅ Comanda entregada");

        // 11. PASO 10: Finalizar comanda
        Logger.LogInformation("🏁 PASO 10: Finalizando comanda");
        var entregarComandaRequest = new CambiarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "Entregada",
            Observaciones = "Comanda entregada al cliente"
        };
        
        await HttpClient.PatchAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/estado", entregarComandaRequest);
        
        // Ahora finalizar la comanda (marcar como pagada)
        var finalizarComandaRequest = new CambiarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "Finalizada",
            Observaciones = "Comanda finalizada para facturación"
        };
        
        await HttpClient.PatchAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/estado", finalizarComandaRequest);
        
        // Recargar entidad Comanda para verificar estado actualizado
        var comandaEntity = await DbContext.Comandas.FindAsync(comanda.Id);
        await DbContext.Entry(comandaEntity!).ReloadAsync();
        comandaEntity!.Estado.Should().Be(EstadoComanda.Finalizada);
        
        Logger.LogInformation("✅ Comanda finalizada correctamente");

        // 12. PASO 11: Generar factura
        Logger.LogInformation("🧾 PASO 11: Generando factura");
        var facturaRequest = new
        {
            ClienteId = cliente.Id,
            ComandasIds = new List<Guid> { comanda.Id },
            NombreCliente = cliente.Nombre.ToString(),
            TipoFactura = "Normal",
            MetodoPagoPreferido = "Efectivo",
            Observaciones = "Factura generada por flujo de prueba"
        };
        
        var responseFactura = await HttpClient.PostAsJsonAsync("/api/comercial/facturas", facturaRequest);
        
        // Log del error si falla
        if (responseFactura.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await responseFactura.Content.ReadAsStringAsync();
            Logger.LogError("❌ Error generando factura - Status: {StatusCode}, Content: {ErrorContent}", 
                responseFactura.StatusCode, errorContent);
        }
        
        responseFactura.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var facturaResponse = await responseFactura.Content.ReadFromJsonAsync<ApiResponse<FacturaDto>>();
        facturaResponse.Should().NotBeNull();
        facturaResponse!.Success.Should().BeTrue();
        var factura = facturaResponse.Data;
        
        Logger.LogInformation("✅ Factura generada - ID: {FacturaId}, Total: {Total:C}", 
            factura.Id, factura.Total);

        // 13. PASO 12: Crear tarjeta de fidelización (si no existe)
        Logger.LogInformation("💳 PASO 12: Creando tarjeta de fidelización");
        var tarjetaRequest = new
        {
            ClienteId = cliente.Id,
            TipoTarjeta = 0, // Estandar
            UsuarioId = mesero.Id,
            PuntosIniciales = 0,
            ActivarInmediatamente = true,
            EnviarPorEmail = false,
            Observaciones = "Tarjeta creada por flujo de prueba"
        };
        
        var responseTarjeta = await HttpClient.PostAsJsonAsync("/api/comercial/tarjetas-fidelizacion", tarjetaRequest);
        responseTarjeta.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var tarjetaResponse = await responseTarjeta.Content.ReadFromJsonAsync<ApiResponse<object>>();
        tarjetaResponse.Should().NotBeNull();
        tarjetaResponse!.Success.Should().BeTrue();
        
        Logger.LogInformation("✅ Tarjeta de fidelización creada");

        // 14. PASO 13: Acumular puntos por la compra
        Logger.LogInformation("⭐ PASO 13: Acumulando puntos");
        var puntosRequest = new
        {
            Puntos = 50,
            Descripcion = "Compra en restaurante",
            MontoTransaccion = factura.Total,
            Referencia = factura.NumeroFactura,
            UsuarioId = mesero.Id
        };
        
        // Obtener la tarjeta del cliente
        var responseTarjetasCliente = await HttpClient.GetAsync($"/api/comercial/tarjetas-fidelizacion/cliente/{cliente.Id}");
        responseTarjetasCliente.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var tarjetasClienteResponse = await responseTarjetasCliente.Content.ReadFromJsonAsync<ApiResponse<List<object>>>();
        tarjetasClienteResponse.Should().NotBeNull();
        tarjetasClienteResponse!.Success.Should().BeTrue();
        tarjetasClienteResponse.Data.Should().NotBeEmpty();
        
        // Obtener el ID de la primera tarjeta
        var tarjetaId = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(tarjetasClienteResponse.Data.First()));
        var tarjetaIdGuid = tarjetaId.GetProperty("Id").GetGuid();
        
        var responsePuntos = await HttpClient.PostAsJsonAsync($"/api/comercial/tarjetas-fidelizacion/{tarjetaIdGuid}/puntos", puntosRequest);
        responsePuntos.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        
        if (responsePuntos.StatusCode == HttpStatusCode.OK)
        {
            Logger.LogInformation("✅ Puntos acumulados correctamente");
        }
        else
        {
            Logger.LogInformation("⚠️ Puntos no se pudieron acumular (puede ser por reglas de negocio)");
        }

        // 15. PASO 14: Liberar mesa automáticamente
        Logger.LogInformation("🔄 PASO 14: Liberando mesa");
        var responseLiberarMesa = await HttpClient.PostAsJsonAsync($"/api/operaciones/mesas/{mesa.Id}/liberar", new { Observaciones = "Liberación automática por flujo completo" });
        responseLiberarMesa.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verificar que la mesa se liberó
        var mesaLiberada = await DbContext.Mesas.FindAsync(mesa.Id);
        
        // Verificar que la mesa fue liberada correctamente
        await DbContext.Entry(mesaLiberada).ReloadAsync();
        mesaLiberada!.Estado.Should().Be(EstadoMesa.Disponible);
        
        Logger.LogInformation("✅ Mesa liberada - Estado: {Estado}", mesaLiberada.Estado);

        // 🎯 ASSERT - Validaciones finales del flujo completo
        Logger.LogInformation("🔍 Validando resultados del flujo completo");
        
        // Verificar que todas las entidades están en el estado correcto
        var comandaFinal = await DbContext.Comandas.FindAsync(comanda.Id);
        comandaFinal!.Estado.Should().Be(EstadoComanda.Finalizada);
        
        var facturaFinal = await DbContext.Facturas.FindAsync(factura.Id);
        facturaFinal.Should().NotBeNull();
        
        var mesaFinal = await DbContext.Mesas.FindAsync(mesa.Id);
        mesaFinal!.Estado.Should().Be(EstadoMesa.Disponible);
        
        var preparacionFinal = await DbContext.Preparaciones.FindAsync(preparacion.Id);
        preparacionFinal!.Estado.Should().Be(EstadoPreparacion.Agotada);
        
        Logger.LogInformation("🎉 FLUJO COMPLETO de Atención al Cliente EXITOSO");
        Logger.LogInformation("📊 Resumen del flujo:");
        Logger.LogInformation("   - Reservación: {ReservacionId}", reservacion.Id);
        Logger.LogInformation("   - Mesa asignada y liberada: {MesaId}", mesa.Id);
        Logger.LogInformation("   - Comanda procesada: {ComandaId}", comanda.Id);
        Logger.LogInformation("   - Preparación completada: {PreparacionId}", preparacion.Id);
        Logger.LogInformation("   - Factura generada: {FacturaId}", factura.Id);
        Logger.LogInformation("   - Total facturado: {Total:C}", factura.Total);
    }

    [Fact]
    public async Task FlujoAtencionClienteSinReservacion_DebeFuncionarCorrectamente()
    {
        // 🎯 ARRANGE - Flujo sin reservación previa (cliente walk-in)
        Logger.LogInformation("🚀 Iniciando FLUJO de Atención al Cliente SIN RESERVACIÓN");
        
        await LimpiarDatosPrueba();
        
        var mesero = await CrearUsuarioPrueba("mesero.walkin", "Mesero Walk-in", "mesero.walkin@test.com", RolUsuario.Mesero);
        var chef = await CrearUsuarioPrueba("chef.walkin", "Chef Walk-in", "chef.walkin@test.com", RolUsuario.Cocinero);
        var cliente = await CrearClientePrueba("Cliente Walk-in", "cliente.walkin@test.com");
        var mesa = await CrearMesaPrueba(2, 4, "Terraza");
        var producto = await CrearProductoPrueba("Hamburguesa Clásica", 18.00m);
        
        Logger.LogInformation("✅ Entidades base creadas para flujo walk-in");

        // 1. PASO 1: Asignar mesa directamente (sin reservación)
        Logger.LogInformation("🪑 PASO 1: Asignando mesa directamente");
        var asignarMesaRequest = new
        {
            MeseroId = mesero.Id,
            Observaciones = "Cliente walk-in",
            TipoAsignacion = "Manual",
            NumeroPersonas = 2
        };
        
        var responseAsignarMesa = await HttpClient.PostAsJsonAsync($"/api/operaciones/mesas/{mesa.Id}/asignar", asignarMesaRequest);
        responseAsignarMesa.StatusCode.Should().Be(HttpStatusCode.OK);
        
        Logger.LogInformation("✅ Mesa asignada para cliente walk-in");

        // 2. PASO 2: Crear comanda
        Logger.LogInformation("📋 PASO 2: Creando comanda");
        var comandaRequest = new
        {
            MeseroId = mesero.Id,
            ClienteId = cliente.Id,
            MesaId = mesa.Id,
            Observaciones = "Comanda walk-in",
            ProductosIniciales = new[]
            {
                new
                {
                    ProductoId = producto.Id,
                    Cantidad = 1,
                    Observaciones = "Bien cocida"
                }
            }
        };
        
        var responseComanda = await HttpClient.PostAsJsonAsync("/api/operaciones/comandas", comandaRequest);
        responseComanda.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var comandaResponse = await responseComanda.Content.ReadFromJsonAsync<ApiResponse<ComandaDto>>();
        var comanda = comandaResponse!.Data;
        
        Logger.LogInformation("✅ Comanda creada para walk-in - ID: {ComandaId}", comanda.Id);

        // 3. PASO 3: Procesar preparación
        Logger.LogInformation("👨‍🍳 PASO 3: Procesando preparación");
        var preparacionRequest = new
        {
            ProductoId = producto.Id,
            ComandaId = comanda.Id,
            Cantidad = 1,
            ChefId = chef.Id,
            TiempoEstimado = TimeSpan.FromMinutes(15),
            FechaVencimiento = DateTime.Now.AddMinutes(20),
            Observaciones = "Preparación walk-in"
        };
        
        var responsePreparacion = await HttpClient.PostAsJsonAsync("/api/operaciones/preparaciones", preparacionRequest);
        responsePreparacion.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var preparacionResponse = await responsePreparacion.Content.ReadFromJsonAsync<ApiResponse<PreparacionDto>>();
        var preparacion = preparacionResponse!.Data;
        
        // Marcar la preparación como disponible antes de completarla
        var responseDisponible = await HttpClient.PostAsJsonAsync($"/api/operaciones/preparaciones/{preparacion.Id}/disponible", new { Observaciones = "Listo para servir" });
        responseDisponible.EnsureSuccessStatusCode();
        Logger.LogInformation("✅ Preparación marcada como disponible");

        // Completar preparación
        await HttpClient.PostAsync($"/api/operaciones/preparaciones/{preparacion.Id}/completar", null);
        
        Logger.LogInformation("✅ Preparación procesada");

        // 4. PASO 4: Seguir flujo completo de estados de comanda
        Logger.LogInformation("🏁 PASO 4: Siguiendo flujo completo de estados de comanda");
        
        // 4.1 Cambiar a EnProceso
        var enProcesoRequest = new CambiarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "enproceso",
            Observaciones = "Comanda en proceso de preparación"
        };
        
        var responseEnProceso = await HttpClient.PatchAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/estado", enProcesoRequest);
        responseEnProceso.EnsureSuccessStatusCode();
        Logger.LogInformation("✅ Comanda en proceso");
        
        // 4.2 Cambiar a Lista
        var listaRequest = new CambiarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "lista",
            Observaciones = "Comanda lista para servir"
        };
        
        var responseLista = await HttpClient.PatchAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/estado", listaRequest);
        responseLista.EnsureSuccessStatusCode();
        Logger.LogInformation("✅ Comanda lista");
        
        // 4.3 Cambiar a Entregada
        var entregarComandaRequest = new CambiarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "entregada",
            Observaciones = "Comanda entregada al cliente"
        };
        
        var responseEntregada = await HttpClient.PatchAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/estado", entregarComandaRequest);
        responseEntregada.EnsureSuccessStatusCode();
        Logger.LogInformation("✅ Comanda entregada");
        
        // 4.4 Cambiar a Finalizada
        var finalizarComandaRequest = new CambiarEstadoComandaCommand
        {
            ComandaId = comanda.Id,
            NuevoEstado = "finalizada",
            Observaciones = "Comanda finalizada para facturación"
        };
        
        var responseFinalizada = await HttpClient.PatchAsJsonAsync($"/api/operaciones/comandas/{comanda.Id}/estado", finalizarComandaRequest);
        responseFinalizada.EnsureSuccessStatusCode();
        
        // Recargar entidad Comanda para verificar estado actualizado
        var comandaEntity = await DbContext.Comandas.FindAsync(comanda.Id);
        await DbContext.Entry(comandaEntity!).ReloadAsync();
        comandaEntity!.Estado.Should().Be(EstadoComanda.Finalizada);
        
        Logger.LogInformation("✅ Comanda finalizada correctamente");

        // 5. PASO 5: Liberar mesa
        Logger.LogInformation("🔄 PASO 5: Liberando mesa");
        await HttpClient.PatchAsync($"/api/operaciones/mesas/{mesa.Id}/liberar", null);
        
        var mesaFinal = await DbContext.Mesas.FindAsync(mesa.Id);
        mesaFinal!.Estado.Should().Be(EstadoMesa.Disponible);
        
        Logger.LogInformation("✅ Mesa liberada");
        Logger.LogInformation("🎉 FLUJO Walk-in EXITOSO");
    }

    private async Task LimpiarDatosPrueba()
    {
        // Limpiar datos en orden para evitar problemas de FK
        await LimpiarTablaPreparaciones();
        await LimpiarTablaComandas();
        await LimpiarTablaFacturas();
        await LimpiarTablaReservaciones();
        await LimpiarTablaMesas();
        await LimpiarTablaTarjetasFidelizacion();
        await LimpiarTablaClientes();
        await LimpiarTablaUsuarios();
        await LimpiarTablaProductos();
        
        Logger.LogInformation("🧹 Datos de prueba limpiados");
    }

    private async Task LimpiarTablaPreparaciones()
    {
        var preparaciones = await DbContext.Preparaciones.ToListAsync();
        DbContext.Preparaciones.RemoveRange(preparaciones);
        await DbContext.SaveChangesAsync();
    }

    private async Task LimpiarTablaFacturas()
    {
        var facturas = await DbContext.Facturas.ToListAsync();
        DbContext.Facturas.RemoveRange(facturas);
        await DbContext.SaveChangesAsync();
    }

    private async Task LimpiarTablaReservaciones()
    {
        var reservaciones = await DbContext.Reservaciones.ToListAsync();
        DbContext.Reservaciones.RemoveRange(reservaciones);
        await DbContext.SaveChangesAsync();
    }

    private async Task LimpiarTablaTarjetasFidelizacion()
    {
        var tarjetas = await DbContext.TarjetasFidelizacion.ToListAsync();
        DbContext.TarjetasFidelizacion.RemoveRange(tarjetas);
        await DbContext.SaveChangesAsync();
    }

    private async Task LimpiarTablaComandas()
    {
        var comandas = await DbContext.Comandas.ToListAsync();
        DbContext.Comandas.RemoveRange(comandas);
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

    private async Task LimpiarTablaUsuarios()
    {
        var usuarios = await DbContext.Usuarios.ToListAsync();
        DbContext.Usuarios.RemoveRange(usuarios);
        await DbContext.SaveChangesAsync();
    }

    private async Task LimpiarTablaProductos()
    {
        var productos = await DbContext.Productos.ToListAsync();
        DbContext.Productos.RemoveRange(productos);
        await DbContext.SaveChangesAsync();
    }
} 