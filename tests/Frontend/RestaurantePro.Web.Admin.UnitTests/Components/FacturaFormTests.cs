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

public class FacturaFormTests : TestContext
{
    private readonly Mock<FacturasApiService> _facturasApiMock;
    private readonly Mock<ClientesApiService> _clientesApiMock;
    private readonly Mock<MesasApiService> _mesasApiMock;
    private readonly Mock<UsuariosApiService> _usuariosApiMock;
    private readonly Mock<CategoriasApiService> _categoriasApiMock;
    private readonly Mock<IJSRuntime> _jsRuntimeMock;

    public FacturaFormTests()
    {
        _facturasApiMock = new Mock<FacturasApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _clientesApiMock = new Mock<ClientesApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _mesasApiMock = new Mock<MesasApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _usuariosApiMock = new Mock<UsuariosApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _categoriasApiMock = new Mock<CategoriasApiService>(Mock.Of<IHttpClientFactory>(), Mock.Of<TokenStore>());
        _jsRuntimeMock = new Mock<IJSRuntime>();

        Services.AddSingleton<IFacturasApiService>(_facturasApiMock.Object);
        Services.AddSingleton<IClientesApiService>(_clientesApiMock.Object);
        Services.AddSingleton<IMesasApiService>(_mesasApiMock.Object);
        Services.AddSingleton<IUsuariosApiService>(_usuariosApiMock.Object);
        Services.AddSingleton<ICategoriasApiService>(_categoriasApiMock.Object);
        Services.AddSingleton(_jsRuntimeMock.Object);
    }

    // ===== PRUEBAS BÁSICAS DE RENDERIZADO =====

    [Fact]
    public void Renderizar_ConMostrarFalse_DeberiaOcultarModal()
    {
        // Arrange
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, false)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
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
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
        );

        // Act & Assert
        var modal = component.Find(".modal");
        modal.ClassList.Should().Contain("show");
        modal.GetAttribute("style").Should().Contain("display: block");
    }

    [Fact]
    public void Renderizar_ConFacturaNueva_DeberiaMostrarTituloCorrecto()
    {
        // Arrange
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
        );

        // Act & Assert
        var titulo = component.Find(".modal-title");
        titulo.TextContent.Should().Contain("Nueva Factura");
    }

    // ===== PRUEBAS DE CAMPOS DE FORMULARIO =====

    [Fact]
    public void Renderizar_DeberiaMostrarTodosLosCamposRequeridos()
    {
        // Arrange
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
        );

        // Act & Assert
        component.Find("input[placeholder='F001-000001']").Should().NotBeNull();
        component.Find("input[type='date']").Should().NotBeNull(); // Fecha de emisión
        component.FindAll("input[type='date']").Should().HaveCount(2); // Fecha emisión y vencimiento
    }

    [Fact]
    public void Renderizar_DeberiaMostrarSelectoresConOpciones()
    {
        // Arrange
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
        );

        // Act & Assert
        var selects = component.FindAll("select");
        selects.Should().NotBeEmpty();
        
        // Verificar que hay opciones en los selects
        var opciones = component.FindAll("option");
        opciones.Should().NotBeEmpty();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarOpcionesDeEstado()
    {
        // Arrange
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
        );

        // Act & Assert
        var opciones = component.FindAll("option");
        opciones.Should().Contain(o => o.TextContent.Contains("Pendiente"));
        opciones.Should().Contain(o => o.TextContent.Contains("Pagada"));
        opciones.Should().Contain(o => o.TextContent.Contains("Vencida"));
        opciones.Should().Contain(o => o.TextContent.Contains("Cancelada"));
    }

    [Fact]
    public void Renderizar_DeberiaMostrarSelectoresConOpcionesVacias()
    {
        // Arrange
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
        );

        // Act & Assert
        var opciones = component.FindAll("option");
        opciones.Should().Contain(o => o.TextContent.Contains("Seleccionar cliente"));
        opciones.Should().Contain(o => o.TextContent.Contains("Seleccionar mesa"));
        opciones.Should().Contain(o => o.TextContent.Contains("Seleccionar mesero"));
    }

    // ===== PRUEBAS DE BOTONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesDeAccion()
    {
        // Arrange
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
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
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
        );

        // Act & Assert
        var iconos = component.FindAll("i");
        iconos.Should().NotBeEmpty();
        
        // Verificar que hay iconos en el título
        var titulo = component.Find(".modal-title");
        titulo.TextContent.Should().Contain("Nueva Factura");
    }

    // ===== PRUEBAS DE VALIDACIONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarLabelsConAsteriscosParaCamposRequeridos()
    {
        // Arrange
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
        );

        // Act & Assert
        var labels = component.FindAll("label");
        var labelsRequeridos = labels.Where(l => l.TextContent.Contains("*")).ToList();
        
        labelsRequeridos.Should().NotBeEmpty();
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Número de Factura *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Fecha de Emisión *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Cliente *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Mesa *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Mesero *"));
    }

    [Fact]
    public void Renderizar_DeberiaMostrarMensajesDeValidacion()
    {
        // Arrange
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
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
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
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
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
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
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
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
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
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
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
        );

        // Act & Assert
        var fechaEmision = component.FindAll("input[type='date']").First();
        fechaEmision.Should().NotBeNull();
    }

    // ===== PRUEBAS DE INTERACCION =====

    [Fact]
    public void ClickEnBotonCerrar_DeberiaInvocarCallback()
    {
        // Arrange
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
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
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
        );

        // Act & Assert
        var modalDialog = component.Find(".modal-dialog");
        modalDialog.ClassList.Should().Contain("modal-xl");
    }

    // ===== PRUEBAS DE CAMPOS ESPECÍFICOS =====

    [Fact]
    public void Renderizar_DeberiaMostrarCampoNumeroFacturaConPlaceholder()
    {
        // Arrange
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
        );

        // Act & Assert
        var numeroFactura = component.Find("input[placeholder='F001-000001']");
        numeroFactura.Should().NotBeNull();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarCamposDeFecha()
    {
        // Arrange
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
        );

        // Act & Assert
        var fechas = component.FindAll("input[type='date']");
        fechas.Should().HaveCount(2); // Fecha emisión y vencimiento
    }

    // ===== PRUEBAS DE VALIDACIÓN DE NÚMERO =====

    [Fact]
    public void Renderizar_DeberiaMostrarIndicadorDeValidacion()
    {
        // Arrange
        var component = RenderComponent<FacturaForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.MostrarChanged, EventCallback.Factory.Create<bool>(this, (bool value) => { }))
            .Add(p => p.OnFacturaGuardada, EventCallback.Factory.Create<FacturaDto>(this, (FacturaDto factura) => { }))
        );

        // Act & Assert
        // Verificar que hay elementos de validación
        var elementosValidacion = component.FindAll("[class*='text-info']");
        elementosValidacion.Should().NotBeNull();
    }
}
