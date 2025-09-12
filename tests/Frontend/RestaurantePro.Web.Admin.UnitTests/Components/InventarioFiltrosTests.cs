using Bunit;
using Microsoft.AspNetCore.Components;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class InventarioFiltrosTests : TestContext
    {
        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraBasica()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.Contains("Filtros de Búsqueda", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTitulo()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Filtros de Búsqueda", componente.Markup);
            Assert.NotNull(componente.Find("span.oi-magnifying-glass"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonToggle()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find("button"));
            Assert.Contains("Ocultar Filtros", componente.Markup);
            Assert.NotNull(componente.Find("span.oi-chevron-up"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFormularioFiltros()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find("form"));
            Assert.NotNull(componente.Find(".card-body"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposBasicos()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Nombre", componente.Markup);
            Assert.Contains("Categoría", componente.Markup);
            Assert.Contains("Proveedor", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposFecha()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Fecha Vencimiento Desde", componente.Markup);
            Assert.Contains("Fecha Vencimiento Hasta", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposCosto()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Costo Desde", componente.Markup);
            Assert.Contains("Costo Hasta", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCheckboxes()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Solo Stock Bajo", componente.Markup);
            Assert.Contains("Vencimiento Proximo", componente.Markup);
            Assert.NotNull(componente.FindAll("input[type=\"checkbox\"]"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotones()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Aplicar Filtros", componente.Markup);
            Assert.Contains("Limpiar", componente.Markup);
            Assert.Contains("Filtros Rápidos", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconos()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find("span.oi-magnifying-glass"));
            Assert.NotNull(componente.Find("span.oi-chevron-up"));
            Assert.NotNull(componente.Find("span.oi-warning"));
            Assert.NotNull(componente.Find("span.oi-clock"));
            Assert.NotNull(componente.Find("span.oi-x"));
            Assert.NotNull(componente.Find("span.oi-lightning"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarOpcionesEstado()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
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
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("placeholder=\"Buscar por nombre\"", componente.Markup);
            Assert.Contains("placeholder=\"Filtrar por categoría\"", componente.Markup);
            Assert.Contains("placeholder=\"Filtrar por proveedor\"", componente.Markup);
            Assert.Contains("placeholder=\"0.00\"", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarAtributosInput()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find("input[step=\"0.01\"]"));
            Assert.NotNull(componente.Find("input[min=\"0\"]"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarClasesBootstrap()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
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
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.FindAll(".row"));
            Assert.NotNull(componente.FindAll(".col-md-3"));
            Assert.NotNull(componente.FindAll(".col-md-12"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarLabels()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.FindAll(".form-label"));
            Assert.True(componente.FindAll(".form-label").Count >= 8);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInputs()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
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
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.FindAll(".form-check"));
            Assert.NotNull(componente.FindAll(".form-check-inline"));
            Assert.True(componente.FindAll(".form-check").Count >= 2);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarColoresTexto()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("text-danger", componente.Markup);
            Assert.Contains("text-warning", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarGap()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.NotNull(componente.Find(".gap-2"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraCompleta()
        {
            // Arrange
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
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
            var filtros = new InventarioFiltrosDto();

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
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
            var filtros = new InventarioFiltrosDto
            {
                Nombre = "Test",
                Categoria = "Categoría Test",
                Proveedor = "Proveedor Test",
                EstaActivo = true,
                StockBajo = true,
                VencimientoProximo = false
            };

            var componente = RenderComponent<InventarioFiltros>(parameters => parameters
                .Add(p => p.Filtros, filtros));

            // Act & Assert
            Assert.Contains("Filtros de Búsqueda", componente.Markup);
            Assert.Contains("Nombre", componente.Markup);
            Assert.Contains("Categoría", componente.Markup);
            Assert.Contains("Proveedor", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
            Assert.Contains("Fecha Vencimiento Desde", componente.Markup);
            Assert.Contains("Fecha Vencimiento Hasta", componente.Markup);
            Assert.Contains("Costo Desde", componente.Markup);
            Assert.Contains("Costo Hasta", componente.Markup);
            Assert.Contains("Solo Stock Bajo", componente.Markup);
            Assert.Contains("Vencimiento Proximo", componente.Markup);
        }
    }
}
