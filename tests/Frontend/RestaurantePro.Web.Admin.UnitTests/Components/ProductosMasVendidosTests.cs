using Bunit;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class ProductosMasVendidosTests : TestContext
    {
        [Fact]
        public void RenderizaEstructuraBasica()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new ProductoMasVendidoDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Pizza Margherita",
                    CategoriaNombre = "Pizzas",
                    CantidadVendida = 50,
                    Ingresos = 2500m,
                    PorcentajeTotal = 25.5m
                }
            };

            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Assert
            Assert.Contains("Productos Más Vendidos", componente.Markup);
            Assert.Contains("oi-star", componente.Markup);
        }

        [Fact]
        public void MuestraMensajeCuandoProductosEsNull()
        {
            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, (List<ProductoMasVendidoDto>?)null));

            // Assert
            Assert.Contains("No hay datos disponibles", componente.Markup);
            Assert.Contains("oi-warning", componente.Markup);
        }

        [Fact]
        public void MuestraMensajeCuandoProductosEstaVacio()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>();

            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Assert
            Assert.Contains("No hay datos disponibles", componente.Markup);
            Assert.Contains("oi-warning", componente.Markup);
        }

        [Fact]
        public void RenderizaTablaCuandoHayProductos()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new ProductoMasVendidoDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Pizza Margherita",
                    CategoriaNombre = "Pizzas",
                    CantidadVendida = 50,
                    Ingresos = 2500m,
                    PorcentajeTotal = 25.5m
                }
            };

            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Assert
            Assert.NotNull(componente.Find("table"));
            Assert.Contains("table-responsive", componente.Markup);
            Assert.Contains("table-sm", componente.Markup);
        }

        [Fact]
        public void RenderizaEncabezadosDeTabla()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new ProductoMasVendidoDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    CategoriaNombre = "Test",
                    CantidadVendida = 1,
                    Ingresos = 100m,
                    PorcentajeTotal = 10m
                }
            };

            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Assert
            Assert.Contains("Producto", componente.Markup);
            Assert.Contains("Categoría", componente.Markup);
            Assert.Contains("Cantidad", componente.Markup);
            Assert.Contains("Ingresos", componente.Markup);
            Assert.Contains("%", componente.Markup);
        }

        [Fact]
        public void RenderizaProductoIndividualCorrectamente()
        {
            // Arrange
            var producto = new ProductoMasVendidoDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Pizza Margherita",
                CategoriaNombre = "Pizzas",
                CantidadVendida = 50,
                Ingresos = 2500m,
                PorcentajeTotal = 25.5m
            };

            var productos = new List<ProductoMasVendidoDto> { producto };

            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Assert
            Assert.Contains(producto.Nombre, componente.Markup);
            Assert.Contains(producto.CategoriaNombre, componente.Markup);
            Assert.Contains(producto.CantidadVendida.ToString(), componente.Markup);
            Assert.Contains("$2,500", componente.Markup);
            Assert.Contains("25.5%", componente.Markup);
        }

        [Fact]
        public void RenderizaMultipleProductos()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new ProductoMasVendidoDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Pizza Margherita",
                    CategoriaNombre = "Pizzas",
                    CantidadVendida = 50,
                    Ingresos = 2500m,
                    PorcentajeTotal = 25.5m
                },
                new ProductoMasVendidoDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Hamburguesa Clásica",
                    CategoriaNombre = "Hamburguesas",
                    CantidadVendida = 30,
                    Ingresos = 1500m,
                    PorcentajeTotal = 15.3m
                }
            };

            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Assert
            Assert.Contains("Pizza Margherita", componente.Markup);
            Assert.Contains("Hamburguesa Clásica", componente.Markup);
            Assert.Contains("Pizzas", componente.Markup);
            Assert.Contains("Hamburguesas", componente.Markup);
        }

        [Fact]
        public void FormateaIngresosCorrectamente()
        {
            // Arrange
            var producto = new ProductoMasVendidoDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Test",
                CategoriaNombre = "Test",
                CantidadVendida = 1,
                Ingresos = 1234.56m,
                PorcentajeTotal = 10m
            };

            var productos = new List<ProductoMasVendidoDto> { producto };

            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Assert
            Assert.Contains("$1,235", componente.Markup); // Formato C0 redondea
        }

        [Fact]
        public void RenderizaBadgesCorrectamente()
        {
            // Arrange
            var producto = new ProductoMasVendidoDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Test",
                CategoriaNombre = "Categoría Test",
                CantidadVendida = 25,
                Ingresos = 1000m,
                PorcentajeTotal = 10m
            };

            var productos = new List<ProductoMasVendidoDto> { producto };

            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Assert
            Assert.Contains("badge bg-secondary", componente.Markup);
            Assert.Contains("badge bg-primary", componente.Markup);
            Assert.Contains("Categoría Test", componente.Markup);
            Assert.Contains("25", componente.Markup);
        }

        [Fact]
        public void RenderizaBarraDeProgresoCorrectamente()
        {
            // Arrange
            var producto = new ProductoMasVendidoDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Test",
                CategoriaNombre = "Test",
                CantidadVendida = 1,
                Ingresos = 1000m,
                PorcentajeTotal = 75.5m
            };

            var productos = new List<ProductoMasVendidoDto> { producto };

            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Assert
            Assert.Contains("progress", componente.Markup);
            Assert.Contains("progress-bar bg-success", componente.Markup);
            Assert.Contains("width: 75.5%", componente.Markup);
            Assert.Contains("75.5%", componente.Markup);
        }

        [Fact]
        public void AplicaClasesCssCorrectas()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new ProductoMasVendidoDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    CategoriaNombre = "Test",
                    CantidadVendida = 1,
                    Ingresos = 1000m,
                    PorcentajeTotal = 10m
                }
            };

            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Assert
            Assert.Contains("card", componente.Markup);
            Assert.Contains("card-header", componente.Markup);
            Assert.Contains("card-body", componente.Markup);
            Assert.Contains("text-primary", componente.Markup);
            Assert.Contains("font-weight-bold", componente.Markup);
            Assert.Contains("text-center", componente.Markup);
            Assert.Contains("text-end", componente.Markup);
        }

        [Fact]
        public void RenderizaIconosCorrectos()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new ProductoMasVendidoDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    CategoriaNombre = "Test",
                    CantidadVendida = 1,
                    Ingresos = 1000m,
                    PorcentajeTotal = 10m
                }
            };

            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Assert
            Assert.Contains("oi-star", componente.Markup);
        }

        [Fact]
        public void MuestraIconoDeAdvertenciaCuandoNoHayDatos()
        {
            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, (List<ProductoMasVendidoDto>?)null));

            // Assert
            Assert.Contains("oi-warning", componente.Markup);
        }

        [Fact]
        public void RenderizaConDatosComplejos()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new ProductoMasVendidoDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Pizza Margherita Especial",
                    CategoriaNombre = "Pizzas Premium",
                    CantidadVendida = 100,
                    Ingresos = 5000.99m,
                    PorcentajeTotal = 50.25m
                },
                new ProductoMasVendidoDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Hamburguesa Deluxe",
                    CategoriaNombre = "Hamburguesas Gourmet",
                    CantidadVendida = 75,
                    Ingresos = 3750.50m,
                    PorcentajeTotal = 37.5m
                },
                new ProductoMasVendidoDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Ensalada César",
                    CategoriaNombre = "Ensaladas",
                    CantidadVendida = 25,
                    Ingresos = 500.25m,
                    PorcentajeTotal = 5.0m
                }
            };

            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Assert
            Assert.Contains("Pizza Margherita Especial", componente.Markup);
            Assert.Contains("Hamburguesa Deluxe", componente.Markup);
            Assert.Contains("Ensalada César", componente.Markup);
            Assert.Contains("Pizzas Premium", componente.Markup);
            Assert.Contains("Hamburguesas Gourmet", componente.Markup);
            Assert.Contains("Ensaladas", componente.Markup);
            Assert.Contains("100", componente.Markup);
            Assert.Contains("75", componente.Markup);
            Assert.Contains("25", componente.Markup);
            Assert.Contains("$5,001", componente.Markup);
            Assert.Contains("$3,751", componente.Markup);
            Assert.Contains("$500", componente.Markup);
            Assert.Contains("50.3%", componente.Markup);
            Assert.Contains("37.5%", componente.Markup);
            Assert.Contains("5.0%", componente.Markup);
        }

        [Fact]
        public void ManejaPorcentajesExtremos()
        {
            // Arrange
            var productos = new List<ProductoMasVendidoDto>
            {
                new ProductoMasVendidoDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Producto 100%",
                    CategoriaNombre = "Test",
                    CantidadVendida = 1,
                    Ingresos = 1000m,
                    PorcentajeTotal = 100m
                },
                new ProductoMasVendidoDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Producto 0%",
                    CategoriaNombre = "Test",
                    CantidadVendida = 1,
                    Ingresos = 1000m,
                    PorcentajeTotal = 0m
                }
            };

            // Act
            var componente = RenderComponent<ProductosMasVendidos>(parameters => parameters
                .Add(p => p.Productos, productos));

            // Assert
            Assert.Contains("width: 100%", componente.Markup);
            Assert.Contains("width: 0%", componente.Markup);
            Assert.Contains("100.0%", componente.Markup);
            Assert.Contains("0.0%", componente.Markup);
        }
    }
}