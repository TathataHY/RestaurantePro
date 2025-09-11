using Bunit;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class ProductosMasVendidosTests : TestContext
    {
        [Fact]
        public void Renderizar_ConListaVacia_DeberiaMostrarMensajeSinDatos()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>();

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.Contains("No hay datos disponibles", componente.Markup);
            Assert.Contains("oi-warning", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductosNull_DeberiaMostrarMensajeSinDatos()
        {
            // Arrange
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, (List<ProductoMasVendidoDto>?)null));

            // Act & Assert
            Assert.Contains("No hay datos disponibles", componente.Markup);
            Assert.Contains("oi-warning", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaMostrarTabla()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Pizza Margherita",
                    CategoriaNombre = "Pizzas",
                    CantidadVendida = 50,
                    Ingresos = 1250.00m,
                    PorcentajeTotal = 25.5m
                }
            };

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.NotNull(componente.Find("table"));
            Assert.NotNull(componente.Find("thead"));
            Assert.NotNull(componente.Find("tbody"));
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaMostrarEncabezados()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    CategoriaNombre = "Test",
                    CantidadVendida = 1,
                    Ingresos = 10m,
                    PorcentajeTotal = 10m
                }
            };

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.Contains("Producto", componente.Markup);
            Assert.Contains("Categoría", componente.Markup);
            Assert.Contains("Cantidad", componente.Markup);
            Assert.Contains("Ingresos", componente.Markup);
            Assert.Contains("%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaMostrarDatosProducto()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Pizza Margherita",
                    CategoriaNombre = "Pizzas",
                    CantidadVendida = 50,
                    Ingresos = 1250.00m,
                    PorcentajeTotal = 25.5m
                }
            };

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.Contains("Pizza Margherita", componente.Markup);
            Assert.Contains("Pizzas", componente.Markup);
            Assert.Contains("50", componente.Markup);
            Assert.Contains("$1,250", componente.Markup);
            Assert.Contains("25.5%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaMostrarBadges()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    CategoriaNombre = "Categoría Test",
                    CantidadVendida = 25,
                    Ingresos = 500m,
                    PorcentajeTotal = 15.5m
                }
            };

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.Contains("badge bg-secondary", componente.Markup);
            Assert.Contains("badge bg-primary", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaMostrarBarraProgreso()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    CategoriaNombre = "Test",
                    CantidadVendida = 1,
                    Ingresos = 10m,
                    PorcentajeTotal = 75.5m
                }
            };

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.Contains("progress", componente.Markup);
            Assert.Contains("progress-bar", componente.Markup);
            Assert.Contains("width: 75.5%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaMostrarTitulo()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>();

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.Contains("Productos Más Vendidos", componente.Markup);
            Assert.Contains("oi-star", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaMostrarEstructuraCard()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>();

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaMostrarTablaResponsive()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    CategoriaNombre = "Test",
                    CantidadVendida = 1,
                    Ingresos = 10m,
                    PorcentajeTotal = 10m
                }
            };

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.NotNull(componente.Find(".table-responsive"));
            Assert.NotNull(componente.Find(".table"));
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaMostrarClasesCorrectas()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    CategoriaNombre = "Test",
                    CantidadVendida = 1,
                    Ingresos = 10m,
                    PorcentajeTotal = 10m
                }
            };

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.Contains("table-sm", componente.Markup);
            Assert.Contains("text-center", componente.Markup);
            Assert.Contains("text-end", componente.Markup);
            Assert.Contains("bg-success", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaFormatearIngresosCorrectamente()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    CategoriaNombre = "Test",
                    CantidadVendida = 1,
                    Ingresos = 1234.56m,
                    PorcentajeTotal = 10m
                }
            };

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.Contains("$1,235", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaFormatearPorcentajeCorrectamente()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    CategoriaNombre = "Test",
                    CantidadVendida = 1,
                    Ingresos = 10m,
                    PorcentajeTotal = 33.333m
                }
            };

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.Contains("33.3%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaMostrarTodosLosProductos()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Producto 1",
                    CategoriaNombre = "Categoría 1",
                    CantidadVendida = 10,
                    Ingresos = 100m,
                    PorcentajeTotal = 50m
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Producto 2",
                    CategoriaNombre = "Categoría 2",
                    CantidadVendida = 5,
                    Ingresos = 50m,
                    PorcentajeTotal = 25m
                }
            };

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.Contains("Producto 1", componente.Markup);
            Assert.Contains("Producto 2", componente.Markup);
            Assert.Contains("Categoría 1", componente.Markup);
            Assert.Contains("Categoría 2", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaMostrarValoresNumericos()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    CategoriaNombre = "Test",
                    CantidadVendida = 42,
                    Ingresos = 999.99m,
                    PorcentajeTotal = 99.9m
                }
            };

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            Assert.Contains("42", componente.Markup);
            Assert.Contains("$1,000", componente.Markup);
            Assert.Contains("99.9%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductos_DeberiaMostrarEstructuraCompleta()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    CategoriaNombre = "Test",
                    CantidadVendida = 1,
                    Ingresos = 10m,
                    PorcentajeTotal = 10m
                }
            };

            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Act & Assert
            var card = componente.Find(".card");
            Assert.NotNull(card);

            var cardHeader = componente.Find(".card-header");
            Assert.NotNull(cardHeader);

            var cardBody = componente.Find(".card-body");
            Assert.NotNull(cardBody);

            var table = componente.Find("table");
            Assert.NotNull(table);

            var thead = componente.Find("thead");
            Assert.NotNull(thead);

            var tbody = componente.Find("tbody");
            Assert.NotNull(tbody);
        }
    }
}
