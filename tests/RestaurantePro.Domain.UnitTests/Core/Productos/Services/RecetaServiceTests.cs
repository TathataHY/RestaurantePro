namespace RestaurantePro.Domain.UnitTests.Core.Productos.Services
{
    public class RecetaServiceTests
    {
        private readonly Mock<IRecetaRepository> _recetaRepositoryMock;
        private readonly Mock<IProductoRepository> _productoRepositoryMock;
        private readonly Mock<IIngredienteRepository> _ingredienteRepositoryMock;
        private readonly RecetaService _recetaService;
        private readonly CancellationToken _cancellationToken = CancellationToken.None;

        public RecetaServiceTests()
        {
            _recetaRepositoryMock = new Mock<IRecetaRepository>();
            _productoRepositoryMock = new Mock<IProductoRepository>();
            _ingredienteRepositoryMock = new Mock<IIngredienteRepository>();

            _recetaService = new RecetaService(
                _recetaRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _ingredienteRepositoryMock.Object);
        }

        #region ObtenerIngredientesParaProductoAsync

        [Fact]
        public async Task ObtenerIngredientesParaProductoAsync_ProductoNoExiste_DebeLanzarException()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Producto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _recetaService.ObtenerIngredientesParaProductoAsync(productoId, _cancellationToken));
        }

        [Fact]
        public async Task ObtenerIngredientesParaProductoAsync_RecetaNoExiste_DebeRetornarDiccionarioVacio()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            // Usar una instancia real en lugar de un mock
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Receta?)null);

            // Act
            var resultado = await _recetaService.ObtenerIngredientesParaProductoAsync(productoId, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeEmpty();
        }

        [Fact]
        public async Task ObtenerIngredientesParaProductoAsync_RecetaConIngredientes_DebeRetornarDiccionarioConIngredientes()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            // Usar una instancia real en lugar de un mock
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Act
            var resultado = await _recetaService.ObtenerIngredientesParaProductoAsync(productoId, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2);
            resultado.Should().ContainKey(ingrediente1Id);
            resultado.Should().ContainKey(ingrediente2Id);
            resultado[ingrediente1Id].Should().Be(0.2m);
            resultado[ingrediente2Id].Should().Be(0.3m);
        }

        #endregion

        #region VerificarDisponibilidadIngredientesAsync

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_CantidadNoPositiva_DebeLanzarException()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken));
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_SinIngredientes_DebeRetornarTrue()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            // Usar una instancia real en lugar de un mock
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Receta?)null);

            // Act
            var resultado = await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_IngredienteNoExiste_DebeRetornarFalse()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            // Usar una instancia real en lugar de un mock
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync((Ingrediente?)null);

            // Act
            var resultado = await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().BeFalse();
        }

        // Helper para crear ingredientes simulados para pruebas
        private Ingrediente CrearIngredienteSimulado(Guid id, decimal stock)
        {
            // Crear un ingrediente real usando el factory method
            var ingrediente = Ingrediente.Crear(
                "Ingrediente de prueba",
                "TEST-" + id.ToString().Substring(0, 8),
                "Ingrediente simulado para pruebas",
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                stock / 2, // StockMinimo (no importa para estas pruebas)
                stock);    // Stock actual

            // Reemplazar el Id generado con el Id específico que queremos
            typeof(EntityBase).GetProperty("Id")!.SetValue(ingrediente, id);
            
            return ingrediente;
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_StockSuficiente_DebeRetornarTrue()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Crear un ingrediente simulado con stock suficiente
            var ingrediente = CrearIngredienteSimulado(ingrediente1Id, 2.0m); // Stock suficiente para 5 unidades (5 * 0.2 = 1kg)

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente);

            // Act
            var resultado = await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_StockInsuficiente_DebeRetornarFalse()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Crear un ingrediente simulado con stock insuficiente
            var ingrediente = CrearIngredienteSimulado(ingrediente1Id, 0.5m); // Stock insuficiente para 5 unidades (5 * 0.2 = 1kg)

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente);

            // Act
            var resultado = await _recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task VerificarDisponibilidadIngredientesAsync_IngredientesFaltantes_DebeAgregarErroresDetallados()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            var precio = new PrecioProducto(10.99m);
            var notificationManager = new NotificationManager();

            var recetaService = new RecetaService(
                _recetaRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _ingredienteRepositoryMock.Object,
                notificationManager);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Crear un ingrediente con stock insuficiente
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 0.5m); // Stock insuficiente para 5 unidades (5 * 0.2 = 1kg)
            ingrediente1.ActualizarNombre("Tomate");

            // El segundo ingrediente no existe en la base de datos

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync((Ingrediente?)null);

            // Act
            var resultado = await recetaService.VerificarDisponibilidadIngredientesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue(); // Retorna True pero con errores en NotificationManager
            resultado.Value.Should().BeFalse(); // El valor indica que no hay disponibilidad
            
            notificationManager.HasErrors.Should().BeTrue();
            notificationManager.GetErrors().Should().HaveCountGreaterThan(1);
            
            // Verificar que hay un error específico para cada ingrediente faltante
            var errores = notificationManager.GetErrors().ToList();
            errores.Should().Contain(e => e.PropertyName == $"Ingrediente_{ingrediente1Id}");
            errores.Should().Contain(e => e.PropertyName == $"Ingrediente_{ingrediente2Id}");
            
            // Verificar que los mensajes contienen información detallada
            var mensajeIngrediente1 = errores.FirstOrDefault(e => e.PropertyName == $"Ingrediente_{ingrediente1Id}")?.Message;
            mensajeIngrediente1.Should().Contain("Tomate");
            mensajeIngrediente1.Should().Contain("0.5"); // Stock disponible
            
            var mensajeIngrediente2 = errores.FirstOrDefault(e => e.PropertyName == $"Ingrediente_{ingrediente2Id}")?.Message;
            mensajeIngrediente2.Should().Contain("Ingrediente no encontrado");
        }

        #endregion

        #region ObtenerIngredientesFaltantesAsync

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_CantidadNoPositiva_DebeLanzarException()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken));
        }

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_SinIngredientes_DebeRetornarDiccionarioVacio()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Receta?)null);

            // Act
            var resultado = await _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeEmpty();
        }

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_IngredientesFaltantes_DebeRetornarIngredientesFaltantes()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Crear ingredientes simulados
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 0.5m); // Stock insuficiente para 5 unidades (5 * 0.2 = 1kg)
            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 2.0m); // Stock suficiente para 5 unidades (5 * 0.3 = 1.5kg)

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);

            // Act
            var resultado = await _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(1);
            resultado.Should().ContainKey(ingrediente1Id);
            resultado[ingrediente1Id].Should().Be(0.5m); // Faltante: 1kg - 0.5kg = 0.5kg
        }

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_SinIngredientesFaltantes_DebeRetornarDiccionarioVacio()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 5;
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Crear un ingrediente simulado con stock suficiente
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 2.0m); // Stock suficiente para 5 unidades (5 * 0.2 = 1kg)

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);

            // Act
            var resultado = await _recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Should().BeEmpty();
        }

        [Fact]
        public async Task ObtenerIngredientesFaltantesAsync_ConFaltantes_DebeRetornarDiccionarioYNotificaciones()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var cantidad = 3;
            var precio = new PrecioProducto(12.99m);
            var notificationManager = new NotificationManager();

            var recetaService = new RecetaService(
                _recetaRepositoryMock.Object,
                _productoRepositoryMock.Object,
                _ingredienteRepositoryMock.Object,
                notificationManager);

            var producto = Producto.Crear(
                "Hamburguesa Completa", 
                "Hamburguesa con todos los ingredientes", 
                precio, 
                Guid.NewGuid(), 
                "Hamburguesas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 20);
            
            var ingrediente1Id = Guid.NewGuid(); // Pan
            var ingrediente2Id = Guid.NewGuid(); // Carne
            var ingrediente3Id = Guid.NewGuid(); // Queso - Faltante
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Pan", 
                1.0m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Unidad);

            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Carne", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
                
            receta.AgregarIngrediente(
                ingrediente3Id, 
                "Queso", 
                0.1m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            // Configurar ingredientes disponibles e insuficientes
            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 10.0m); // Stock suficiente
            ingrediente1.ActualizarNombre("Pan");
            
            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 1.0m); // Stock suficiente para 5 hamburguesas
            ingrediente2.ActualizarNombre("Carne");
            
            var ingrediente3 = CrearIngredienteSimulado(ingrediente3Id, 0.1m); // Stock solo para 1 hamburguesa
            ingrediente3.ActualizarNombre("Queso");

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente3Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente3);

            // Act
            var resultado = await recetaService.ObtenerIngredientesFaltantesAsync(productoId, cantidad, _cancellationToken);

            // Assert
            resultado.Succeeded.Should().BeTrue();
            resultado.Value.Should().NotBeEmpty();
            resultado.Value.Should().ContainKey(ingrediente3Id); // Solo el queso debería faltar
            resultado.Value.Should().NotContainKey(ingrediente1Id);
            resultado.Value.Should().NotContainKey(ingrediente2Id);
            
            // Verificar la cantidad faltante de queso
            var cantidadFaltanteQueso = resultado.Value[ingrediente3Id];
            cantidadFaltanteQueso.Should().Be(0.2m); // Falta 0.2kg de queso (se necesita 0.3kg, hay 0.1kg)
            
            // Verificar notificaciones
            notificationManager.HasErrors.Should().BeTrue();
            
            var errores = notificationManager.GetErrors().ToList();
            errores.Should().Contain(e => e.PropertyName == "Producto"); // Error general del producto
            errores.Should().Contain(e => e.PropertyName == $"Ingrediente_{ingrediente3Id}"); // Error específico del queso
            
            // Verificar contenido de los mensajes
            var mensajeProducto = errores.FirstOrDefault(e => e.PropertyName == "Producto")?.Message;
            mensajeProducto.Should().Contain("Hamburguesa Completa");
            mensajeProducto.Should().Contain("3 unidad");
            
            var mensajeQueso = errores.FirstOrDefault(e => e.PropertyName == $"Ingrediente_{ingrediente3Id}")?.Message;
            mensajeQueso.Should().Contain("Queso");
            mensajeQueso.Should().Contain("0.1"); // Stock disponible
            mensajeQueso.Should().Contain("0.3"); // Stock requerido
        }

        #endregion

        #region CalcularCostoRecetaAsync

        [Fact]
        public async Task CalcularCostoRecetaAsync_CuandoProductoNoExiste_DebeLanzarExcepcion()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Producto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken));
        }

        [Fact]
        public async Task CalcularCostoRecetaAsync_SinReceta_DebeRetornarCero()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Receta?)null);

            // Act
            var resultado = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            resultado.Should().Be(0m);
        }

        [Fact]
        public async Task CalcularCostoRecetaAsync_ConRecetaSinIngredientes_DebeRetornarCero()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Act
            var resultado = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            resultado.Should().Be(0m);
        }

        [Fact]
        public async Task CalcularCostoRecetaAsync_IngredienteNoExiste_DebeExcluirDelCalculo()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 5.0m);
            ingrediente2.ActualizarCostoPromedio(20m); // $20 por kilogramo

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync((Ingrediente?)null);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);

            // Act
            var resultado = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            // Solo debe contar el costo del queso: 0.3kg * 20.0 = 6.0
            resultado.Should().Be(6.0m);
        }

        [Fact]
        public async Task CalcularCostoRecetaAsync_ConTodosLosIngredientes_DebeCalcularCostoTotal()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 10.0m);
            ingrediente1.ActualizarCostoPromedio(20m); // $20 por kilogramo
            
            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 5.0m);
            ingrediente2.ActualizarCostoPromedio(5m);  // $5 por pieza

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);

            // Act
            var resultado = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            // Tomate: 0.2kg * 20.0 = 4.0
            // Queso: 0.3kg * 5.0 = 1.5
            // Total: 5.5
            resultado.Should().Be(5.5m);
        }

        [Fact]
        public async Task CalcularCostoRecetaAsync_ConIngredientesOpcionales_DebeIncluirOpcionalesEnCalculo()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(10.99m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            var ingrediente3Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
                
            receta.AgregarIngrediente(
                ingrediente3Id, 
                "Aceitunas", 
                0.05m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo,
                true); // Ingrediente opcional

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 10.0m);
            ingrediente1.ActualizarCostoPromedio(20m); // $20 por kilogramo
            
            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 5.0m);
            ingrediente2.ActualizarCostoPromedio(5m);  // $5 por pieza
            
            var ingrediente3 = CrearIngredienteSimulado(ingrediente3Id, 2.0m);
            ingrediente3.ActualizarCostoPromedio(30m); // $30 por litro

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);
                
            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente3Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente3);

            // Act
            var resultado = await _recetaService.CalcularCostoRecetaAsync(productoId, _cancellationToken);

            // Assert
            // Tomate: 0.2kg * 20.0 = 4.0
            // Queso: 0.3kg * 5.0 = 1.5
            // Aceitunas: 0.05kg * 30.0 = 1.5
            // Total: 7.0
            resultado.Should().Be(7.0m);
        }

        #endregion

        #region CalcularRentabilidadProductoAsync

        [Fact]
        public async Task CalcularRentabilidadProductoAsync_ProductoNoExiste_DebeLanzarExcepcion()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Producto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _recetaService.CalcularRentabilidadProductoAsync(productoId, _cancellationToken));
        }

        [Fact]
        public async Task CalcularRentabilidadProductoAsync_SinReceta_DebeCalcularSoloConPrecioVenta()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(10.0m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync((Receta?)null);

            // Act
            var resultado = await _recetaService.CalcularRentabilidadProductoAsync(productoId, _cancellationToken);

            // Assert
            // Sin receta, el costo es 0, así que la rentabilidad es 100%
            resultado.Rentabilidad.Should().Be(100.0m);
            resultado.MargenGanancia.Should().Be(10.0m);
            resultado.CostoTotal.Should().Be(0.0m);
            resultado.PrecioVenta.Should().Be(10.0m);
        }

        [Fact]
        public async Task CalcularRentabilidadProductoAsync_ConRecetaSinIngredientes_DebeCalcularSoloConPrecioVenta()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(15.0m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            // Act
            var resultado = await _recetaService.CalcularRentabilidadProductoAsync(productoId, _cancellationToken);

            // Assert
            // Sin ingredientes, el costo es 0, así que la rentabilidad es 100%
            resultado.Rentabilidad.Should().Be(100.0m);
            resultado.MargenGanancia.Should().Be(15.0m);
            resultado.CostoTotal.Should().Be(0.0m);
            resultado.PrecioVenta.Should().Be(15.0m);
        }

        [Fact]
        public async Task CalcularRentabilidadProductoAsync_ConReceta_DebeCalcularCorrectamente()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(20.0m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            var ingrediente2Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Tomate", 
                0.2m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);
            
            receta.AgregarIngrediente(
                ingrediente2Id, 
                "Queso Mozzarella", 
                0.3m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 10.0m);
            ingrediente1.ActualizarCostoPromedio(20m); // $20 por kilogramo
            
            var ingrediente2 = CrearIngredienteSimulado(ingrediente2Id, 5.0m);
            ingrediente2.ActualizarCostoPromedio(5m);  // $5 por pieza

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente2Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente2);

            // Act
            var resultado = await _recetaService.CalcularRentabilidadProductoAsync(productoId, _cancellationToken);

            // Assert
            // Costo: Tomate (0.2kg * 20.0 = 4.0) + Queso (0.3kg * 5.0 = 1.5) = 5.5
            // Precio venta: 20.0
            // Margen: 20.0 - 5.5 = 14.5
            // Rentabilidad: (14.5 / 20.0) * 100 = 72.5%
            resultado.CostoTotal.Should().Be(5.5m);
            resultado.PrecioVenta.Should().Be(20.0m);
            resultado.MargenGanancia.Should().Be(14.5m);
            resultado.Rentabilidad.Should().Be(72.5m);
        }

        [Fact]
        public async Task CalcularRentabilidadProductoAsync_ConCostoAlto_DebeCalcularRentabilidadBaja()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(10.0m);

            var producto = Producto.Crear(
                "Pizza Margarita", 
                "Pizza clásica italiana", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Trufa", 
                0.05m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 1.0m);
            ingrediente1.ActualizarCostoPromedio(150.0m); // 150 por kg de trufa (ingrediente caro)

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);

            // Act
            var resultado = await _recetaService.CalcularRentabilidadProductoAsync(productoId, _cancellationToken);

            // Assert
            // Costo: Trufa (0.05kg * 150.0 = 7.5)
            // Precio venta: 10.0
            // Margen: 10.0 - 7.5 = 2.5
            // Rentabilidad: (2.5 / 10.0) * 100 = 25%
            resultado.CostoTotal.Should().Be(7.5m);
            resultado.PrecioVenta.Should().Be(10.0m);
            resultado.MargenGanancia.Should().Be(2.5m);
            resultado.Rentabilidad.Should().Be(25.0m);
        }

        [Fact]
        public async Task CalcularRentabilidadProductoAsync_ConCostoMayorQuePrecio_DebeCalcularRentabilidadNegativa()
        {
            // Arrange
            var productoId = Guid.NewGuid();
            var precio = new PrecioProducto(8.0m);

            var producto = Producto.Crear(
                "Pizza Especial", 
                "Pizza con ingredientes premium", 
                precio, 
                Guid.NewGuid(), 
                "Pizzas");

            var receta = Receta.Crear(productoId, "Instrucciones de preparación", 30);
            
            var ingrediente1Id = Guid.NewGuid();
            
            receta.AgregarIngrediente(
                ingrediente1Id, 
                "Trufa Blanca", 
                0.1m, 
                RestaurantePro.Domain.Inventario.Ingredientes.Enums.UnidadMedida.Kilogramo);

            _productoRepositoryMock.Setup(r => r.ObtenerPorIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(producto);

            _recetaRepositoryMock.Setup(r => r.ObtenerPorProductoIdAsync(productoId, _cancellationToken))
                .ReturnsAsync(receta);

            var ingrediente1 = CrearIngredienteSimulado(ingrediente1Id, 1.0m);
            ingrediente1.ActualizarCostoPromedio(200.0m); // 200 por kg de trufa blanca (muy caro)

            _ingredienteRepositoryMock.Setup(r => r.ObtenerPorIdAsync(ingrediente1Id, false, _cancellationToken))
                .ReturnsAsync(ingrediente1);

            // Act
            var resultado = await _recetaService.CalcularRentabilidadProductoAsync(productoId, _cancellationToken);

            // Assert
            // Costo: Trufa Blanca (0.1kg * 200.0 = 20.0)
            // Precio venta: 8.0
            // Margen: 8.0 - 20.0 = -12.0
            // Rentabilidad: (-12.0 / 8.0) * 100 = -150%
            resultado.CostoTotal.Should().Be(20.0m);
            resultado.PrecioVenta.Should().Be(8.0m);
            resultado.MargenGanancia.Should().Be(-12.0m);
            resultado.Rentabilidad.Should().Be(-150.0m);
        }

        #endregion
    }
} 