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
using RestaurantePro.Domain.Comercial.Clientes.ValueObjects;
using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Core.SharedKernel.Services.Notification;

namespace RestaurantePro.Domain.UnitTests.Comercial.Policies
{
    public class ClientesFrecuentesPolicyTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
        private readonly Mock<IServicioFidelizacion> _servicioFidelizacionMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<INotificationManager> _notificationManagerMock;
        private readonly INotificationManager _notificationManager;
        private readonly ClientesFrecuentesPolicy _policy;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public ClientesFrecuentesPolicyTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
            _servicioFidelizacionMock = new Mock<IServicioFidelizacion>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _notificationManagerMock = new Mock<INotificationManager>();
            
            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 1));
            
            // Usar NotificationManager real en lugar de mock para evitar errores
            _notificationManager = new NotificationManager();
            
            _policy = new ClientesFrecuentesPolicy(
                _clienteRepositoryMock.Object,
                _tarjetaRepositoryMock.Object,
                _servicioFidelizacionMock.Object,
                _dateTimeServiceMock.Object,
                _notificationManager);
        }
        
        [Fact]
        public async Task EjecutarPolicy_ClientesConVisitasFrecuentes_DebeActualizarNivelFidelizacion()
        {
            // Arrange
            var clientes = new List<Cliente>
            {
                CrearClienteConVisitas("Cliente Frecuente", 12, NivelFidelizacion.Basico),
                CrearClienteConVisitas("Cliente Regular", 20, NivelFidelizacion.Basico),
                CrearClienteConVisitas("Cliente Premium", 35, NivelFidelizacion.Basico)
            };
            
            IEnumerable<Cliente> clientesEnumerable = clientes;
            _clienteRepositoryMock.Setup(r => r.ObtenerClientesActivosConVisitasAsync(90, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(clientesEnumerable));
                
            // Simulamos que tienen tarjetas, pero con nivel basico por defecto
            foreach (var cliente in clientes)
            {
                var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, $"TF-{Guid.NewGuid():N}");
                tarjeta.Activar();
                
                _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(tarjeta);
                    
                _tarjetaRepositoryMock.Setup(r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
            }
            
            // Act
            var resultado = await _policy.EjecutarPolicy(_cancellationToken);
            
            // Assert
            resultado.ClientesActualizados.Should().HaveCount(3);
            
            // Verificar actualizaciones (al menos una tarjeta por cada nivel de fidelizacion)
            _tarjetaRepositoryMock.Verify(
                r => r.ActualizarAsync(It.Is<TarjetaFidelizacion>(t => t.NivelFidelizacion == NivelFidelizacion.Plata), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
                
            _tarjetaRepositoryMock.Verify(
                r => r.ActualizarAsync(It.Is<TarjetaFidelizacion>(t => t.NivelFidelizacion == NivelFidelizacion.Oro), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }
        
        [Fact]
        public async Task EjecutarPolicy_SinClientesActivos_NoDebeActualizarNada()
        {
            // Arrange
            IEnumerable<Cliente> clientesVacios = new List<Cliente>();
            _clienteRepositoryMock.Setup(r => r.ObtenerClientesActivosConVisitasAsync(90, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(clientesVacios));
                
            // Act
            var resultado = await _policy.EjecutarPolicy(_cancellationToken);
            
            // Assert
            resultado.ClientesActualizados.Should().BeEmpty();
            
            _tarjetaRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
        
        [Fact]
        public async Task EjecutarPolicy_ClientesSinTarjetaFidelizacion_DebeCrearTarjetaYActualizarNivel()
        {
            // Arrange
            var clientes = new List<Cliente>
            {
                CrearClienteConVisitas("Cliente 1", 15, NivelFidelizacion.Basico),
                CrearClienteConVisitas("Cliente 2", 30, NivelFidelizacion.Basico)
            };
            
            IEnumerable<Cliente> clientesEnumerable = clientes;
            _clienteRepositoryMock.Setup(r => r.ObtenerClientesActivosConVisitasAsync(90, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(clientesEnumerable));
                
            // Simular que no tienen tarjeta - usamos variable explicitamente nula
            foreach (var cliente in clientes)
            {
                _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TarjetaFidelizacion?)null);
            }
            
            _tarjetaRepositoryMock.Setup(r => r.AgregarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            var resultado = await _policy.EjecutarPolicy(_cancellationToken);
            
            // Assert
            resultado.ClientesActualizados.Should().HaveCount(2);
            resultado.TarjetasCreadas.Should().HaveCount(2);
            
            _tarjetaRepositoryMock.Verify(
                r => r.AgregarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }
        
        [Fact]
        public async Task EjecutarPolicyParaCliente_ClienteFrecuente_DebeActualizarNivel()
        {
            // Arrange
            var cliente = CrearClienteConVisitas("Cliente Test", 25, NivelFidelizacion.Basico);
            
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<Cliente?>(cliente));
                
            var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, $"TF-{Guid.NewGuid():N}");
            tarjeta.Activar();
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjeta);
                
            _tarjetaRepositoryMock.Setup(r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            var resultado = await _policy.EjecutarPolicyParaCliente(cliente.Id, _cancellationToken);
            
            // Assert
            resultado.ClientesActualizados.Should().HaveCount(1);
            resultado.ClientesActualizados.First().Should().Be(cliente.Id);
            resultado.TarjetasCreadas.Should().BeEmpty();
            
            _tarjetaRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task EjecutarSegmentacionClientes_DebeAsignarSegmentosCorrectamente()
        {
            // Arrange
            // Creamos clientes con diferentes perfiles pero segmentos iniciales que sabemos van a cambiar
            var clienteFrecuente = CrearClienteConVisitas("ClienteFrecuente", 20, NivelFidelizacion.Oro);
            var clienteInactivo = CrearClienteConVisitas("ClienteInactivo", 5, NivelFidelizacion.Basico);
            var clientePremium = CrearClienteConVisitas("ClientePremium", 40, NivelFidelizacion.Platino);
            
            // Asegurarnos que tengan segmentos que van a cambiar
            // Usamos reflection para establecer segmentos iniciales diferentes
            var segmentoProperty = typeof(Cliente).GetProperty("Segmento");
            segmentoProperty?.SetValue(clienteFrecuente, SegmentoCliente.SinClasificar);
            segmentoProperty?.SetValue(clienteInactivo, SegmentoCliente.FrecuenciaAlta); // Cambiara a Inactivo
            segmentoProperty?.SetValue(clientePremium, SegmentoCliente.TicketAlto);      // Cambiara a Premium
            
            var clientes = new List<Cliente> { clienteFrecuente, clienteInactivo, clientePremium };
            
            // Configurar el repositorio para devolver los clientes
            _clienteRepositoryMock
                .Setup(r => r.ObtenerClientesConHistorialVisitasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientes);
                
            // Simulamos que el repositorio devuelve tarjetas
            // Para clienteFrecuente - configuramos para que aparezca como de alta frecuencia
            var tarjetaFrecuente = TarjetaFidelizacion.Crear(clienteFrecuente.Id, $"TF-{Guid.NewGuid():N}");
            tarjetaFrecuente.Activar();
            tarjetaFrecuente.ActualizarNivel(NivelFidelizacion.Oro);
            
            // Para clienteInactivo - configuramos para que aparezca como inactivo
            var tarjetaInactivo = TarjetaFidelizacion.Crear(clienteInactivo.Id, $"TF-{Guid.NewGuid():N}");
            tarjetaInactivo.Activar();
            tarjetaInactivo.ActualizarNivel(NivelFidelizacion.Basico);
            
            // Para clientePremium - configuramos para que aparezca como premium (alta frecuencia y alto ticket)
            var tarjetaPremium = TarjetaFidelizacion.Crear(clientePremium.Id, $"TF-{Guid.NewGuid():N}");
            tarjetaPremium.Activar();
            tarjetaPremium.ActualizarNivel(NivelFidelizacion.Platino);
            
            _tarjetaRepositoryMock
                .Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteFrecuente.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjetaFrecuente);
                
            _tarjetaRepositoryMock
                .Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clienteInactivo.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjetaInactivo);
                
            _tarjetaRepositoryMock
                .Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(clientePremium.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tarjetaPremium);
            
            // Configuraciones especificas para simular el comportamiento que queremos probar
            // Para clienteInactivo, simulamos que paso mucho tiempo desde la ultima visita
            _dateTimeServiceMock
                .Setup(d => d.Now)
                .Returns(new DateTime(2023, 1, 1));
            
            // Configurar el mock de actualizacion
            _clienteRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var resultado = await _policy.EjecutarSegmentacionClientes(_cancellationToken);
            
            // Assert
            // Verificar que se hayan llamado los metodos adecuados
            _clienteRepositoryMock.Verify(
                r => r.ObtenerClientesConHistorialVisitasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Once);
                
            _clienteRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
                
            // Verificar que el cliente frecuente haya pasado a FrecuenciaAlta
            _clienteRepositoryMock.Verify(
                r => r.ActualizarAsync(It.Is<Cliente>(c => c.Id == clienteFrecuente.Id && c.Segmento == SegmentoCliente.FrecuenciaAlta), 
                It.IsAny<CancellationToken>()),
                Times.Once);
                
            // Verificar que el cliente premium haya pasado a Premium
            _clienteRepositoryMock.Verify(
                r => r.ActualizarAsync(It.Is<Cliente>(c => c.Id == clientePremium.Id && c.Segmento == SegmentoCliente.Premium), 
                It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        private Cliente CrearClienteConVisitas(string nombre, int cantidadVisitas, NivelFidelizacion nivelActual)
        {
            var nombreParts = nombre.Split(' ');
            var nombreCliente = ClienteNombre.Crear(
                nombreParts[0],
                nombreParts.Length > 1 ? nombreParts[1] : "Apellido");

            var emailObj = Email.Create("test@email.com");
            var telefonoObj = PhoneNumber.Create("+5491112345678");
            
            var cliente = Cliente.Crear(
                Guid.NewGuid(),
                nombreCliente,
                emailObj,
                telefonoObj,
                DateTime.Now.AddYears(-30) // Mayor de edad
            );
            
            // Simular la cantidad de visitas usando reflection
            var visitasField = typeof(Cliente).GetField("_cantidadVisitas", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            visitasField?.SetValue(cliente, cantidadVisitas);
            
            // Asignar fecha de ultima visita reciente
            var ultimaVisitaField = typeof(Cliente).GetField("_fechaUltimaVisita", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            ultimaVisitaField?.SetValue(cliente, DateTime.Now.AddDays(-7));
            
            // Asignar nivel de fidelización
            var nivelField = typeof(Cliente).GetField("_nivelFidelizacion", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            nivelField?.SetValue(cliente, nivelActual);
            
            return cliente;
        }
    }
} 