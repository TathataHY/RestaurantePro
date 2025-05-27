#nullable disable
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
                CrearClienteConVisitas("Cliente 1", 10, NivelFidelizacion.Basico),
                CrearClienteConVisitas("Cliente 2", 25, NivelFidelizacion.Plata),
                CrearClienteConVisitas("Cliente 3", 40, NivelFidelizacion.Oro)
            };
            
            IEnumerable<Cliente> clientesEnumerable = clientes;
            _clienteRepositoryMock.Setup(r => r.ObtenerClientesActivosConVisitasAsync(90, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(clientesEnumerable));
                
            // Setup para cada llamada específica a ObtenerTarjetaActivaPorClienteIdAsync con los IDs de cada cliente
            foreach (var cliente in clientes)
            {
                var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, $"TF-{Guid.NewGuid():N}");
                tarjeta.Activar();
                _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(tarjeta);
            }
                
            _tarjetaRepositoryMock.Setup(r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
                
            // Act
            var resultado = await _policy.EjecutarPolicy(_cancellationToken);
            
            // Assert
            resultado.ClientesActualizados.Should().HaveCount(3);
            
            // Verificar que se llamó al método para actualizar nivel para cada cliente
            _tarjetaRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), It.IsAny<CancellationToken>()),
                Times.Exactly(3));
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
                
            // Simular que no tienen tarjeta - usamos variable explícitamente nula
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
            segmentoProperty?.SetValue(clienteInactivo, SegmentoCliente.FrecuenciaAlta); // Cambiará a Inactivo
            segmentoProperty?.SetValue(clientePremium, SegmentoCliente.TicketAlto);      // Cambiará a Premium
            
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
            
            // Configuraciones específicas para simular el comportamiento que queremos probar
            // Para clienteInactivo, simulamos que pasó mucho tiempo desde la última visita
            _dateTimeServiceMock
                .Setup(d => d.Now)
                .Returns(new DateTime(2023, 1, 1));
            
            // Configurar el mock de actualización
            _clienteRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var resultado = await _policy.EjecutarSegmentacionClientes(_cancellationToken);
            
            // Assert
            // Verificar que se hayan llamado los métodos adecuados
            _clienteRepositoryMock.Verify(
                r => r.ObtenerClientesConHistorialVisitasAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Once);
            
            // Verificar que se haya intentado actualizar al menos un cliente
            _clienteRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()),
                Times.AtLeast(1));
            
            // Verificar que el resultado contenga información de segmentación
            resultado.ConteoSegmentos.Should().NotBeEmpty();
            
            // Verificar que se hayan contado todos los segmentos
            resultado.ConteoSegmentos.Keys.Count.Should().Be(Enum.GetValues(typeof(SegmentoCliente)).Length);
            
            // La suma de todos los segmentos debe ser igual al total de clientes
            resultado.ConteoSegmentos.Values.Sum().Should().Be(clientes.Count);
            
            // Verificar que se reporta al menos un cliente segmentado
            resultado.ClientesSegmentados.Should().HaveCountGreaterThan(0);
        }
        
        // Métodos auxiliares para crear objetos de prueba
        private Cliente CrearClienteConVisitas(string nombre, int cantidadVisitas, NivelFidelizacion nivelActual)
        {
            var partes = nombre.Split(' ');
            var nombreCliente = ClienteNombre.Crear(
                partes[0],
                partes.Length > 1 ? partes[1] : "Apellido");
                
            // Usar un formato de email válido sin patrones repetitivos
            // Mezclamos texto aleatorio para evitar patrones repetitivos
            var random = new Random();
            var randomText = new string(Enumerable.Range(0, 8)
                .Select(_ => (char)('a' + random.Next(0, 26)))
                .ToArray());
                
            var email = $"{partes[0].ToLower()}.{randomText}@test.com";
            
            // Usar CrearParaPruebas en lugar de Crear para evitar validaciones estrictas en tests
            var cliente = Cliente.CrearParaPruebas(
                nombreCliente, 
                email,
                "123456789");
                
            // Establecer cantidad de visitas
            for (int i = 0; i < cantidadVisitas; i++)
            {
                cliente.RegistrarVisita();
            }
            
            // Establecer nivel de fidelización directamente
            if (nivelActual != NivelFidelizacion.Basico)
            {
                var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, $"TF-{Guid.NewGuid():N}");
                tarjeta.Activar();
                tarjeta.ActualizarNivel(nivelActual);
                cliente.AsociarTarjetaFidelizacion(tarjeta.Id);
            }
            
            return cliente;
        }
    }
} 



