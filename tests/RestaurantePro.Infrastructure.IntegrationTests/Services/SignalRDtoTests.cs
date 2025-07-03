using System;
using System.Collections.Generic;
using Xunit;
using RestaurantePro.Infrastructure.DTOs.SignalR;

namespace RestaurantePro.Infrastructure.IntegrationTests.Services
{
    /// <summary>
    /// Tests unitarios para DTOs de SignalR
    /// </summary>
    public class SignalRDtoTests
    {
        [Fact]
        public void ComandaSignalRDto_ConstructorPorDefecto_DeberiaCrearInstanciaValida()
        {
            // Act
            var dto = new ComandaSignalRDto();

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(Guid.Empty, dto.Id);
            Assert.Empty(dto.NumeroComanda);
            Assert.Empty(dto.NumeroMesa);
            Assert.Empty(dto.Estado);
            Assert.NotNull(dto.Items);
            Assert.Empty(dto.Items);
            Assert.Equal(DateTime.MinValue, dto.FechaCreacion);
            Assert.Null(dto.FechaEstimadaEntrega);
            Assert.Null(dto.Observaciones);
            Assert.Empty(dto.NombreMesero);
            Assert.Equal("Normal", dto.Prioridad);
            Assert.Equal(Guid.Empty, dto.MeseroId);
            Assert.Null(dto.ClienteId);
            Assert.Equal(0, dto.Total);
        }

        [Fact]
        public void ComandaSignalRDto_ConstructorConParametros_DeberiaCrearInstanciaCorrecta()
        {
            // Arrange
            var id = Guid.NewGuid();
            var mesaId = Guid.NewGuid();
            var meseroId = Guid.NewGuid();
            var numeroComanda = "COM-001";
            var numeroMesa = "Mesa 5";
            var estado = "Creada";
            var fechaCreacion = DateTime.UtcNow;
            var nombreMesero = "Juan Pérez";

            // Act
            var dto = new ComandaSignalRDto(id, numeroComanda, mesaId, numeroMesa, estado, fechaCreacion, nombreMesero, meseroId);

            // Assert
            Assert.Equal(id, dto.Id);
            Assert.Equal(numeroComanda, dto.NumeroComanda);
            Assert.Equal(mesaId, dto.MesaId);
            Assert.Equal(numeroMesa, dto.NumeroMesa);
            Assert.Equal(estado, dto.Estado);
            Assert.Equal(fechaCreacion, dto.FechaCreacion);
            Assert.Equal(nombreMesero, dto.NombreMesero);
            Assert.Equal(meseroId, dto.MeseroId);
            Assert.NotNull(dto.Items);
            Assert.Empty(dto.Items);
        }

        [Fact]
        public void ItemComandaSignalRDto_ConstructorConParametros_DeberiaCalcularSubtotalCorrectamente()
        {
            // Arrange
            var id = Guid.NewGuid();
            var productoId = Guid.NewGuid();
            var nombreProducto = "Hamburguesa";
            var cantidad = 2;
            var precioUnitario = 15.50m;
            var observaciones = "Sin cebolla";

            // Act
            var dto = new ItemComandaSignalRDto(id, productoId, nombreProducto, cantidad, precioUnitario, observaciones);

            // Assert
            Assert.Equal(id, dto.Id);
            Assert.Equal(productoId, dto.ProductoId);
            Assert.Equal(nombreProducto, dto.NombreProducto);
            Assert.Equal(cantidad, dto.Cantidad);
            Assert.Equal(precioUnitario, dto.PrecioUnitario);
            Assert.Equal(cantidad * precioUnitario, dto.Subtotal);
            Assert.Equal(observaciones, dto.Observaciones);
            Assert.Equal("Pendiente", dto.Estado);
        }

        [Fact]
        public void NotificacionSignalRDto_ConstructorPorDefecto_DeberiaCrearInstanciaValida()
        {
            // Act
            var dto = new NotificacionSignalRDto();

            // Assert
            Assert.NotNull(dto);
            Assert.NotEqual(Guid.Empty, dto.Id);
            Assert.Empty(dto.Titulo);
            Assert.Empty(dto.Mensaje);
            Assert.Equal("info", dto.Tipo);
            Assert.True(dto.FechaHora > DateTime.UtcNow.AddMinutes(-1));
            Assert.Null(dto.DestinatarioId);
            Assert.Null(dto.Rol);
            Assert.Null(dto.Datos);
            Assert.False(dto.RequiereConfirmacion);
            Assert.Equal(0, dto.DuracionSegundos);
        }

        [Fact]
        public void NotificacionSignalRDto_ConstructorGlobal_DeberiaCrearNotificacionGlobal()
        {
            // Arrange
            var titulo = "Mantenimiento";
            var mensaje = "El sistema estará en mantenimiento";
            var tipo = "warning";

            // Act
            var dto = new NotificacionSignalRDto(titulo, mensaje, tipo);

            // Assert
            Assert.Equal(titulo, dto.Titulo);
            Assert.Equal(mensaje, dto.Mensaje);
            Assert.Equal(tipo, dto.Tipo);
            Assert.Null(dto.DestinatarioId);
            Assert.Null(dto.Rol);
        }

        [Fact]
        public void NotificacionSignalRDto_ConstructorPorRol_DeberiaCrearNotificacionPorRol()
        {
            // Arrange
            var titulo = "Nueva comanda";
            var mensaje = "Hay una nueva comanda pendiente";
            var rol = "Cocina";
            var tipo = "info";

            // Act
            var dto = new NotificacionSignalRDto(titulo, mensaje, rol, tipo);

            // Assert
            Assert.Equal(titulo, dto.Titulo);
            Assert.Equal(mensaje, dto.Mensaje);
            Assert.Equal(rol, dto.Rol);
            Assert.Equal(tipo, dto.Tipo);
            Assert.Null(dto.DestinatarioId);
        }

        [Fact]
        public void NotificacionSignalRDto_ConstructorIndividual_DeberiaCrearNotificacionIndividual()
        {
            // Arrange
            var titulo = "Mensaje personal";
            var mensaje = "Tienes un mensaje personal";
            var destinatarioId = Guid.NewGuid();
            var tipo = "info";

            // Act
            var dto = new NotificacionSignalRDto(titulo, mensaje, destinatarioId, tipo);

            // Assert
            Assert.Equal(titulo, dto.Titulo);
            Assert.Equal(mensaje, dto.Mensaje);
            Assert.Equal(destinatarioId, dto.DestinatarioId);
            Assert.Equal(tipo, dto.Tipo);
            Assert.Null(dto.Rol);
        }

        [Fact]
        public void AlertaInventarioSignalRDto_ConstructorStockBajo_DeberiaCalcularNivelUrgenciaCorrectamente()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Tomate";
            var stockActual = 5.0m;
            var stockMinimo = 20.0m;
            var unidadMedida = "kg";

            // Act
            var dto = new AlertaInventarioSignalRDto(ingredienteId, nombreIngrediente, stockActual, stockMinimo, unidadMedida);

            // Assert
            Assert.Equal(ingredienteId, dto.IngredienteId);
            Assert.Equal(nombreIngrediente, dto.NombreIngrediente);
            Assert.Equal(stockActual, dto.StockActual);
            Assert.Equal(stockMinimo, dto.StockMinimo);
            Assert.Equal(unidadMedida, dto.UnidadMedida);
            Assert.Equal(TiposAlerta.StockBajo, dto.TipoAlerta);
            Assert.Equal("Alto", dto.NivelUrgencia);
            Assert.Contains("Stock bajo", dto.Mensaje);
            Assert.False(dto.RequiereAccionInmediata);
        }

        [Fact]
        public void AlertaInventarioSignalRDto_ConstructorStockAgotado_DeberiaMarcarComoCritico()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Lechuga";
            var unidadMedida = "kg";

            // Act
            var dto = new AlertaInventarioSignalRDto(ingredienteId, nombreIngrediente, unidadMedida);

            // Assert
            Assert.Equal(ingredienteId, dto.IngredienteId);
            Assert.Equal(nombreIngrediente, dto.NombreIngrediente);
            Assert.Equal(0, dto.StockActual);
            Assert.Equal(TiposAlerta.StockAgotado, dto.TipoAlerta);
            Assert.Equal("Crítico", dto.NivelUrgencia);
            Assert.True(dto.RequiereAccionInmediata);
            Assert.Contains("Stock agotado", dto.Mensaje);
        }

        [Fact]
        public void AlertaInventarioSignalRDto_ConstructorVencimiento_DeberiaCalcularDiasRestantes()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombreIngrediente = "Queso";
            var fechaVencimiento = DateTime.UtcNow.AddDays(2);
            var diasRestantes = 2;

            // Act
            var dto = new AlertaInventarioSignalRDto(ingredienteId, nombreIngrediente, fechaVencimiento, diasRestantes);

            // Assert
            Assert.Equal(ingredienteId, dto.IngredienteId);
            Assert.Equal(nombreIngrediente, dto.NombreIngrediente);
            Assert.Equal(TiposAlerta.PorVencer, dto.TipoAlerta);
            Assert.Equal(fechaVencimiento, dto.FechaVencimiento);
            Assert.Equal(diasRestantes, dto.DiasRestantes);
            Assert.Equal("Alto", dto.NivelUrgencia);
            Assert.True(dto.RequiereAccionInmediata);
            Assert.Contains("Vencimiento próximo", dto.Mensaje);
        }

        [Theory]
        [InlineData(0, "Crítico")]
        [InlineData(5, "Alto")]
        [InlineData(10, "Medio")]
        [InlineData(20, "Bajo")]
        public void AlertaInventarioSignalRDto_NivelUrgenciaStock_DeberiaCalcularCorrectamente(decimal stockActual, string nivelEsperado)
        {
            // Arrange
            var stockMinimo = 20.0m;

            // Act
            var dto = new AlertaInventarioSignalRDto(Guid.NewGuid(), "Test", stockActual, stockMinimo, "kg");

            // Assert
            Assert.Equal(nivelEsperado, dto.NivelUrgencia);
        }

        [Theory]
        [InlineData(1, "Crítico")]
        [InlineData(3, "Alto")]
        [InlineData(7, "Medio")]
        [InlineData(15, "Bajo")]
        public void AlertaInventarioSignalRDto_NivelUrgenciaVencimiento_DeberiaCalcularCorrectamente(int diasRestantes, string nivelEsperado)
        {
            // Arrange
            var fechaVencimiento = DateTime.UtcNow.AddDays(diasRestantes);

            // Act
            var dto = new AlertaInventarioSignalRDto(Guid.NewGuid(), "Test", fechaVencimiento, diasRestantes);

            // Assert
            Assert.Equal(nivelEsperado, dto.NivelUrgencia);
        }

        [Fact]
        public void TiposNotificacion_Constantes_DeberianTenerValoresCorrectos()
        {
            // Assert
            Assert.Equal("info", TiposNotificacion.Info);
            Assert.Equal("success", TiposNotificacion.Success);
            Assert.Equal("warning", TiposNotificacion.Warning);
            Assert.Equal("error", TiposNotificacion.Error);
            Assert.Equal("system", TiposNotificacion.System);
            Assert.Equal("alert", TiposNotificacion.Alert);
        }

        [Fact]
        public void TiposAlerta_Constantes_DeberianTenerValoresCorrectos()
        {
            // Assert
            Assert.Equal("StockBajo", TiposAlerta.StockBajo);
            Assert.Equal("StockAgotado", TiposAlerta.StockAgotado);
            Assert.Equal("PorVencer", TiposAlerta.PorVencer);
            Assert.Equal("StockExcesivo", TiposAlerta.StockExcesivo);
            Assert.Equal("MovimientoInusual", TiposAlerta.MovimientoInusual);
        }

        [Fact]
        public void NivelesUrgencia_Constantes_DeberianTenerValoresCorrectos()
        {
            // Assert
            Assert.Equal("Bajo", NivelesUrgencia.Bajo);
            Assert.Equal("Medio", NivelesUrgencia.Medio);
            Assert.Equal("Alto", NivelesUrgencia.Alto);
            Assert.Equal("Crítico", NivelesUrgencia.Critico);
        }
    }
} 