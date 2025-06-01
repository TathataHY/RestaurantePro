namespace RestaurantePro.Domain.UnitTests.Core.Services
{
    public class CoreServiceFacadeTests
    {
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IProductoCategoriaRepository> _productoCategoriaRepositoryMock;
        private readonly Mock<IRecetaRepository> _recetaRepositoryMock;
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly Mock<IRolRepository> _rolRepositoryMock;
        private readonly Mock<INotificacionRepository> _notificacionRepositoryMock;
        private readonly Mock<IProductoCategoriaService> _productoCategoriaServiceMock;
        private readonly Mock<IRecetaService> _recetaServiceMock;
        private readonly Mock<IEventBasedNotificationService> _notificationServiceMock;
        private readonly Mock<IDateTimeService> _dateTimeServiceMock;
        private readonly Mock<INotificationManager> _notificationManagerMock;
        private readonly Mock<ILogger<ProductoBuilder>> _productoBuilderLoggerMock;
        private readonly Mock<ILogger<CoreServiceFacade>> _coreServiceFacadeLoggerMock;
        private readonly INotificationManager _notificationManager;
        
        private readonly CoreServiceFacade _sut; // System Under Test
        
        public CoreServiceFacadeTests()
        {
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _productoCategoriaRepositoryMock = new Mock<IProductoCategoriaRepository>();
            _recetaRepositoryMock = new Mock<IRecetaRepository>();
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();
            _rolRepositoryMock = new Mock<IRolRepository>();
            _notificacionRepositoryMock = new Mock<INotificacionRepository>();
            _productoCategoriaServiceMock = new Mock<IProductoCategoriaService>();
            _recetaServiceMock = new Mock<IRecetaService>();
            _notificationServiceMock = new Mock<IEventBasedNotificationService>();
            _dateTimeServiceMock = new Mock<IDateTimeService>();
            _notificationManagerMock = new Mock<INotificationManager>();
            _productoBuilderLoggerMock = new Mock<ILogger<ProductoBuilder>>();
            _coreServiceFacadeLoggerMock = new Mock<ILogger<CoreServiceFacade>>();
            
            // Usamos una implementación real para evitar problemas con los mocks
            _notificationManager = new NotificationManager();
            
            _sut = new CoreServiceFacade(
                _productoRepositoryMock.Object,
                _productoCategoriaRepositoryMock.Object,
                _recetaRepositoryMock.Object,
                _usuarioRepositoryMock.Object,
                _rolRepositoryMock.Object,
                _notificacionRepositoryMock.Object,
                _productoCategoriaServiceMock.Object,
                _recetaServiceMock.Object,
                _notificationServiceMock.Object,
                _notificationManager,
                _productoBuilderLoggerMock.Object,
                _coreServiceFacadeLoggerMock.Object,
                _dateTimeServiceMock.Object);
        }
        
        #region Productos Tests
        
        [Fact]
        public async Task ObtenerProductoPorIdAsync_DebeRetornarProducto_CuandoExiste()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var productoEsperado = Producto.Crear(
                "Producto Test", 
                "Descripción Test", 
                new PrecioProducto(10.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
            
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, CancellationToken.None))
                .ReturnsAsync(productoEsperado);
            
            // Act
            var resultado = await _sut.ObtenerProductoPorIdAsync(productoId);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(productoEsperado, resultado);
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId, CancellationToken.None), Times.Once);
        }
        
        [Fact]
        public async Task RegistrarProductoAsync_DebeCrearProducto_ConDatosValidos()
        {
            // Arrange
            var nombre = "Producto Test";
            var descripcion = "Descripción Test";
            var precio = 10.99m;
            var categoriaId = Guid.NewGuid();
            var categoriaNombre = "Categoría Test";
            
            var categoria = ProductoCategoria.Crear(categoriaNombre, "Descripción de categoría", 1);
            
            _productoCategoriaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(categoria);
            
            // Act
            var resultado = await _sut.RegistrarProductoAsync(nombre, descripcion, precio, categoriaId);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.Succeeded);
            Assert.NotNull(resultado.Value);
            Assert.Equal(nombre, resultado.Value.Nombre);
            Assert.Equal(descripcion, resultado.Value.Descripcion);
            Assert.Equal(precio, resultado.Value.Precio.Valor);
            Assert.Equal(categoriaId, resultado.Value.CategoriaId);
            
            _productoRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ActualizarProductoAsync_DebeActualizarProducto_ConDatosValidos()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var nuevoNombre = "Producto Actualizado";
            var nuevaDescripcion = "Descripción Actualizada";
            var nuevoPrecio = 15.99m;
            var nuevaCategoriaId = Guid.NewGuid();
            
            var productoExistente = Producto.Crear(
                "Producto Original", 
                "Descripción Original", 
                new PrecioProducto(10.99m), 
                Guid.NewGuid(), 
                "Categoría Original");
                
            var nuevaCategoria = ProductoCategoria.Crear("Nueva Categoría", "Descripción Nueva Categoría", 2);
            
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productoExistente);
                
            _productoCategoriaRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(nuevaCategoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(nuevaCategoria);
            
            // Act
            var resultado = await _sut.ActualizarProductoAsync(
                productoId, 
                nuevoNombre, 
                nuevaDescripcion, 
                nuevoPrecio, 
                nuevaCategoriaId, 
                true);
            
            // Assert
            Assert.True(resultado.Succeeded);
            Assert.NotNull(resultado.Value);
            Assert.Equal(nuevoNombre, resultado.Value.Nombre);
            Assert.Equal(nuevaDescripcion, resultado.Value.Descripcion);
            Assert.Equal(nuevoPrecio, resultado.Value.Precio.Valor);
            Assert.Equal(nuevaCategoriaId, resultado.Value.CategoriaId);
            Assert.True(resultado.Value.EstaActivo);
            
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
            _productoRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ActualizarProductoAsync_DebeRetornarError_CuandoProductoNoExiste()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var nuevoNombre = "Producto Actualizado";
            
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Producto)null);
                
            // Crear un NotificationManager real específico para esta prueba
            var notificationManager = new NotificationManager();
            
            // Crear una instancia específica de CoreServiceFacade para esta prueba
            var sut = new CoreServiceFacade(
                _productoRepositoryMock.Object,
                _productoCategoriaRepositoryMock.Object,
                _recetaRepositoryMock.Object,
                _usuarioRepositoryMock.Object,
                _rolRepositoryMock.Object,
                _notificacionRepositoryMock.Object,
                _productoCategoriaServiceMock.Object,
                _recetaServiceMock.Object,
                _notificationServiceMock.Object,
                notificationManager,
                _productoBuilderLoggerMock.Object,
                _coreServiceFacadeLoggerMock.Object,
                _dateTimeServiceMock.Object);
            
            // Act
            var resultado = await sut.ActualizarProductoAsync(productoId, nuevoNombre);
            
            // Assert
            Assert.False(resultado.Succeeded);
            Assert.Null(resultado.Value);
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
            _productoRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Producto>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task ObtenerProductosPorCategoriaAsync_DebeUsarProductoCategoriaService()
        {
            // Arrange
            var categoriaId = Guid.NewGuid();
            var soloActivos = true;
            
            var productos = new List<Producto>
            {
                Producto.Crear("Producto 1", "Descripción 1", new PrecioProducto(10.99m), categoriaId, "Categoría Test"),
                Producto.Crear("Producto 2", "Descripción 2", new PrecioProducto(15.99m), categoriaId, "Categoría Test")
            };
            
            _productoCategoriaServiceMock
                .Setup(s => s.ObtenerProductosPorCategoriaAsync(categoriaId, soloActivos, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productos);
            
            // Act
            var resultado = await _sut.ObtenerProductosPorCategoriaAsync(categoriaId, soloActivos);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(2, resultado.Count);
            _productoCategoriaServiceMock.Verify(
                s => s.ObtenerProductosPorCategoriaAsync(categoriaId, soloActivos, It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        [Fact]
        public async Task ActualizarCategoriaProductosAsync_DebeUsarProductoCategoriaService()
        {
            // Arrange
            var productosIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var categoriaId = Guid.NewGuid();
            var productosActualizados = 2;
            
            _productoCategoriaServiceMock
                .Setup(s => s.ActualizarCategoriaProductosAsync(productosIds, categoriaId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productosActualizados);
            
            // Act
            var resultado = await _sut.ActualizarCategoriaProductosAsync(productosIds, categoriaId);
            
            // Assert
            Assert.Equal(productosActualizados, resultado);
            _productoCategoriaServiceMock.Verify(
                s => s.ActualizarCategoriaProductosAsync(productosIds, categoriaId, It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        #endregion
        
        #region Recetas Tests
        
        [Fact]
        public async Task VerificarDisponibilidadProductoAsync_DebeUsarRecetaService()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            
            _recetaServiceMock
                .Setup(s => s.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(true));
            
            // Act
            var resultado = await _sut.VerificarDisponibilidadProductoAsync(productoId, cantidad);
            
            // Assert
            Assert.True(resultado);
            _recetaServiceMock.Verify(s => s.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ObtenerIngredientesFaltantesProductoAsync_DebeUsarRecetaService()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            var ingredientesFaltantes = new Dictionary<Guid, decimal>
            {
                { Guid.NewGuid(), 1.5m },
                { Guid.NewGuid(), 2.0m }
            };
            
            _recetaServiceMock
                .Setup(s => s.ObtenerIngredientesFaltantesAsync(productoId, cantidad, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(ingredientesFaltantes));
            
            // Act
            var resultado = await _sut.ObtenerIngredientesFaltantesProductoAsync(productoId, cantidad);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(ingredientesFaltantes.Count, resultado.Count);
            _recetaServiceMock.Verify(s => s.ObtenerIngredientesFaltantesAsync(productoId, cantidad, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ObtenerIngredientesFaltantesProductoAsync_DebeRetornarDiccionarioVacio_CuandoTodosLosIngredientesEstanDisponibles()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            
            var productoExistente = Producto.Crear(
                "Producto Test", 
                "Descripción Test", 
                new PrecioProducto(10.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(productoExistente);
                
            // Configurar servicio de recetas para devolver diccionario vacío (sin faltantes)
            _recetaServiceMock
                .Setup(s => s.ObtenerIngredientesFaltantesAsync(productoId, cantidad, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(new Dictionary<Guid, decimal>()));
            
            // Act
            var resultado = await _sut.ObtenerIngredientesFaltantesProductoAsync(productoId, cantidad);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Empty(resultado);
            
            _recetaServiceMock.Verify(s => s.ObtenerIngredientesFaltantesAsync(productoId, cantidad, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task CalcularCostoRecetaProductoAsync_DebeUsarRecetaService()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var costoEsperado = 15.5m;
            
            _recetaServiceMock
                .Setup(s => s.CalcularCostoRecetaAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(costoEsperado));
            
            // Act
            var resultado = await _sut.CalcularCostoRecetaProductoAsync(productoId);
            
            // Assert
            Assert.Equal(costoEsperado, resultado);
            _recetaServiceMock.Verify(s => s.CalcularCostoRecetaAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task CalcularRentabilidadProductoAsync_DebeUsarRecetaService()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var categoriaId = Guid.NewGuid();
            var producto = Producto.Crear(
                "Producto Test", 
                "Descripción Test", 
                new PrecioProducto(25.99m), 
                categoriaId, 
                "Categoría Test");
                
            // Establecer el ID manualmente para pruebas
            var propiedadId = producto.GetType().GetProperty("Id");
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(producto, productoId);
            }
            
            var costoReceta = 10.5m;
            var rentabilidadEsperada = RentabilidadProducto.Calcular(costoReceta, producto.Precio.Valor);
            
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
                
            _recetaServiceMock
                .Setup(s => s.CalcularRentabilidadProductoAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(rentabilidadEsperada));
            
            // Act
            var resultado = await _sut.CalcularRentabilidadProductoAsync(productoId);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(costoReceta, resultado.CostoTotal);
            Assert.Equal(producto.Precio.Valor, resultado.PrecioVenta);
            _recetaServiceMock.Verify(s => s.CalcularRentabilidadProductoAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task RegistrarRecetaProductoAsync_DebeRegistrarReceta_ConDatosValidos()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var preparacion = "Instrucciones de preparación";
            var tiempoPreparacion = 30;
            var ingredientes = new Dictionary<Guid, decimal>
            {
                { Guid.NewGuid(), 200 }, // 200g de ingrediente 1
                { Guid.NewGuid(), 50 } // 50g de ingrediente 2
            };
            
            var producto = Producto.Crear(
                "Producto Test", 
                "Descripción Test", 
                new PrecioProducto(15.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            var recetaCreada = Receta.Crear(
                productoId,
                preparacion,
                tiempoPreparacion);
                
            // Mock de nombres de ingredientes para usar en AgregarIngrediente
            var nombresIngredientes = new Dictionary<Guid, string>();
            foreach (var ing in ingredientes)
            {
                var ingredienteId = ing.Key;
                nombresIngredientes[ingredienteId] = $"Ingrediente {ingredienteId}";
                // Usamos el método correcto con todos los parámetros requeridos
                recetaCreada.AgregarIngrediente(
                    ingredienteId, 
                    nombresIngredientes[ingredienteId], 
                    ing.Value, 
                    RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Gramo, 
                    false);
            }
            
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
                
            _recetaRepositoryMock
                .Setup(r => r.ObtenerPorProductoIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Receta)null);
                
            _recetaRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Receta>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback<Receta, CancellationToken>((r, c) => 
                {
                    // Simulamos que se asigna un ID
                    var propertyInfo = typeof(Receta).GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                    propertyInfo?.SetValue(r, Guid.NewGuid());
                });
            
            // Act
            var resultado = await _sut.RegistrarRecetaProductoAsync(
                productoId, 
                preparacion, 
                tiempoPreparacion, 
                ingredientes);
            
            // Assert
            Assert.True(resultado.Succeeded);
            Assert.NotNull(resultado.Value);
            Assert.Equal(productoId, resultado.Value.ProductoId);
            Assert.Equal(preparacion, resultado.Value.Preparacion);
            Assert.Equal(tiempoPreparacion, resultado.Value.TiempoPreparacionMinutos);
            Assert.Equal(ingredientes.Count, resultado.Value.Ingredientes.Count);
            
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
            _recetaRepositoryMock.Verify(r => r.ObtenerPorProductoIdAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
            _recetaRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Receta>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task RegistrarRecetaProductoAsync_DebeActualizarReceta_CuandoYaExiste()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var preparacion = "Instrucciones actualizadas";
            var tiempoPreparacion = 25;
            var ingredientes = new Dictionary<Guid, decimal>
            {
                { Guid.NewGuid(), 150 }, // 150g de ingrediente 1
                { Guid.NewGuid(), 75 } // 75g de ingrediente 2
            };
            
            var producto = Producto.Crear(
                "Producto Test", 
                "Descripción Test", 
                new PrecioProducto(15.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            var recetaExistente = Receta.Crear(
                productoId,
                "Instrucciones originales",
                30);
                
            var ingredienteId = Guid.NewGuid();
            recetaExistente.AgregarIngrediente(
                ingredienteId, 
                "Ingrediente original", 
                100, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Gramo, 
                false);
            
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
                
            _recetaRepositoryMock
                .Setup(r => r.ObtenerPorProductoIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(recetaExistente);
                
            _recetaRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Receta>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var resultado = await _sut.RegistrarRecetaProductoAsync(
                productoId, 
                preparacion, 
                tiempoPreparacion, 
                ingredientes);
            
            // Assert
            Assert.True(resultado.Succeeded);
            Assert.NotNull(resultado.Value);
            Assert.Equal(productoId, resultado.Value.ProductoId);
            Assert.Equal(preparacion, resultado.Value.Preparacion);
            Assert.Equal(tiempoPreparacion, resultado.Value.TiempoPreparacionMinutos);
            Assert.Equal(ingredientes.Count, resultado.Value.Ingredientes.Count);
            
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
            _recetaRepositoryMock.Verify(r => r.ObtenerPorProductoIdAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
            _recetaRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Receta>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task RegistrarRecetaProductoAsync_DebeRetornarError_CuandoProductoNoExiste()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var preparacion = "Instrucciones de preparación";
            var tiempoPreparacion = 30;
            var ingredientes = new Dictionary<Guid, decimal>
            {
                { Guid.NewGuid(), 200 }, // 200g de ingrediente 1
                { Guid.NewGuid(), 50 } // 50g de ingrediente 2
            };
            
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Producto)null);
            
            // Crear un NotificationManager real específico para esta prueba
            var notificationManager = new NotificationManager();
            
            // Crear una instancia específica de CoreServiceFacade para esta prueba
            var sut = new CoreServiceFacade(
                _productoRepositoryMock.Object,
                _productoCategoriaRepositoryMock.Object,
                _recetaRepositoryMock.Object,
                _usuarioRepositoryMock.Object,
                _rolRepositoryMock.Object,
                _notificacionRepositoryMock.Object,
                _productoCategoriaServiceMock.Object,
                _recetaServiceMock.Object,
                _notificationServiceMock.Object,
                notificationManager,
                _productoBuilderLoggerMock.Object,
                _coreServiceFacadeLoggerMock.Object,
                _dateTimeServiceMock.Object);
            
            // Act
            var resultado = await sut.RegistrarRecetaProductoAsync(
                productoId, 
                preparacion, 
                tiempoPreparacion, 
                ingredientes);
            
            // Assert
            Assert.False(resultado.Succeeded);
            Assert.Null(resultado.Value);
            
            _productoRepositoryMock.Verify(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
            _recetaRepositoryMock.Verify(r => r.ObtenerPorProductoIdAsync(productoId, It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task CalcularRentabilidadProductoAsync_DebeRetornarRentabilidadBaja_CuandoNoExisteReceta()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var producto = Producto.Crear(
                "Producto Test", 
                "Descripción Test", 
                new PrecioProducto(10.99m), 
                Guid.NewGuid(), 
                "Categoría Test");
                
            _productoRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(producto);
                
            _recetaRepositoryMock
                .Setup(r => r.ObtenerPorProductoIdAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Receta)null);
                
            var rentabilidadEsperada = RentabilidadProducto.Calcular(0, 10.99m);
            
            _recetaServiceMock
                .Setup(s => s.CalcularRentabilidadProductoAsync(productoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(rentabilidadEsperada));
            
            // Act
            var resultado = await _sut.CalcularRentabilidadProductoAsync(productoId);
            
            // Assert
            Assert.Equal(rentabilidadEsperada.Rentabilidad, resultado.Rentabilidad);
            Assert.Equal(rentabilidadEsperada.MargenGanancia, resultado.MargenGanancia);
            Assert.Equal(rentabilidadEsperada.Nivel, resultado.Nivel);
            
            _recetaServiceMock.Verify(s => s.CalcularRentabilidadProductoAsync(productoId, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task RegistrarRecetaProductoAsync_DebeRetornarError_ConDatosInvalidos()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var instrucciones = ""; // Instrucciones vacías
            var tiempoPreparacion = -5; // Tiempo negativo
            var ingredientes = new Dictionary<Guid, decimal>();
            
            var notificationManager = new NotificationManager();
            
            var sut = new CoreServiceFacade(
                _productoRepositoryMock.Object,
                _productoCategoriaRepositoryMock.Object,
                _recetaRepositoryMock.Object,
                _usuarioRepositoryMock.Object,
                _rolRepositoryMock.Object,
                _notificacionRepositoryMock.Object,
                _productoCategoriaServiceMock.Object,
                _recetaServiceMock.Object,
                _notificationServiceMock.Object,
                notificationManager,
                _productoBuilderLoggerMock.Object,
                _coreServiceFacadeLoggerMock.Object,
                _dateTimeServiceMock.Object);
                
            // Act
            var resultado = await sut.RegistrarRecetaProductoAsync(
                productoId, 
                instrucciones, 
                tiempoPreparacion, 
                ingredientes);
            
            // Assert
            Assert.False(resultado.Succeeded);
            Assert.Null(resultado.Value);
            Assert.True(resultado.Errors.Count > 0);
            
            // Verificamos que no se llamó al repositorio
            _recetaRepositoryMock.Verify(r => r.ObtenerPorProductoIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _recetaRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Receta>(), It.IsAny<CancellationToken>()), Times.Never);
            _recetaRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Receta>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task BuscarSustitutoIngredienteAsync_ConIngredienteSustitutoDisponible_DebeRetornarSustituto()
        {
            // Arrange
            var ingredienteOriginalId = Guid.NewGuid();
            var ingredienteSustitutoId = Guid.NewGuid();
            
            var ingredienteSustituto = RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente.Crear(
                "Pan Integral", 
                "PAN002", 
                "Pan de molde integral", 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                0.5m,  // Stock mínimo
                2.0m); // Stock abundante
            
            // Establecer ID manualmente usando reflexión
            typeof(RestaurantePro.Domain.Core.Base.EntityBase).GetProperty("Id")!.SetValue(ingredienteSustituto, ingredienteSustitutoId);
            
            // Configurar el servicio mock para devolver el ingrediente sustituto
            _recetaServiceMock
                .Setup(s => s.BuscarSustitutoIngredienteAsync(ingredienteOriginalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success(ingredienteSustituto));
            
            // Act
            var resultado = await _sut.BuscarSustitutoIngredienteAsync(ingredienteOriginalId);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(ingredienteSustitutoId, resultado.Id);
            Assert.Equal("Pan Integral", resultado.Nombre);
        }
        
        [Fact]
        public async Task BuscarSustitutoIngredienteAsync_SinIngredienteSustitutoDisponible_DebeRetornarNull()
        {
            // Arrange
            var ingredienteOriginalId = Guid.NewGuid();
            
            // Configurar el servicio mock para devolver null
            _recetaServiceMock
                .Setup(s => s.BuscarSustitutoIngredienteAsync(ingredienteOriginalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Success<RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente>(null));
            
            // Act
            var resultado = await _sut.BuscarSustitutoIngredienteAsync(ingredienteOriginalId);
            
            // Assert
            Assert.Null(resultado);
        }
        
        [Fact]
        public async Task BuscarSustitutoIngredienteAsync_ConError_DebeRetornarNull()
        {
            // Arrange
            var ingredienteOriginalId = Guid.NewGuid();
            
            // Configurar el servicio mock para devolver un error
            _recetaServiceMock
                .Setup(s => s.BuscarSustitutoIngredienteAsync(ingredienteOriginalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result.Failure<RestaurantePro.Domain.Inventario.Ingredientes.Entities.Ingrediente>("Ingrediente no encontrado"));
            
            // Configurar que _notificationManagerMock para AddError (método void)
            _notificationManagerMock
                .Setup(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            
            // Crear una instancia específica de CoreServiceFacade para esta prueba
            // usando el mock de notificationManager en lugar de la implementación real
            var sutForTest = new CoreServiceFacade(
                _productoRepositoryMock.Object,
                _productoCategoriaRepositoryMock.Object,
                _recetaRepositoryMock.Object,
                _usuarioRepositoryMock.Object,
                _rolRepositoryMock.Object,
                _notificacionRepositoryMock.Object,
                _productoCategoriaServiceMock.Object,
                _recetaServiceMock.Object,
                _notificationServiceMock.Object,
                _notificationManagerMock.Object,
                _productoBuilderLoggerMock.Object,
                _coreServiceFacadeLoggerMock.Object,
                _dateTimeServiceMock.Object);
            
            // Act
            var resultado = await sutForTest.BuscarSustitutoIngredienteAsync(ingredienteOriginalId);
            
            // Assert
            Assert.Null(resultado);
            
            // Verificar que se haya registrado el error
            _notificationManagerMock.Verify(n => n.AddError(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce);
        }
        
        #endregion
        
        #region Usuarios Tests
        
        [Fact]
        public async Task CrearUsuarioAsync_DebeCrearUsuario_ConDatosValidos()
        {
            // Arrange
            var nombreUsuario = "usuario_test";
            var nombre = "Usuario Test";
            var emailString = "usuario@test.com";
            var email = Email.Create(emailString);
            
            // Configurar mocks para verificar que no existe un usuario con el mismo nombre o email
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorNombreUsuarioAsync(nombreUsuario, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Usuario)null);
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorEmailAsync(emailString, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Usuario)null);
            
            // Mock para el usuario creado
            var usuarioCreado = Usuario.Crear(nombreUsuario, nombre, email, RolUsuario.Cajero);
            
            // Setup para métodos utilizados en la implementación
            _usuarioRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var resultado = await _sut.CrearUsuarioAsync(nombreUsuario, nombre, emailString, "Cajero");
            
            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.Succeeded);
            Assert.NotNull(resultado.Value);
            Assert.Equal(nombreUsuario, resultado.Value.NombreUsuario);
            Assert.Equal(nombre, resultado.Value.NombreCompleto);
            Assert.Equal(emailString, resultado.Value.Email);
            
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorNombreUsuarioAsync(nombreUsuario, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task CrearUsuarioAsync_DebeRetornarError_CuandoNombreUsuarioExiste()
        {
            // Arrange
            var nombreUsuario = "usuario_existente";
            var nombre = "Usuario Test";
            var emailString = "usuario@test.com";
            var email = Email.Create(emailString);
            
            var usuarioExistente = Usuario.Crear(nombreUsuario, "Otro Usuario", email, RolUsuario.Cajero);
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorNombreUsuarioAsync(nombreUsuario, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuarioExistente);
            
            // Crear un NotificationManager real específico para esta prueba
            var notificationManager = new NotificationManager();
            
            // Crear una instancia específica de CoreServiceFacade para esta prueba
            var sut = new CoreServiceFacade(
                _productoRepositoryMock.Object,
                _productoCategoriaRepositoryMock.Object,
                _recetaRepositoryMock.Object,
                _usuarioRepositoryMock.Object,
                _rolRepositoryMock.Object,
                _notificacionRepositoryMock.Object,
                _productoCategoriaServiceMock.Object,
                _recetaServiceMock.Object,
                _notificationServiceMock.Object,
                notificationManager,
                _productoBuilderLoggerMock.Object,
                _coreServiceFacadeLoggerMock.Object,
                _dateTimeServiceMock.Object);
            
            // Act
            var resultado = await sut.CrearUsuarioAsync(nombreUsuario, nombre, emailString, "Cajero");
            
            // Assert
            Console.WriteLine($"Resultado: {resultado?.Succeeded}");
            Assert.NotNull(resultado);
            Assert.False(resultado.Succeeded);
            
            // Verificar que se llamó al repositorio para comprobar si existe el usuario
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorNombreUsuarioAsync(nombreUsuario, It.IsAny<CancellationToken>()), Times.Once);
            // Verificar que no se llamó al método AgregarAsync
            _usuarioRepositoryMock.Verify(r => r.AgregarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task ActualizarNombreUsuarioAsync_DebeActualizarUsuario_ConDatosValidos()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var nombreOriginal = "Usuario Original";
            var nombreNuevo = "Usuario Actualizado";
            var emailOriginal = "original@test.com";
            
            var usuario = Usuario.Crear("usuario_test", nombreOriginal, emailOriginal, RolUsuario.Cajero);
            
            // Establecer ID manualmente para pruebas (normalmente lo hace EF Core)
            var propiedadId = usuario.GetType().GetProperty("Id");
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(usuario, usuarioId);
            }
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            _usuarioRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var resultado = await _sut.ActualizarNombreUsuarioAsync(usuarioId, nombreNuevo);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.Succeeded);
            Assert.NotNull(resultado.Value);
            Assert.Equal(nombreNuevo, resultado.Value.NombreCompleto);
            
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task AsignarRolUsuarioAsync_DebeAsignarRoles_CuandoUsuarioExiste()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var emailVO = Email.Create("usuario@test.com");
            var usuario = Usuario.Crear("usuario_test", "Usuario Test", emailVO, RolUsuario.Cajero);
            
            // Establecer ID manualmente para pruebas
            var propiedadId = usuario.GetType().GetProperty("Id");
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(usuario, usuarioId);
            }
            
            var rolId1 = Guid.NewGuid();
            var rol = "Administrador";
            
            // Usar el método factory de Rol
            var rol1 = Rol.Crear("Rol1", "Descripción Rol 1", TipoUsuario.Administrador);
            
            // Establecer IDs manualmente para pruebas
            typeof(EntityBase).GetProperty("Id")!.SetValue(rol1, rolId1);
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            _rolRepositoryMock
                .Setup(r => r.ObtenerPorTipoUsuarioAsync(TipoUsuario.Administrador, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rol1);
            
            // Act
            var resultado = await _sut.AsignarRolUsuarioAsync(usuarioId, rol);
            
            // Assert
            Assert.NotNull(resultado);
            
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task AsignarRolUsuarioAsync_DebeRetornarFalse_CuandoUsuarioNoExiste()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var rol = "Administrador";
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Usuario)null);
            
            // Crear un NotificationManager real específico para esta prueba
            var notificationManager = new NotificationManager();
            
            // Crear una instancia específica de CoreServiceFacade para esta prueba
            var sut = new CoreServiceFacade(
                _productoRepositoryMock.Object,
                _productoCategoriaRepositoryMock.Object,
                _recetaRepositoryMock.Object,
                _usuarioRepositoryMock.Object,
                _rolRepositoryMock.Object,
                _notificacionRepositoryMock.Object,
                _productoCategoriaServiceMock.Object,
                _recetaServiceMock.Object,
                _notificationServiceMock.Object,
                notificationManager,
                _productoBuilderLoggerMock.Object,
                _coreServiceFacadeLoggerMock.Object,
                _dateTimeServiceMock.Object);
            
            // Act
            var resultado = await sut.AsignarRolUsuarioAsync(usuarioId, rol);
            
            // Assert
            Console.WriteLine($"Resultado: {resultado?.Succeeded}");
            Assert.NotNull(resultado);
            Assert.False(resultado.Succeeded);
            
            // Verificar que se consultó el repositorio
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            // Verificar que no se llamó al método ActualizarAsync
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task ActualizarEmailUsuarioAsync_DebeActualizarUsuario_ConEmailValido()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var nuevoEmail = "nuevo.email@test.com";
            
            var nombreUsuario = "usuario_test";
            var nombreCompleto = "Usuario Test";
            var emailActual = Email.Create("viejo.email@test.com");
            var usuario = Usuario.Crear(nombreUsuario, nombreCompleto, emailActual, RolUsuario.Cajero);
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
                
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorEmailAsync(nuevoEmail, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Usuario)null);
            
            // Act
            var resultado = await _sut.ActualizarEmailUsuarioAsync(usuarioId, nuevoEmail);
            
            // Assert
            Assert.True(resultado.Succeeded);
            Assert.Equal(nuevoEmail, resultado.Value.Email);
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task ActualizarEmailUsuarioAsync_DebeLanzarExcepcion_CuandoEmailEsInvalido()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var nuevoEmail = "email-invalido"; // Email inválido
            
            var nombreUsuario = "usuario_test";
            var nombreCompleto = "Usuario Test";
            var emailActual = Email.Create("viejo.email@test.com");
            var usuario = Usuario.Crear(nombreUsuario, nombreCompleto, emailActual, RolUsuario.Cajero);
            
            // Establecer ID manualmente para pruebas
            var propiedadId = usuario.GetType().GetProperty("Id");
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(usuario, usuarioId);
            }
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
                
            // Crear un NotificationManager real específico para esta prueba
            var notificationManager = new NotificationManager();
            
            // Crear una instancia específica de CoreServiceFacade para esta prueba
            var sut = new CoreServiceFacade(
                _productoRepositoryMock.Object,
                _productoCategoriaRepositoryMock.Object,
                _recetaRepositoryMock.Object,
                _usuarioRepositoryMock.Object,
                _rolRepositoryMock.Object,
                _notificacionRepositoryMock.Object,
                _productoCategoriaServiceMock.Object,
                _recetaServiceMock.Object,
                _notificationServiceMock.Object,
                notificationManager,
                _productoBuilderLoggerMock.Object,
                _coreServiceFacadeLoggerMock.Object,
                _dateTimeServiceMock.Object);
            
            // Act
            var resultado = await sut.ActualizarEmailUsuarioAsync(usuarioId, nuevoEmail);
            
            // Assert
            Assert.False(resultado.Succeeded);
            Assert.Null(resultado.Value);
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
        }
        
        [Fact]
        public async Task CambiarEstadoUsuarioAsync_DebeActivarUsuario_CuandoActivarEsTrue()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var nombreUsuario = "usuario_test";
            var nombreCompleto = "Usuario Test";
            var email = "usuario@test.com";
            var usuario = Usuario.Crear(nombreUsuario, nombreCompleto, email, RolUsuario.Cajero);
            usuario.Desactivar(); // Aseguramos que el usuario está desactivado inicialmente
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            // Act
            var resultado = await _sut.CambiarEstadoUsuarioAsync(usuarioId, true);
            
            // Assert
            Assert.True(resultado.Succeeded);
            Assert.Equal(EstadoUsuario.Activo, resultado.Value.Estado);
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task CambiarEstadoUsuarioAsync_DebeDesactivarUsuario_CuandoActivarEsFalse()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var nombreUsuario = "usuario_test";
            var nombreCompleto = "Usuario Test";
            var email = "usuario@test.com";
            var usuario = Usuario.Crear(nombreUsuario, nombreCompleto, email, RolUsuario.Cajero);
            usuario.ConfirmarCuenta(); // Confirmamos la cuenta para que esté activa
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            // Act
            var resultado = await _sut.CambiarEstadoUsuarioAsync(usuarioId, false);
            
            // Assert
            Assert.True(resultado.Succeeded);
            Assert.Equal(EstadoUsuario.Inactivo, resultado.Value.Estado);
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task LimpiarRolesUsuarioAsync_DebeLimpiarRoles_YAsignarRolPredeterminado()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var rolPredeterminado = "Cajero"; // Este nombre debe coincidir con un valor del enum RolUsuario
            
            var nombreUsuario = "usuario_test";
            var nombreCompleto = "Usuario Test";
            var email = "usuario@test.com";
            var usuario = Usuario.Crear(nombreUsuario, nombreCompleto, email, RolUsuario.Administrador);
            usuario.AsignarRol(RolUsuario.Mesero);
            usuario.AsignarRol(RolUsuario.Cocinero);
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            // No necesitamos configurar _rolRepositoryMock ya que la implementación usa Enum.TryParse
                
            // Act
            var resultado = await _sut.LimpiarRolesUsuarioAsync(usuarioId, rolPredeterminado);
            
            // Assert
            Assert.True(resultado.Succeeded);
            Assert.Contains(resultado.Value.Roles, r => r == RolUsuario.Cajero);
            Assert.Single(resultado.Value.Roles); // Solo debe tener el rol predeterminado
            _usuarioRepositoryMock.Verify(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
            _usuarioRepositoryMock.Verify(r => r.ActualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        }
        
        #endregion
        
        #region Notificaciones Tests
        
        [Fact]
        public async Task EnviarNotificacionAsync_DebeCrearYPersistirNotificacion()
        {
            // Arrange
            var destinatarioId = Guid.NewGuid();
            var tipo = "Informativa";
            var titulo = "Título Test";
            var mensaje = "Mensaje Test";
            var datos = "Datos Test";
            var prioridad = 1;
            
            _notificacionRepositoryMock
                .Setup(r => r.AgregarAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Create a test for conversion from string to TipoNotificacion enum
            var tipoNotificacion = Enum.Parse<RestaurantePro.Domain.Core.Notificaciones.Enums.TipoNotificacion>("Informativa");

            await _sut.EnviarNotificacionAsync(
                destinatarioId, 
                tipo, 
                titulo, 
                mensaje, 
                datos, 
                prioridad);
            
            // Assert
            _notificacionRepositoryMock.Verify(
                r => r.AgregarAsync(
                    It.Is<Notificacion>(n => 
                        n.DestinatarioId == destinatarioId && 
                        n.Titulo == titulo && 
                        n.Mensaje == mensaje),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
        
        [Fact]
        public async Task MarcarNotificacionComoLeidaAsync_DebeMarcarComoLeida_CuandoNotificacionExiste()
        {
            // Arrange
            var notificacionId = Guid.NewGuid();
            var destinatarioId = Guid.NewGuid();
            var ahora = new DateTime(2023, 1, 1, 12, 0, 0);
            
            _dateTimeServiceMock.Setup(d => d.Now).Returns(ahora);
            
            var notificacion = Notificacion.Crear(
                "Título Test",
                "Mensaje Test",
                RestaurantePro.Domain.Core.Notificaciones.Enums.TipoNotificacion.Informativa,
                destinatarioId);
                
            // Establecer ID manualmente para pruebas
            var propiedadId = notificacion.GetType().GetProperty("Id");
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(notificacion, notificacionId);
            }
            
            _notificacionRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(notificacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificacion);
                
            _notificacionRepositoryMock
                .Setup(r => r.ActualizarAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            
            // Act
            var resultado = await _sut.MarcarNotificacionComoLeidaAsync(notificacionId);
            
            // Assert
            Assert.True(resultado.Succeeded);
            Assert.True(notificacion.EstaLeida);
            
            _notificacionRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(notificacionId, It.IsAny<CancellationToken>()), 
                Times.Once);
            
            _notificacionRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        [Fact]
        public async Task MarcarNotificacionComoLeidaAsync_DebeRetornarFalse_CuandoNotificacionNoExiste()
        {
            // Arrange
            var notificacionId = Guid.NewGuid();
            
            _notificacionRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(notificacionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Notificacion)null);
            
            // Act
            var resultado = await _sut.MarcarNotificacionComoLeidaAsync(notificacionId);
            
            // Assert
            Assert.False(resultado.Succeeded);
            
            _notificacionRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(notificacionId, It.IsAny<CancellationToken>()), 
                Times.Once);
            
            _notificacionRepositoryMock.Verify(
                r => r.ActualizarAsync(It.IsAny<Notificacion>(), It.IsAny<CancellationToken>()), 
                Times.Never);
        }
        
        [Fact]
        public async Task ObtenerNotificacionesUsuarioAsync_DebeRetornarNotificacionesDeUsuario()
        {
            // Arrange
            var usuarioId = Guid.NewGuid();
            var otroUsuarioId = Guid.NewGuid();
            
            // Configurar el mock para que devuelva un usuario válido
            var emailVO = Email.Create("usuario@test.com");
            var usuario = Usuario.Crear("usuario_test", "Usuario Test", emailVO, RolUsuario.Cajero);
            
            // Establecer ID manualmente para pruebas
            var propiedadId = usuario.GetType().GetProperty("Id");
            if (propiedadId != null && propiedadId.CanWrite)
            {
                propiedadId.SetValue(usuario, usuarioId);
            }
            
            _usuarioRepositoryMock
                .Setup(r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(usuario);
            
            // Convertir el tipo a TipoNotificacion
            var tipoNotificacionInformativa = RestaurantePro.Domain.Core.Notificaciones.Enums.TipoNotificacion.Informativa;
            var tipoNotificacionAlerta = RestaurantePro.Domain.Core.Notificaciones.Enums.TipoNotificacion.Alerta;
            var tipoNotificacionError = RestaurantePro.Domain.Core.Notificaciones.Enums.TipoNotificacion.Error;

            // Crear algunas notificaciones de prueba
            var notificaciones = new List<Notificacion>();
            
            var notificacion1 = Notificacion.Crear(
                "Título 1",
                "Mensaje 1",
                tipoNotificacionInformativa,
                usuarioId);
                
            var notificacion2 = Notificacion.Crear(
                "Título 2",
                "Mensaje 2",
                tipoNotificacionAlerta,
                usuarioId);
                
            var notificacion3 = Notificacion.Crear(
                "Título 3",
                "Mensaje 3", 
                tipoNotificacionError,
                otroUsuarioId);
                
            notificaciones.Add(notificacion1);
            notificaciones.Add(notificacion2);
            notificaciones.Add(notificacion3);
            
            _notificacionRepositoryMock
                .Setup(r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(notificaciones);
            
            // Act
            var resultado = await _sut.ObtenerNotificacionesUsuarioAsync(usuarioId);
            
            // Assert
            Assert.NotNull(resultado);
            Assert.True(resultado.Succeeded);
            Assert.Equal(2, resultado.Value.Count);
            Assert.All(resultado.Value, n => Assert.Equal(usuarioId, n.DestinatarioId));
            
            _notificacionRepositoryMock.Verify(
                r => r.ObtenerTodosAsync(It.IsAny<CancellationToken>()), 
                Times.Once);
            
            _usuarioRepositoryMock.Verify(
                r => r.ObtenerPorIdAsync(usuarioId, It.IsAny<CancellationToken>()), 
                Times.Once);
        }
        
        #endregion
    }
} 