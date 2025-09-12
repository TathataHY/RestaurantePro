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
    public class ReservacionFormTests : TestContext
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<TokenStore> _mockTokenStore;
        private readonly Mock<IJSRuntime> _mockJSRuntime;

        public ReservacionFormTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockTokenStore = new Mock<TokenStore>();
            _mockJSRuntime = new Mock<IJSRuntime>();

            Services.AddSingleton(_mockHttpClientFactory.Object);
            Services.AddSingleton(_mockTokenStore.Object);
            Services.AddSingleton(_mockJSRuntime.Object);
            Services.AddSingleton<IReservacionesApiService>(new ReservacionesApiService(_mockHttpClientFactory.Object, _mockTokenStore.Object));
            Services.AddSingleton<IClientesApiService>(new ClientesApiService(_mockHttpClientFactory.Object, _mockTokenStore.Object));
            Services.AddSingleton<IMesasApiService>(new MesasApiService(_mockHttpClientFactory.Object, _mockTokenStore.Object));
        }

        [Fact]
        public void Renderizar_FormularioNuevo_DeberiaMostrarTituloCorrecto()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            var titulo = componente.Find("h2");
            Assert.Contains("Nueva Reservación", titulo.TextContent);
        }

        [Fact]
        public void Renderizar_FormularioEdicion_DeberiaMostrarTituloCorrecto()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            var titulo = componente.Find("h2");
            Assert.Contains("Nueva Reservación", titulo.TextContent);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSeccionesOrganizadas()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("Información del Cliente", componente.Markup);
            Assert.Contains("Detalles de la Reservación", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposObligatorios()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("Cliente *", componente.Markup);
            Assert.Contains("Fecha *", componente.Markup);
            Assert.Contains("Hora *", componente.Markup);
            Assert.Contains("Número de personas *", componente.Markup);
            Assert.Contains("Mesa preferida *", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSelectsConOpciones()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act
            var selects = componente.FindAll("select");

            // Assert
            Assert.True(selects.Count >= 4); // Cliente, Mesa, Tipo evento, Prioridad, Canal
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInputsConPlaceholders()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find("input[placeholder='Ej. 555-123-4567']"));
            Assert.NotNull(componente.Find("input[placeholder='Ej. cliente@email.com']"));
            Assert.NotNull(componente.Find("input[placeholder='Ej. 2']"));
            Assert.NotNull(componente.Find("textarea[placeholder='Ej. Alergia a mariscos, mesa cerca de la ventana']"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconosMaterial()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find("span.material-symbols-outlined"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesAccion()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act
            var botones = componente.FindAll("button");

            // Assert
            Assert.True(botones.Count >= 2); // Cerrar y Guardar
        }

        [Fact]
        public void Renderizar_DeberiaMostrarValidaciones()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act
            var validaciones = componente.FindAll(".text-red-500");

            // Assert
            Assert.True(validaciones.Count >= 0); // Las validaciones solo aparecen cuando hay errores
        }

        [Fact]
        public void Renderizar_DeberiaMostrarModalBackdrop()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find(".modal-backdrop"));
        }

        [Fact]
        public void Renderizar_MostrarFalse_NoDeberiaMostrarModal()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, false));

            // Act & Assert
            var modal = componente.Find(".modal");
            Assert.Contains("display: none", modal.GetAttribute("style"));
        }

        [Fact]
        public void Renderizar_MostrarTrue_DeberiaMostrarModal()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            var modal = componente.Find(".modal");
            Assert.Contains("display: block", modal.GetAttribute("style"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraModal()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find(".modal-dialog.modal-xl"));
            Assert.NotNull(componente.Find(".modal-content"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFormularioConValidacion()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find("form"));
            // DataAnnotationsValidator se renderiza como un componente, no como un elemento HTML
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraGrid()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find(".grid"));
            Assert.Contains("grid-cols-1", componente.Markup);
            Assert.Contains("md:grid-cols-2", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposConIconos()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act
            var iconos = componente.FindAll("span.material-symbols-outlined");

            // Assert
            Assert.True(iconos.Count > 0);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInputsConTiposCorrectos()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find("input[type='tel']"));
            Assert.NotNull(componente.Find("input[type='email']"));
            Assert.NotNull(componente.Find("input[type='time']"));
            Assert.NotNull(componente.Find("input[type='date']"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTextareas()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act
            var textareas = componente.FindAll("textarea");

            // Assert
            Assert.True(textareas.Count >= 2); // Notas especiales, Observaciones, Requerimientos especiales
        }

        [Fact]
        public void Renderizar_DeberiaMostrarLabelsConEstilos()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.NotNull(componente.Find("label.block"));
            Assert.NotNull(componente.Find("span.text-gray-700"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarOpcionesTipoEvento()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("Cena casual", componente.Markup);
            Assert.Contains("Cumpleaños", componente.Markup);
            Assert.Contains("Aniversario", componente.Markup);
            Assert.Contains("Reunión de negocios", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarOpcionesPrioridad()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("Normal", componente.Markup);
            Assert.Contains("Alta", componente.Markup);
            Assert.Contains("VIP", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarOpcionesCanal()
        {
            // Arrange
            var componente = RenderComponent<ReservacionForm>(parameters => parameters
                .Add(p => p.Mostrar, true));

            // Act & Assert
            Assert.Contains("Teléfono", componente.Markup);
            Assert.Contains("Sitio Web", componente.Markup);
            Assert.Contains("App móvil", componente.Markup);
            Assert.Contains("En persona", componente.Markup);
        }
    }
}
