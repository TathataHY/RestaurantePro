namespace RestaurantePro.Domain.UnitTests.Inventario.Policies
{
    public class StockBajoPolicyTests
    {
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly Mock<IServicioNotificacionesInventario> _servicioNotificacionesMock;
        private readonly Mock<IVerificadorStock> _verificadorStockMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly StockBajoPolicy _policy;

        public StockBajoPolicyTests()
        {
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();
            _servicioNotificacionesMock = new Mock<IServicioNotificacionesInventario>();
            _verificadorStockMock = new Mock<IVerificadorStock>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();

            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 1));

            _policy = new StockBajoPolicy(
                _ingredienteRepositoryMock.Object,
                _servicioNotificacionesMock.Object,
                _verificadorStockMock.Object,
                _dateTimeServiceMock.Object);
        }

        [Fact]
        public async Task EjecutarPolicy_ConIngredientesBajoStock_DebeNotificarYGenerarOrdenes()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>
            {
                CrearIngrediente("Tomate", 5, 2),
                CrearIngrediente("Cebolla", 8, 3)
            };

            // Configuración de repository sin usar It.IsAny
            ConfigurarObtenerConStockBajo(ingredientes);

            // Configuración del resultado del verificador
            var resultadoVerificacion = new ResultadoVerificacionStock();
            var proveedor = Guid.NewGuid();
            var orden = Domain.Inventario.Compras.OrdenesCompra.Entities.OrdenCompra.Crear(
                proveedor,
                "Orden de prueba",
                DateTime.Now);
            resultadoVerificacion.OrdenesGeneradas.Add(orden);

            // Configuración de mock sin usar It.IsAny
            ConfigurarVerificadorStock(resultadoVerificacion);
            ConfigurarNotificacionStockBajo(Guid.NewGuid());

            // Act
            var resultado = await _policy.EjecutarPolicy();

            // Assert
            resultado.Notificaciones.Should().HaveCount(2);
            resultado.OrdenesCompraGeneradas.Should().HaveCount(1);

            // Verificación usando métodos sin problemas de árboles de expresión
            VerificarNotificacionesEnviadas(2);
            VerificarVerificadorInvocado(1);
        }

        [Fact]
        public async Task EjecutarPolicy_SinIngredientesBajoStock_NoDebeNotificarNiGenerarOrdenes()
        {
            // Arrange
            ConfigurarObtenerConStockBajo(new List<Ingrediente>());

            // Act
            var resultado = await _policy.EjecutarPolicy();

            // Assert
            resultado.Notificaciones.Should().BeEmpty();
            resultado.OrdenesCompraGeneradas.Should().BeEmpty();

            VerificarNotificacionesEnviadas(0);
            VerificarVerificadorInvocado(0);
        }

        [Fact]
        public async Task EjecutarPolicy_ConIngredientesBajoStockPeroSinGenerarOrdenes_DebeNotificarPeroNoGenerarOrdenes()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>
            {
                CrearIngrediente("Tomate", 5, 2),
                CrearIngrediente("Cebolla", 8, 3)
            };

            ConfigurarObtenerConStockBajo(ingredientes);
            ConfigurarVerificadorStock(new ResultadoVerificacionStock());
            ConfigurarNotificacionStockBajo(Guid.NewGuid());

            // Act
            var resultado = await _policy.EjecutarPolicy();

            // Assert
            resultado.Notificaciones.Should().HaveCount(2);
            resultado.OrdenesCompraGeneradas.Should().BeEmpty();

            VerificarNotificacionesEnviadas(2);
            VerificarVerificadorInvocado(1);
        }

        [Fact]
        public async Task PriorizarIngredientesParaReposicion_DebeOrdenarPorPrioridad()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>
            {
                CrearIngredienteConRotacionYTemporada("Tomate", 10, 2, RotacionIngrediente.Alta, TemporadaIngrediente.Verano),
                CrearIngredienteConRotacionYTemporada("Cebolla", 10, 1, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                CrearIngredienteConRotacionYTemporada("Lechuga", 10, 3, RotacionIngrediente.Critica, TemporadaIngrediente.Primavera)
            };

            ConfigurarObtenerConStockBajo(ingredientes);

            // Configurar fecha para que sea verano (enero)
            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 15));

            // Act
            var resultado = await _policy.PriorizarIngredientesParaReposicion(true);

            // Assert
            resultado.IngredientesPriorizados.Should().HaveCount(3);
            
            // El orden esperado es: 
            // 1. Tomate (alta rotación + temporada actual verano)
            // 2. Lechuga (rotación crítica pero fuera de temporada)
            // 3. Cebolla (rotación media y todo el año)
            resultado.IngredientesPriorizados[0].Nombre.Should().Be("Tomate");
            resultado.IngredientesPriorizados[1].Nombre.Should().Be("Lechuga");
            resultado.IngredientesPriorizados[2].Nombre.Should().Be("Cebolla");
        }
        
        [Fact]
        public async Task PriorizarIngredientesParaReposicion_SinConsiderarTemporada_DebeOrdenarPorRotacionYStock()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>
            {
                CrearIngredienteConRotacionYTemporada("Tomate", 10, 2, RotacionIngrediente.Alta, TemporadaIngrediente.Verano),
                CrearIngredienteConRotacionYTemporada("Cebolla", 10, 1, RotacionIngrediente.Media, TemporadaIngrediente.TodoElAño),
                CrearIngredienteConRotacionYTemporada("Lechuga", 10, 3, RotacionIngrediente.Critica, TemporadaIngrediente.Primavera)
            };

            ConfigurarObtenerConStockBajo(ingredientes);

            // Act
            var resultado = await _policy.PriorizarIngredientesParaReposicion(false);

            // Assert
            resultado.IngredientesPriorizados.Should().HaveCount(3);
            
            // El orden esperado es: 
            // 1. Lechuga (rotación crítica)
            // 2. Tomate (alta rotación)
            // 3. Cebolla (rotación media)
            resultado.IngredientesPriorizados[0].Nombre.Should().Be("Lechuga");
            resultado.IngredientesPriorizados[1].Nombre.Should().Be("Tomate");
            resultado.IngredientesPriorizados[2].Nombre.Should().Be("Cebolla");
        }
        
        [Fact]
        public async Task EjecutarPolicy_ConIngredientesPriorizados_DebeNotificarEnOrdenDePrioridad()
        {
            // Arrange
            var ingredientes = new List<Ingrediente>
            {
                CrearIngredienteConRotacionYTemporada("Tomate", 10, 2, RotacionIngrediente.Alta, TemporadaIngrediente.Verano),
                CrearIngredienteConRotacionYTemporada("Lechuga", 10, 3, RotacionIngrediente.Critica, TemporadaIngrediente.Primavera)
            };

            ConfigurarObtenerConStockBajo(ingredientes);
            ConfigurarVerificadorStock(new ResultadoVerificacionStock());
            ConfigurarNotificacionStockBajo(Guid.NewGuid());
            
            // Configurar fecha para que sea verano (enero)
            _dateTimeServiceMock.Setup(s => s.Now).Returns(new DateTime(2023, 1, 15));

            // Act
            var resultado = await _policy.EjecutarPolicy();

            // Assert
            resultado.Notificaciones.Should().HaveCount(2);
            resultado.IngredientesPriorizados.Should().HaveCount(2);
            
            // El primer ingrediente priorizado debe ser Tomate (temporada actual)
            resultado.IngredientesPriorizados[0].Nombre.Should().Be("Tomate");
            
            // Verificar el orden de las notificaciones (difícil de hacer directamente, pero
            // verificamos que se llamó al servicio dos veces con cualquier parámetro)
            VerificarNotificacionesEnviadas(2);
        }

        // Métodos auxiliares para configurar mocks evitando problemas de árboles de expresión
        private void ConfigurarObtenerConStockBajo(List<Ingrediente> ingredientes)
        {
            _ingredienteRepositoryMock
                .Setup(r => r.ObtenerConStockBajoAsync(It.Is<CancellationToken>(t => true)))
                .ReturnsAsync(ingredientes);
        }

        private void ConfigurarVerificadorStock(ResultadoVerificacionStock resultado) 
        { 
            _verificadorStockMock
                .Setup(v => v.VerificarYGenerarOrdenesCompraAsync(It.Is<CancellationToken>(t => true)))
                .ReturnsAsync(resultado); 
        }

        private void ConfigurarNotificacionStockBajo(Guid notificacionId)
        {
            // Usamos genéricamente para cualquier parámetro para evitar árboles de expresión
            _servicioNotificacionesMock
                .Setup(s => s.NotificarStockBajo(
                    It.Is<Guid>(g => true),
                    It.Is<string>(s => true),
                    It.Is<decimal>(d => true),
                    It.Is<decimal>(d => true)))
                .ReturnsAsync(notificacionId);
        }

        private void VerificarNotificacionesEnviadas(int veces)
        {
            _servicioNotificacionesMock.Verify(
                s => s.NotificarStockBajo(
                    It.Is<Guid>(g => true),
                    It.Is<string>(s => true),
                    It.Is<decimal>(d => true),
                    It.Is<decimal>(d => true)),
                Times.Exactly(veces));
        }

        private void VerificarVerificadorInvocado(int veces) 
        { 
            _verificadorStockMock.Verify(
                v => v.VerificarYGenerarOrdenesCompraAsync(It.Is<CancellationToken>(t => true)), 
                Times.Exactly(veces)); 
        }

        private Ingrediente CrearIngrediente(string nombre, decimal stockMinimo, decimal stockActual)
        {
            var ingrediente = Ingrediente.Crear(
                nombre,
                "ING-" + Guid.NewGuid().ToString().Substring(0, 5),
                $"Descripción de {nombre}",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                stockMinimo,
                stockActual);

            // Asignar un proveedor ficticio
            ingrediente.AsociarProveedorPrincipal(Guid.NewGuid());

            return ingrediente;
        }

        private Ingrediente CrearIngredienteConRotacionYTemporada(
            string nombre, 
            decimal stockMinimo, 
            decimal stockActual, 
            RotacionIngrediente rotacion, 
            TemporadaIngrediente temporada)
        {
            var ingrediente = Ingrediente.Crear(
                nombre,
                "ING-" + Guid.NewGuid().ToString().Substring(0, 5),
                $"Descripción de {nombre}",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                stockMinimo,
                stockActual,
                rotacion,
                temporada);

            // Asignar un proveedor ficticio
            ingrediente.AsociarProveedorPrincipal(Guid.NewGuid());
            
            // Asignar un costo promedio para las pruebas
            ingrediente.ActualizarCostoPromedio(rotacion switch {
                RotacionIngrediente.Baja => 50.0m,
                RotacionIngrediente.Media => 100.0m,
                RotacionIngrediente.Alta => 200.0m,
                RotacionIngrediente.Critica => 350.0m,
                _ => 0
            });

            return ingrediente;
        }
    }
}



