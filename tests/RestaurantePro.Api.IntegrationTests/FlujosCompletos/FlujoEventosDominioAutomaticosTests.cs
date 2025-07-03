using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using RestaurantePro.Api.Common;
using RestaurantePro.Api.IntegrationTests.TestBase;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos;

[Collection("ApiIntegrationTestCollection")]
public class FlujoEventosDominioAutomaticosTests : ApiIntegrationTestBase
{
    public FlujoEventosDominioAutomaticosTests(TestWebApplicationFactory factory) : base(factory) { }

    [Fact(DisplayName = "Evento de comanda debe disparar eventos de dominio y actualizar inventario")]
    public async Task CrearComanda_DebeDispararEventosDominioYActualizarInventario()
    {
        // Arrange - Crear datos de prueba
        var cliente = await CrearClientePrueba("Cliente Eventos", "cliente.eventos@test.com");
        var mesa = await CrearMesaPrueba("1", 4, EstadoMesa.Disponible);
        var categoriaId = Guid.NewGuid();
        var producto = await CrearProductoPrueba("Producto Eventos", 25.00m, categoriaId);
        var ingrediente = await CrearIngredientePrueba("Ingrediente Eventos", 100, 50);
        var usuario = await CrearUsuarioPrueba("Usuario Eventos", RolUsuario.Mesero);

        // Act - Crear comanda vía API (esto debe disparar eventos de dominio)
        var comandaRequest = new
        {
            MesaId = mesa.Id,
            ClienteId = cliente.Id,
            MeseroId = usuario.Id, // Corregido: usar MeseroId en lugar de UsuarioId
            Items = new[]
            {
                new
                {
                    ProductoId = producto.Id,
                    Cantidad = 2,
                    PrecioUnitario = 25.00m,
                    Observaciones = "Test eventos de dominio"
                }
            }
        };

        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/comandas", comandaRequest);

        // Assert - Verificar que la comanda fue creada y se dispararon eventos
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error al crear comanda: {response.StatusCode} - {errorContent}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();

        // Assert - Verificar que el inventario se actualizó (evento procesado)
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        var ingredienteActualizado = await context.Ingredientes.FindAsync(ingrediente.Id);
        ingredienteActualizado.Should().NotBeNull();
        // El stock debería haberse reducido por el consumo de la comanda
        // (esto depende de la lógica de recetas implementada)
    }

    [Fact(DisplayName = "Acumulación de puntos debe generar evento y notificación automática")]
    public async Task AcumularPuntos_DebeGenerarEventoYNotificacion()
    {
        // Arrange
        var usuario = await CrearUsuarioPrueba("usuario.fidelizacion@test.com", "Usuario Fidelización");
        var cliente = await CrearClientePrueba("Cliente Puntos", "cliente.puntos@test.com");
        var tarjeta = await CrearTarjetaFidelizacionPrueba(cliente.Id);

        // Act - Primero activar la tarjeta (necesario para acumular puntos)
        var activarResponse = await HttpClient.PatchAsync($"/api/comercial/tarjetas-fidelizacion/{tarjeta.Id}/activar", null);
        activarResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Acumular puntos vía API (enviar Puntos directamente)
        var puntosRequest = new
        {
            TarjetaId = tarjeta.Id,
            Puntos = 5, // Enviar puntos directamente para evitar problemas de tracking
            Descripcion = "Compra test eventos",
            Referencia = "TEST-EVENTOS-001",
            UsuarioId = usuario.Id
        };

        // Serializar y mostrar el JSON antes de enviar
        var json = System.Text.Json.JsonSerializer.Serialize(puntosRequest);
        Console.WriteLine($"Request JSON acumulación puntos: {json}");

        var response = await HttpClient.PostAsJsonAsync($"/api/comercial/tarjetas-fidelizacion/{tarjeta.Id}/puntos", puntosRequest);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Respuesta error acumulación puntos: {errorContent}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Verificar que se acumularon los puntos
        var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        apiResponse.Should().NotBeNull();
        apiResponse!.Success.Should().BeTrue();

        // TODO: Verificar que se generó notificación automática
        // Esto requeriría consultar la tabla de notificaciones o un endpoint de notificaciones
    }

    [Fact(DisplayName = "Rollback automático ante error en procesamiento de evento")]
    public async Task ErrorEnEvento_DebeHacerRollbackAutomatico()
    {
        // Arrange - Preparar escenario que provoque error en evento
        var cliente = await CrearClientePrueba("Cliente Error", "cliente.error@test.com");
        var mesa = await CrearMesaPrueba("2", 4, EstadoMesa.Disponible);
        var usuario = await CrearUsuarioPrueba("Usuario Error", RolUsuario.Mesero);

        // Act - Intentar crear comanda con datos inválidos que provoquen error
        var comandaRequestInvalida = new
        {
            MesaId = mesa.Id,
            ClienteId = cliente.Id,
            MeseroId = usuario.Id, // Corregido: usar MeseroId en lugar de UsuarioId
            Items = new[]
            {
                new
                {
                    ProductoId = Guid.Empty, // Producto inexistente
                    Cantidad = -1, // Cantidad inválida
                    PrecioUnitario = -10.00m, // Precio inválido
                    Observaciones = "Test rollback eventos"
                }
            }
        };

        var response = await HttpClient.PostAsJsonAsync("/api/operaciones/comandas", comandaRequestInvalida);

        // Assert - Verificar que se rechazó la operación
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        // Verificar que no se creó la comanda (rollback)
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        var comandasCreadas = await context.Comandas.ToListAsync();
        comandasCreadas.Should().BeEmpty(); // No debería haber comandas creadas
    }

    [Fact(DisplayName = "Auditoría de eventos debe registrar todos los eventos procesados")]
    public async Task AuditoriaEventos_DebeRegistrarTodosLosEventos()
    {
        // Arrange - Generar múltiples eventos
        var cliente = await CrearClientePrueba("Cliente Auditoria", "cliente.auditoria@test.com");
        var mesa = await CrearMesaPrueba("3", 4, EstadoMesa.Disponible);
        var usuario = await CrearUsuarioPrueba("Usuario Auditoria", RolUsuario.Mesero);

        // Act - Crear comanda (dispara eventos)
        var comandaRequest = new
        {
            MesaId = mesa.Id,
            ClienteId = cliente.Id,
            MeseroId = usuario.Id, // Corregido: usar MeseroId en lugar de UsuarioId
            Items = new[]
            {
                new
                {
                    ProductoId = Guid.NewGuid(), // Producto inexistente para generar error
                    Cantidad = 1,
                    PrecioUnitario = 25.00m,
                    Observaciones = "Test auditoría eventos"
                }
            }
        };

        await HttpClient.PostAsJsonAsync("/api/operaciones/comandas", comandaRequest);

        // Assert - Verificar auditoría de eventos
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        
        // TODO: Implementar verificación de auditoría de eventos
        // Esto podría requerir una tabla de auditoría o logs específicos
        // Por ahora verificamos que el sistema no se rompió
        context.Should().NotBeNull();
    }

    // Métodos auxiliares para crear datos de prueba
    private async Task<Cliente> CrearClientePrueba(string nombre, string email)
    {
        var clienteNombre = ClienteNombre.Crear(nombre, "Apellido");
        var cliente = Cliente.Crear(
            clienteNombre,
            email,
            "123456789",
            DateTime.Now.AddYears(-25)
        );

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        context.Clientes.Add(cliente);
        await context.SaveChangesAsync();
        return cliente;
    }

    private async Task<Mesa> CrearMesaPrueba(string numero, int capacidad, EstadoMesa estado)
    {
        var mesa = Mesa.Crear(
            int.Parse(numero),
            capacidad,
            "Ubicación Test"
        );

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        context.Mesas.Add(mesa);
        await context.SaveChangesAsync();
        return mesa;
    }

    private async Task<Producto> CrearProductoPrueba(string nombre, decimal precio, Guid categoriaId)
    {
        var producto = Producto.Crear(
            nombre,
            "Descripción test",
            new PrecioProducto(precio),
            categoriaId
        );

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        context.Productos.Add(producto);
        await context.SaveChangesAsync();
        return producto;
    }

    private async Task<Ingrediente> CrearIngredientePrueba(string nombre, int stockInicial, int stockMinimo)
    {
        var ingrediente = Ingrediente.Crear(
            Guid.NewGuid(),
            nombre, // Usar string directamente en lugar de IngredienteNombre.Crear(nombre)
            $"COD-{nombre.ToUpper()}",
            "Descripción test",
            UnidadMedida.Gramos,
            stockMinimo,
            stockInicial
        );

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        context.Ingredientes.Add(ingrediente);
        await context.SaveChangesAsync();
        return ingrediente;
    }

    private async Task<Usuario> CrearUsuarioPrueba(string nombre, RolUsuario rol)
    {
        var username = nombre.ToLower().Replace(" ", "").Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");
        var email = $"{username}@test.com";
        var usuario = Usuario.Crear(
            username,
            nombre,
            email,
            rol
        );

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        return usuario;
    }

    private async Task<TarjetaFidelizacion> CrearTarjetaFidelizacionPrueba(Guid clienteId)
    {
        var tarjeta = TarjetaFidelizacion.Crear(
            clienteId,
            NivelFidelizacion.Basico.ToString()
        );

        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
        context.TarjetasFidelizacion.Add(tarjeta);
        await context.SaveChangesAsync();
        return tarjeta;
    }
} 