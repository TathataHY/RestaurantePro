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
            
            _clienteRepositoryMock.Setup(r => r.ObtenerClientesActivosConVisitasAsync(_cancellationToken))
                .ReturnsAsync(clientes);
                
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(It.IsAny<Guid>(), _cancellationToken))
                .ReturnsAsync((Guid clienteId) => {
                    var cliente = clientes.FirstOrDefault(c => c.Id == clienteId);
                    if (cliente == null) return null;
                    
                    var tarjeta = TarjetaFidelizacion.Crear(clienteId, $"TF-{Guid.NewGuid().ToString().Substring(0, 8)}");
                    tarjeta.Activar();
                    
                    return tarjeta;
                });
                
            _tarjetaRepositoryMock.Setup(r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), _cancellationToken))
                .Returns(Task.CompletedTask);
                
            // Act
            var resultado = await _policy.EjecutarPolicy(_cancellationToken);
            
            // Assert
            resultado.ClientesActualizados.Should().HaveCount(3);
            
            // Verificar que se llamó al método para actualizar nivel para cada cliente
            _tarjetaRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), _cancellationToken),
                Times.Exactly(3));
        }
        
        [Fact]
        public async Task EjecutarPolicy_SinClientesActivos_NoDebeActualizarNada()
        {
            // Arrange
            _clienteRepositoryMock.Setup(r => r.ObtenerClientesActivosConVisitasAsync(_cancellationToken))
                .ReturnsAsync(new List<Cliente?>());
                
            // Act
            var resultado = await _policy.EjecutarPolicy(_cancellationToken);
            
            // Assert
            resultado.ClientesActualizados.Should().BeEmpty();
            
            _tarjetaRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), _cancellationToken),
                Times.Never());
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
            
            _clienteRepositoryMock.Setup(r => r.ObtenerClientesActivosConVisitasAsync(_cancellationToken))
                .ReturnsAsync(clientes);
                
            // Simular que no tienen tarjeta
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(It.IsAny<Guid>(), _cancellationToken))
                .ReturnsAsync((TarjetaFidelizacion)null);
                
            _tarjetaRepositoryMock.Setup(r => r.AddAsync(It.IsAny<TarjetaFidelizacion>()))
                .ReturnsAsync((TarjetaFidelizacion tarjeta) => tarjeta);
                
            // Act
            var resultado = await _policy.EjecutarPolicy(_cancellationToken);
            
            // Assert
            resultado.ClientesActualizados.Should().HaveCount(2);
            resultado.TarjetasCreadas.Should().HaveCount(2);
            
            _tarjetaRepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<TarjetaFidelizacion>()),
                Times.Exactly(2));
        }
        
        [Fact]
        public async Task EjecutarPolicyParaCliente_ClienteFrecuente_DebeActualizarNivel()
        {
            // Arrange
            var cliente = CrearClienteConVisitas("Cliente Test", 25, NivelFidelizacion.Basico);
            
            _clienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(cliente.Id, _cancellationToken))
                .ReturnsAsync(cliente);
                
            var tarjeta = TarjetaFidelizacion.Crear(cliente.Id, $"TF-{Guid.NewGuid().ToString().Substring(0, 8)}");
            tarjeta.Activar();
            
            _tarjetaRepositoryMock.Setup(r => r.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id, _cancellationToken))
                .ReturnsAsync(tarjeta);
                
            _tarjetaRepositoryMock.Setup(r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), _cancellationToken))
                .Returns(Task.CompletedTask);
                
            // Act
            var resultado = await _policy.EjecutarPolicyParaCliente(cliente.Id, _cancellationToken);
            
            // Assert
            resultado.ClientesActualizados.Should().HaveCount(1);
            resultado.ClientesActualizados.First().Should().Be(cliente.Id);
            resultado.TarjetasCreadas.Should().BeEmpty();
            
            _tarjetaRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<TarjetaFidelizacion>(), _cancellationToken),
                Times.Once());
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
                $"cliente{Guid.NewGuid().ToString().Substring(0, 5)}@test.com", 
                "123456789");
                
            // Simular historial de visitas
            var propiedadVisitas = cliente.GetType().GetProperty("CantidadVisitas", 
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.NonPublic);
                
            if (propiedadVisitas != null)
            {
                propiedadVisitas.SetValue(cliente, cantidadVisitas);
            }
            
            return cliente;
        }
    }
} 



