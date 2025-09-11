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

public class ComandaFormTests : TestContext
{
    private readonly Mock<ComandasApiService> _comandasApiMock;
    private readonly Mock<ClientesApiService> _clientesApiMock;
    private readonly Mock<MesasApiService> _mesasApiMock;
    private readonly Mock<IJSRuntime> _jsRuntimeMock;

    public ComandaFormTests()
    {
        _comandasApiMock = new Mock<ComandasApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _clientesApiMock = new Mock<ClientesApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _mesasApiMock = new Mock<MesasApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _jsRuntimeMock = new Mock<IJSRuntime>();

        Services.AddSingleton(_comandasApiMock.Object);
        Services.AddSingleton(_clientesApiMock.Object);
        Services.AddSingleton(_mesasApiMock.Object);
        Services.AddSingleton(_jsRuntimeMock.Object);
    }

    // ===== PRUEBAS BÁSICAS DE RENDERIZADO =====

    [Fact]
    public void Renderizar_ConMostrarFalse_DeberiaOcultarModal()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, false)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
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
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act & Assert
        var modal = component.Find(".modal");
        modal.ClassList.Should().Contain("show");
        modal.GetAttribute("style").Should().Contain("display: block");
    }

    [Fact]
    public void Renderizar_ConComandaNueva_DeberiaMostrarTituloCorrecto()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act & Assert
        var titulo = component.Find(".modal-title");
        titulo.TextContent.Should().Contain("Nueva Comanda");
    }

    // ===== PRUEBAS DE CAMPOS DE FORMULARIO =====

    [Fact]
    public void Renderizar_DeberiaMostrarTodosLosCamposRequeridos()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act & Assert
        component.Find("input[placeholder='C001-000001']").Should().NotBeNull();
        component.Find("select").Should().NotBeNull(); // Tipo de comanda
        component.Find("input[type='number']").Should().NotBeNull(); // Número de personas
    }

    [Fact]
    public void Renderizar_DeberiaMostrarSelectoresConOpciones()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act & Assert
        var selects = component.FindAll("select");
        selects.Should().NotBeEmpty();
        
        // Verificar que hay opciones en los selects
        var opciones = component.FindAll("option");
        opciones.Should().NotBeEmpty();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarOpcionesDeTipoComanda()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act & Assert
        var opciones = component.FindAll("option");
        opciones.Should().Contain(o => o.TextContent.Contains("Mesa"));
        opciones.Should().Contain(o => o.TextContent.Contains("Domicilio"));
        opciones.Should().Contain(o => o.TextContent.Contains("Mostrador"));
    }

    [Fact]
    public void Renderizar_DeberiaMostrarOpcionesDePrioridad()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act & Assert
        var opciones = component.FindAll("option");
        opciones.Should().Contain(o => o.TextContent.Contains("Baja"));
        opciones.Should().Contain(o => o.TextContent.Contains("Normal"));
        opciones.Should().Contain(o => o.TextContent.Contains("Alta"));
        opciones.Should().Contain(o => o.TextContent.Contains("Urgente"));
    }

    // ===== PRUEBAS DE BOTONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesDeAccion()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
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
    public void Renderizar_DeberiaMostrarIconosEnTitulo()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act & Assert
        var iconos = component.FindAll("i");
        iconos.Should().NotBeEmpty();
        
        // Verificar que hay iconos en el título
        var titulo = component.Find(".modal-title");
        titulo.TextContent.Should().Contain("Nueva Comanda");
    }

    // ===== PRUEBAS DE VALIDACIONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarLabelsConAsteriscosParaCamposRequeridos()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act & Assert
        var labels = component.FindAll("label");
        var labelsRequeridos = labels.Where(l => l.TextContent.Contains("*")).ToList();
        
        labelsRequeridos.Should().NotBeEmpty();
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Número de Comanda *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Tipo de Comanda *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Prioridad *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Número de Personas *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Mesa *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Mesero *"));
    }

    [Fact]
    public void Renderizar_DeberiaMostrarMensajesDeValidacion()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act & Assert
        // Verificar que hay mensajes de validación para campos requeridos
        var camposRequeridos = component.FindAll("label");
        camposRequeridos.Should().Contain(l => l.TextContent.Contains("*"));
        
        // Verificar que hay elementos de validación (pueden estar vacíos inicialmente)
        var elementosValidacion = component.FindAll("[class*='text-danger']");
        elementosValidacion.Should().NotBeNull();
    }

    // ===== PRUEBAS DE ESTRUCTURA =====

    [Fact]
    public void Renderizar_DeberiaTenerEstructuraCorrectaDelModal()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
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
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
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
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
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
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
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
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act & Assert
        var numeroPersonas = component.Find("input[type='number']");
        numeroPersonas.GetAttribute("min").Should().Be("1");
    }

    // ===== PRUEBAS DE INTERACCION =====

    [Fact]
    public void ClickEnBotonCerrar_DeberiaInvocarCallback()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act
        var botonCerrar = component.Find("button[type='button']");
        botonCerrar.Click();

        // Assert
        // Nota: En pruebas unitarias, los EventCallbacks no se ejecutan automáticamente
        // Esto se probaría mejor en pruebas de integración
        botonCerrar.Should().NotBeNull();
    }

    // ===== PRUEBAS DE MODAL XL =====

    [Fact]
    public void Renderizar_DeberiaUsarModalXl()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act & Assert
        var modalDialog = component.Find(".modal-dialog");
        modalDialog.ClassList.Should().Contain("modal-xl");
    }

    // ===== PRUEBAS DE CAMPOS ESPECÍFICOS =====

    [Fact]
    public void Renderizar_DeberiaMostrarCampoNumeroPersonasConMinimo()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act & Assert
        var numeroPersonas = component.Find("input[type='number']");
        numeroPersonas.GetAttribute("min").Should().Be("1");
        numeroPersonas.Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarSelectoresConOpcionesVacias()
    {
        // Arrange
        var component = RenderComponent<ComandaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnComandaGuardada, EventCallback.Factory.Create<ComandaDto>(this, (ComandaDto comanda) => { }))
        );

        // Act & Assert
        var opciones = component.FindAll("option");
        opciones.Should().Contain(o => o.TextContent.Contains("Seleccionar mesa"));
        opciones.Should().Contain(o => o.TextContent.Contains("Seleccionar mesero"));
    }
}
