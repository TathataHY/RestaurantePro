namespace RestaurantePro.Domain.UnitTests.Operaciones.Comandas.Entities
{
    using Xunit;
    using FluentAssertions;
    using System;
    using System.Linq;
    using System.Reflection;
    using RestaurantePro.Domain.Operaciones.Comandas.Entities;
    using RestaurantePro.Domain.Operaciones.Comandas.Enums;
    using RestaurantePro.Domain.Operaciones.Comandas.Events;

    public class ComandaTests
    {
        [Fact]
        public void CrearComanda_ConParametrosValidos_DebeCrearComandaConEstadoCreada()
        {
            // Arrange
            var mesaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var clienteId = Guid.NewGuid();
            var observaciones = "Observaciones de prueba";

            // Act
            var comanda = Comanda.Crear(mesaId, meseroId, clienteId, observaciones);

            // Assert
            comanda.Should().NotBeNull();
            comanda.Id.Should().NotBe(Guid.Empty);
            comanda.MesaId.Should().Be(mesaId);
            comanda.MeseroId.Should().Be(meseroId);
            comanda.ClienteId.Should().Be(clienteId);
            comanda.Observaciones.Should().Be(observaciones);
            comanda.Estado.Should().Be(EstadoComanda.Creada);
            comanda.Items.Should().BeEmpty();

            // Verificamos que se generó el evento de dominio
            comanda.DomainEvents.Should().ContainSingle(e => e is ComandaCreada);
            var evento = comanda.DomainEvents.OfType<ComandaCreada>().First();
            evento.ComandaId.Should().Be(comanda.Id);
            evento.MesaId.Should().Be(mesaId);
            evento.MeseroId.Should().Be(meseroId);
        }

        [Fact]
        public void AgregarProducto_CuandoComandaEstaActiva_DebeAgregarProductoYRecalcularTotal()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            var productoId = Guid.NewGuid();
            var cantidad = 2;
            var precioUnitario = 100m;
            var observaciones = "Sin cebolla";

            // Act
            comanda.AgregarProducto(productoId, cantidad, precioUnitario, observaciones);

            // Assert
            comanda.Items.Should().HaveCount(1);
            var item = comanda.Items.First();
            item.ProductoId.Should().Be(productoId);
            item.Cantidad.Should().Be(cantidad);
            item.PrecioUnitario.Should().Be(precioUnitario);
            item.Subtotal.Should().Be(cantidad * precioUnitario);
            item.Observaciones.Should().Be(observaciones);

            // Verificar el total
            comanda.Total.Should().NotBeNull();
            comanda.Total.Subtotal.Should().Be(cantidad * precioUnitario);
            comanda.Total.Impuestos.Should().Be(cantidad * precioUnitario * 0.16m);
            comanda.Total.Total.Should().Be(cantidad * precioUnitario * 1.16m);

            // Verificar evento de dominio
            comanda.DomainEvents.Should().Contain(e => e is ProductoAgregadoAComanda);
            var evento = comanda.DomainEvents.OfType<ProductoAgregadoAComanda>().First();
            evento.ComandaId.Should().Be(comanda.Id);
            evento.ProductoId.Should().Be(productoId);
            evento.Cantidad.Should().Be(cantidad);
        }

        [Fact]
        public void AgregarProducto_CuandoComandaNoEstaActiva_DebeLanzarExcepcion()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());

            // Cambiamos el estado a uno no activo (simulando una comanda finalizada)
            PropertyInfo propEstado = comanda.GetType().GetProperty("Estado", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (propEstado != null)
            {
                propEstado.SetValue(comanda, EstadoComanda.Finalizada);
            }

            // Act & Assert
            Action action = () => comanda.AgregarProducto(Guid.NewGuid(), 1, 100m);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*no se pueden realizar cambios*",
                    because: "No se deben permitir cambios en comandas no activas");
        }

        [Fact]
        public void Comanda_SinProductos_NoPuedeFinalizarse()
        {
            // Arrange
            var comanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());

            // Cambiamos manualmente el estado para probar la transición
            // (normalmente pasaría por todos los estados intermedios)
            PropertyInfo propEstado = comanda.GetType().GetProperty("Estado", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (propEstado != null)
            {
                propEstado.SetValue(comanda, EstadoComanda.Entregada);
            }

            // Act & Assert
            Action action = () => comanda.ActualizarEstado(EstadoComanda.Finalizada);
            action.Should().Throw<InvalidOperationException>()
                .WithMessage("*No se puede finalizar*",
                    because: "No se deben permitir finalizar comandas sin productos");
        }
    }
}




