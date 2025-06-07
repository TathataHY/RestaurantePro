#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using RestaurantePro.Domain.Comercial.Clientes.Entities;
using RestaurantePro.Domain.Comercial.Clientes.Enums;
using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
using RestaurantePro.Domain.Comercial.Policies;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Services;
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;

namespace RestaurantePro.Domain.UnitTests.Comercial.Policies
{
    public class ClientesFrecuentesPolicyTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
        private readonly Mock<IServicioFidelizacion> _servicioFidelizacionMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<INotificationManager> _notificationManagerMock;
        private readonly ClientesFrecuentesPolicy _policy;

        public ClientesFrecuentesPolicyTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
            _servicioFidelizacionMock = new Mock<IServicioFidelizacion>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _notificationManagerMock = new Mock<INotificationManager>();
            
            _dateTimeServiceMock.Setup(tp => tp.Now).Returns(new DateTime(2023, 1, 1));
            
            _policy = new ClientesFrecuentesPolicy(
                _clienteRepositoryMock.Object,
                _tarjetaRepositoryMock.Object,
                _servicioFidelizacionMock.Object,
                _dateTimeServiceMock.Object,
                _notificationManagerMock.Object);
        }
        
        [Fact]
        public async Task EjecutarSegmentacionClientes_DebeAsignarSegmentosCorrectamente()
        {
            // Arrange
            var clienteFrecuente = CrearCliente(15);
            var clienteRegular = CrearCliente(8);
            var clienteOcasional = CrearCliente(3);
            
            var clientes = new List<Cliente> 
            { 
                clienteFrecuente, 
                clienteRegular, 
                clienteOcasional 
            };
            
            _clienteRepositoryMock.Setup(r => r.ObtenerClientesConHistorialVisitasAsync(
                It.IsAny<DateTime>(), 
                It.IsAny<DateTime>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientes);
                
            // Act
            await _policy.EjecutarSegmentacionClientes(CancellationToken.None);
            
            // Assert
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(
                It.Is<Cliente>(c => c == clienteFrecuente && c.Segmento == SegmentoCliente.FrecuenciaAlta), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
                
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(
                It.Is<Cliente>(c => c == clienteRegular && c.Segmento == SegmentoCliente.Regular), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
                
            _clienteRepositoryMock.Verify(r => r.ActualizarAsync(
                It.Is<Cliente>(c => c == clienteOcasional && c.Segmento == SegmentoCliente.SinClasificar), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        [Fact]
        public async Task EjecutarPolicy_ClientesConVisitasFrecuentes_DebeActualizarNivelFidelizacion()
        {
            // Arrange
            var cliente = CrearCliente(15);
            var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, "TF-20230101");
            tarjeta.Activar();
            
            _clienteRepositoryMock.Setup(r => r.ObtenerClientesActivosConVisitasAsync(
                It.IsAny<int>(), 
                It.IsAny<int>(), 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Cliente> { cliente });
                
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(
                cliente.Id, 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);
                
            // Act
            await _policy.EjecutarPolicy(CancellationToken.None);
            
            // Assert
            _tarjetaRepositoryMock.Verify(r => r.ActualizarAsync(
                It.Is<TarjetaFidelizacion>(t => t.NivelFidelizacion == NivelFidelizacion.Plata), 
                It.IsAny<CancellationToken>()), 
                Times.Once);
                
            _notificationManagerMock.Verify(n => n.CreateNewNotification(), Times.AtLeastOnce);
        }
        
        private Cliente CrearCliente(int numeroVisitas)
        {
            var clienteId = Guid.NewGuid();
            var fechaNacimiento = new DateTime(1980, 1, 1);
            var nombre = ClienteNombre.Crear($"Cliente{clienteId}", $"Apellido{clienteId}");
            var email = Email.Create($"cliente{clienteId}@email.com");
            var telefono = PhoneNumber.Create("+123456789");
            
            var cliente = Cliente.Crear(clienteId, nombre, email, telefono, fechaNacimiento);
                
            // Configurar las visitas
            for (int i = 0; i < numeroVisitas; i++)
            {
                cliente.RegistrarVisita();
            }
            
            // Agregar puntos para simular actividad
            cliente.AgregarPuntos(numeroVisitas * 10);
            
            return cliente;
        }
    }
}