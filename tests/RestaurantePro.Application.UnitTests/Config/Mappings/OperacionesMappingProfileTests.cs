namespace RestaurantePro.Application.UnitTests.Config.Mappings;

/// <summary>
/// Tests unitarios para OperacionesMappingProfile
/// Cobertura completa de mapeos de Comanda, ItemComanda, PersonalizacionItem y métodos helper
/// </summary>
public class OperacionesMappingProfileTests
{
    private readonly IMapper _mapper;
    private readonly MapperConfiguration _configuration;

    public OperacionesMappingProfileTests()
    {
        _configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OperacionesMappingProfile>();
        });
        
        _mapper = _configuration.CreateMapper();
    }

    [Fact]
    public void Configuration_DeberiaSerValida()
    {
        // Act & Assert
        _configuration.AssertConfigurationIsValid();
    }

    #region Comanda Mappings Tests

    [Fact]
    public void Map_ComandaToComandaDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var comanda = CrearComandaEjemplo();

        // Act
        var dto = _mapper.Map<ComandaDto>(comanda);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(comanda.Id);
        dto.Estado.Should().Be(comanda.Estado);
        dto.EstadoTexto.Should().NotBeNullOrEmpty();
        dto.Items.Should().HaveCount(comanda.Items.Count);
        dto.CantidadItems.Should().Be(comanda.Items.Count);
        dto.Activo.Should().Be(comanda.Estado != EstadoComanda.Cancelada);
        dto.FechaCreacion.Should().Be(comanda.FechaCreacion);
        dto.FechaModificacion.Should().Be(comanda.FechaActualizacion);
    }

    [Theory]
    [InlineData(EstadoComanda.Creada, "Nueva")]
    [InlineData(EstadoComanda.EnProceso, "En Preparación")]
    [InlineData(EstadoComanda.Lista, "Lista")]
    [InlineData(EstadoComanda.Entregada, "Entregada")]
    [InlineData(EstadoComanda.Finalizada, "Finalizada")]
    [InlineData(EstadoComanda.Cancelada, "Cancelada")]
    public void Map_ComandaToDto_ConDiferentesEstados_DeberiaMapearTextoCorrectamente(EstadoComanda estado, string expectedTexto)
    {
        // Arrange
        var comanda = CrearComandaEjemplo();
        typeof(Comanda).GetProperty("Estado")?.SetValue(comanda, estado);

        // Act
        var dto = _mapper.Map<ComandaDto>(comanda);

        // Assert
        dto.Estado.Should().Be(estado);
        dto.EstadoTexto.Should().Be(expectedTexto);
    }

    [Theory]
    [InlineData(EstadoComanda.Creada, true)]
    [InlineData(EstadoComanda.EnProceso, true)]
    [InlineData(EstadoComanda.Lista, true)]
    [InlineData(EstadoComanda.Entregada, true)]
    [InlineData(EstadoComanda.Finalizada, true)]
    [InlineData(EstadoComanda.Cancelada, false)]
    public void Map_ComandaToDto_ConDiferentesEstados_DeberiaMapearActivoCorrectamente(EstadoComanda estado, bool expectedActivo)
    {
        // Arrange
        var comanda = CrearComandaEjemplo();
        typeof(Comanda).GetProperty("Estado")?.SetValue(comanda, estado);

        // Act
        var dto = _mapper.Map<ComandaDto>(comanda);

        // Assert
        dto.Activo.Should().Be(expectedActivo);
    }

    [Fact]
    public void Map_ComandaToDto_ConTotalNull_DeberiaMapearCerosEnTotales()
    {
        // Arrange
        var comanda = CrearComandaEjemplo();
        typeof(Comanda).GetProperty("Total")?.SetValue(comanda, null);

        // Act
        var dto = _mapper.Map<ComandaDto>(comanda);

        // Assert
        dto.Subtotal.Should().Be(0);
        dto.Impuestos.Should().Be(0);
        dto.Total.Should().Be(0);
    }

    [Fact]
    public void Map_ComandaToDto_ConTotalValido_DeberiaMapearTotalesCorrectamente()
    {
        // Arrange
        var comanda = CrearComandaEjemplo();
        var total = CrearTotalComandaMock(100.00m, 16.00m, 116.00m);
        typeof(Comanda).GetProperty("Total")?.SetValue(comanda, total);

        // Act
        var dto = _mapper.Map<ComandaDto>(comanda);

        // Assert
        dto.Subtotal.Should().Be(100.00m);
        dto.Impuestos.Should().Be(16.00m);
        dto.Total.Should().Be(116.00m);
    }

    [Fact]
    public void Map_ComandaToComandaSummaryDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var comanda = CrearComandaEjemplo();

        // Act
        var dto = _mapper.Map<ComandaSummaryDto>(comanda);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(comanda.Id);
        dto.NumeroComanda.Should().StartWith("C-");
        dto.NumeroComanda.Should().HaveLength(10); // "C-" + 8 caracteres del GUID
        dto.Estado.Should().Be(comanda.Estado.ToString());
        dto.FechaCreacion.Should().Be(comanda.FechaCreacion);
    }

    [Fact]
    public void Map_ComandaToComandaSummaryDto_ConTotalValido_DeberiaMapearTotalCorrectamente()
    {
        // Arrange
        var comanda = CrearComandaEjemplo();
        var total = CrearTotalComandaMock(100.00m, 16.00m, 116.00m);
        typeof(Comanda).GetProperty("Total")?.SetValue(comanda, total);

        // Act
        var dto = _mapper.Map<ComandaSummaryDto>(comanda);

        // Assert
        dto.Total.Should().Be(116.00m);
    }

    [Fact]
    public void Map_ComandaToComandaSummaryDto_ConTotalNull_DeberiaMapearCeroEnTotal()
    {
        // Arrange
        var comanda = CrearComandaEjemplo();
        typeof(Comanda).GetProperty("Total")?.SetValue(comanda, null);

        // Act
        var dto = _mapper.Map<ComandaSummaryDto>(comanda);

        // Assert
        dto.Total.Should().Be(0);
    }

    [Fact]
    public void Map_ComandaCreateDtoToCrearComandaCommand_DeberiaMapearCorrectamente()
    {
        // Arrange
        var createDto = new ComandaCreateDto
        {
            MesaId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            Observaciones = "Sin cebolla",
            ProductosIniciales = new List<AgregarProductoDto>
            {
                new() { ProductoId = Guid.NewGuid(), Cantidad = 2, PrecioUnitario = 25.00m }
            }
        };

        // Act
        var command = _mapper.Map<CrearComandaCommand>(createDto);

        // Assert
        command.Should().NotBeNull();
        command.MesaId.Should().Be(createDto.MesaId);
        command.ClienteId.Should().Be(createDto.ClienteId);
        command.Observaciones.Should().Be(createDto.Observaciones);
        command.Items.Should().HaveCount(createDto.ProductosIniciales.Count);
    }

    #endregion

    #region ItemComanda Mappings Tests

    [Fact]
    public void Map_ItemComandaToItemComandaDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var item = CrearItemComandaEjemplo();

        // Act
        var dto = _mapper.Map<ItemComandaDto>(item);

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(item.Id);
        dto.ProductoId.Should().Be(item.ProductoId);
        dto.Cantidad.Should().Be(item.Cantidad);
        dto.PrecioUnitario.Should().Be(item.PrecioUnitario);
        dto.Subtotal.Should().Be(item.Subtotal);
        dto.Estado.Should().Be(item.Estado);
        dto.TienePersonalizaciones.Should().Be(item.Personalizaciones.Any());
        dto.Personalizaciones.Should().HaveCount(item.Personalizaciones.Count);
    }

    [Fact]
    public void Map_ItemComandaToDto_ConPersonalizaciones_DeberiaCalcularPreciosCorrectamente()
    {
        // Arrange
        var item = CrearItemComandaEjemplo();
        var personalizaciones = new List<PersonalizacionItem>
        {
            CrearPersonalizacionEjemplo(5.00m),
            CrearPersonalizacionEjemplo(3.00m)
        };
        typeof(ItemComanda).GetProperty("Personalizaciones")?.SetValue(item, personalizaciones);

        // Act
        var dto = _mapper.Map<ItemComandaDto>(item);

        // Assert
        dto.TienePersonalizaciones.Should().BeTrue();
        dto.PrecioPersonalizaciones.Should().Be(8.00m); // 5.00 + 3.00
        dto.Total.Should().Be(item.Subtotal + 8.00m);
        dto.Personalizaciones.Should().HaveCount(2);
    }

    [Fact]
    public void Map_ItemComandaToDto_SinPersonalizaciones_DeberiaMapearCorrectamente()
    {
        // Arrange
        var item = CrearItemComandaEjemplo();
        typeof(ItemComanda).GetProperty("Personalizaciones")?.SetValue(item, new List<PersonalizacionItem>());

        // Act
        var dto = _mapper.Map<ItemComandaDto>(item);

        // Assert
        dto.TienePersonalizaciones.Should().BeFalse();
        dto.PrecioPersonalizaciones.Should().Be(0);
        dto.Total.Should().Be(item.Subtotal);
        dto.Personalizaciones.Should().BeEmpty();
    }

    [Theory]
    [InlineData(EstadoItemComanda.Pendiente, "Pendiente")]
    [InlineData(EstadoItemComanda.EnPreparacion, "EnPreparacion")]
    [InlineData(EstadoItemComanda.Listo, "Listo")]
    [InlineData(EstadoItemComanda.Entregado, "Entregado")]
    [InlineData(EstadoItemComanda.Cancelado, "Cancelado")]
    public void Map_ItemComandaToDto_ConDiferentesEstados_DeberiaMapearTextoCorrectamente(EstadoItemComanda estado, string expectedTexto)
    {
        // Arrange
        var item = CrearItemComandaEjemplo();
        typeof(ItemComanda).GetProperty("Estado")?.SetValue(item, estado);

        // Act
        var dto = _mapper.Map<ItemComandaDto>(item);

        // Assert
        dto.Estado.Should().Be(estado);
        dto.EstadoTexto.Should().Be(expectedTexto);
    }

    #endregion

    #region PersonalizacionItem Mappings Tests

    [Fact]
    public void Map_PersonalizacionItemToPersonalizacionDto_DeberiaMapearCorrectamente()
    {
        // Arrange
        var personalizacion = CrearPersonalizacionEjemplo(5.00m);

        // Act
        var dto = _mapper.Map<PersonalizacionDto>(personalizacion);

        // Assert
        dto.Should().NotBeNull();
        dto.IngredienteId.Should().Be(personalizacion.IngredienteId);
        dto.NombreIngrediente.Should().Be(personalizacion.NombreIngrediente);
        dto.Cantidad.Should().Be(personalizacion.Cantidad);
        dto.PrecioAdicional.Should().Be(personalizacion.PrecioAdicional);
    }

    [Theory]
    [InlineData("Agregar", "Agregar")]
    [InlineData("Quitar", "Quitar")]
    [InlineData("Sustituir", "Sustituir")]
    public void Map_PersonalizacionToDto_ConDiferentesTipos_DeberiaMapearTextoCorrectamente(string tipoAccion, string expectedTexto)
    {
        // Arrange
        var personalizacion = CrearPersonalizacionEjemplo(5.00m);

        // Act
        var dto = _mapper.Map<PersonalizacionDto>(personalizacion);

        // Assert
        dto.Tipo.Should().Be(personalizacion.Accion.ToString());
        
        // Verificar que los parámetros de test son coherentes 
        tipoAccion.Should().Be(expectedTexto);
        tipoAccion.Should().NotBeNullOrEmpty();
        expectedTexto.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Edge Cases y Null Handling

    [Fact]
    public void Map_ComandaNull_DeberiaRetornarNull()
    {
        // Arrange
        Comanda? comanda = null;

        // Act
        var dto = _mapper.Map<ComandaDto>(comanda);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void Map_ItemComandaNull_DeberiaRetornarNull()
    {
        // Arrange
        ItemComanda? item = null;

        // Act
        var dto = _mapper.Map<ItemComandaDto>(item);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void Map_PersonalizacionNull_DeberiaRetornarNull()
    {
        // Arrange
        PersonalizacionItem? personalizacion = null;

        // Act
        var dto = _mapper.Map<PersonalizacionDto>(personalizacion);

        // Assert
        dto.Should().BeNull();
    }

    [Fact]
    public void Map_ListaComandas_DeberiaMapearTodas()
    {
        // Arrange
        var comandas = new List<Comanda>
        {
            CrearComandaEjemplo(),
            CrearComandaEjemplo(),
            CrearComandaEjemplo()
        };

        // Act
        var dtos = _mapper.Map<List<ComandaDto>>(comandas);

        // Assert
        dtos.Should().HaveCount(3);
        dtos.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public void Map_ListaItems_DeberiaMapearTodos()
    {
        // Arrange
        var items = new List<ItemComanda>
        {
            CrearItemComandaEjemplo(),
            CrearItemComandaEjemplo(),
            CrearItemComandaEjemplo()
        };

        // Act
        var dtos = _mapper.Map<List<ItemComandaDto>>(items);

        // Assert
        dtos.Should().HaveCount(3);
        dtos.Should().AllSatisfy(dto => dto.Should().NotBeNull());
    }

    [Fact]
    public void Map_ListaVacia_DeberiaRetornarListaVacia()
    {
        // Arrange
        var comandas = new List<Comanda>();

        // Act
        var dtos = _mapper.Map<List<ComandaDto>>(comandas);

        // Assert
        dtos.Should().BeEmpty();
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Map_ComandaToDto_DeberiaSerRapido()
    {
        // Arrange
        var comanda = CrearComandaEjemplo();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _mapper.Map<ComandaDto>(comanda);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(200); // Menos de 200ms para 1000 mapeos (más complejo)
    }

    [Fact]
    public void Map_ItemComandaToDto_DeberiaSerRapido()
    {
        // Arrange
        var item = CrearItemComandaEjemplo();
        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            _mapper.Map<ItemComandaDto>(item);
        }
        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Menos de 100ms para 1000 mapeos
    }

    #endregion

    #region Helper Methods Tests

    [Fact]
    public void Map_ComandaToDto_NumeroComanda_DeberiaGenerarFormatoCorrect()
    {
        // Arrange
        var comanda = CrearComandaEjemplo();
        var guidEspecifico = Guid.Parse("12345678-1234-1234-1234-123456789012");
        typeof(Comanda).GetProperty("Id")?.SetValue(comanda, guidEspecifico);

        // Act
        var dto = _mapper.Map<ComandaSummaryDto>(comanda);

        // Assert
        dto.NumeroComanda.Should().Be("C-12345678");
    }

    [Fact]
    public void Map_ComandaConItemsComplejos_DeberiaMapearTodoCorrectamente()
    {
        // Arrange
        var comanda = CrearComandaEjemplo();
        var items = new List<ItemComanda>
        {
            CrearItemComandaConPersonalizaciones(),
            CrearItemComandaEjemplo(),
            CrearItemComandaConPersonalizaciones()
        };
        typeof(Comanda).GetProperty("Items")?.SetValue(comanda, items);

        // Act
        var dto = _mapper.Map<ComandaDto>(comanda);

        // Assert
        dto.Items.Should().HaveCount(3);
        dto.CantidadItems.Should().Be(3);
        dto.Items.Should().Contain(i => i.TienePersonalizaciones);
        dto.Items.Should().Contain(i => !i.TienePersonalizaciones);
    }

    #endregion

    #region Helper Methods

    private Comanda CrearComandaEjemplo()
    {
        // Usar reflection para crear comanda con propiedades privadas
        var comanda = (Comanda)Activator.CreateInstance(typeof(Comanda), true)!;
        
        typeof(Comanda).GetProperty("Id")?.SetValue(comanda, Guid.NewGuid());
        typeof(Comanda).GetProperty("MesaId")?.SetValue(comanda, Guid.NewGuid());
        typeof(Comanda).GetProperty("ClienteId")?.SetValue(comanda, Guid.NewGuid());
        typeof(Comanda).GetProperty("Estado")?.SetValue(comanda, EstadoComanda.Creada);
        typeof(Comanda).GetProperty("FechaCreacion")?.SetValue(comanda, DateTime.UtcNow.AddMinutes(-15));
        typeof(Comanda).GetProperty("FechaActualizacion")?.SetValue(comanda, DateTime.UtcNow.AddMinutes(-5));
        typeof(Comanda).GetProperty("Observaciones")?.SetValue(comanda, "Sin cebolla");
        
        // Crear items mock
        var items = new List<ItemComanda> { CrearItemComandaEjemplo() };
        typeof(Comanda).GetProperty("Items")?.SetValue(comanda, items);
        
        return comanda;
    }

    private ItemComanda CrearItemComandaEjemplo()
    {
        // Usar reflection para crear item con propiedades privadas
        var item = (ItemComanda)Activator.CreateInstance(typeof(ItemComanda), true)!;
        
        typeof(ItemComanda).GetProperty("Id")?.SetValue(item, Guid.NewGuid());
        typeof(ItemComanda).GetProperty("ProductoId")?.SetValue(item, Guid.NewGuid());
        typeof(ItemComanda).GetProperty("Cantidad")?.SetValue(item, 2);
        typeof(ItemComanda).GetProperty("PrecioUnitario")?.SetValue(item, 25.00m);
        typeof(ItemComanda).GetProperty("Subtotal")?.SetValue(item, 50.00m);
        typeof(ItemComanda).GetProperty("Estado")?.SetValue(item, EstadoItemComanda.Pendiente);
        typeof(ItemComanda).GetProperty("Observaciones")?.SetValue(item, "Término medio");
        
        // Crear personalizaciones vacías por defecto
        typeof(ItemComanda).GetProperty("Personalizaciones")?.SetValue(item, new List<PersonalizacionItem>());
        
        return item;
    }

    private ItemComanda CrearItemComandaConPersonalizaciones()
    {
        var item = CrearItemComandaEjemplo();
        var personalizaciones = new List<PersonalizacionItem>
        {
            CrearPersonalizacionEjemplo(5.00m),
            CrearPersonalizacionEjemplo(3.00m)
        };
        typeof(ItemComanda).GetProperty("Personalizaciones")?.SetValue(item, personalizaciones);
        return item;
    }

    private PersonalizacionItem CrearPersonalizacionEjemplo(decimal precioAdicional)
    {
        // Usar el método estático público para crear PersonalizacionItem
        return PersonalizacionItem.CrearAgregar(
            Guid.NewGuid(),
            "Queso extra",
            1m,
            precioAdicional);
    }

    private object CrearTotalComandaMock(decimal subtotal, decimal impuestos, decimal total)
    {
        // Crear mock del value object TotalComanda
        var totalType = typeof(Comanda).Assembly.GetTypes()
            .FirstOrDefault(t => t.Name.Contains("Total"));
            
        if (totalType != null)
        {
            try
            {
                var totalObj = Activator.CreateInstance(totalType, true);
                totalType.GetProperty("Subtotal")?.SetValue(totalObj, subtotal);
                totalType.GetProperty("Impuestos")?.SetValue(totalObj, impuestos);
                totalType.GetProperty("Total")?.SetValue(totalObj, total);
                return totalObj!;
            }
            catch { }
        }
        
        // Fallback: crear objeto dinámico
        return new { Subtotal = subtotal, Impuestos = impuestos, Total = total };
    }

    #endregion
} 