using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using Xunit;

namespace RestaurantePro.Domain.UnitTests.Operaciones.Comandas.Repositories
{
    public class ComandaRepositoryTests
    {
        private readonly Mock<IComandaRepository> _mockRepository;
        private readonly List<Comanda> _comandas;
        private readonly Guid _mesaId;
        private readonly Guid _meseroId;
        private readonly Guid _clienteId;

        public ComandaRepositoryTests()
        {
            _mockRepository = new Mock<IComandaRepository>();
            _mesaId = Guid.NewGuid();
            _meseroId = Guid.NewGuid();
            _clienteId = Guid.NewGuid();
            
            // Crear datos de prueba
            _comandas = new List<Comanda>
            {
                Comanda.Crear(_mesaId, _meseroId, _clienteId, "Comanda 1"),
                Comanda.Crear(_mesaId, _meseroId, _clienteId, "Comanda 2"),
                Comanda.Crear(Guid.NewGuid(), Guid.NewGuid(), null, "Comanda 3")
            };
            
            // Agregar productos a las comandas
            _comandas[0].AgregarProducto(Guid.NewGuid(), 2, 100.50m, "Sin picante");
            _comandas[1].AgregarProducto(Guid.NewGuid(), 1, 85.75m, "Extra queso");
            _comandas[2].AgregarProducto(Guid.NewGuid(), 3, 120m);
            
            // Cambiar estado de algunas comandas para pruebas
            _comandas[0].ActualizarEstado(EstadoComanda.EnProceso);
            _comandas[1].ActualizarEstado(EstadoComanda.EnProceso);
            _comandas[1].ActualizarEstado(EstadoComanda.Lista);
        }
        
        [Fact]
        public async Task ObtenerPorIdAsync_IdExistente_DebeRetornarComanda()
        {
            // Arrange
            var comandaId = _comandas[0].Id;
            var comandaEsperada = _comandas[0];
            
            _mockRepository.Setup(repo => repo.ObtenerPorIdAsync(comandaId))
                .ReturnsAsync(comandaEsperada);
                
            // Act
            var resultado = await _mockRepository.Object.ObtenerPorIdAsync(comandaId);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeSameAs(comandaEsperada);
            _mockRepository.Verify(repo => repo.ObtenerPorIdAsync(comandaId), Times.Once);
        }
        
        [Fact]
        public async Task ObtenerPorEstadoAsync_EstadoEnProceso_DebeRetornarComandasEnProceso()
        {
            // Arrange
            var estado = EstadoComanda.EnProceso;
            var comandasEnProceso = _comandas.Where(c => c.Estado == estado).ToList();
            
            _mockRepository.Setup(repo => repo.ObtenerPorEstadoAsync(estado))
                .ReturnsAsync(comandasEnProceso);
                
            // Act
            var resultado = await _mockRepository.Object.ObtenerPorEstadoAsync(estado);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1); // Solo comanda[0] debería estar en proceso
            resultado.All(c => c.Estado == estado).Should().BeTrue();
        }
        
        [Fact]
        public async Task ObtenerPorMesaAsync_MesaConComandas_DebeRetornarComandasDeLaMesa()
        {
            // Arrange
            var comandasDeMesa = _comandas.Where(c => c.MesaId == _mesaId).ToList();
            
            _mockRepository.Setup(repo => repo.ObtenerPorMesaAsync(_mesaId))
                .ReturnsAsync(comandasDeMesa);
                
            // Act
            var resultado = await _mockRepository.Object.ObtenerPorMesaAsync(_mesaId);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2); // Hay 2 comandas para la mesa
            resultado.All(c => c.MesaId == _mesaId).Should().BeTrue();
        }
        
        [Fact]
        public async Task ObtenerPorMeseroAsync_MeseroConComandas_DebeRetornarComandasDelMesero()
        {
            // Arrange
            var comandasDeMesero = _comandas.Where(c => c.MeseroId == _meseroId).ToList();
            
            _mockRepository.Setup(repo => repo.ObtenerPorMeseroAsync(_meseroId))
                .ReturnsAsync(comandasDeMesero);
                
            // Act
            var resultado = await _mockRepository.Object.ObtenerPorMeseroAsync(_meseroId);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2); // Hay 2 comandas del mesero
            resultado.All(c => c.MeseroId == _meseroId).Should().BeTrue();
        }
        
        [Fact]
        public async Task ObtenerPorClienteAsync_ClienteConComandas_DebeRetornarComandasDelCliente()
        {
            // Arrange
            var comandasDeCliente = _comandas.Where(c => c.ClienteId == _clienteId).ToList();
            
            _mockRepository.Setup(repo => repo.ObtenerPorClienteAsync(_clienteId))
                .ReturnsAsync(comandasDeCliente);
                
            // Act
            var resultado = await _mockRepository.Object.ObtenerPorClienteAsync(_clienteId);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2); // Hay 2 comandas del cliente
            resultado.All(c => c.ClienteId == _clienteId).Should().BeTrue();
        }
        
        [Fact]
        public async Task ObtenerPorRangoFechasAsync_FechasConComandas_DebeRetornarComandasEnRango()
        {
            // Arrange
            var fechaInicio = DateTime.Now.AddDays(-1);
            var fechaFin = DateTime.Now.AddDays(1);
            
            _mockRepository.Setup(repo => repo.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin))
                .ReturnsAsync(_comandas);
                
            // Act
            var resultado = await _mockRepository.Object.ObtenerPorRangoFechasAsync(fechaInicio, fechaFin);
            
            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(3); // Todas las comandas están en el rango
        }
        
        [Fact]
        public async Task AgregarAsync_ComandaValida_DebeAgregarCorrectamente()
        {
            // Arrange
            var nuevaComanda = Comanda.Crear(Guid.NewGuid(), Guid.NewGuid());
            nuevaComanda.AgregarProducto(Guid.NewGuid(), 1, 75m);
            
            _mockRepository.Setup(repo => repo.AgregarAsync(nuevaComanda))
                .Returns(Task.CompletedTask);
                
            // Act
            await _mockRepository.Object.AgregarAsync(nuevaComanda);
            
            // Assert
            _mockRepository.Verify(repo => repo.AgregarAsync(nuevaComanda), Times.Once);
        }
        
        [Fact]
        public async Task ActualizarAsync_ComandaExistente_DebeActualizarCorrectamente()
        {
            // Arrange
            var comanda = _comandas[2]; // Comanda creada
            comanda.ActualizarEstado(EstadoComanda.EnProceso); // La actualizamos a en proceso
            
            _mockRepository.Setup(repo => repo.ActualizarAsync(comanda))
                .Returns(Task.CompletedTask);
                
            // Act
            await _mockRepository.Object.ActualizarAsync(comanda);
            
            // Assert
            _mockRepository.Verify(repo => repo.ActualizarAsync(comanda), Times.Once);
            comanda.Estado.Should().Be(EstadoComanda.EnProceso);
        }
        
        [Fact]
        public async Task EliminarAsync_ComandaExistente_DebeEliminarCorrectamente()
        {
            // Arrange
            var comandaId = _comandas[0].Id;
            
            _mockRepository.Setup(repo => repo.EliminarAsync(comandaId))
                .Returns(Task.CompletedTask);
                
            // Act
            await _mockRepository.Object.EliminarAsync(comandaId);
            
            // Assert
            _mockRepository.Verify(repo => repo.EliminarAsync(comandaId), Times.Once);
        }
    }
} 