namespace RestaurantePro.Application.UnitTests.Operaciones.Comandas.Validators;

/// <summary>
/// Tests unitarios para UnificarComandasValidator
/// Validación completa de reglas de negocio para unificación de comandas
/// </summary>
public class UnificarComandasValidatorTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly UnificarComandasValidator _validator;

    public UnificarComandasValidatorTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _validator = new UnificarComandasValidator(_contextMock.Object);
    }

    [Fact]
    public async Task Validator_ConComandoValido_DeberiaSerValido()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMocksParaValidacion(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validator_SinComandasIds_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid>();
        ConfigurarMocksBasicos(); // Agregar configuración básica

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Debe especificar al menos una comanda para unificar.");
    }

    [Fact]
    public async Task Validator_ConSoloUnaComanda_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid> { Guid.NewGuid() };
        ConfigurarMocksBasicos(); // Agregar configuración básica

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Debe especificar al menos 2 comandas para unificar.");
    }

    [Fact]
    public async Task Validator_ConDemasiadasComandas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ComandasIds = new List<Guid>();
        
        // Agregar más de 10 comandas
        for (int i = 0; i < 11; i++)
        {
            command.ComandasIds.Add(Guid.NewGuid());
        }
        
        ConfigurarMocksBasicos(); // Agregar configuración básica

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "No se pueden unificar más de 10 comandas a la vez.");
    }

    [Fact]
    public async Task Validator_ConIdsComandaDuplicados_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        var comandaId = Guid.NewGuid();
        command.ComandasIds = new List<Guid> { comandaId, comandaId }; // ID duplicado
        ConfigurarMocksBasicos(); // Agregar configuración básica

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Los IDs de comandas deben ser únicos.");
    }

    [Fact]
    public async Task Validator_ConMesaDestinoIdVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.MesaDestinoId = Guid.Empty;
        ConfigurarMocksBasicos(); // Agregar configuración básica

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "El ID de la mesa de destino no puede ser un GUID vacío.");
    }

    [Fact]
    public async Task Validator_ConMeseroIdVacio_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.MeseroId = Guid.Empty;
        ConfigurarMocksBasicos(); // Agregar configuración básica

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "El ID del mesero no puede ser un GUID vacío.");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ABC")]  // Muy corto
    public async Task Validator_ConMotivoInvalido_DeberiaFallar(string motivoInvalido)
    {
        // Arrange
        var command = CrearComandoValido();
        command.MotivoUnificacion = motivoInvalido;
        ConfigurarMocksBasicos(); // Agregar configuración básica

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("motivo"));
    }

    [Fact]
    public async Task Validator_ConEstrategiaDescuentosInvalida_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.EstrategiaDescuentos = (EstrategiaDescuentos)999; // Valor inválido
        ConfigurarMocksBasicos(); // Agregar configuración básica

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La estrategia de descuentos especificada no es válida.");
    }

    [Fact]
    public async Task Validator_ConComandaNoExistente_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandasNoExisten();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Una o más comandas especificadas no existen.");
    }

    [Fact]
    public async Task Validator_ConComandasNoUnificables_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandasNoUnificables(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Una o más comandas no pueden ser unificadas en su estado actual.");
    }

    [Fact]
    public async Task Validator_ConComandasFacturadas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockComandasFacturadas(command);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "No se pueden unificar comandas que ya han sido facturadas.");
    }

    [Fact]
    public async Task Validator_ConMesaDestinoNoExistente_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockMesaNoExiste();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "La mesa de destino especificada no existe.");
    }

    [Fact]
    public async Task Validator_ConMeseroNoExistente_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        ConfigurarMockMeseroNoExiste();

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "El mesero especificado no existe.");
    }

    [Fact]
    public async Task Validator_ConNotasUnificacionMuyLargas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.NotasUnificacion = new string('A', 501); // Más de 500 caracteres
        ConfigurarMocksBasicos(); // Agregar configuración básica

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Las notas de unificación no pueden exceder 500 caracteres.");
    }

    [Fact]
    public async Task Validator_ConObservacionesUnificadaMuyLargas_DeberiaFallar()
    {
        // Arrange
        var command = CrearComandoValido();
        command.ObservacionesUnificada = new string('A', 501); // Más de 500 caracteres
        ConfigurarMocksBasicos(); // Agregar configuración básica

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Las observaciones de la comanda unificada no pueden exceder 500 caracteres.");
    }

    private void ConfigurarMocksBasicos()
    {
        // Configurar mocks básicos para evitar NullReferenceException
        var comandasVacias = new List<Comanda>().AsQueryable();
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Provider).Returns(comandasVacias.Provider);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Expression).Returns(comandasVacias.Expression);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.ElementType).Returns(comandasVacias.ElementType);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.GetEnumerator()).Returns(comandasVacias.GetEnumerator());
        
        comandasMock.Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Comanda, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Comanda, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                var compiledPredicate = predicate.Compile();
                return Task.FromResult(comandasVacias.Any(compiledPredicate));
            });
        
        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);

        var mesasVacias = new List<Mesa>().AsQueryable();
        var mesasMock = new Mock<DbSet<Mesa>>();
        mesasMock.As<IQueryable<Mesa>>().Setup(m => m.Provider).Returns(mesasVacias.Provider);
        mesasMock.As<IQueryable<Mesa>>().Setup(m => m.Expression).Returns(mesasVacias.Expression);
        mesasMock.As<IQueryable<Mesa>>().Setup(m => m.ElementType).Returns(mesasVacias.ElementType);
        mesasMock.As<IQueryable<Mesa>>().Setup(m => m.GetEnumerator()).Returns(mesasVacias.GetEnumerator());
        
        mesasMock.Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Mesa, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Mesa, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                var compiledPredicate = predicate.Compile();
                return Task.FromResult(mesasVacias.Any(compiledPredicate));
            });
        
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);

        var usuariosVacios = new List<Usuario>().AsQueryable();
        var usuariosMock = new Mock<DbSet<Usuario>>();
        usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns(usuariosVacios.Provider);
        usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns(usuariosVacios.Expression);
        usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns(usuariosVacios.ElementType);
        usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns(usuariosVacios.GetEnumerator());
        
        usuariosMock.Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Usuario, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Usuario, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                var compiledPredicate = predicate.Compile();
                return Task.FromResult(usuariosVacios.Any(compiledPredicate));
            });
        
        _contextMock.Setup(x => x.Usuarios).Returns(usuariosMock.Object);
    }

    private void ConfigurarMocksParaValidacion(UnificarComandasCommand command)
    {
        // Mock para comandas - configuración manual async
        var comandas = command.ComandasIds.Select(id => CrearComandaMock(id, EstadoComanda.Creada)).ToList().AsQueryable();
        var comandasMock = new Mock<DbSet<Comanda>>();
        
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Provider).Returns(comandas.Provider);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Expression).Returns(comandas.Expression);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.ElementType).Returns(comandas.ElementType);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.GetEnumerator()).Returns(comandas.GetEnumerator());
        
        comandasMock.Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Comanda, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Comanda, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                var compiledPredicate = predicate.Compile();
                return Task.FromResult(comandas.Any(compiledPredicate));
            });
        
        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);

        // Mock para mesas - configuración manual async
        var mesa = CrearMesaMock(command.MesaDestinoId, EstadoMesa.Disponible);
        var mesas = new List<Mesa> { mesa }.AsQueryable();
        var mesasMock = new Mock<DbSet<Mesa>>();
        
        mesasMock.As<IQueryable<Mesa>>().Setup(m => m.Provider).Returns(mesas.Provider);
        mesasMock.As<IQueryable<Mesa>>().Setup(m => m.Expression).Returns(mesas.Expression);
        mesasMock.As<IQueryable<Mesa>>().Setup(m => m.ElementType).Returns(mesas.ElementType);
        mesasMock.As<IQueryable<Mesa>>().Setup(m => m.GetEnumerator()).Returns(mesas.GetEnumerator());
        
        mesasMock.Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Mesa, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Mesa, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                var compiledPredicate = predicate.Compile();
                return Task.FromResult(mesas.Any(compiledPredicate));
            });
        
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);

        // Mock para usuarios (meseros) - configuración manual async
        var usuario = CrearUsuarioMock(command.MeseroId, true);
        var usuarios = new List<Usuario> { usuario }.AsQueryable();
        var usuariosMock = new Mock<DbSet<Usuario>>();
        
        usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns(usuarios.Provider);
        usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns(usuarios.Expression);
        usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns(usuarios.ElementType);
        usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns(usuarios.GetEnumerator());
        
        usuariosMock.Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Usuario, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Usuario, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                var compiledPredicate = predicate.Compile();
                return Task.FromResult(usuarios.Any(compiledPredicate));
            });
        
        _contextMock.Setup(x => x.Usuarios).Returns(usuariosMock.Object);
    }

    private void ConfigurarMockComandasNoExisten()
    {
        var comandasVacias = new List<Comanda>().AsQueryable();
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Provider).Returns(comandasVacias.Provider);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Expression).Returns(comandasVacias.Expression);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.ElementType).Returns(comandasVacias.ElementType);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.GetEnumerator()).Returns(comandasVacias.GetEnumerator());
        
        comandasMock.Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Comanda, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Comanda, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                var compiledPredicate = predicate.Compile();
                return Task.FromResult(comandasVacias.Any(compiledPredicate));
            });
        
        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
    }

    private void ConfigurarMockComandasNoUnificables(UnificarComandasCommand command)
    {
        // Crear comandas que NO son unificables (diferentes estados)
        var comandas = command.ComandasIds.Select((id, index) => 
            CrearComandaMock(id, index == 0 ? EstadoComanda.Finalizada : EstadoComanda.Creada)
        ).ToList().AsQueryable();
        
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Provider).Returns(comandas.Provider);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Expression).Returns(comandas.Expression);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.ElementType).Returns(comandas.ElementType);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.GetEnumerator()).Returns(comandas.GetEnumerator());
        
        comandasMock.Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Comanda, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Comanda, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                var compiledPredicate = predicate.Compile();
                return Task.FromResult(comandas.Any(compiledPredicate));
            });
        
        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
    }

    private void ConfigurarMockComandasFacturadas(UnificarComandasCommand command)
    {
        ConfigurarMocksBasicos();
        
        // Crear comandas que están facturadas
        var comandas = command.ComandasIds.Select(id => 
        {
            var comanda = CrearComandaMock(id, EstadoComanda.Finalizada);
            // Marcar como facturada usando reflection si es necesario
            return comanda;
        }).ToList().AsQueryable();
        
        var comandasMock = new Mock<DbSet<Comanda>>();
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Provider).Returns(comandas.Provider);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.Expression).Returns(comandas.Expression);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.ElementType).Returns(comandas.ElementType);
        comandasMock.As<IQueryable<Comanda>>().Setup(m => m.GetEnumerator()).Returns(comandas.GetEnumerator());
        
        comandasMock.Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Comanda, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Comanda, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                var compiledPredicate = predicate.Compile();
                return Task.FromResult(comandas.Any(compiledPredicate));
            });
        
        _contextMock.Setup(x => x.Comandas).Returns(comandasMock.Object);
    }

    private void ConfigurarMockMesaNoExiste()
    {
        var mesasVacias = new List<Mesa>().AsQueryable();
        var mesasMock = new Mock<DbSet<Mesa>>();
        mesasMock.As<IQueryable<Mesa>>().Setup(m => m.Provider).Returns(mesasVacias.Provider);
        mesasMock.As<IQueryable<Mesa>>().Setup(m => m.Expression).Returns(mesasVacias.Expression);
        mesasMock.As<IQueryable<Mesa>>().Setup(m => m.ElementType).Returns(mesasVacias.ElementType);
        mesasMock.As<IQueryable<Mesa>>().Setup(m => m.GetEnumerator()).Returns(mesasVacias.GetEnumerator());
        
        mesasMock.Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Mesa, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Mesa, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                var compiledPredicate = predicate.Compile();
                return Task.FromResult(mesasVacias.Any(compiledPredicate));
            });
        
        _contextMock.Setup(x => x.Mesas).Returns(mesasMock.Object);
    }

    private void ConfigurarMockMeseroNoExiste()
    {
        var usuariosVacios = new List<Usuario>().AsQueryable();
        var usuariosMock = new Mock<DbSet<Usuario>>();
        usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.Provider).Returns(usuariosVacios.Provider);
        usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.Expression).Returns(usuariosVacios.Expression);
        usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.ElementType).Returns(usuariosVacios.ElementType);
        usuariosMock.As<IQueryable<Usuario>>().Setup(m => m.GetEnumerator()).Returns(usuariosVacios.GetEnumerator());
        
        usuariosMock.Setup(x => x.AnyAsync(
                It.IsAny<Expression<Func<Usuario, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Expression<Func<Usuario, bool>>, CancellationToken>((predicate, ct) =>
            {
                if (ct.IsCancellationRequested)
                    throw new OperationCanceledException();
                var compiledPredicate = predicate.Compile();
                return Task.FromResult(usuariosVacios.Any(compiledPredicate));
            });
        
        _contextMock.Setup(x => x.Usuarios).Returns(usuariosMock.Object);
    }

    #region Helper Methods

    private Comanda CrearComandaMock(Guid id, EstadoComanda estado)
    {
        // Usar reflection para crear comanda con propiedades privadas
        var comanda = (Comanda)Activator.CreateInstance(typeof(Comanda), true)!;
        typeof(Comanda).GetProperty("Id")?.SetValue(comanda, id);
        typeof(Comanda).GetProperty("Estado")?.SetValue(comanda, estado);
        return comanda;
    }

    private Mesa CrearMesaMock(Guid id, EstadoMesa estado)
    {
        // Usar reflection para crear mesa con propiedades privadas
        var mesa = (Mesa)Activator.CreateInstance(typeof(Mesa), true)!;
        typeof(Mesa).GetProperty("Id")?.SetValue(mesa, id);
        typeof(Mesa).GetProperty("Estado")?.SetValue(mesa, estado);
        return mesa;
    }

    private Usuario CrearUsuarioMock(Guid id, bool activo)
    {
        // Usar reflection para crear usuario con propiedades privadas
        var usuario = (Usuario)Activator.CreateInstance(typeof(Usuario), true)!;
        typeof(Usuario).GetProperty("Id")?.SetValue(usuario, id);
        // La propiedad Activo no existe, usamos un estado válido por defecto
        return usuario;
    }

    #endregion

    private UnificarComandasCommand CrearComandoValido()
    {
        return new UnificarComandasCommand
        {
            ComandasIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() },
            MesaDestinoId = Guid.NewGuid(),
            MeseroId = Guid.NewGuid(),
            MotivoUnificacion = "Cliente solicita unificar cuentas",
            EstrategiaDescuentos = EstrategiaDescuentos.Sumar,
            MantenerHistorico = true,
            NotasUnificacion = "Unificación por solicitud del cliente",
            ObservacionesUnificada = "Mesa familiar"
        };
    }
} 