using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using RestaurantePro.Domain.Core.Base.Services;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Core.SharedKernel.Validation;
using RestaurantePro.Domain.Inventario.Ingredientes.Entities;
using RestaurantePro.Domain.Inventario.Ingredientes.Enums;
using RestaurantePro.Domain.Inventario.Ingredientes.Interfaces;
using RestaurantePro.Domain.Inventario.Services;
using RestaurantePro.Domain.Inventario.Policies;
using RestaurantePro.Domain.Inventario.Compras.OrdenesCompra.Entities;

namespace RestaurantePro.Domain.UnitTests.Inventario.Policies
{
    public class StockBajoPolicyTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IServicioNotificacionesInventario> _servicioNotificacionesMock;
        private readonly Mock<IVerificadorStock> _verificadorStockMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly NotificationManager _notificationManager;
        private readonly StockBajoPolicy _sut;
        private readonly DateTime _fechaActual = new DateTime(2025, 7, 15);

        public StockBajoPolicyTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _servicioNotificacionesMock = new Mock<IServicioNotificacionesInventario>();
            _verificadorStockMock = new Mock<IVerificadorStock>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _notificationManager = new NotificationManager();

            // Configuración básica
            _dateTimeServiceMock
                .Setup(s => s.Now)
                .Returns(_fechaActual);

            _sut = new StockBajoPolicy(
                _ingredienteRepositoryMock.Object,
                _servicioNotificacionesMock.Object,
                _verificadorStockMock.Object,
                _dateTimeServiceMock.Object,
                _notificationManager);
        }

        [Fact]
        public async Task EjecutarPolicy_SinIngredientesConStockBajo_DebeRetornarResultadoVacio()
        {
            // Arrange
            var ingredientesVacios = new List<Ingrediente>();
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientesVacios);

            // Act
            var result = await _sut.EjecutarPolicy();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Empty(result.Value.IngredientesPriorizados);
            Assert.Empty(result.Value.Notificaciones);
            Assert.Empty(result.Value.OrdenesCompraGeneradas);
            Assert.False(result.Value.TieneResultados);
        }

        [Fact]
        public async Task EjecutarPolicy_ConIngredientesConStockBajo_DebeRetornarResultadoConIngredientesPriorizados()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>
            {
                CrearIngredientePrueba("Tomate", 5, 10, RotacionIngrediente.Alta, TemporadaIngrediente.Verano),
                CrearIngredientePrueba("Cebolla", 3, 8, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño)
            };

            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientes);

            var resultadoVerificacion = new ResultadoVerificacionStock();
            _verificadorStockMock
                .Setup(v => v.VerificarYGenerarOrdenesCompraAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(resultadoVerificacion));

            _servicioNotificacionesMock
                .Setup(s => s.NotificarStockBajo(
                    It.IsAny<Guid>(), 
                    It.IsAny<string>(), 
                    It.IsAny<decimal>(), 
                    It.IsAny<decimal>()))
                .ReturnsAsync(Guid.NewGuid());

            // Act
            var result = await _sut.EjecutarPolicy();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Equal(2, result.Value.IngredientesPriorizados.Count);
            Assert.Equal(2, result.Value.Notificaciones.Count);
            Assert.Empty(result.Value.OrdenesCompraGeneradas);
            Assert.True(result.Value.TieneResultados);
            
            // Verificar que el tomate (alta rotación en verano) tenga mayor prioridad que la cebolla
            var tomate = result.Value.IngredientesPriorizados.FirstOrDefault(i => i.Nombre == "Tomate");
            var cebolla = result.Value.IngredientesPriorizados.FirstOrDefault(i => i.Nombre == "Cebolla");
            Assert.NotNull(tomate);
            Assert.NotNull(cebolla);
            Assert.True(tomate.Prioridad > cebolla.Prioridad);
        }

        [Fact]
        public async Task EjecutarPolicyParaIngrediente_IdVacio_DebeRetornarError()
        {
            // Arrange
            var ingredienteId = Guid.Empty;

            // Act
            var result = await _sut.EjecutarPolicyParaIngrediente(ingredienteId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Contains("ID del ingrediente no puede estar vacío", result.Error);
        }

        [Fact]
        public async Task EjecutarPolicyParaIngrediente_IngredienteNoExiste_DebeRetornarError()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Ingrediente)null);

            // Act
            var result = await _sut.EjecutarPolicyParaIngrediente(ingredienteId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Contains($"No existe un ingrediente con el ID {ingredienteId}", result.Error);
        }

        [Fact]
        public async Task EjecutarPolicyParaIngrediente_StockNoEsBajo_DebeRetornarResultadoVacio()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var ingrediente = CrearIngredientePrueba("Tomate", 10, 10, RotacionIngrediente.Alta, TemporadaIngrediente.Verano);
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);

            // Act
            var result = await _sut.EjecutarPolicyParaIngrediente(ingredienteId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Empty(result.Value.IngredientesPriorizados);
            Assert.Empty(result.Value.Notificaciones);
            Assert.False(result.Value.TieneResultados);
        }

        [Fact]
        public async Task EjecutarPolicyParaIngrediente_StockEsBajo_DebeNotificarYGenerarOrden()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var notificacionId = Guid.NewGuid();
            var ordenId = Guid.NewGuid();
            var proveedorId = Guid.NewGuid();
            var ingrediente = CrearIngredientePrueba("Tomate", 5, 10, RotacionIngrediente.Alta, TemporadaIngrediente.Verano);
            ingrediente.AsociarProveedorPrincipal(proveedorId);

            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(ingredienteId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingrediente);

            _servicioNotificacionesMock
                .Setup(s => s.NotificarStockBajo(
                    It.IsAny<Guid>(), 
                    It.IsAny<string>(), 
                    It.IsAny<decimal>(), 
                    It.IsAny<decimal>()))
                .ReturnsAsync(notificacionId);

            // Crear la orden de compra para el test - consultando la firma correcta
            var ordenCompra = OrdenCompra.Crear(
                proveedorId,
                _fechaActual.AddDays(7),
                "Orden de prueba");
                
            // Usamos reflection para modificar el ID, ya que es un campo privado
            SetPrivateId(ordenCompra, ordenId);

            var resultadoVerificacion = new ResultadoVerificacionStock();
            resultadoVerificacion.OrdenesGeneradas.Add(ordenCompra);

            _verificadorStockMock
                .Setup(v => v.VerificarYGenerarOrdenesCompraAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(resultadoVerificacion));

            // Act
            var result = await _sut.EjecutarPolicyParaIngrediente(ingredienteId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Single(result.Value.IngredientesPriorizados);
            Assert.Single(result.Value.Notificaciones);
            Assert.Contains(notificacionId, result.Value.Notificaciones);
            Assert.Single(result.Value.OrdenesCompraGeneradas);
            Assert.Contains(ordenId, result.Value.OrdenesCompraGeneradas);
            Assert.True(result.Value.TieneResultados);
        }

        [Fact]
        public async Task PriorizarIngredientesParaReposicion_DebeOrdenarPorPrioridad()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>
            {
                CrearIngredientePrueba("Tomate", 2, 10, RotacionIngrediente.Alta, TemporadaIngrediente.Verano),
                CrearIngredientePrueba("Cebolla", 3, 8, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                CrearIngredientePrueba("Lechuga", 1, 5, RotacionIngrediente.Critica, TemporadaIngrediente.Primavera),
                CrearIngredientePrueba("Zanahoria", 4, 15, RotacionIngrediente.Baja, TemporadaIngrediente.Invierno)
            };

            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientes);

            // Act
            var result = await _sut.PriorizarIngredientesParaReposicion();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded);
            Assert.Equal(4, result.Value.IngredientesPriorizados.Count);
            
            // Verificar el orden por prioridad (mayor a menor)
            var nombres = result.Value.IngredientesPriorizados.Select(i => i.Nombre).ToList();
            Assert.Equal("Lechuga", nombres[0]); // Crítica
            Assert.Equal("Tomate", nombres[1]);  // Alta + En temporada
            Assert.Equal("Cebolla", nombres[2]); // Media + Todo el año
            Assert.Equal("Zanahoria", nombres[3]); // Baja + Fuera de temporada
        }

        [Fact]
        public async Task EjecutarPolicy_ErrorEnNotificacion_DebeRetornarResultadoConErrores()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>
            {
                CrearIngredientePrueba("Tomate", 5, 10, RotacionIngrediente.Alta, TemporadaIngrediente.Verano)
            };

            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(ingredientes);

            _servicioNotificacionesMock
                .Setup(s => s.NotificarStockBajo(
                    It.IsAny<Guid>(), 
                    It.IsAny<string>(), 
                    It.IsAny<decimal>(), 
                    It.IsAny<decimal>()))
                .ThrowsAsync(new Exception("Error en notificación"));

            // Act
            var result = await _sut.EjecutarPolicy();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Succeeded); // Aún retorna éxito pero con errores en la notificación
            Assert.NotNull(result.Errors); // Debe contener errores
            Assert.NotEmpty(result.Errors); // Asegurarse que no es null ni vacío
            Assert.Contains(result.Errors, e => e.Contains("Error en notificación"));
        }

        [Fact]
        public async Task EjecutarPolicy_ExcepcionNoControlada_DebeRetornarResultadoFallido()
        {
            // Arrange
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error de base de datos"));

            // Act
            var result = await _sut.EjecutarPolicy();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Succeeded);
            Assert.Contains("Error al ejecutar política de stock bajo", result.Error);
            Assert.Contains("Error de base de datos", result.Error);
        }

        // Método auxiliar para crear ingredientes de prueba
        private Ingrediente CrearIngredientePrueba(
            string nombre, 
            decimal stock, 
            decimal stockMinimo, 
            RotacionIngrediente rotacion, 
            TemporadaIngrediente temporada)
        {
            var ingrediente = Ingrediente.Crear(
                nombre,
                $"COD-{nombre}",
                $"Descripción de {nombre}",
                UnidadMedida.Kilogramo,
                stockMinimo,
                stock,
                rotacion,
                temporada);
                
            // Usamos reflection para modificar el ID, ya que es un campo privado
            SetPrivateId(ingrediente, Guid.NewGuid());
            
            return ingrediente;
        }

        // Método auxiliar para establecer el ID privado usando reflection
        private void SetPrivateId(object entity, Guid id)
        {
            Type baseType = entity.GetType();
            while (baseType.BaseType != null && baseType.Name != "EntityBase")
            {
                baseType = baseType.BaseType;
            }
            
            var field = baseType.GetField("_id", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(entity, id);
        }
    }
}



