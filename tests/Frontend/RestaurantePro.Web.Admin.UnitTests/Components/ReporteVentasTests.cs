using Bunit;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class ReporteVentasTests : TestContext
    {
        [Fact]
        public void Renderizar_ConReporteNull_DeberiaMostrarSpinner()
        {
            // Arrange
            var componente = RenderComponent<ReporteVentas>();

            // Act & Assert
            Assert.NotNull(componente.Find(".spinner-border"));
            Assert.Contains("Generando reporte de ventas...", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarTitulo()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Reporte de Ventas", componente.Markup);
            Assert.Contains("oi-bar-chart", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarBotonesExportar()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
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
            var reporte = new ReporteVentasDto
            {
                FechaInicio = new DateTime(2024, 1, 1),
                FechaFin = new DateTime(2024, 1, 31),
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500,
                PromedioVentaPorFactura = 625
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Total Ventas", componente.Markup);
            Assert.Contains("Total Comandas", componente.Markup);
            Assert.Contains("Total Facturas", componente.Markup);
            Assert.Contains("Promedio por Comanda", componente.Markup);
            Assert.Contains("$50,000", componente.Markup);
            Assert.Contains("100", componente.Markup);
            Assert.Contains("80", componente.Markup);
            Assert.Contains("$500", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarCardsResumen()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.NotNull(componente.Find(".card.border-left-primary"));
            Assert.NotNull(componente.Find(".card.border-left-success"));
            Assert.NotNull(componente.Find(".card.border-left-info"));
            Assert.NotNull(componente.Find(".card.border-left-warning"));
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarPeriodo()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = new DateTime(2024, 1, 1),
                FechaFin = new DateTime(2024, 1, 31),
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Período:", componente.Markup);
            Assert.Contains("01/01/2024 - 31/01/2024", componente.Markup);
            Assert.NotNull(componente.Find(".alert.alert-info"));
        }

        [Fact]
        public void Renderizar_ConVentasPorDia_DeberiaMostrarTabla()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500,
                VentasPorDia = new List<VentaPorDiaDto>
                {
                    new VentaPorDiaDto
                    {
                        Fecha = DateTime.Today.AddDays(-1),
                        TotalVentas = 1500,
                        TotalComandas = 5,
                        TotalFacturas = 4,
                        PromedioVenta = 300
                    }
                }
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Ventas por Día", componente.Markup);
            Assert.NotNull(componente.Find("table"));
            Assert.Contains("Fecha", componente.Markup);
            Assert.Contains("Total Ventas", componente.Markup);
            Assert.Contains("Comandas", componente.Markup);
            Assert.Contains("Facturas", componente.Markup);
            Assert.Contains("Promedio", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConVentasPorDia_DeberiaMostrarDatos()
        {
            // Arrange
            var fecha = DateTime.Today.AddDays(-1);
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500,
                VentasPorDia = new List<VentaPorDiaDto>
                {
                    new VentaPorDiaDto
                    {
                        Fecha = fecha,
                        TotalVentas = 1500,
                        TotalComandas = 5,
                        TotalFacturas = 4,
                        PromedioVenta = 300
                    }
                }
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains(fecha.ToString("dd/MM/yyyy"), componente.Markup);
            Assert.Contains("$1,500", componente.Markup);
            Assert.Contains("5", componente.Markup);
            Assert.Contains("4", componente.Markup);
            Assert.Contains("$300", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConVentasPorHora_DeberiaMostrarTabla()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500,
                VentasPorHora = new List<VentaPorHoraDto>
                {
                    new VentaPorHoraDto
                    {
                        Hora = 12,
                        TotalVentas = 800,
                        TotalComandas = 3,
                        PromedioVenta = 266.67m
                    }
                }
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Ventas por Hora", componente.Markup);
            Assert.Contains("Hora", componente.Markup);
            Assert.Contains("12:00", componente.Markup);
            Assert.Contains("$800", componente.Markup);
            Assert.Contains("3", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConVentasPorMesero_DeberiaMostrarTabla()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500,
                VentasPorMesero = new List<VentaPorMeseroDto>
                {
                    new VentaPorMeseroDto
                    {
                        MeseroId = Guid.NewGuid(),
                        MeseroNombre = "Juan Pérez",
                        TotalVentas = 5000,
                        TotalComandas = 20,
                        PromedioVenta = 250,
                        Comision = 250
                    }
                }
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Ventas por Mesero", componente.Markup);
            Assert.Contains("Mesero", componente.Markup);
            Assert.Contains("Comisión", componente.Markup);
            Assert.Contains("Juan Pérez", componente.Markup);
            Assert.Contains("$5,000", componente.Markup);
            Assert.Contains("20", componente.Markup);
            Assert.Contains("$250", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConVentasPorMesa_DeberiaMostrarTabla()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500,
                VentasPorMesa = new List<VentaPorMesaDto>
                {
                    new VentaPorMesaDto
                    {
                        MesaId = Guid.NewGuid(),
                        MesaNombre = "Mesa 1",
                        TotalVentas = 3000,
                        TotalComandas = 10,
                        PromedioVenta = 300,
                        TiempoPromedioOcupacion = TimeSpan.FromHours(2.5)
                    }
                }
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Ventas por Mesa", componente.Markup);
            Assert.Contains("Mesa", componente.Markup);
            Assert.Contains("Tiempo Promedio", componente.Markup);
            Assert.Contains("Mesa 1", componente.Markup);
            Assert.Contains("$3,000", componente.Markup);
            Assert.Contains("10", componente.Markup);
            Assert.Contains("02:30", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarEstructuraBasica()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
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
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
            Assert.NotNull(componente.Find(".row"));
            Assert.NotNull(componente.Find(".col-md-3"));
        }

        [Fact]
        public void Renderizar_ConVentasPorDia_DeberiaMostrarTablaResponsive()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500,
                VentasPorDia = new List<VentaPorDiaDto>
                {
                    new VentaPorDiaDto
                    {
                        Fecha = DateTime.Today.AddDays(-1),
                        TotalVentas = 1500,
                        TotalComandas = 5,
                        TotalFacturas = 4,
                        PromedioVenta = 300
                    }
                }
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
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
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("oi-bar-chart", componente.Markup);
            Assert.Contains("oi-document", componente.Markup);
            Assert.Contains("oi-spreadsheet", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarTextosDescriptivos()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Reporte de Ventas", componente.Markup);
            Assert.Contains("Total Ventas", componente.Markup);
            Assert.Contains("Total Comandas", componente.Markup);
            Assert.Contains("Total Facturas", componente.Markup);
            Assert.Contains("Promedio por Comanda", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarEstructuraCompleta()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            // Verificar estructura principal
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
            
            // Verificar resumen
            Assert.NotNull(componente.Find(".row"));
            var columnas = componente.FindAll(".col-md-3");
            Assert.Equal(4, columnas.Count);
            
            // Verificar cards de resumen
            Assert.NotNull(componente.Find(".card.border-left-primary"));
            Assert.NotNull(componente.Find(".card.border-left-success"));
            Assert.NotNull(componente.Find(".card.border-left-info"));
            Assert.NotNull(componente.Find(".card.border-left-warning"));
        }

        [Fact]
        public void Renderizar_ConVentasPorDia_DeberiaMostrarHeadersTabla()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500,
                VentasPorDia = new List<VentaPorDiaDto>
                {
                    new VentaPorDiaDto
                    {
                        Fecha = DateTime.Today.AddDays(-1),
                        TotalVentas = 1500,
                        TotalComandas = 5,
                        TotalFacturas = 4,
                        PromedioVenta = 300
                    }
                }
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Fecha", componente.Markup);
            Assert.Contains("Total Ventas", componente.Markup);
            Assert.Contains("Comandas", componente.Markup);
            Assert.Contains("Facturas", componente.Markup);
            Assert.Contains("Promedio", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConVentasPorHora_DeberiaMostrarHeadersTabla()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500,
                VentasPorHora = new List<VentaPorHoraDto>
                {
                    new VentaPorHoraDto
                    {
                        Hora = 12,
                        TotalVentas = 800,
                        TotalComandas = 3,
                        PromedioVenta = 266.67m
                    }
                }
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Hora", componente.Markup);
            Assert.Contains("Total Ventas", componente.Markup);
            Assert.Contains("Comandas", componente.Markup);
            Assert.Contains("Promedio", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConVentasPorMesero_DeberiaMostrarHeadersTabla()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500,
                VentasPorMesero = new List<VentaPorMeseroDto>
                {
                    new VentaPorMeseroDto
                    {
                        MeseroId = Guid.NewGuid(),
                        MeseroNombre = "Juan Pérez",
                        TotalVentas = 5000,
                        TotalComandas = 20,
                        PromedioVenta = 250,
                        Comision = 250
                    }
                }
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Mesero", componente.Markup);
            Assert.Contains("Total Ventas", componente.Markup);
            Assert.Contains("Comandas", componente.Markup);
            Assert.Contains("Promedio", componente.Markup);
            Assert.Contains("Comisión", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConVentasPorMesa_DeberiaMostrarHeadersTabla()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = DateTime.Today.AddDays(-30),
                FechaFin = DateTime.Today,
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500,
                VentasPorMesa = new List<VentaPorMesaDto>
                {
                    new VentaPorMesaDto
                    {
                        MesaId = Guid.NewGuid(),
                        MesaNombre = "Mesa 1",
                        TotalVentas = 3000,
                        TotalComandas = 10,
                        PromedioVenta = 300,
                        TiempoPromedioOcupacion = TimeSpan.FromHours(2.5)
                    }
                }
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.Contains("Mesa", componente.Markup);
            Assert.Contains("Total Ventas", componente.Markup);
            Assert.Contains("Comandas", componente.Markup);
            Assert.Contains("Promedio", componente.Markup);
            Assert.Contains("Tiempo Promedio", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConReporte_DeberiaMostrarAlertInfo()
        {
            // Arrange
            var reporte = new ReporteVentasDto
            {
                FechaInicio = new DateTime(2024, 1, 1),
                FechaFin = new DateTime(2024, 1, 31),
                TotalVentas = 50000,
                TotalComandas = 100,
                TotalFacturas = 80,
                PromedioVentaPorComanda = 500
            };

            var componente = RenderComponent<ReporteVentas>(parameters => parameters
                .Add(p => p.Reporte, reporte));

            // Act & Assert
            Assert.NotNull(componente.Find(".alert.alert-info"));
            Assert.Contains("Período:", componente.Markup);
            Assert.Contains("01/01/2024 - 31/01/2024", componente.Markup);
        }
    }
}
