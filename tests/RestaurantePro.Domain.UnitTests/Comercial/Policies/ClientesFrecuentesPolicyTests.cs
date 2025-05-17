namespace RestaurantePro.Domain.UnitTests.Comercial.Policies
{
    public class ClientesFrecuentesPolicyTests
    {
        private readonly Mock<IClienteRepository> _clienteRepositoryMock;
        private readonly Mock<ITarjetaFidelizacionRepository> _tarjetaRepositoryMock;
        private readonly Mock<IServicioFidelizacion> _servicioFidelizacionMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly ClientesFrecuentesPolicy _policy;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public ClientesFrecuentesPolicyTests()
        {
            _clienteRepositoryMock = new Mock<IClienteRepository>();
            _tarjetaRepositoryMock = new Mock<ITarjetaFidelizacionRepository>();
            _servicioFidelizacionMock = new Mock<IServicioFidelizacion>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            
            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 1));
            
            _policy = new ClientesFrecuentesPolicy(
                _clienteRepositoryMock.Object,
                _tarjetaRepositoryMock.Object,
                _servicioFidelizacionMock.Object,
                _dateTimeServiceMock.Object);
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
            _clienteRepositoryMock.Setup(r => r.ObtenerClientesActivosConVisitasAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(clientesEnumerable));
                
            // Setup para cada llamada específica a ObtenerTarjetaActivaPorClienteIdAsync con los IDs de cada cliente
            foreach (var cliente in clientes)
            {
                var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, $"TF-{Guid.NewGuid():N}");
                tarjeta.Activar();
                _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult<TarjetaFidelizacion?>(tarjeta));
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
            _clienteRepositoryMock.Setup(r => r.ObtenerClientesActivosConVisitasAsync(It.IsAny<CancellationToken>()))
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
            _clienteRepositoryMock.Setup(r => r.ObtenerClientesActivosConVisitasAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(clientesEnumerable));
                
            // Simular que no tienen tarjeta - usamos variable explícitamente nula
            TarjetaFidelizacion? tarjetaNull = null;
            foreach (var cliente in clientes)
            {
                _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
                    .Returns(Task.FromResult(tarjetaNull));
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
                .Returns(Task.FromResult<TarjetaFidelizacion?>(tarjeta));
                
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
        
        // Métodos auxiliares para crear objetos de prueba
        private Cliente CrearClienteConVisitas(string nombre, int cantidadVisitas, NivelFidelizacion nivelActual)
        {
            var partes = nombre.Split(' ');
            var nombreCliente = ClienteNombre.Crear(
                partes[0],
                partes.Length > 1 ? partes[1] : "Apellido");
                
            var cliente = Cliente.Crear(
                nombreCliente, 
                $"cliente{Guid.NewGuid():N8}@test.com", 
                "123456789");
                
            // Simular historial de visitas
            var propiedadVisitas = cliente.GetType().GetProperty("CantidadVisitas", 
                BindingFlags.Instance | BindingFlags.NonPublic);
                
            propiedadVisitas?.SetValue(cliente, cantidadVisitas);
            
            return cliente;
        }
    }
} 



