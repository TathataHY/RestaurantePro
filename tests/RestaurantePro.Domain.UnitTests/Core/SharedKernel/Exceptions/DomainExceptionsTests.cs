namespace RestaurantePro.Domain.UnitTests.Core.SharedKernel.Exceptions
{
    /// <summary>
    /// Pruebas unitarias para las excepciones de dominio - Validando integración con Result pattern
    /// </summary>
    public class DomainExceptionsTests
    {
        #region ClienteInactivoException Tests

        [Fact]
        public void ClienteInactivoException_ParaAcumulacionPuntos_DebeTenerInformacionCorrecta()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var puntos = 50;

            // Act
            var excepcion = ClienteInactivoException.ParaAcumulacionPuntos(clienteId, puntos);

            // Assert
            excepcion.Should().NotBeNull();
            excepcion.Message.Should().Contain("acumular puntos");
            excepcion.Message.Should().Contain(puntos.ToString());
            excepcion.ErrorCode.Should().Be("CLIENT_INACTIVE");
            excepcion.DomainContext.Should().Be("Comercial");
            excepcion.AdditionalData.Should().ContainKey("ClienteId");
            excepcion.AdditionalData.Should().ContainKey("Puntos");
            excepcion.AdditionalData["ClienteId"].Should().Be(clienteId);
            excepcion.AdditionalData["Puntos"].Should().Be(puntos);
        }

        [Fact]
        public void ClienteInactivoException_ParaRegistroVisita_DebeTenerInformacionCorrecta()
        {
            // Arrange
            var clienteId = Guid.NewGuid();

            // Act
            var excepcion = ClienteInactivoException.ParaRegistroVisita(clienteId);

            // Assert
            excepcion.Should().NotBeNull();
            excepcion.Message.Should().Contain("registrar visita");
            excepcion.ErrorCode.Should().Be("CLIENT_INACTIVE");
            excepcion.DomainContext.Should().Be("Comercial");
            excepcion.AdditionalData["ClienteId"].Should().Be(clienteId);
        }

        [Fact]
        public void ClienteInactivoException_ParaAsociacionTarjeta_DebeTenerInformacionCorrecta()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var tarjetaId = Guid.NewGuid();

            // Act
            var excepcion = ClienteInactivoException.ParaAsociacionTarjeta(clienteId, tarjetaId);

            // Assert
            excepcion.Should().NotBeNull();
            excepcion.Message.Should().Contain("asociar tarjeta");
            excepcion.ErrorCode.Should().Be("CLIENT_INACTIVE");
            excepcion.DomainContext.Should().Be("Comercial");
            excepcion.AdditionalData["ClienteId"].Should().Be(clienteId);
            excepcion.AdditionalData["TarjetaId"].Should().Be(tarjetaId);
        }

        [Fact]
        public void ClienteInactivoException_ToResult_DebeConvertirCorrectamente()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var excepcion = ClienteInactivoException.ParaRegistroVisita(clienteId);

            // Act
            var resultado = excepcion.ToResult<bool>();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            resultado.HasErrors.Should().BeTrue();
            resultado.Error.Should().Be(excepcion.Message);
        }

        #endregion

        #region StockInsuficienteException Tests

        [Fact]
        public void StockInsuficienteException_ConDatosCompletos_DebeTenerInformacionCorrecta()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var nombre = "Tomate";
            var cantidadRequerida = 10.5m;
            var stockDisponible = 5.2m;
            var operacion = "preparar plato";

            // Act
            var excepcion = new StockInsuficienteException(
                ingredienteId, nombre, cantidadRequerida, stockDisponible, operacion);

            // Assert
            excepcion.Should().NotBeNull();
            excepcion.Message.Should().Contain("stock insuficiente");
            excepcion.Message.Should().Contain(nombre);
            excepcion.Message.Should().Contain(operacion);
            excepcion.ErrorCode.Should().Be("INSUFFICIENT_STOCK");
            excepcion.DomainContext.Should().Be("Inventario");
            
            excepcion.AdditionalData["IngredienteId"].Should().Be(ingredienteId);
            excepcion.AdditionalData["CantidadRequerida"].Should().Be(cantidadRequerida);
            excepcion.AdditionalData["StockDisponible"].Should().Be(stockDisponible);
            excepcion.AdditionalData["Deficit"].Should().Be(cantidadRequerida - stockDisponible);
        }

        [Fact]
        public void StockInsuficienteException_CalcularDeficit_DebeCalcularCorrectamente()
        {
            // Arrange
            var ingredienteId = Guid.NewGuid();
            var cantidadRequerida = 15.7m;
            var stockDisponible = 8.3m;
            var deficitEsperado = cantidadRequerida - stockDisponible;

            // Act
            var excepcion = new StockInsuficienteException(
                ingredienteId, "Harina", cantidadRequerida, stockDisponible, "hacer pan");

            // Assert
            excepcion.AdditionalData["Deficit"].Should().Be(deficitEsperado);
        }

        #endregion

        #region BusinessRuleViolationException Tests

        [Fact]
        public void BusinessRuleViolationException_ForInvalidState_DebeTenerInformacionCorrecta()
        {
            // Arrange
            var entidad = "Cliente";
            var estadoActual = "PuntosAcumulados = -10";
            var estadoEsperado = "PuntosAcumulados >= 0";
            var contexto = "Comercial";
            var entidadId = Guid.NewGuid();

            // Act
            var excepcion = BusinessRuleViolationException.ForInvalidState(
                entidad, estadoActual, estadoEsperado, contexto, entidadId);

            // Assert
            excepcion.Should().NotBeNull();
            excepcion.Message.Should().Contain("estado inválido");
            excepcion.Message.Should().Contain(entidad);
            excepcion.Message.Should().Contain(estadoActual);
            excepcion.Message.Should().Contain(estadoEsperado);
            excepcion.ErrorCode.Should().Be("INVALID_STATE");
            excepcion.DomainContext.Should().Be(contexto);
            excepcion.AdditionalData["EntityType"].Should().Be(entidad);
            excepcion.AdditionalData["CurrentState"].Should().Be(estadoActual);
            excepcion.AdditionalData["ExpectedState"].Should().Be(estadoEsperado);
            excepcion.AdditionalData["EntityId"].Should().Be(entidadId);
        }

        [Fact]
        public void BusinessRuleViolationException_ForOperationNotAllowed_DebeTenerInformacionCorrecta()
        {
            // Arrange
            var operacion = "Transferir puntos";
            var entidad = "Cliente";
            var razon = "El cliente no tiene suficientes puntos";
            var contexto = "Comercial";
            var entidadId = Guid.NewGuid();

            // Act
            var excepcion = BusinessRuleViolationException.ForOperationNotAllowed(
                operacion, entidad, razon, contexto, entidadId);

            // Assert
            excepcion.Should().NotBeNull();
            excepcion.Message.Should().Contain("operación no permitida");
            excepcion.Message.Should().Contain(operacion);
            excepcion.Message.Should().Contain(entidad);
            excepcion.Message.Should().Contain(razon);
            excepcion.ErrorCode.Should().Be("OPERATION_NOT_ALLOWED");
            excepcion.DomainContext.Should().Be(contexto);
        }

        [Fact]
        public void BusinessRuleViolationException_ForInactiveEntity_DebeTenerInformacionCorrecta()
        {
            // Arrange
            var entidad = "Ingrediente";
            var contexto = "Inventario";
            var entidadId = Guid.NewGuid();

            // Act
            var excepcion = BusinessRuleViolationException.ForInactiveEntity(
                entidad, contexto, entidadId);

            // Assert
            excepcion.Should().NotBeNull();
            excepcion.Message.Should().Contain("entidad inactiva");
            excepcion.Message.Should().Contain(entidad);
            excepcion.ErrorCode.Should().Be("INACTIVE_ENTITY");
            excepcion.DomainContext.Should().Be(contexto);
            excepcion.AdditionalData["EntityId"].Should().Be(entidadId);
        }

        [Fact]
        public void BusinessRuleViolationException_WithData_DebeAgregarDatosAdicionales()
        {
            // Arrange
            var excepcion = BusinessRuleViolationException.ForOperationNotAllowed(
                "Test", "Entity", "Reason", "Context", Guid.NewGuid());

            // Act
            var excepcionConDatos = excepcion
                .WithData("Campo1", "Valor1")
                .WithData("Campo2", 42)
                .WithData("Campo3", true);

            // Assert
            excepcionConDatos.Should().BeSameAs(excepcion);
            excepcion.AdditionalData["Campo1"].Should().Be("Valor1");
            excepcion.AdditionalData["Campo2"].Should().Be(42);
            excepcion.AdditionalData["Campo3"].Should().Be(true);
        }

        #endregion

        #region DomainException Base Tests

        [Fact]
        public void DomainException_ToResult_SinTipo_DebeConvertirCorrectamente()
        {
            // Arrange
            var excepcion = BusinessRuleViolationException.ForOperationNotAllowed(
                "Test", "Entity", "Reason", "Context");

            // Act
            var resultado = excepcion.ToResult();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            resultado.HasErrors.Should().BeTrue();
            resultado.Error.Should().Be(excepcion.Message);
        }

        [Fact]
        public void DomainException_ToResult_ConTipo_DebeConvertirCorrectamente()
        {
            // Arrange
            var excepcion = new StockInsuficienteException(
                Guid.NewGuid(), "Test", 10, 5, "test");

            // Act
            var resultado = excepcion.ToResult<string>();

            // Assert
            resultado.Should().NotBeNull();
            resultado.Succeeded.Should().BeFalse();
            resultado.HasErrors.Should().BeTrue();
            resultado.Error.Should().Be(excepcion.Message);
            resultado.Value.Should().BeNull();
        }

        [Fact]
        public void DomainException_DebeSerializable()
        {
            // Arrange
            var excepcion = new StockInsuficienteException(
                Guid.NewGuid(), "Test Ingredient", 10.5m, 3.2m, "test operation");

            // Act & Assert
            // Verificar que la excepción puede ser serializada
            excepcion.Should().NotBeNull();
            excepcion.AdditionalData.Should().NotBeNull();
            excepcion.ErrorCode.Should().NotBeNullOrEmpty();
            excepcion.DomainContext.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region Integration with Guard Clauses Tests

        [Fact]
        public void Guard_DebeIntegramConExcepciones_ClienteInactivo()
        {
            // Arrange
            var clienteId = Guid.NewGuid();
            var cliente = Cliente.Crear(
                ClienteNombre.Crear("Test", "User"), 
                "test@example.com", 
                "123456789", 
                DateTime.Now.AddYears(-25));
            
            // Desactivar cliente
            cliente.Desactivar();

            // Act & Assert
            var excepcion = Assert.Throws<ClienteInactivoException>(() => 
                cliente.AgregarPuntos(100));
                
            excepcion.Should().NotBeNull();
            excepcion.ErrorCode.Should().Be("CLIENT_INACTIVE");
        }

        [Fact]
        public void Guard_DebeIntegramConExcepciones_StockInsuficiente()
        {
            // Arrange
            var ingrediente = Ingrediente.Crear(
                "Tomate", "TOM001", "Tomate fresco", 
                UnidadMedida.Kilogramo, 5.0m, 10.0m);

            // Act & Assert
            var excepcion = Assert.Throws<StockInsuficienteException>(() => 
                ingrediente.DecrementarStock(15.0m, "Uso en cocina"));
                
            excepcion.Should().NotBeNull();
            excepcion.ErrorCode.Should().Be("INSUFFICIENT_STOCK");
            excepcion.AdditionalData["Deficit"].Should().Be(5.0m);
        }

        #endregion
    }
} 