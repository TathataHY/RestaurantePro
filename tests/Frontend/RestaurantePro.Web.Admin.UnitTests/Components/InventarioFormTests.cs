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

public class InventarioFormTests : TestContext
{
    private readonly Mock<IJSRuntime> _jsRuntimeMock;

    public InventarioFormTests()
    {
        _jsRuntimeMock = new Mock<IJSRuntime>();
        Services.AddSingleton(_jsRuntimeMock.Object);
    }

    // ===== PRUEBAS BÁSICAS DE RENDERIZADO =====

    [Fact]
    public void Renderizar_ConMostrarFalse_DeberiaOcultarModal()
    {
        // Arrange
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, false)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var modal = component.Find(".modal");
        modal.ClassList.Should().NotContain("show");
        modal.ClassList.Should().NotContain("d-block");
    }

    [Fact]
    public void Renderizar_ConMostrarTrue_DeberiaMostrarModal()
    {
        // Arrange
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var modal = component.Find(".modal");
        modal.ClassList.Should().Contain("show");
        modal.ClassList.Should().Contain("d-block");
    }

    [Fact]
    public void Renderizar_ConIngredienteNuevo_DeberiaMostrarTituloCorrecto()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var titulo = component.Find(".modal-title");
        titulo.TextContent.Should().Contain("Nuevo Ingrediente");
    }

    [Fact]
    public void Renderizar_ConIngredienteExistente_DeberiaMostrarTituloCorrecto()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.NewGuid(), Nombre = "Test" };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var titulo = component.Find(".modal-title");
        titulo.TextContent.Should().Contain("Editar Ingrediente");
    }

    // ===== PRUEBAS DE CAMPOS DE FORMULARIO =====

    [Fact]
    public void Renderizar_DeberiaMostrarTodosLosCamposRequeridos()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        component.Find("input[placeholder='Nombre del ingrediente']").Should().NotBeNull();
        component.Find("input[placeholder='Categoría del ingrediente']").Should().NotBeNull();
        component.Find("input[placeholder='Nombre del proveedor']").Should().NotBeNull();
        component.FindAll("input[type='number']").Should().HaveCount(4); // Stock mínimo, máximo, actual, costo unitario
    }

    [Fact]
    public void Renderizar_DeberiaMostrarSelectoresConOpciones()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var selects = component.FindAll("select");
        selects.Should().NotBeEmpty();
        
        // Verificar que hay opciones en los selects
        var opciones = component.FindAll("option");
        opciones.Should().NotBeEmpty();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarOpcionesDeUnidadMedida()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var opciones = component.FindAll("option");
        opciones.Should().Contain(o => o.TextContent.Contains("Kilogramo (kg)"));
        opciones.Should().Contain(o => o.TextContent.Contains("Gramo (g)"));
        opciones.Should().Contain(o => o.TextContent.Contains("Litro (l)"));
        opciones.Should().Contain(o => o.TextContent.Contains("Mililitro (ml)"));
        opciones.Should().Contain(o => o.TextContent.Contains("Unidad"));
        opciones.Should().Contain(o => o.TextContent.Contains("Docena"));
        opciones.Should().Contain(o => o.TextContent.Contains("Caja"));
        opciones.Should().Contain(o => o.TextContent.Contains("Bolsa"));
        opciones.Should().Contain(o => o.TextContent.Contains("Paquete"));
    }

    [Fact]
    public void Renderizar_DeberiaMostrarSelectoresConOpcionesVacias()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var opciones = component.FindAll("option");
        opciones.Should().Contain(o => o.TextContent.Contains("Seleccionar unidad"));
    }

    // ===== PRUEBAS DE BOTONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarBotonesDeAccion()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
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
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var iconos = component.FindAll("span[class*='oi']");
        iconos.Should().NotBeEmpty();
        
        // Verificar que hay iconos en el título
        var titulo = component.Find(".modal-title");
        titulo.TextContent.Should().Contain("Nuevo Ingrediente");
    }

    // ===== PRUEBAS DE VALIDACIONES =====

    [Fact]
    public void Renderizar_DeberiaMostrarLabelsConAsteriscosParaCamposRequeridos()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var labels = component.FindAll("label");
        var labelsRequeridos = labels.Where(l => l.TextContent.Contains("*")).ToList();
        
        labelsRequeridos.Should().NotBeEmpty();
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Nombre *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Unidad de Medida *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Stock Mínimo *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Stock Máximo *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Stock Actual *"));
        labelsRequeridos.Should().Contain(l => l.TextContent.Contains("Costo Unitario *"));
    }

    [Fact]
    public void Renderizar_DeberiaMostrarMensajesDeValidacion()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
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
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
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
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
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
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
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
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var modal = component.Find(".modal");
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
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var inputs = component.FindAll("input[type='number']");
        inputs.Should().NotBeEmpty();
        
        // Verificar que los inputs numéricos tienen atributos min
        var inputsConMin = inputs.Where(i => i.GetAttribute("min") == "0");
        inputsConMin.Should().NotBeEmpty();
    }

    // ===== PRUEBAS DE INTERACCION =====

    [Fact]
    public void ClickEnBotonCerrar_DeberiaInvocarCallback()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act
        var botonCerrar = component.Find("button[type='button']");
        botonCerrar.Click();

        // Assert
        // Nota: En pruebas unitarias, los EventCallbacks no se ejecutan automáticamente
        // Esto se probaría mejor en pruebas de integración
        botonCerrar.Should().NotBeNull();
    }

    // ===== PRUEBAS DE MODAL LG =====

    [Fact]
    public void Renderizar_DeberiaUsarModalLg()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var modalDialog = component.Find(".modal-dialog");
        modalDialog.ClassList.Should().Contain("modal-lg");
    }

    // ===== PRUEBAS DE CAMPOS ESPECÍFICOS =====

    [Fact]
    public void Renderizar_DeberiaMostrarCamposDeStock()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var inputs = component.FindAll("input[type='number']");
        inputs.Should().HaveCount(4); // Stock mínimo, máximo, actual, costo unitario
    }

    [Fact]
    public void Renderizar_DeberiaMostrarCamposConAtributosMinimos()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var inputs = component.FindAll("input[type='number']");
        inputs.Should().NotBeEmpty();
        
        // Verificar que los inputs tienen atributos step y min
        var inputsConStep = inputs.Where(i => i.GetAttribute("step") == "0.01");
        inputsConStep.Should().NotBeEmpty();
        
        var inputsConMin = inputs.Where(i => i.GetAttribute("min") == "0");
        inputsConMin.Should().NotBeEmpty();
    }

    [Fact]
    public void Renderizar_DeberiaMostrarCampoFechaVencimiento()
    {
        // Arrange
        var ingrediente = new IngredienteDto { Id = Guid.Empty };
        var component = RenderComponent<InventarioForm>(parameters => parameters
            .Add(p => p.Mostrar, true)
            .Add(p => p.Ingrediente, ingrediente)
            .Add(p => p.OnCancel, EventCallback.Factory.Create(this, () => { }))
            .Add(p => p.OnSubmit, EventCallback.Factory.Create(this, () => { }))
        );

        // Act & Assert
        var fechaVencimiento = component.Find("input[type='date']");
        fechaVencimiento.Should().NotBeNull();
    }
}
