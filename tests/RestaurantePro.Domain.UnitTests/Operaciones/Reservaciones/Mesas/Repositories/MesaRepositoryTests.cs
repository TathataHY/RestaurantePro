namespace RestaurantePro.Domain.UnitTests.Operaciones.Reservaciones.Mesas.Repositories
{
    using Xunit;
    using Moq;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Entities;
    using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
    using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;

    public class MesaRepositoryTests
    {
        private readonly Mock<IMesaRepository> _mockRepository;
        private readonly List<Mesa> _mesas;

        public MesaRepositoryTests()
        {
            _mockRepository = new Mock<IMesaRepository>();

            // Crear datos de prueba
            _mesas = new List<Mesa>
            {
                Mesa.Crear(1, 2, "Terraza"),
                Mesa.Crear(2, 4, "Interior"),
                Mesa.Crear(3, 6, "Salón VIP"),
                Mesa.Crear(4, 2, "Terraza"),
                Mesa.Crear(5, 8, "Interior")
            };

            // Marcar algunas mesas con diferentes estados para pruebas
            _mesas[1].MarcarComoOcupada();     // Mesa 2 ocupada
            _mesas[2].MarcarComoReservada();   // Mesa 3 reservada
            _mesas[4].MarcarComoFueraDeServicio("En mantenimiento"); // Mesa 5 fuera de servicio
        }

        [Fact]
        public async Task ObtenerTodasAsync_DebeRetornarTodasLasMesas()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.ObtenerTodasAsync())
                .ReturnsAsync(_mesas);

            // Act
            var resultado = await _mockRepository.Object.ObtenerTodasAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(5);
            resultado.Should().BeEquivalentTo(_mesas);
        }

        [Fact]
        public async Task ObtenerPorIdAsync_IdExistente_DebeRetornarMesa()
        {
            // Arrange
            var mesaId = _mesas[0].Id;
            var mesaEsperada = _mesas[0];

            _mockRepository.Setup(repo => repo.ObtenerPorIdAsync(mesaId))
                .ReturnsAsync(mesaEsperada);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorIdAsync(mesaId);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeSameAs(mesaEsperada);
            _mockRepository.Verify(repo => repo.ObtenerPorIdAsync(mesaId), Times.Once());
        }

        [Fact]
        public async Task ObtenerPorNumeroAsync_NumeroExistente_DebeRetornarMesa()
        {
            // Arrange
            var numeroMesa = 3; // Mesa 3
            var mesaEsperada = _mesas.FirstOrDefault(m => m.Numero == numeroMesa);

            _mockRepository.Setup(repo => repo.ObtenerPorNumeroAsync(numeroMesa))
                .ReturnsAsync(mesaEsperada);

            // Act
            var resultado = await _mockRepository.Object.ObtenerPorNumeroAsync(numeroMesa);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeSameAs(mesaEsperada);
            resultado.Numero.Should().Be(numeroMesa);
        }

        [Fact]
        public async Task BuscarPorUbicacionAsync_UbicacionConMesas_DebeRetornarMesasEnEsaUbicacion()
        {
            // Arrange
            var ubicacion = "Terraza";
            var mesasEnTerraza = _mesas.Where(m => m.Ubicacion == ubicacion).ToList();

            _mockRepository.Setup(repo => repo.BuscarPorUbicacionAsync(ubicacion))
                .ReturnsAsync(mesasEnTerraza);

            // Act
            var resultado = await _mockRepository.Object.BuscarPorUbicacionAsync(ubicacion);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2); // Hay 2 mesas en Terraza
            resultado.All(m => m.Ubicacion == ubicacion).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerMesasDisponiblesAsync_DebeRetornarSoloMesasDisponibles()
        {
            // Arrange
            var mesasDisponibles = _mesas.Where(m => m.Estado == EstadoMesa.Disponible).ToList();

            _mockRepository.Setup(repo => repo.ObtenerMesasDisponiblesAsync())
                .ReturnsAsync(mesasDisponibles);

            // Act
            var resultado = await _mockRepository.Object.ObtenerMesasDisponiblesAsync();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2); // Solo hay 2 mesas disponibles (mesa 1 y 4)
            resultado.All(m => m.Estado == EstadoMesa.Disponible).Should().BeTrue();
        }

        [Fact]
        public async Task ObtenerTotalComensalesActualesAsync_DebeRetornarSumaDeCapacidadesDeMesasOcupadas()
        {
            // Arrange
            var totalComensalesEsperados = _mesas
                .Where(m => m.Estado == EstadoMesa.Ocupada)
                .Sum(m => m.Capacidad); // Suma la capacidad de la mesa 2: 4 comensales

            _mockRepository.Setup(repo => repo.ObtenerTotalComensalesActualesAsync())
                .ReturnsAsync(totalComensalesEsperados);

            // Act
            var resultado = await _mockRepository.Object.ObtenerTotalComensalesActualesAsync();

            // Assert
            resultado.Should().Be(4); // 4 comensales en la mesa 2
        }

        [Fact]
        public async Task AgregarAsync_MesaValida_DebeAgregarCorrectamente()
        {
            // Arrange
            var nuevaMesa = Mesa.Crear(6, 4, "Jardín");

            _mockRepository.Setup(repo => repo.AgregarAsync(nuevaMesa))
                .Returns(Task.CompletedTask);

            // Act
            await _mockRepository.Object.AgregarAsync(nuevaMesa);

            // Assert
            _mockRepository.Verify(repo => repo.AgregarAsync(nuevaMesa), Times.Once());
        }

        [Fact]
        public async Task ActualizarAsync_MesaExistente_DebeActualizarCorrectamente()
        {
            // Arrange
            var mesa = _mesas[0]; // Mesa disponible
            mesa.MarcarComoOcupada(); // La marcamos como ocupada

            _mockRepository.Setup(repo => repo.ActualizarAsync(mesa))
                .Returns(Task.CompletedTask);

            // Act
            await _mockRepository.Object.ActualizarAsync(mesa);

            // Assert
            _mockRepository.Verify(repo => repo.ActualizarAsync(mesa), Times.Once());
            mesa.Estado.Should().Be(EstadoMesa.Ocupada);
        }

        [Fact]
        public async Task EliminarAsync_MesaExistente_DebeEliminarCorrectamente()
        {
            // Arrange
            var mesaId = _mesas[4].Id; // Mesa 5 (fuera de servicio)

            _mockRepository.Setup(repo => repo.EliminarAsync(mesaId))
                .Returns(Task.CompletedTask);

            // Act
            await _mockRepository.Object.EliminarAsync(mesaId);

            // Assert
            _mockRepository.Verify(repo => repo.EliminarAsync(mesaId), Times.Once());
        }
    }
}




