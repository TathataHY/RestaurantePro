using Bunit;
using Microsoft.Extensions.DependencyInjection;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class InventarioListTests : TestContext
    {
        [Fact]
        public void Renderizar_ConIngredientesNull_DeberiaMostrarMensajeVacio()
        {
            // Arrange
            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, (List<IngredienteDto>?)null));

            // Act & Assert
            Assert.Contains("No hay ingredientes", componente.Markup);
            Assert.Contains("No se encontraron ingredientes con los filtros aplicados", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConListaVacia_DeberiaMostrarMensajeVacio()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>();

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("No hay ingredientes", componente.Markup);
            Assert.Contains("No se encontraron ingredientes con los filtros aplicados", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConIngredientes_DeberiaMostrarTabla()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Ingrediente Test",
                    Categoria = "Vegetales",
                    StockActual = 10,
                    StockMinimo = 5,
                    StockMaximo = 20,
                    UnidadMedida = UnidadMedida.Kilogramo,
                    CostoUnitario = 15.50m,
                    FechaVencimiento = DateTime.Now.AddDays(30),
                    Proveedor = "Proveedor Test",
                    EstaActivo = true
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.NotNull(componente.Find("table"));
            Assert.NotNull(componente.Find("thead"));
            Assert.NotNull(componente.Find("tbody"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEncabezadosTabla()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test" }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("Ingrediente", componente.Markup);
            Assert.Contains("Categoría", componente.Markup);
            Assert.Contains("Stock", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
            Assert.Contains("Costo", componente.Markup);
            Assert.Contains("Vencimiento", componente.Markup);
            Assert.Contains("Proveedor", componente.Markup);
            Assert.Contains("Acciones", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInformacionIngrediente()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Ingrediente Test",
                    Descripcion = "Descripción del ingrediente",
                    Categoria = "Vegetales",
                    StockActual = 10.5m,
                    StockMinimo = 5.0m,
                    StockMaximo = 20.0m,
                    UnidadMedida = UnidadMedida.Kilogramo,
                    CostoUnitario = 15.50m,
                    FechaVencimiento = new DateTime(2024, 12, 31),
                    Proveedor = "Proveedor Test",
                    EstaActivo = true
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("Ingrediente Test", componente.Markup);
            Assert.Contains("Descripción del ingrediente", componente.Markup);
            Assert.Contains("Vegetales", componente.Markup);
            Assert.Contains("10.50", componente.Markup);
            Assert.Contains("5.00", componente.Markup);
            Assert.Contains("20.00", componente.Markup);
            Assert.Contains("kg", componente.Markup);
            Assert.Contains("$15.50", componente.Markup);
            Assert.Contains("31/12/2024", componente.Markup);
            Assert.Contains("Proveedor Test", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCategoria()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Categoria = "Vegetales"
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("Vegetales", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSinCategoria()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Categoria = ""
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("-", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInformacionStock()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    StockActual = 15.75m,
                    StockMinimo = 5.0m,
                    StockMaximo = 25.0m,
                    UnidadMedida = UnidadMedida.Kilogramo
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("15.75", componente.Markup);
            Assert.Contains("5.00", componente.Markup);
            Assert.Contains("25.00", componente.Markup);
            Assert.Contains("kg", componente.Markup);
            Assert.Contains("Actual:", componente.Markup);
            Assert.Contains("Min:", componente.Markup);
            Assert.Contains("Max:", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstadoStockBajo()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    StockActual = 3.0m,
                    StockMinimo = 5.0m,
                    StockMaximo = 20.0m
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("Stock Bajo", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstadoStockAlto()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    StockActual = 25.0m,
                    StockMinimo = 5.0m,
                    StockMaximo = 20.0m
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("Stock Alto", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstadoNormal()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    StockActual = 10.0m,
                    StockMinimo = 5.0m,
                    StockMaximo = 20.0m
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("Normal", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarVencePronto()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    FechaVencimiento = DateTime.Now.AddDays(3)
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("Vence Pronto", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarVencido()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    FechaVencimiento = DateTime.Now.AddDays(-1)
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("Vencido", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInactivo()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    EstaActivo = false
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("Inactivo", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInformacionCosto()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    StockActual = 10.0m,
                    CostoUnitario = 15.50m
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("$15.50", componente.Markup);
            Assert.Contains("$155.00", componente.Markup); // Total: 10 * 15.50
            Assert.Contains("Total:", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFechaVencimiento()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    FechaVencimiento = new DateTime(2024, 12, 31)
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("31/12/2024", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSinFechaVencimiento()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    FechaVencimiento = DateTime.MinValue
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("-", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarProveedor()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Proveedor = "Proveedor Test"
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("Proveedor Test", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSinProveedor()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Proveedor = ""
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("-", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesAccion()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test"
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act
            var botones = componente.FindAll("button");

            // Assert
            Assert.True(botones.Count >= 4); // Editar, Ver detalles, Crear movimiento, Eliminar
            Assert.Contains("Editar", componente.Markup);
            Assert.Contains("Ver detalles", componente.Markup);
            Assert.Contains("Crear movimiento", componente.Markup);
            Assert.Contains("Eliminar", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConPaginacion_DeberiaMostrarPaginacion()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test" }
            };

            var paginacion = new PaginatedList<IngredienteDto>
            {
                Items = ingredientes,
                PageSize = 10,
                PageNumber = 1,
                TotalPages = 5,
                TotalCount = 50
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes)
                .Add(p => p.Paginacion, paginacion));

            // Act & Assert
            Assert.NotNull(componente.Find(".pagination"));
            Assert.Contains("Mostrando 1 a", componente.Markup);
            Assert.Contains("10", componente.Markup);
            Assert.Contains("de 50 ingredientes", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconos()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test"
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.NotNull(componente.Find("span.oi-pencil"));
            Assert.NotNull(componente.Find("span.oi-eye"));
            Assert.NotNull(componente.Find("span.oi-plus"));
            Assert.NotNull(componente.Find("span.oi-trash"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTablaResponsive()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test" }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.NotNull(componente.Find(".table-responsive"));
            Assert.NotNull(componente.Find(".table.table-hover"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarGrupoBotones()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test"
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.NotNull(componente.Find(".btn-group"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTitulosBotones()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test"
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("title=\"Editar\"", componente.Markup);
            Assert.Contains("title=\"Ver detalles\"", componente.Markup);
            Assert.Contains("title=\"Crear movimiento\"", componente.Markup);
            Assert.Contains("title=\"Eliminar\"", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBadges()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    Categoria = "Vegetales",
                    StockActual = 3.0m,
                    StockMinimo = 5.0m,
                    StockMaximo = 20.0m
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act
            var badges = componente.FindAll(".badge");

            // Assert
            Assert.True(badges.Count >= 2); // Categoría y Estado
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInformacionStockDetallada()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    StockActual = 12.75m,
                    StockMinimo = 8.0m,
                    StockMaximo = 30.0m,
                    UnidadMedida = UnidadMedida.Kilogramo
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("12.75", componente.Markup);
            Assert.Contains("8.00", componente.Markup);
            Assert.Contains("30.00", componente.Markup);
            Assert.Contains("kg", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarDiasVencimiento()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    Nombre = "Test",
                    FechaVencimiento = DateTime.Now.AddDays(5)
                }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.Contains("Vence en", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraTabla()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new() { Id = Guid.NewGuid(), Nombre = "Test" }
            };

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.NotNull(componente.Find("thead.table-dark"));
            Assert.NotNull(componente.Find("tbody"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconoVacio()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>();

            var componente = RenderComponent<InventarioList>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Act & Assert
            Assert.NotNull(componente.Find(".oi-box"));
        }
    }
}
