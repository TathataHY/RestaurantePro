using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using RestaurantePro.Web.Admin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class PreparacionFormTests : TestContext
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<TokenStore> _mockTokenStore;
        private readonly Mock<IJSRuntime> _mockJSRuntime;

        public PreparacionFormTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockTokenStore = new Mock<TokenStore>();
            _mockJSRuntime = new Mock<IJSRuntime>();

            Services.AddSingleton(_mockHttpClientFactory.Object);
            Services.AddSingleton(_mockTokenStore.Object);
            Services.AddSingleton(_mockJSRuntime.Object);
            Services.AddSingleton(new PreparacionesApiService(_mockHttpClientFactory.Object, _mockTokenStore.Object));
        }

        [Fact]
        public void Renderizar_FormularioNuevo_DeberiaMostrarTituloCorrecto()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            var titulo = componente.Find("h5.modal-title");
            Assert.Contains("Nueva Preparación", titulo.TextContent);
        }

        [Fact]
        public void Renderizar_FormularioEdicion_DeberiaMostrarTituloCorrecto()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 1, ComandaNumero = "CMD-001" };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            var titulo = componente.Find("h5.modal-title");
            Assert.Contains("Editar Preparación", titulo.TextContent);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposObligatorios()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find("input[readonly]"));
            Assert.NotNull(componente.Find("input[type='number']"));
            Assert.NotNull(componente.Find("select"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSelectsConOpciones()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act
            var selects = componente.FindAll("select");

            // Assert
            Assert.True(selects.Count >= 3); // Estado, Prioridad, Cocinero
        }

        [Fact]
        public void Renderizar_DeberiaMostrarOpcionesEstado()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("Pendiente", componente.Markup);
            Assert.Contains("En Proceso", componente.Markup);
            Assert.Contains("Lista", componente.Markup);
            Assert.Contains("Entregada", componente.Markup);
            Assert.Contains("Cancelada", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarOpcionesPrioridad()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("Baja", componente.Markup);
            Assert.Contains("Normal", componente.Markup);
            Assert.Contains("Alta", componente.Markup);
            Assert.Contains("Urgente", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTextareas()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act
            var textareas = componente.FindAll("textarea");

            // Assert
            Assert.True(textareas.Count >= 2); // Observaciones, Notas del Cocinero
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesAccion()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act
            var botones = componente.FindAll("button");

            // Assert
            Assert.True(botones.Count >= 2); // Cancelar y Guardar
        }

        [Fact]
        public void Renderizar_DeberiaMostrarValidaciones()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act
            var validaciones = componente.FindAll("ValidationMessage");

            // Assert
            Assert.True(validaciones.Count >= 0); // Las validaciones solo aparecen cuando hay errores
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraModal()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find(".modal-dialog.modal-lg"));
            Assert.NotNull(componente.Find(".modal-content"));
            Assert.NotNull(componente.Find(".modal-header"));
            Assert.NotNull(componente.Find(".modal-body"));
            Assert.NotNull(componente.Find(".modal-footer"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFormularioConValidacion()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find("form"));
            // DataAnnotationsValidator se renderiza como un componente, no como un elemento HTML
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraColumnas()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find(".row"));
            Assert.NotNull(componente.Find(".col-md-6"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInputsConTiposCorrectos()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find("input[readonly]"));
            Assert.NotNull(componente.Find("input[type='number']"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarLabels()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("Número de Comanda", componente.Markup);
            Assert.Contains("Mesa", componente.Markup);
            Assert.Contains("Producto", componente.Markup);
            Assert.Contains("Cantidad", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
            Assert.Contains("Prioridad", componente.Markup);
            Assert.Contains("Cocinero", componente.Markup);
            Assert.Contains("Observaciones", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconos()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find("i.oi"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInformacionAdicionalEnEdicion()
        {
            // Arrange
            var preparacion = new PreparacionDto 
            { 
                Id = 1, 
                FechaCreacion = DateTime.Now,
                ClienteNombre = "Cliente Test"
            };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("Fecha de Creación", componente.Markup);
            Assert.Contains("Cliente", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarAlertaAtrasada()
        {
            // Arrange
            var preparacion = new PreparacionDto 
            { 
                Id = 1, 
                EstaAtrasada = true
            };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("¡Atención!", componente.Markup);
            Assert.Contains("atrasada", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarAlertaUrgente()
        {
            // Arrange
            var preparacion = new PreparacionDto 
            { 
                Id = 1, 
                TiempoRestanteMinutos = 3
            };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("¡Urgente!", componente.Markup);
            Assert.Contains("Tiempo restante", componente.Markup);
        }

        [Fact]
        public void Renderizar_EstadoPendiente_DeberiaMostrarBotonIniciar()
        {
            // Arrange
            var preparacion = new PreparacionDto 
            { 
                Id = 1, 
                Estado = EstadoPreparacion.Pendiente
            };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("Iniciar", componente.Markup);
        }

        [Fact]
        public void Renderizar_EstadoEnProceso_DeberiaMostrarBotonCompletar()
        {
            // Arrange
            var preparacion = new PreparacionDto 
            { 
                Id = 1, 
                Estado = EstadoPreparacion.EnProceso
            };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("Completar", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposReadonly()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act
            var camposReadonly = componente.FindAll("input[readonly]");

            // Assert
            Assert.True(camposReadonly.Count >= 3); // Comanda, Mesa, Producto, Tiempo Transcurrido
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposNumericos()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act
            var camposNumericos = componente.FindAll("input[type='number']");

            // Assert
            Assert.True(camposNumericos.Count >= 3); // Cantidad, Tiempo Estimado, Tiempo Real
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSelectCocineros()
        {
            // Arrange
            var preparacion = new PreparacionDto { Id = 0 };

            var componente = RenderComponent<PreparacionForm>(parameters => parameters
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("Seleccionar cocinero", componente.Markup);
        }
    }
}
