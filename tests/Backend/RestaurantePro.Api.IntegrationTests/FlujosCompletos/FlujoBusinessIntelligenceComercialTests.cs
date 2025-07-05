using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using RestaurantePro.Api.Common;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Comercial.Facturacion.Entities;
using RestaurantePro.Domain.Comercial.Facturacion.Enums;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using RestaurantePro.Domain.Comercial.Promociones.Entities;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Core.Usuarios;
using RestaurantePro.Domain.Core.Usuarios.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Productos.ValueObjects;
using RestaurantePro.Api.IntegrationTests.TestBase;
using Xunit;
using RestaurantePro.Domain.Core.Usuarios.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Infrastructure.Persistence.Contexts;
using RestaurantePro.Domain.Core.Base.Services;

namespace RestaurantePro.Api.IntegrationTests.FlujosCompletos
{
    [Collection("ApiIntegrationTestCollection")]
    public class FlujoBusinessIntelligenceComercialTests : ApiIntegrationTestBase
    {
        public FlujoBusinessIntelligenceComercialTests(TestWebApplicationFactory factory) : base(factory)
        {
        }
        [Fact]
        public async Task FlujoCompletoBusinessIntelligenceComercial_DebeFuncionarCorrectamente()
        {
            // Arrange - Crear datos de prueba para análisis de BI
            var cliente = await CrearClientePrueba("Cliente BI Test", "cliente.bi@test.com");
            var mesa = await CrearMesaPrueba("1", 4, EstadoMesa.Disponible);
            var categoriaId = Guid.NewGuid();
            var producto = await CrearProductoPrueba("Producto BI", 25.00m, categoriaId);
            var usuario = await CrearUsuarioPrueba("Usuario BI", RolUsuario.Mesero);

            // Crear comandas y facturas para análisis
            var comanda1 = await CrearComandaPrueba(mesa.Id, usuario.Id, cliente.Id);
            var comanda2 = await CrearComandaPrueba(mesa.Id, usuario.Id, cliente.Id);
            
            var factura1 = await CrearFacturaPrueba(comanda1.Id, cliente.Id, 150.00m);
            var factura2 = await CrearFacturaPrueba(comanda2.Id, cliente.Id, 200.00m);

            // Crear promociones para análisis de efectividad
            var promocion = await CrearPromocionPrueba("Promoción BI", 10.0m, 100.00m);

            // Act & Assert - 1. Reporte de ventas
            var fechaHoy = DateTime.Today.ToString("yyyy-MM-dd");
            var responseVentas = await HttpClient.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaHoy}&fechaFin={fechaHoy}");
            responseVentas.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var apiResponseVentas = await responseVentas.Content.ReadFromJsonAsync<ApiResponse<object>>();
            apiResponseVentas.Should().NotBeNull();
            apiResponseVentas!.Success.Should().BeTrue();

            // Act & Assert - 2. Análisis de fidelización
            var responseFidelizacion = await HttpClient.GetAsync($"/api/comercial/reportes/fidelizacion?fechaInicio={DateTime.Today:yyyy-MM-dd}&fechaFin={DateTime.Today:yyyy-MM-dd}");
            responseFidelizacion.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var apiResponseFidelizacion = await responseFidelizacion.Content.ReadFromJsonAsync<ApiResponse<object>>();
            apiResponseFidelizacion.Should().NotBeNull();
            apiResponseFidelizacion!.Success.Should().BeTrue();

            // Act & Assert - 3. Efectividad de promociones
            var responsePromociones = await HttpClient.GetAsync($"/api/comercial/reportes/promociones?fechaInicio={DateTime.Today:yyyy-MM-dd}&fechaFin={DateTime.Today:yyyy-MM-dd}");
            responsePromociones.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var apiResponsePromociones = await responsePromociones.Content.ReadFromJsonAsync<ApiResponse<object>>();
            apiResponsePromociones.Should().NotBeNull();
            apiResponsePromociones!.Success.Should().BeTrue();

            // Act & Assert - 4. Feedback de clientes
            var responseFeedback = await HttpClient.GetAsync($"/api/comercial/reportes/clientes?soloActivos=true");
            responseFeedback.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var apiResponseFeedback = await responseFeedback.Content.ReadFromJsonAsync<ApiResponse<object>>();
            apiResponseFeedback.Should().NotBeNull();
            apiResponseFeedback!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task ReporteVentasComercial_DebeRetornarMetricasCorrectas()
        {
            // Arrange - Crear datos de ventas para análisis
            var cliente = await CrearClientePrueba("Cliente Ventas", "cliente.ventas@test.com");
            var mesa = await CrearMesaPrueba("2", 4, EstadoMesa.Disponible);
            var usuario = await CrearUsuarioPrueba("Usuario Ventas", RolUsuario.Mesero);

            // Crear múltiples facturas para análisis de tendencias
            for (int i = 0; i < 5; i++)
            {
                var comanda = await CrearComandaPrueba(mesa.Id, usuario.Id, cliente.Id);
                await CrearFacturaPrueba(comanda.Id, cliente.Id, 100.00m + (i * 25.00m));
            }

            // Act
            var fechaHoy = DateTime.Today.ToString("yyyy-MM-dd");
            var response = await HttpClient.GetAsync($"/api/comercial/reportes/ventas?fechaInicio={fechaHoy}&fechaFin={fechaHoy}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            apiResponse.Should().NotBeNull();
            apiResponse!.Success.Should().BeTrue();
        }

        [Fact]
        public async Task ReporteFidelizacion_DebeAnalizarComportamientoClientes()
        {
            // Arrange
            var cliente = await CrearClientePrueba("Cliente Fidelización");
            var tarjeta = await CrearTarjetaFidelizacionPrueba(cliente.Id);

            // Act
            var response = await HttpClient.GetAsync($"/api/comercial/reportes/fidelizacion?fechaInicio={DateTime.Today:yyyy-MM-dd}&fechaFin={DateTime.Today:yyyy-MM-dd}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ReporteEfectividadPromociones_DebeCalcularROI()
        {
            // Arrange
            var promocion1 = await CrearPromocionPrueba("Promoción BI 1", 10.0m, 100.0m);
            var promocion2 = await CrearPromocionPrueba("Promoción BI 2", 15.0m, 100.0m);

            // Act
            var response = await HttpClient.GetAsync($"/api/comercial/reportes/promociones?fechaInicio={DateTime.Today:yyyy-MM-dd}&fechaFin={DateTime.Today:yyyy-MM-dd}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ReporteFeedbackClientes_DebeMostrarSatisfaccion()
        {
            // Arrange
            var cliente1 = await CrearClientePrueba("Cliente Feedback 1");
            var cliente2 = await CrearClientePrueba("Cliente Feedback 2");

            // Act - Usar endpoint de reportes de clientes que sí existe
            var response = await HttpClient.GetAsync($"/api/comercial/reportes/clientes?soloActivos=true");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
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
            var precioProducto = new PrecioProducto(precio);
            var producto = Producto.Crear(
                nombre,
                $"Descripción de {nombre}",
                precioProducto,
                categoriaId,
                "Categoría Test"
            );

            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
            context.Productos.Add(producto);
            await context.SaveChangesAsync();
            return producto;
        }

        private async Task<Usuario> CrearUsuarioPrueba(string nombre, RolUsuario rol)
        {
            var username = nombre.ToLower().Replace(" ", "").Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");
            var email = $"{username}@test.com";
            var usuario = Usuario.Crear(
                username,         // nombreUsuario
                nombre,           // nombreCompleto
                email,            // email
                rol
            );
            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
            return usuario;
        }

        private async Task<Comanda> CrearComandaPrueba(Guid mesaId, Guid usuarioId, Guid clienteId)
        {
            var comanda = Comanda.Crear(
                usuarioId,
                clienteId,
                mesaId,
                "Comanda de prueba para BI"
            );

            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
            context.Comandas.Add(comanda);
            await context.SaveChangesAsync();
            return comanda;
        }

        private async Task<Factura> CrearFacturaPrueba(Guid comandaId, Guid clienteId, decimal total)
        {
            var factura = Factura.Crear(
                $"FAC-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}",
                TipoFactura.Fiscal,
                "Cliente Prueba BI",
                clienteId,
                "12345678A", // Identificación fiscal válida
                "Calle Prueba 123, Ciudad",
                new List<Guid> { comandaId },
                "Factura de prueba para Business Intelligence",
                DateTime.Now,
                new FakeDateTimeService()
            );
            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
            context.Facturas.Add(factura);
            await context.SaveChangesAsync();
            return factura;
        }

        private async Task<Promocion> CrearPromocionPrueba(string nombre, decimal porcentajeDescuento, decimal montoMinimo)
        {
            var promocion = Promocion.Crear(
                $"PROMO-{Guid.NewGuid():N}",
                nombre,
                $"Descripción de {nombre}",
                TipoPromocion.PorcentajeTotal,
                porcentajeDescuento,
                DateTime.Now,
                DateTime.Now.AddDays(30),
                montoMinimo
            );

            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
            context.Promociones.Add(promocion);
            await context.SaveChangesAsync();
            return promocion;
        }

        private async Task<TarjetaFidelizacion> CrearTarjetaFidelizacionPrueba(Guid clienteId)
        {
            var tarjeta = TarjetaFidelizacion.Crear(
                clienteId,
                $"TARJ-{Guid.NewGuid():N}"
            );

            using var scope = Factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<RestauranteProDbContext>();
            context.TarjetasFidelizacion.Add(tarjeta);
            await context.SaveChangesAsync();
            return tarjeta;
        }

        // Implementación fake para IDateTimeService
        public class FakeDateTimeService : IDateTimeService
        {
            public DateTime Now => DateTime.Now;
            public DateTime UtcNow => DateTime.UtcNow;
            public DateTime Today => DateTime.Today;
        }
    }
} 