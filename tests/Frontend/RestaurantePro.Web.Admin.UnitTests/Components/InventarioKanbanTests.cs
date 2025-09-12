using Bunit;
using Microsoft.AspNetCore.Components;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class InventarioKanbanTests : TestContext
    {
        [Fact]
        public void MuestraMensajeCuandoIngredientesEsNull()
        {
            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, (List<IngredienteDto>?)null));

            // Assert
            Assert.Contains("No hay ingredientes", componente.Markup);
            Assert.Contains("oi-box", componente.Markup);
        }

        [Fact]
        public void MuestraMensajeCuandoIngredientesEstaVacio()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>();

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Assert
            Assert.Contains("No hay ingredientes", componente.Markup);
            Assert.Contains("oi-box", componente.Markup);
        }

        [Fact]
        public void RenderizaColumnasKanban()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new IngredienteDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Tomate",
                    StockActual = 5,
                    StockMinimo = 10,
                    StockMaximo = 50,
                    EstaActivo = true
                }
            };

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Assert
            Assert.Contains("Stock Bajo", componente.Markup);
            Assert.Contains("Vence Pronto", componente.Markup);
            Assert.Contains("Stock Normal", componente.Markup);
            Assert.Contains("Inactivos", componente.Markup);
        }

        [Fact]
        public void RenderizaIngredienteEnStockBajo()
        {
            // Arrange
            var ingrediente = new IngredienteDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Tomate",
                StockActual = 5,
                StockMinimo = 10,
                StockMaximo = 50,
                UnidadMedida = UnidadMedida.Kilogramo,
                CostoUnitario = 2.50m,
                EstaActivo = true
            };

            var ingredientes = new List<IngredienteDto> { ingrediente };

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Assert
            Assert.Contains("Tomate", componente.Markup);
            Assert.Contains("5.00 kg", componente.Markup);
            Assert.Contains("10.00", componente.Markup);
            Assert.Contains("$2.50", componente.Markup);
            Assert.Contains("bg-danger", componente.Markup);
        }

        [Fact]
        public void RenderizaIngredienteEnVencePronto()
        {
            // Arrange
            var ingrediente = new IngredienteDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Lechuga",
                StockActual = 15,
                StockMinimo = 5,
                StockMaximo = 30,
                FechaVencimiento = DateTime.Now.AddDays(3),
                UnidadMedida = UnidadMedida.Unidad,
                EstaActivo = true
            };

            var ingredientes = new List<IngredienteDto> { ingrediente };

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Assert
            Assert.Contains("Lechuga", componente.Markup);
            Assert.Contains("bg-warning", componente.Markup);
        }

        [Fact]
        public void RenderizaIngredienteEnStockNormal()
        {
            // Arrange
            var ingrediente = new IngredienteDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Cebolla",
                StockActual = 25,
                StockMinimo = 10,
                StockMaximo = 40,
                UnidadMedida = UnidadMedida.Kilogramo,
                CostoUnitario = 1.80m,
                EstaActivo = true
            };

            var ingredientes = new List<IngredienteDto> { ingrediente };

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Assert
            Assert.Contains("Cebolla", componente.Markup);
            Assert.Contains("25.00 kg", componente.Markup);
            Assert.Contains("10.00", componente.Markup);
            Assert.Contains("40.00", componente.Markup);
            Assert.Contains("$1.80", componente.Markup);
            Assert.Contains("bg-success", componente.Markup);
        }

        [Fact]
        public void RenderizaIngredienteInactivo()
        {
            // Arrange
            var ingrediente = new IngredienteDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Ingrediente Viejo",
                StockActual = 0,
                StockMinimo = 5,
                StockMaximo = 20,
                UnidadMedida = UnidadMedida.Unidad,
                CostoUnitario = 5.00m,
                EstaActivo = false
            };

            var ingredientes = new List<IngredienteDto> { ingrediente };

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Assert
            Assert.Contains("Ingrediente Viejo", componente.Markup);
            Assert.Contains("Inactivo", componente.Markup);
            Assert.Contains("bg-secondary", componente.Markup);
        }

        [Fact]
        public void CuentaCorrectamenteIngredientesPorCategoria()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                // Stock bajo
                new IngredienteDto { Id = Guid.NewGuid(), Nombre = "Tomate", StockActual = 5, StockMinimo = 10, EstaActivo = true },
                new IngredienteDto { Id = Guid.NewGuid(), Nombre = "Papa", StockActual = 3, StockMinimo = 8, EstaActivo = true },
                
                // Vence pronto
                new IngredienteDto { Id = Guid.NewGuid(), Nombre = "Lechuga", StockActual = 15, StockMinimo = 5, FechaVencimiento = DateTime.Now.AddDays(2), EstaActivo = true },
                
                // Stock normal
                new IngredienteDto { Id = Guid.NewGuid(), Nombre = "Cebolla", StockActual = 25, StockMinimo = 10, StockMaximo = 40, EstaActivo = true },
                
                // Inactivo
                new IngredienteDto { Id = Guid.NewGuid(), Nombre = "Viejo", StockActual = 0, EstaActivo = false }
            };

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Assert
            Assert.Contains("Stock Bajo", componente.Markup);
            Assert.Contains("Vence Pronto", componente.Markup);
            Assert.Contains("Stock Normal", componente.Markup);
            Assert.Contains("Inactivos", componente.Markup);
        }

        [Fact]
        public void RenderizaBotonesDeAccion()
        {
            // Arrange
            var ingrediente = new IngredienteDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Test",
                StockActual = 5, // Stock bajo para que aparezca en la primera columna
                StockMinimo = 10,
                EstaActivo = true
            };

            var ingredientes = new List<IngredienteDto> { ingrediente };

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Assert
            Assert.Contains("oi-pencil", componente.Markup);
            Assert.Contains("oi-eye", componente.Markup);
            Assert.Contains("oi-plus", componente.Markup);
        }

        [Fact]
        public void InvocaOnEditarAlHacerClicEnBotonEditar()
        {
            // Arrange
            var ingrediente = new IngredienteDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Test",
                StockActual = 5, // Stock bajo para que aparezca en la primera columna
                StockMinimo = 10,
                EstaActivo = true
            };

            var ingredientes = new List<IngredienteDto> { ingrediente };
            var onEditarInvocado = false;
            IngredienteDto? ingredienteEditado = null;

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes)
                .Add(p => p.OnEditar, EventCallback.Factory.Create<IngredienteDto>(this, (ing) =>
                {
                    onEditarInvocado = true;
                    ingredienteEditado = ing;
                })));

            var botonEditar = componente.Find("button[title='Editar']");
            botonEditar.Click();

            // Assert
            Assert.True(onEditarInvocado);
            Assert.Equal(ingrediente, ingredienteEditado);
        }

        [Fact]
        public void InvocaOnVerDetallesAlHacerClicEnBotonVerDetalles()
        {
            // Arrange
            var ingrediente = new IngredienteDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Test",
                StockActual = 5, // Stock bajo para que aparezca en la primera columna
                StockMinimo = 10,
                EstaActivo = true
            };

            var ingredientes = new List<IngredienteDto> { ingrediente };
            var onVerDetallesInvocado = false;
            IngredienteDto? ingredienteDetallado = null;

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes)
                .Add(p => p.OnVerDetalles, EventCallback.Factory.Create<IngredienteDto>(this, (ing) =>
                {
                    onVerDetallesInvocado = true;
                    ingredienteDetallado = ing;
                })));

            var botonVerDetalles = componente.Find("button[title='Ver detalles']");
            botonVerDetalles.Click();

            // Assert
            Assert.True(onVerDetallesInvocado);
            Assert.Equal(ingrediente, ingredienteDetallado);
        }

        [Fact]
        public void InvocaOnCrearMovimientoAlHacerClicEnBotonCrearMovimiento()
        {
            // Arrange
            var ingrediente = new IngredienteDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Test",
                StockActual = 5, // Stock bajo para que aparezca en la primera columna
                StockMinimo = 10,
                EstaActivo = true
            };

            var ingredientes = new List<IngredienteDto> { ingrediente };
            var onCrearMovimientoInvocado = false;
            IngredienteDto? ingredienteMovimiento = null;

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes)
                .Add(p => p.OnCrearMovimiento, EventCallback.Factory.Create<IngredienteDto>(this, (ing) =>
                {
                    onCrearMovimientoInvocado = true;
                    ingredienteMovimiento = ing;
                })));

            var botonCrearMovimiento = componente.Find("button[title='Crear movimiento']");
            botonCrearMovimiento.Click();

            // Assert
            Assert.True(onCrearMovimientoInvocado);
            Assert.Equal(ingrediente, ingredienteMovimiento);
        }


        [Fact]
        public void AplicaClasesCssCorrectas()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new IngredienteDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    StockActual = 5,
                    StockMinimo = 10,
                    EstaActivo = true
                }
            };

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Assert
            Assert.Contains("row", componente.Markup);
            Assert.Contains("col-md-3", componente.Markup);
            Assert.Contains("card", componente.Markup);
            Assert.Contains("card-header", componente.Markup);
            Assert.Contains("card-body", componente.Markup);
            Assert.Contains("btn-group", componente.Markup);
        }

        [Fact]
        public void RenderizaIconosCorrectos()
        {
            // Arrange
            var ingredientes = new List<IngredienteDto>
            {
                new IngredienteDto
                {
                    Id = Guid.NewGuid(),
                    Nombre = "Test",
                    StockActual = 5,
                    StockMinimo = 10,
                    EstaActivo = true
                }
            };

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Assert
            Assert.Contains("oi-warning", componente.Markup);
            Assert.Contains("oi-clock", componente.Markup);
            Assert.Contains("oi-check", componente.Markup);
            Assert.Contains("oi-power-standby", componente.Markup);
        }

        [Fact]
        public void ManejaIngredientesConDatosCompletos()
        {
            // Arrange
            var ingrediente = new IngredienteDto
            {
                Id = Guid.NewGuid(),
                Nombre = "Ingrediente Completo",
                Descripcion = "Descripción del ingrediente",
                UnidadMedida = UnidadMedida.Kilogramo,
                StockActual = 25.5m,
                StockMinimo = 10.0m,
                StockMaximo = 50.0m,
                CostoUnitario = 3.75m,
                FechaVencimiento = DateTime.Now.AddDays(7),
                EstaActivo = true
            };

            var ingredientes = new List<IngredienteDto> { ingrediente };

            // Act
            var componente = RenderComponent<InventarioKanban>(parameters => parameters
                .Add(p => p.Ingredientes, ingredientes));

            // Assert
            Assert.Contains("Ingrediente Completo", componente.Markup);
            Assert.Contains("25.50 kg", componente.Markup);
            Assert.Contains("10.00", componente.Markup);
            Assert.Contains("50.00", componente.Markup);
            Assert.Contains("$3.75", componente.Markup);
        }
    }
}
