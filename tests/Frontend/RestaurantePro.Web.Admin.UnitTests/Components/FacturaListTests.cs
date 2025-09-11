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
    public class FacturaListTests : TestContext
    {
        [Fact]
        public void Renderizar_ConFacturasNull_DeberiaMostrarSpinner()
        {
            // Arrange
            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, (List<FacturaDto>?)null));

            // Act & Assert
            Assert.NotNull(componente.Find(".spinner-border"));
            Assert.Contains("Cargando facturas...", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConListaVacia_DeberiaMostrarMensajeVacio()
        {
            // Arrange
            var facturas = new List<FacturaDto>();

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.Contains("No hay facturas", componente.Markup);
            Assert.Contains("Comienza creando tu primera factura", componente.Markup);
            Assert.Contains("Crear Factura", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConFacturas_DeberiaMostrarTabla()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    ClienteNombre = "Juan Pérez",
                    MesaNombre = "Mesa 1",
                    MeseroNombre = "Carlos",
                    FechaEmision = DateTime.Now,
                    Total = 150.50m,
                    Estado = "Pendiente",
                    TipoPago = "Efectivo"
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.NotNull(componente.Find("table"));
            Assert.NotNull(componente.Find("thead"));
            Assert.NotNull(componente.Find("tbody"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEncabezadosTabla()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroFactura = "FAC-001" }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.Contains("Factura", componente.Markup);
            Assert.Contains("Cliente", componente.Markup);
            Assert.Contains("Mesa", componente.Markup);
            Assert.Contains("Mesero", componente.Markup);
            Assert.Contains("Fecha", componente.Markup);
            Assert.Contains("Total", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
            Assert.Contains("Pago", componente.Markup);
            Assert.Contains("Acciones", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarInformacionFactura()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    ClienteNombre = "Juan Pérez",
                    ClienteEmail = "juan@email.com",
                    MesaNombre = "Mesa 1",
                    MeseroNombre = "Carlos",
                    FechaEmision = DateTime.Now,
                    FechaVencimiento = DateTime.Now.AddDays(30),
                    Total = 150.50m,
                    Descuento = 10.00m,
                    Estado = "Pendiente",
                    TipoPago = "Efectivo",
                    MetodoPago = "Pago en efectivo",
                    MontoPagado = 100.00m
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.Contains("FAC-001", componente.Markup);
            Assert.Contains("Juan Pérez", componente.Markup);
            Assert.Contains("juan@email.com", componente.Markup);
            Assert.Contains("Mesa 1", componente.Markup);
            Assert.Contains("Carlos", componente.Markup);
            Assert.Contains("Efectivo", componente.Markup);
            Assert.Contains("Pago en efectivo", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTotalYDescuento()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    Total = 150.50m,
                    Descuento = 10.00m
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.Contains("$150.50", componente.Markup);
            Assert.Contains("-$10.00 desc.", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBadgesEstado()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act
            var badges = componente.FindAll(".badge");

            // Assert
            Assert.True(badges.Count >= 1); // Estado
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesAccion()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act
            var botones = componente.FindAll("button");

            // Assert
            Assert.True(botones.Count >= 4); // Ver, Editar, Registrar Pago, etc.
            Assert.Contains("Ver detalles", componente.Markup);
            Assert.Contains("Editar", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesHeader()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroFactura = "FAC-001" }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.Contains("Excel", componente.Markup);
            Assert.Contains("Nueva Factura", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarContadorFacturas()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroFactura = "FAC-001" },
                new() { Id = Guid.NewGuid(), NumeroFactura = "FAC-002" }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.Contains("Lista de Facturas (2)", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConPaginacion_DeberiaMostrarPaginacion()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroFactura = "FAC-001" }
            };

            var paginacion = new PaginatedList<FacturaDto>
            {
                Items = facturas,
                PageSize = 10,
                PageNumber = 1,
                TotalPages = 5,
                TotalCount = 50
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas)
                .Add(p => p.Paginacion, paginacion));

            // Act & Assert
            Assert.NotNull(componente.Find(".pagination"));
            Assert.Contains("Mostrando 1 de 5 páginas", componente.Markup);
            Assert.Contains("50 facturas en total", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconos()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    MesaNombre = "Mesa 1"
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.NotNull(componente.Find("i.oi-document"));
            Assert.NotNull(componente.Find("i.oi-eye"));
            Assert.NotNull(componente.Find("i.oi-pencil"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraCard()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroFactura = "FAC-001" }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTablaResponsive()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() { Id = Guid.NewGuid(), NumeroFactura = "FAC-001" }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.NotNull(componente.Find(".table-responsive"));
            Assert.NotNull(componente.Find(".table.table-hover"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarGrupoBotones()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.NotNull(componente.Find(".btn-group"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTitulosBotones()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.Contains("title=\"Ver detalles\"", componente.Markup);
            Assert.Contains("title=\"Editar\"", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFechaFormateada()
        {
            // Arrange
            var fecha = new DateTime(2024, 1, 15);
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    FechaEmision = fecha
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.Contains("15/01/2024", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarFechaVencimiento()
        {
            // Arrange
            var fechaVencimiento = new DateTime(2024, 2, 15);
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    FechaVencimiento = fechaVencimiento
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.Contains("Vence: 15/02/2024", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarMontoPagado()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    MontoPagado = 100.00m
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.Contains("Pagado: $100.00", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBadgeElectronica()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    EsFacturaElectronica = true
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.Contains("Electrónica", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarDiasVencimiento()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    FechaVencimiento = DateTime.Now.AddDays(-5),
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.Contains("días vencida", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotonesCondicionales()
        {
            // Arrange
            var facturas = new List<FacturaDto>
            {
                new() 
                { 
                    Id = Guid.NewGuid(), 
                    NumeroFactura = "FAC-001",
                    Estado = "Pendiente"
                }
            };

            var componente = RenderComponent<FacturaList>(parameters => parameters
                .Add(p => p.Facturas, facturas));

            // Act & Assert
            Assert.Contains("Registrar pago", componente.Markup);
            Assert.Contains("Reimprimir", componente.Markup);
            Assert.Contains("Cancelar", componente.Markup);
        }
    }
}
