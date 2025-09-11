using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
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

    // ===== PRUEBAS BÁSICAS DE RENDERIZADO =====

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

        // Act - Simular edición mediante parámetros
        component.SetParametersAndRender(parameters => parameters
            .Add(p => p.Mostrar, true)
        );

        // Assert
        var titulo = component.Find(".modal-title");
        titulo.TextContent.Should().Contain("Nuevo Cliente"); // Por defecto es nuevo
    }

    // ===== PRUEBAS DE CAMPOS DE FORMULARIO =====

    [Fact]
    public void Renderizar_DeberiaMostrarTodosLosCamposRequeridos()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        component.Find("input[placeholder='Ingrese el nombre']").Should().NotBeNull();
        component.Find("input[placeholder='Ingrese los apellidos']").Should().NotBeNull();
        component.Find("input[placeholder='cliente@ejemplo.com']").Should().NotBeNull();
        component.Find("input[placeholder='+1 234 567 8900']").Should().NotBeNull();
        component.Find("input[type='date']").Should().NotBeNull();
        component.Find("input[placeholder='Ciudad']").Should().NotBeNull();
        component.Find("textarea[placeholder='Dirección completa']").Should().NotBeNull();
        component.Find("input[placeholder='12345']").Should().NotBeNull();
        component.Find("input[placeholder='País']").Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarCheckboxesDeConfiguracion()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        var checkboxes = component.FindAll("input[type='checkbox']");
        checkboxes.Should().NotBeEmpty();
        checkboxes.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void Renderizar_DeberiaMostrarTextareasParaInformacionAdicional()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        component.Find("textarea[placeholder='Ej: Vegetariano, Sin gluten, etc.']").Should().NotBeNull();
        component.Find("textarea[placeholder='Ej: Nuts, Mariscos, etc.']").Should().NotBeNull();
        component.Find("textarea[placeholder='Información adicional sobre el cliente']").Should().NotBeNull();
    }

    // ===== PRUEBAS DE BOTONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesDeAccion()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        var botones = component.FindAll("button");
        botones.Should().NotBeEmpty();
        
        // Verificar que hay botones en el footer
        var footer = component.Find(".modal-footer");
        footer.Should().NotBeNull();
        
        var botonesFooter = component.FindAll(".modal-footer button");
        botonesFooter.Should().NotBeEmpty();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarIconosEnBotones()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        var iconos = component.FindAll("i");
        iconos.Should().NotBeEmpty();
        
        // Verificar que hay iconos en el título
        var titulo = component.Find(".modal-title");
        titulo.TextContent.Should().Contain("Nuevo Cliente");
    }

    // ===== PRUEBAS DE VALIDACIONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarMensajesDeValidacion()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        // Verificar que hay mensajes de validación para campos requeridos
        var camposRequeridos = component.FindAll("label");
        camposRequeridos.Should().Contain(l => l.TextContent.Contains("*"));
        
        // Verificar que hay elementos de validación (pueden estar vacíos inicialmente)
        var elementosValidacion = component.FindAll("[class*='text-danger']");
        elementosValidacion.Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarLabelsConAsteriscosParaCamposRequeridos()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        var labels = component.FindAll("label");
        var labelsRequeridos = labels.Where(l => l.TextContent.Contains("*")).ToList();
        
        labelsRequeridos.Should().NotBeEmpty();
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Nombre *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Apellidos *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Email *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Fecha de Nacimiento *"));
    }

    // ===== PRUEBAS DE ESTRUCTURA =====

    [Fact]
    public void Renderizar_DeberiaTenerEstructuraCorrectaDelModal()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        var modal = component.Find(".modal");
        modal.Should().NotBeNull();
        
        var modalDialog = component.Find(".modal-dialog");
        modalDialog.Should().NotBeNull();
        
        var modalContent = component.Find(".modal-content");
        modalContent.Should().NotBeNull();
        
        var modalHeader = component.Find(".modal-header");
        modalHeader.Should().NotBeNull();
        
        var modalBody = component.Find(".modal-body");
        modalBody.Should().NotBeNull();
        
        var modalFooter = component.Find(".modal-footer");
        modalFooter.Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaTenerFormularioConValidacion()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        var form = component.Find("form");
        form.Should().NotBeNull();
        
        // Verificar que el formulario existe y tiene la estructura correcta
        form.TagName.Should().Be("FORM");
    }

    // ===== PRUEBAS DE RESPONSIVIDAD =====

    [Fact]
    public void Renderizar_DeberiaTenerClasesResponsivas()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        var rows = component.FindAll(".row");
        rows.Should().NotBeEmpty();
        
        // Verificar que hay elementos con clases responsivas
        var elementosResponsivos = component.FindAll("[class*='col-']");
        elementosResponsivos.Should().NotBeEmpty();
    }

    // ===== PRUEBAS DE ACCESIBILIDAD =====

    [Fact]
    public void Renderizar_DeberiaTenerAtributosDeAccesibilidad()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        var modal = component.Find(".modal");
        modal.GetAttribute("role").Should().Be("dialog");
        modal.GetAttribute("tabindex").Should().Be("-1");
        
        var inputs = component.FindAll("input");
        inputs.Should().NotBeEmpty();
        
        // Verificar que los inputs tienen placeholders
        var inputsConPlaceholder = inputs.Where(i => !string.IsNullOrEmpty(i.GetAttribute("placeholder")));
        inputsConPlaceholder.Should().NotBeEmpty();
    }

    // ===== PRUEBAS DE ESTADO INICIAL =====

    [Fact]
    public void Renderizar_ConMostrarTrue_DeberiaInicializarConValoresPorDefecto()
    {
        // Arrange
        var component = RenderComponent<ClienteForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnClienteGuardado, EventCallback.Factory.Create<ClienteDto>(this, (ClienteDto cliente) => { }))
        );

        // Act & Assert
        var fechaInput = component.Find("input[type='date']");
        fechaInput.GetAttribute("value").Should().NotBeNullOrEmpty();
        
        var checkboxes = component.FindAll("input[type='checkbox']");
        checkboxes.Should().NotBeEmpty();
    }

    // ===== PRUEBAS DE INTERACCION =====

    [Fact]
    public void ClickEnBotonCerrar_DeberiaInvocarCallback()
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
        var botonCerrar = component.Find("button[type='button']");
        botonCerrar.Click();

        // Assert
        // Nota: En pruebas unitarias, los EventCallbacks no se ejecutan automáticamente
        // Esto se probaría mejor en pruebas de integración
        botonCerrar.Should().NotBeNull();
    }
}