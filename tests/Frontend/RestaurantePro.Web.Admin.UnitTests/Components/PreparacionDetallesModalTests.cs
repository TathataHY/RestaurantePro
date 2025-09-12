using Bunit;
using Microsoft.AspNetCore.Components;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class PreparacionDetallesModalTests : TestContext
    {
        [Fact]
        public void NoRenderizaCuandoMostrarEsFalse()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita"
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, false)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.DoesNotContain("modal", componente.Markup);
        }

        [Fact]
        public void RenderizaEstructuraBasicaCuandoMostrarEsTrue()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita"
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("modal fade show", componente.Markup);
            Assert.Contains("Detalles de Preparación", componente.Markup);
            Assert.Contains("Información General", componente.Markup);
            Assert.Contains("Información de Cocina", componente.Markup);
        }

        [Fact]
        public void RenderizaInformacionGeneral()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                Cantidad = 2,
                Estado = EstadoPreparacion.EnProceso,
                Prioridad = PrioridadPreparacion.Alta,
                MesaNumero = 5
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("CMD-001", componente.Markup);
            Assert.Contains("Pizza Margherita", componente.Markup);
            Assert.Contains("2", componente.Markup);
            Assert.Contains("EnProceso", componente.Markup);
            Assert.Contains("Alta", componente.Markup);
            Assert.Contains("5", componente.Markup);
        }

        [Fact]
        public void RenderizaInformacionDeCocina()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                CocineroNombre = "Juan Pérez",
                FechaInicio = DateTime.Now.AddMinutes(-30),
                TiempoEstimadoMinutos = 45,
                TiempoRealMinutos = 30,
                TiempoTranscurridoMinutos = 30,
                EstaAtrasada = false
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("Juan Pérez", componente.Markup);
            Assert.Contains("45 minutos", componente.Markup);
            Assert.Contains("30 minutos", componente.Markup);
            Assert.Contains("A tiempo", componente.Markup);
        }

        [Fact]
        public void MuestraMesaAsignada()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                MesaNumero = 3
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("3", componente.Markup);
            Assert.DoesNotContain("No asignada", componente.Markup);
        }

        [Fact]
        public void MuestraMesaNoAsignada()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                MesaNumero = null
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("No asignada", componente.Markup);
        }

        [Fact]
        public void MuestraCocineroAsignado()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                CocineroNombre = "María García"
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("María García", componente.Markup);
            Assert.DoesNotContain("Sin asignar", componente.Markup);
        }

        [Fact]
        public void MuestraCocineroNoAsignado()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                CocineroNombre = null
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("Sin asignar", componente.Markup);
        }

        [Fact]
        public void MuestraHoraInicioCuandoEstaDisponible()
        {
            // Arrange
            var fechaInicio = DateTime.Now.AddMinutes(-20);
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                FechaInicio = fechaInicio
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains(fechaInicio.ToString("HH:mm"), componente.Markup);
        }

        [Fact]
        public void MuestraNoIniciadaCuandoFechaInicioEsNull()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                FechaInicio = null
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("No iniciada", componente.Markup);
        }

        [Fact]
        public void MuestraTiempoRealCuandoEstaDisponible()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                TiempoRealMinutos = 35
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("35 minutos", componente.Markup);
            Assert.DoesNotContain("En proceso", componente.Markup);
        }

        [Fact]
        public void MuestraEnProcesoCuandoTiempoRealEsNull()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                TiempoRealMinutos = null
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("En proceso", componente.Markup);
        }

        [Fact]
        public void MuestraTiempoTranscurridoCuandoEstaDisponible()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                TiempoTranscurridoMinutos = 25,
                EstaAtrasada = false
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("25 minutos", componente.Markup);
        }

        [Fact]
        public void MuestraNoIniciadaCuandoTiempoTranscurridoEsNull()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                TiempoTranscurridoMinutos = null
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("No iniciada", componente.Markup);
        }

        [Fact]
        public void MuestraAtrasadaCuandoEstaAtrasadaEsTrue()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                EstaAtrasada = true
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("Atrasada", componente.Markup);
            Assert.DoesNotContain("A tiempo", componente.Markup);
        }

        [Fact]
        public void MuestraATiempoCuandoEstaAtrasadaEsFalse()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                EstaAtrasada = false
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("A tiempo", componente.Markup);
            Assert.DoesNotContain("Atrasada", componente.Markup);
        }

        [Fact]
        public void MuestraObservacionesCuandoEstanDisponibles()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                Observaciones = "Sin cebolla, extra queso"
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("Sin cebolla, extra queso", componente.Markup);
            Assert.DoesNotContain("Sin observaciones", componente.Markup);
        }

        [Fact]
        public void MuestraSinObservacionesCuandoObservacionesEsNull()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                Observaciones = null
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("Sin observaciones", componente.Markup);
        }

        [Fact]
        public void MuestraNotasCocineroCuandoEstanDisponibles()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                NotasCocinero = "Horno caliente, listo en 10 min"
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("Horno caliente, listo en 10 min", componente.Markup);
            Assert.DoesNotContain("Sin notas", componente.Markup);
        }

        [Fact]
        public void MuestraSinNotasCuandoNotasCocineroEsNull()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                NotasCocinero = null
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("Sin notas", componente.Markup);
        }

        [Fact]
        public void AplicaClasesBadgeCorrectasParaEstados()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                Estado = EstadoPreparacion.Lista,
                Prioridad = PrioridadPreparacion.Urgente
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("bg-success", componente.Markup); // Estado Lista
            Assert.Contains("bg-danger", componente.Markup); // Prioridad Urgente
        }

        [Fact]
        public void AplicaClasesDeColorCorrectasParaTiempos()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita",
                TiempoEstimadoMinutos = 30,
                TiempoRealMinutos = 25, // Menor que estimado
                TiempoTranscurridoMinutos = 20,
                EstaAtrasada = false
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("text-success", componente.Markup); // Tiempo real menor que estimado
            Assert.Contains("text-primary", componente.Markup); // No atrasada
        }

        [Fact]
        public void InvocaOnCerrarAlHacerClicEnBotonCerrar()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita"
            };
            var onCerrarInvocado = false;

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.OnCerrar, EventCallback.Factory.Create(this, () => onCerrarInvocado = true)));

            var botonCerrar = componente.Find("button[class*='btn-close']");
            botonCerrar.Click();

            // Assert
            Assert.True(onCerrarInvocado);
        }

        [Fact]
        public void InvocaOnCerrarAlHacerClicEnBotonCerrarFooter()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita"
            };
            var onCerrarInvocado = false;

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion)
                .Add(p => p.OnCerrar, EventCallback.Factory.Create(this, () => onCerrarInvocado = true)));

            var botonCerrar = componente.Find("button[class*='btn-secondary']");
            botonCerrar.Click();

            // Assert
            Assert.True(onCerrarInvocado);
        }

        [Fact]
        public void RenderizaIconosCorrectos()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita"
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("oi-eye", componente.Markup);
            Assert.Contains("oi-info", componente.Markup);
            Assert.Contains("oi-restaurant", componente.Markup);
            Assert.Contains("oi-clipboard", componente.Markup);
            Assert.Contains("oi-x", componente.Markup);
        }

        [Fact]
        public void RenderizaBackdrop()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita"
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("modal-backdrop", componente.Markup);
        }

        [Fact]
        public void AplicaClasesCssCorrectas()
        {
            // Arrange
            var preparacion = new PreparacionDto
            {
                Id = 1,
                ComandaNumero = "CMD-001",
                ProductoNombre = "Pizza Margherita"
            };

            // Act
            var componente = RenderComponent<PreparacionDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Preparacion, preparacion));

            // Assert
            Assert.Contains("modal fade show", componente.Markup);
            Assert.Contains("modal-dialog modal-lg", componente.Markup);
            Assert.Contains("modal-content", componente.Markup);
            Assert.Contains("modal-header", componente.Markup);
            Assert.Contains("modal-body", componente.Markup);
            Assert.Contains("modal-footer", componente.Markup);
        }
    }
}
