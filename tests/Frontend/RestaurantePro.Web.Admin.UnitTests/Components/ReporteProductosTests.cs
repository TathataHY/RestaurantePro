using Bunit;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class ReporteProductosTests : TestContext
    {
        [Fact]
        public void Renderizar_ConReporteNull_DeberiaMostrarSpinner()
        {
            // Arrange
            var componente = RenderComponent<ReporteProductos>();

            // Act & Assert
            Assert.NotNull(componente.Find(".spinner-border"));
            Assert.Contains("Generando reporte de productos...", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarTitulo()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Reporte de Productos", componente.Markup);
            Assert.Contains("oi-list", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarBotonesExportar()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("PDF", componente.Markup);
            Assert.Contains("Excel", componente.Markup);
            Assert.Contains("oi-document", componente.Markup);
            Assert.Contains("oi-spreadsheet", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarResumenGeneral()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = new DateTime(2024, 1, 1),
                FechaFin = new DateTime(2024, 1, 31),
                TotalIngresos = 50000,
                TotalProductosVendidos = 100
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Total Ingresos", componente.Markup);
            Assert.Contains("Total Productos Vendidos", componente.Markup);
            Assert.Contains("Período", componente.Markup);
            Assert.Contains("$50,000", componente.Markup);
            Assert.Contains("100", componente.Markup);
            Assert.Contains("01/01 - 31/01", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarCardsResumen()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.NotNull(componente.Find(".card.border-left-primary"));
            Assert.NotNull(componente.Find(".card.border-left-success"));
            Assert.NotNull(componente.Find(".card.border-left-info"));
        }

        [Fact]
        public void Renderizar_ConProductosMasVendidos_DeberiaMostrarTabla()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosMasVendidos = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Pizza Margherita",
                        CategoriaNombre = "Pizzas",
                        Precio = 15.99m,
                        CantidadVendida = 50,
                        TotalVentas = 799.50m,
                        PorcentajeDelTotal = 25.5m,
                        VecesPedido = 45
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Productos Más Vendidos", componente.Markup);
            Assert.Contains("oi-arrow-top", componente.Markup);
            Assert.NotNull(componente.Find("table"));
            Assert.Contains("Pizza Margherita", componente.Markup);
            Assert.Contains("Pizzas", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductosMasVendidos_DeberiaMostrarDatosProducto()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosMasVendidos = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Pizza Margherita",
                        CategoriaNombre = "Pizzas",
                        Precio = 15.99m,
                        CantidadVendida = 50,
                        TotalVentas = 799.50m,
                        PorcentajeDelTotal = 25.5m,
                        VecesPedido = 45
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Pizza Margherita", componente.Markup);
            Assert.Contains("Pizzas", componente.Markup);
            Assert.Contains("$16", componente.Markup);
            Assert.Contains("50", componente.Markup);
            Assert.Contains("$800", componente.Markup);
            Assert.Contains("25.5%", componente.Markup);
            Assert.Contains("45", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductosMenosVendidos_DeberiaMostrarTabla()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosMenosVendidos = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Ensalada César",
                        CategoriaNombre = "Ensaladas",
                        Precio = 8.99m,
                        CantidadVendida = 5,
                        TotalVentas = 44.95m,
                        PorcentajeDelTotal = 2.1m,
                        VecesPedido = 3
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Productos Menos Vendidos", componente.Markup);
            Assert.Contains("oi-arrow-bottom", componente.Markup);
            Assert.Contains("Ensalada César", componente.Markup);
            Assert.Contains("Ensaladas", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductosPorCategoria_DeberiaMostrarTabla()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosPorCategoria = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Hamburguesa Clásica",
                        CategoriaNombre = "Hamburguesas",
                        Precio = 12.99m,
                        CantidadVendida = 30,
                        TotalVentas = 389.70m,
                        PorcentajeDelTotal = 15.2m,
                        VecesPedido = 28
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Productos por Categoría", componente.Markup);
            Assert.Contains("oi-tags", componente.Markup);
            Assert.Contains("Hamburguesa Clásica", componente.Markup);
            Assert.Contains("Hamburguesas", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductosMasVendidos_DeberiaMostrarHeadersTabla()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosMasVendidos = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Test Product",
                        CategoriaNombre = "Test Category",
                        Precio = 10.00m,
                        CantidadVendida = 1,
                        TotalVentas = 10.00m,
                        PorcentajeDelTotal = 100.0m,
                        VecesPedido = 1
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("#", componente.Markup);
            Assert.Contains("Producto", componente.Markup);
            Assert.Contains("Categoría", componente.Markup);
            Assert.Contains("Precio", componente.Markup);
            Assert.Contains("Cantidad", componente.Markup);
            Assert.Contains("Total Ventas", componente.Markup);
            Assert.Contains("% del Total", componente.Markup);
            Assert.Contains("Veces Pedido", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductosMasVendidos_DeberiaMostrarBadges()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosMasVendidos = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Test Product",
                        CategoriaNombre = "Test Category",
                        Precio = 10.00m,
                        CantidadVendida = 1,
                        TotalVentas = 10.00m,
                        PorcentajeDelTotal = 100.0m,
                        VecesPedido = 1
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.NotNull(componente.Find(".badge.bg-info"));
            Assert.NotNull(componente.Find(".badge.bg-primary"));
        }

        [Fact]
        public void Renderizar_ConProductosMenosVendidos_DeberiaMostrarBadges()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosMenosVendidos = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Test Product",
                        CategoriaNombre = "Test Category",
                        Precio = 10.00m,
                        CantidadVendida = 1,
                        TotalVentas = 10.00m,
                        PorcentajeDelTotal = 100.0m,
                        VecesPedido = 1
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.NotNull(componente.Find(".badge.bg-warning"));
        }

        [Fact]
        public void Renderizar_ConProductosMasVendidos_DeberiaMostrarProgressBars()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosMasVendidos = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Test Product",
                        CategoriaNombre = "Test Category",
                        Precio = 10.00m,
                        CantidadVendida = 1,
                        TotalVentas = 10.00m,
                        PorcentajeDelTotal = 75.5m,
                        VecesPedido = 1
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.NotNull(componente.Find(".progress"));
            Assert.NotNull(componente.Find(".progress-bar.bg-success"));
            Assert.Contains("75.5%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductosMenosVendidos_DeberiaMostrarProgressBars()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosMenosVendidos = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Test Product",
                        CategoriaNombre = "Test Category",
                        Precio = 10.00m,
                        CantidadVendida = 1,
                        TotalVentas = 10.00m,
                        PorcentajeDelTotal = 25.3m,
                        VecesPedido = 1
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.NotNull(componente.Find(".progress-bar.bg-warning"));
            Assert.Contains("25.3%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductosPorCategoria_DeberiaMostrarProgressBars()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosPorCategoria = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Test Product",
                        CategoriaNombre = "Test Category",
                        Precio = 10.00m,
                        CantidadVendida = 1,
                        TotalVentas = 10.00m,
                        PorcentajeDelTotal = 50.0m,
                        VecesPedido = 1
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.NotNull(componente.Find(".progress-bar.bg-info"));
            Assert.Contains("50.0%", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarEstructuraBasica()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
            Assert.NotNull(componente.Find(".row"));
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarClasesCSS()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
            Assert.NotNull(componente.Find(".row"));
            Assert.NotNull(componente.Find(".col-md-4"));
        }

        [Fact]
        public void Renderizar_ConProductosMasVendidos_DeberiaMostrarTablaResponsive()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosMasVendidos = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Test Product",
                        CategoriaNombre = "Test Category",
                        Precio = 10.00m,
                        CantidadVendida = 1,
                        TotalVentas = 10.00m,
                        PorcentajeDelTotal = 100.0m,
                        VecesPedido = 1
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.NotNull(componente.Find(".table-responsive"));
            Assert.NotNull(componente.Find(".table"));
            Assert.NotNull(componente.Find(".table-bordered"));
            Assert.NotNull(componente.Find(".table-hover"));
            Assert.NotNull(componente.Find(".thead-light"));
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarIconos()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("oi-list", componente.Markup);
            Assert.Contains("oi-document", componente.Markup);
            Assert.Contains("oi-spreadsheet", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductosMasVendidos_DeberiaMostrarIconosEspecificos()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosMasVendidos = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Test Product",
                        CategoriaNombre = "Test Category",
                        Precio = 10.00m,
                        CantidadVendida = 1,
                        TotalVentas = 10.00m,
                        PorcentajeDelTotal = 100.0m,
                        VecesPedido = 1
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("oi-arrow-top", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductosMenosVendidos_DeberiaMostrarIconosEspecificos()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosMenosVendidos = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Test Product",
                        CategoriaNombre = "Test Category",
                        Precio = 10.00m,
                        CantidadVendida = 1,
                        TotalVentas = 10.00m,
                        PorcentajeDelTotal = 100.0m,
                        VecesPedido = 1
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("oi-arrow-bottom", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConProductosPorCategoria_DeberiaMostrarIconosEspecificos()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100,
                ProductosPorCategoria = new List<ProductoVendidoDto>
                {
                    new ProductoVendidoDto
                    {
                        ProductoId = Guid.NewGuid(),
                        ProductoNombre = "Test Product",
                        CategoriaNombre = "Test Category",
                        Precio = 10.00m,
                        CantidadVendida = 1,
                        TotalVentas = 10.00m,
                        PorcentajeDelTotal = 100.0m,
                        VecesPedido = 1
                    }
                }
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("oi-tags", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarTextosDescriptivos()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Reporte de Productos", componente.Markup);
            Assert.Contains("Total Ingresos", componente.Markup);
            Assert.Contains("Total Productos Vendidos", componente.Markup);
            Assert.Contains("Período", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarEstructuraCompleta()
        {
            // Arrange
            var reporte = new ReporteProductosDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalIngresos = 50000,
                TotalProductosVendidos = 100
            };

            var componente = RenderComponent<ReporteProductos>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            // Verificar estructura principal
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
            
            // Verificar resumen
            Assert.NotNull(componente.Find(".row"));
            var columnas = componente.FindAll(".col-md-4");
            Assert.Equal(3, columnas.Count);
            
            // Verificar cards de resumen
            Assert.NotNull(componente.Find(".card.border-left-primary"));
            Assert.NotNull(componente.Find(".card.border-left-success"));
            Assert.NotNull(componente.Find(".card.border-left-info"));
        }
    }
}
