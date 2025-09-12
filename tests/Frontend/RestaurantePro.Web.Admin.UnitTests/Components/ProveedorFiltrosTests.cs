using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class ProveedorFiltrosTests : TestContext
    {
        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraBasica()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
            Assert.Contains("Filtros de Búsqueda", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTitulo()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Filtros de Búsqueda", componente.Markup);
            Assert.NotNull(componente.Find("span.oi-magnifying-glass"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonToggle()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find("button"));
            Assert.Contains("Ocultar Filtros", componente.Markup);
            Assert.NotNull(componente.Find("span.oi-chevron-up"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFormulario()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find("form"));
            Assert.NotNull(componente.FindComponent<EditForm>());
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposBasicos()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Nombre", componente.Markup);
            Assert.Contains("RUC", componente.Markup);
            Assert.Contains("Ciudad", componente.Markup);
            Assert.Contains("País", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposFecha()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Fecha Creación Desde", componente.Markup);
            Assert.Contains("Fecha Creación Hasta", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposMonto()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Monto Mínimo Compras", componente.Markup);
            Assert.Contains("Monto Máximo Compras", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCheckboxes()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Solo con contactos", componente.Markup);
            Assert.Contains("Solo con órdenes de compra", componente.Markup);
            Assert.NotNull(componente.FindAll("input[type=\"checkbox\"]"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarOpcionesEstado()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Todos", componente.Markup);
            Assert.Contains("Activos", componente.Markup);
            Assert.Contains("Inactivos", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarPlaceholders()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("placeholder=\"Buscar por nombre\"", componente.Markup);
            Assert.Contains("placeholder=\"Buscar por RUC\"", componente.Markup);
            Assert.Contains("placeholder=\"Buscar por ciudad\"", componente.Markup);
            Assert.Contains("placeholder=\"Buscar por país\"", componente.Markup);
            Assert.Contains("placeholder=\"0.00\"", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotones()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Aplicar Filtros", componente.Markup);
            Assert.Contains("Limpiar", componente.Markup);
            Assert.NotNull(componente.Find("button[type=\"submit\"]"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFiltrosRapidos()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Filtros Rápidos", componente.Markup);
            Assert.Contains("Activos", componente.Markup);
            Assert.Contains("Inactivos", componente.Markup);
            Assert.Contains("Con Contactos", componente.Markup);
            Assert.Contains("Con Órdenes", componente.Markup);
            Assert.Contains("Sin Contactos", componente.Markup);
            Assert.Contains("Sin Órdenes", componente.Markup);
            Assert.Contains("Creados Hoy", componente.Markup);
            Assert.Contains("Esta Semana", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconos()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find("span.oi-magnifying-glass"));
            Assert.NotNull(componente.Find("span.oi-chevron-up"));
            Assert.NotNull(componente.Find("span.oi-reload"));
            Assert.NotNull(componente.Find("span.oi-check"));
            Assert.NotNull(componente.Find("span.oi-x"));
            Assert.NotNull(componente.Find("span.oi-people"));
            Assert.NotNull(componente.Find("span.oi-cart"));
            Assert.NotNull(componente.Find("span.oi-warning"));
            Assert.NotNull(componente.Find("span.oi-calendar"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarClasesBootstrap()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
            Assert.NotNull(componente.Find(".form-control"));
            Assert.NotNull(componente.Find(".form-select"));
            Assert.NotNull(componente.Find(".btn"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraGrid()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.FindAll(".row"));
            Assert.NotNull(componente.FindAll(".col-md-4"));
            Assert.NotNull(componente.FindAll(".col-md-6"));
            Assert.NotNull(componente.FindAll(".col-12"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarLabels()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.FindAll(".form-label"));
            Assert.True(componente.FindAll(".form-label").Count >= 8);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInputs()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.FindAll("input"));
            Assert.NotNull(componente.FindAll("select"));
            Assert.True(componente.FindAll("input").Count >= 6);
            Assert.True(componente.FindAll("select").Count >= 1);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFormCheck()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.FindAll(".form-check"));
            Assert.NotNull(componente.FindAll(".form-check-input"));
            Assert.NotNull(componente.FindAll(".form-check-label"));
            Assert.True(componente.FindAll(".form-check").Count >= 2);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarGap()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find(".gap-2"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFlexWrap()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find(".flex-wrap"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarDGrid()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find(".d-flex"));
            Assert.NotNull(componente.Find(".justify-content-between"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTextMuted()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find(".text-muted"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraCompleta()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            var card = componente.Find(".card");
            Assert.NotNull(card);

            var cardHeader = componente.Find(".card-header");
            Assert.NotNull(cardHeader);

            var cardBody = componente.Find(".card-body");
            Assert.NotNull(cardBody);

            var form = componente.Find("form");
            Assert.NotNull(form);

            var rows = componente.FindAll(".row");
            Assert.True(rows.Count >= 3);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarValoresPorDefecto()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto();

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("value=\"\"", componente.Markup);
            Assert.Contains("value=\"true\"", componente.Markup);
            Assert.Contains("value=\"false\"", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFormularioCompleto()
        {
            // Arrange
            var filtros = new ProveedorFiltrosDto
            {
                Nombre = "Test",
                Ruc = "12345678901",
                Ciudad = "Lima",
                Pais = "Perú",
                EstaActivo = true,
                FechaCreacionDesde = DateTime.Today,
                FechaCreacionHasta = DateTime.Today.AddDays(1),
                MontoMinimoCompras = 100.50m,
                MontoMaximoCompras = 1000.00m,
                TieneContactos = true,
                TieneOrdenesCompra = false
            };

            var componente = RenderComponent<ProveedorFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Filtros de Búsqueda", componente.Markup);
            Assert.Contains("Nombre", componente.Markup);
            Assert.Contains("RUC", componente.Markup);
            Assert.Contains("Ciudad", componente.Markup);
            Assert.Contains("País", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
            Assert.Contains("Fecha Creación Desde", componente.Markup);
            Assert.Contains("Fecha Creación Hasta", componente.Markup);
            Assert.Contains("Monto Mínimo Compras", componente.Markup);
            Assert.Contains("Monto Máximo Compras", componente.Markup);
            Assert.Contains("Solo con contactos", componente.Markup);
            Assert.Contains("Solo con órdenes de compra", componente.Markup);
            Assert.Contains("Filtros Rápidos", componente.Markup);
        }
    }
}
