using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Services;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components;

public class ClienteFormTests : TestContext
{
    private readonly Mock<ClientesApiService> _clientesApiMock;
    private readonly Mock<IJSRuntime> _jsRuntimeMock;

    public ClienteFormTests()
    {
        _clientesApiMock = new Mock<ClientesApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _jsRuntimeMock = new Mock<IJSRuntime>();

        Services.AddSingleton(_clientesApiMock.Object);
        Services.AddSingleton(_jsRuntimeMock.Object);
    }

    // ===== PRUEBAS BÁSICAS =====

    [Fact]
    public void Renderizar_ConMostrarFalse_DeberiaOcultarModal()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, false)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        var modal = component.Find(".modal");
        modal.ClassList.Should().NotContain("show");
        modal.GetAttribute("style").Should().Contain("display: none");
    }

    [Fact]
    public void Renderizar_ConMostrarTrue_DeberiaMostrarModal()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        var modal = component.Find(".modal");
        modal.ClassList.Should().Contain("show");
        modal.GetAttribute("style").Should().Contain("display: block");
    }

    [Fact]
    public void Renderizar_ConClienteNuevo_DeberiaMostrarTituloCorrecto()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        var titulo = component.Find(".modal-title");
        titulo.TextContent.Should().Contain("Nuevo Cliente");
    }

    [Fact]
    public void Renderizar_ConClienteExistente_DeberiaMostrarTituloCorrecto()
    {
        // Arrange
        var cliente = new ClienteDto { Id = Guid.NewGuid(), Nombre = "Test" };
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act
        component.Instance.AbrirParaEditar(cliente);
        component.Render();

        // Assert
        var titulo = component.Find(".modal-title");
        titulo.TextContent.Should().Contain("Editar Cliente");
    }

    // ===== PRUEBAS DE INICIALIZACIÓN =====

    [Fact]
    public void OnParametersSet_ConClienteNuevo_DeberiaInicializarValoresPorDefecto()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act
        component.SetParametersAndRender(parameters => parameters
            .Add(p => p.Mostrar, true)
        );

        // Assert
        var fechaNacimiento = component.Find("input[type='date']");
        var estaActivo = component.Find("input[type='checkbox']");
        var aceptaTerminos = component.FindAll("input[type='checkbox']").Last();

        fechaNacimiento.GetAttribute("value").Should().NotBeNullOrEmpty();
        estaActivo.HasAttribute("checked").Should().BeTrue();
        aceptaTerminos.HasAttribute("checked").Should().BeTrue();
    }

    [Fact]
    public void AbrirParaCrear_DeberiaInicializarClienteNuevo()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, false)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act
        component.Instance.AbrirParaCrear();

        // Assert
        component.Instance.Cliente.Id.Should().Be(Guid.Empty);
        component.Instance.Cliente.EstaActivo.Should().BeTrue();
        component.Instance.Cliente.AceptaTerminos.Should().BeTrue();
        component.Instance.Mostrar.Should().BeTrue();
    }

    [Fact]
    public void AbrirParaEditar_DeberiaCargarDatosDelCliente()
    {
        // Arrange
        var cliente = new ClienteDto
        {
            Id = Guid.NewGuid(),
            Nombre = "Juan",
            Apellidos = "Pérez",
            Email = "juan@test.com",
            Telefono = "123456789",
            TotalGastado = 1000,
            TotalCompras = 5,
            TotalVisitas = 10,
            PromedioGasto = 200,
            Segmento = "VIP",
            PuntosFidelizacion = 100
        };

        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, false)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act
        component.Instance.AbrirParaEditar(cliente);

        // Assert
        component.Instance.Cliente.Id.Should().Be(cliente.Id);
        component.Instance.Cliente.Nombre.Should().Be(cliente.Nombre);
        component.Instance.Cliente.Apellidos.Should().Be(cliente.Apellidos);
        component.Instance.Cliente.Email.Should().Be(cliente.Email);
        component.Instance.Cliente.Telefono.Should().Be(cliente.Telefono);
        component.Instance.Cliente.TotalGastado.Should().Be(cliente.TotalGastado);
        component.Instance.Cliente.TotalCompras.Should().Be(cliente.TotalCompras);
        component.Instance.Cliente.TotalVisitas.Should().Be(cliente.TotalVisitas);
        component.Instance.Cliente.PromedioGasto.Should().Be(cliente.PromedioGasto);
        component.Instance.Cliente.Segmento.Should().Be(cliente.Segmento);
        component.Instance.Cliente.PuntosFidelizacion.Should().Be(cliente.PuntosFidelizacion);
        component.Instance.Mostrar.Should().BeTrue();
    }

    // ===== PRUEBAS DE VALIDACIÓN =====

    [Fact]
    public async Task GuardarCliente_ConEmailDuplicado_DeberiaMostrarError()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        component.Instance.Cliente.Email = "test@test.com";

        _clientesApiMock.Setup(x => x.ValidarEmailAsync("test@test.com", null))
            .ReturnsAsync(new ApiResponse<bool> { Success = true, Data = true });

        _jsRuntimeMock.Setup(x => x.InvokeVoidAsync("alert", It.IsAny<string>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await component.Instance.GuardarCliente();

        // Assert
        _jsRuntimeMock.Verify(x => x.InvokeVoidAsync("alert", "El email ya está registrado por otro cliente."), Times.Once);
        component.Instance.guardando.Should().BeFalse();
    }

    [Fact]
    public async Task GuardarCliente_ConEmailValido_DeberiaContinuarConGuardado()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        component.Instance.Cliente.Email = "test@test.com";

        _clientesApiMock.Setup(x => x.ValidarEmailAsync("test@test.com", null))
            .ReturnsAsync(new ApiResponse<bool> { Success = true, Data = false });

        _clientesApiMock.Setup(x => x.CrearClienteAsync(It.IsAny<CrearClienteRequest>()))
            .ReturnsAsync(new ApiResponse<ClienteDto> { Success = true, Data = new ClienteDto { Id = Guid.NewGuid() } });

        _jsRuntimeMock.Setup(x => x.InvokeVoidAsync("alert", It.IsAny<string>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await component.Instance.GuardarCliente();

        // Assert
        _clientesApiMock.Verify(x => x.CrearClienteAsync(It.IsAny<CrearClienteRequest>()), Times.Once);
    }

    // ===== PRUEBAS DE CREACIÓN =====

    [Fact]
    public async Task GuardarCliente_ConClienteNuevo_DeberiaLlamarCrearClienteAsync()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        component.Instance.Cliente = new ClienteDto
        {
            Nombre = "Juan",
            Apellidos = "Pérez",
            Email = "juan@test.com",
            Telefono = "123456789",
            FechaNacimiento = DateTime.Today.AddYears(-25),
            AceptaTerminos = true
        };

        _clientesApiMock.Setup(x => x.ValidarEmailAsync(It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new ApiResponse<bool> { Success = true, Data = false });

        _clientesApiMock.Setup(x => x.CrearClienteAsync(It.IsAny<CrearClienteRequest>()))
            .ReturnsAsync(new ApiResponse<ClienteDto> { Success = true, Data = new ClienteDto { Id = Guid.NewGuid() } });

        _jsRuntimeMock.Setup(x => x.InvokeVoidAsync("alert", It.IsAny<string>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await component.Instance.GuardarCliente();

        // Assert
        _clientesApiMock.Verify(x => x.CrearClienteAsync(It.Is<CrearClienteRequest>(r =>
            r.Nombre == "Juan" &&
            r.Apellidos == "Pérez" &&
            r.Email == "juan@test.com" &&
            r.Telefono == "123456789" &&
            r.AceptaTerminos == true
        )), Times.Once);
    }

    [Fact]
    public async Task GuardarCliente_ConClienteNuevoExitoso_DeberiaInvocarCallback()
    {
        // Arrange
        var clienteGuardado = new ClienteDto { Id = Guid.NewGuid(), Nombre = "Juan" };
        var callbackInvocado = false;
        ClienteDto? clienteCallback = null;

        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => 
            {
                callbackInvocado = true;
                clienteCallback = cliente;
            }))
        );

        component.Instance.Cliente = new ClienteDto
        {
            Nombre = "Juan",
            Apellidos = "Pérez",
            Email = "juan@test.com",
            AceptaTerminos = true
        };

        _clientesApiMock.Setup(x => x.ValidarEmailAsync(It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new ApiResponse<bool> { Success = true, Data = false });

        _clientesApiMock.Setup(x => x.CrearClienteAsync(It.IsAny<CrearClienteRequest>()))
            .ReturnsAsync(new ApiResponse<ClienteDto> { Success = true, Data = clienteGuardado });

        _jsRuntimeMock.Setup(x => x.InvokeVoidAsync("alert", It.IsAny<string>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await component.Instance.GuardarCliente();

        // Assert
        callbackInvocado.Should().BeTrue();
        clienteCallback.Should().Be(clienteGuardado);
    }

    // ===== PRUEBAS DE ACTUALIZACIÓN =====

    [Fact]
    public async Task GuardarCliente_ConClienteExistente_DeberiaLlamarActualizarClienteAsync()
    {
        // Arrange
        var cliente = new ClienteDto { Id = Guid.NewGuid(), Nombre = "Juan" };
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        component.Instance.AbrirParaEditar(cliente);
        component.Instance.Cliente.Nombre = "Juan Actualizado";

        _clientesApiMock.Setup(x => x.ValidarEmailAsync(It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new ApiResponse<bool> { Success = true, Data = false });

        _clientesApiMock.Setup(x => x.ActualizarClienteAsync(It.IsAny<ActualizarClienteRequest>()))
            .ReturnsAsync(new ApiResponse<ClienteDto> { Success = true, Data = cliente });

        _jsRuntimeMock.Setup(x => x.InvokeVoidAsync("alert", It.IsAny<string>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await component.Instance.GuardarCliente();

        // Assert
        _clientesApiMock.Verify(x => x.ActualizarClienteAsync(It.Is<ActualizarClienteRequest>(r =>
            r.Id == cliente.Id &&
            r.Nombre == "Juan Actualizado"
        )), Times.Once);
    }

    // ===== PRUEBAS DE ERRORES =====

    [Fact]
    public async Task GuardarCliente_ConErrorEnAPI_DeberiaMostrarMensajeDeError()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        component.Instance.Cliente = new ClienteDto
        {
            Nombre = "Juan",
            Apellidos = "Pérez",
            Email = "juan@test.com",
            AceptaTerminos = true
        };

        _clientesApiMock.Setup(x => x.ValidarEmailAsync(It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new ApiResponse<bool> { Success = true, Data = false });

        _clientesApiMock.Setup(x => x.CrearClienteAsync(It.IsAny<CrearClienteRequest>()))
            .ReturnsAsync(new ApiResponse<ClienteDto> { Success = false, Message = "Error del servidor" });

        _jsRuntimeMock.Setup(x => x.InvokeVoidAsync("alert", It.IsAny<string>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await component.Instance.GuardarCliente();

        // Assert
        _jsRuntimeMock.Verify(x => x.InvokeVoidAsync("alert", "Error del servidor"), Times.Once);
    }

    [Fact]
    public async Task GuardarCliente_ConExcepcion_DeberiaMostrarMensajeDeError()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        component.Instance.Cliente = new ClienteDto
        {
            Nombre = "Juan",
            Apellidos = "Pérez",
            Email = "juan@test.com",
            AceptaTerminos = true
        };

        _clientesApiMock.Setup(x => x.ValidarEmailAsync(It.IsAny<string>(), It.IsAny<Guid?>()))
            .ThrowsAsync(new Exception("Error de conexión"));

        _jsRuntimeMock.Setup(x => x.InvokeVoidAsync("alert", It.IsAny<string>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        await component.Instance.GuardarCliente();

        // Assert
        _jsRuntimeMock.Verify(x => x.InvokeVoidAsync("alert", "Error: Error de conexión"), Times.Once);
    }

    // ===== PRUEBAS DE UI =====

    [Fact]
    public void Renderizar_ConClienteEnEdicion_DeberiaMostrarInformacionAdicional()
    {
        // Arrange
        var cliente = new ClienteDto
        {
            Id = Guid.NewGuid(),
            TotalGastado = 1000,
            TotalCompras = 5,
            TotalVisitas = 10,
            PromedioGasto = 200,
            Segmento = "VIP",
            PuntosFidelizacion = 100
        };

        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act
        component.Instance.AbrirParaEditar(cliente);
        component.Render();

        // Assert
        var alertInfo = component.Find(".alert-info");
        alertInfo.Should().NotBeNull();
        alertInfo.TextContent.Should().Contain("Total Gastado");
        alertInfo.TextContent.Should().Contain("Total Compras");
        alertInfo.TextContent.Should().Contain("Total Visitas");
        alertInfo.TextContent.Should().Contain("Promedio Gasto");
        alertInfo.TextContent.Should().Contain("Segmento");
        alertInfo.TextContent.Should().Contain("Puntos Fidelización");
    }

    [Fact]
    public void Renderizar_ConClienteNuevo_NoDeberiaMostrarInformacionAdicional()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        var alertInfo = component.FindAll(".alert-info");
        alertInfo.Should().BeEmpty();
    }

    [Fact]
    public async Task GuardarCliente_ConGuardandoTrue_DeberiaDeshabilitarBoton()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        component.Instance.Cliente = new ClienteDto
        {
            Nombre = "Juan",
            Apellidos = "Pérez",
            Email = "juan@test.com",
            AceptaTerminos = true
        };

        _clientesApiMock.Setup(x => x.ValidarEmailAsync(It.IsAny<string>(), It.IsAny<Guid?>()))
            .ReturnsAsync(new ApiResponse<bool> { Success = true, Data = false });

        _clientesApiMock.Setup(x => x.CrearClienteAsync(It.IsAny<CrearClienteRequest>()))
            .ReturnsAsync(new ApiResponse<ClienteDto> { Success = true, Data = new ClienteDto { Id = Guid.NewGuid() } });

        _jsRuntimeMock.Setup(x => x.InvokeVoidAsync("alert", It.IsAny<string>()))
            .Returns(ValueTask.CompletedTask);

        // Act
        var guardarTask = component.Instance.GuardarCliente();
        
        // Assert - Durante el guardado
        component.Instance.guardando.Should().BeTrue();
        
        await guardarTask;
        
        // Assert - Después del guardado
        component.Instance.guardando.Should().BeFalse();
    }

    // ===== PRUEBAS DE CERRAR =====

    [Fact]
    public async Task Cerrar_DeberiaInvocarMostrarChanged()
    {
        // Arrange
        var mostrarChangedInvocado = false;
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => 
            {
                mostrarChangedInvocado = true;
            }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act
        await component.Instance.Cerrar();

        // Assert
        mostrarChangedInvocado.Should().BeTrue();
    }
}
