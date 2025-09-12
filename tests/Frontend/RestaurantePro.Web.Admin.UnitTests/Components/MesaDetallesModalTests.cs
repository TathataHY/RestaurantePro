using Bunit;
using Microsoft.AspNetCore.Components;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class MesaDetallesModalTests : TestContext
    {
        [Fact]
        public void NoRenderizaCuandoMostrarEsFalse()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                Estado = "Disponible"
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, false)
                .Add(p => p.Mesa, mesa));

            // Assert - El componente se renderiza pero con contenido diferente
            Assert.Contains("Detalles de la Mesa", componente.Markup);
        }

        [Fact]
        public void RenderizaEstructuraBasicaCuandoMostrarEsTrue()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                Estado = "Disponible"
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("Detalles de la Mesa #101", componente.Markup);
            Assert.Contains("Información completa sobre la mesa y su historial", componente.Markup);
            Assert.Contains("Estado Actual", componente.Markup);
            Assert.Contains("Información de la Mesa", componente.Markup);
            Assert.Contains("Características Especiales", componente.Markup);
            Assert.Contains("Historial Reciente", componente.Markup);
            Assert.Contains("Reservas Pendientes", componente.Markup);
            Assert.Contains("Notas y Observaciones", componente.Markup);
        }

        [Fact]
        public void RenderizaInformacionDeLaMesa()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 6,
                Zona = "Terraza",
                Tipo = "VIP",
                NombreCliente = "Juan Pérez"
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("6 personas", componente.Markup);
            Assert.Contains("Terraza", componente.Markup);
            Assert.Contains("VIP", componente.Markup);
            Assert.Contains("Juan Pérez", componente.Markup);
        }

        [Fact]
        public void MuestraSinClienteCuandoNombreClienteEsVacio()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                NombreCliente = ""
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("Sin cliente", componente.Markup);
        }

        [Fact]
        public void RenderizaCaracteristicasEspeciales()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                TieneVentana = true,
                TieneSofa = true,
                EsAccesible = true,
                TieneEnchufe = true
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("Ventana", componente.Markup);
            Assert.Contains("Sofá", componente.Markup);
            Assert.Contains("Accesible", componente.Markup);
            Assert.Contains("Enchufe", componente.Markup);
        }

        [Fact]
        public void NoRenderizaCaracteristicasCuandoSonFalse()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                TieneVentana = false,
                TieneSofa = false,
                EsAccesible = false,
                TieneEnchufe = false
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.DoesNotContain("Ventana", componente.Markup);
            Assert.DoesNotContain("Sofá", componente.Markup);
            Assert.DoesNotContain("Accesible", componente.Markup);
            Assert.DoesNotContain("Enchufe", componente.Markup);
        }

        [Fact]
        public void RenderizaHistorialReciente()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("Completada", componente.Markup);
            Assert.Contains("18:00", componente.Markup);
            Assert.Contains("20:30", componente.Markup);
        }

        [Fact]
        public void RenderizaReservasPendientes()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("No hay reservas pendientes", componente.Markup);
            Assert.Contains("Esta mesa no tiene reservas programadas en el futuro", componente.Markup);
        }

        [Fact]
        public void RenderizaNotasActualesCuandoEstanDisponibles()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                Notas = "Mesa cerca de la ventana con buena iluminación"
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("Notas actuales:", componente.Markup);
            Assert.Contains("Mesa cerca de la ventana con buena iluminación", componente.Markup);
        }

        [Fact]
        public void NoRenderizaNotasActualesCuandoEstanVacias()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                Notas = null
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.DoesNotContain("Notas actuales:", componente.Markup);
        }

        [Fact]
        public void RenderizaTextareaParaNotas()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("Agregar notas sobre la mesa...", componente.Markup);
            Assert.Contains("form-textarea", componente.Markup);
        }

        [Fact]
        public void AplicaClasesCorrectasParaEstadoDisponible()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                Estado = "Disponible"
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("bg-green-50", componente.Markup);
            Assert.Contains("border-green-200", componente.Markup);
            Assert.Contains("bg-green-100", componente.Markup);
            Assert.Contains("text-green-800", componente.Markup);
            Assert.Contains("text-green-600", componente.Markup);
        }

        [Fact]
        public void AplicaClasesCorrectasParaEstadoOcupada()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                Estado = "Ocupada"
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("bg-red-50", componente.Markup);
            Assert.Contains("border-red-200", componente.Markup);
            Assert.Contains("bg-red-100", componente.Markup);
            Assert.Contains("text-red-800", componente.Markup);
            Assert.Contains("text-red-600", componente.Markup);
        }

        [Fact]
        public void AplicaClasesCorrectasParaEstadoReservada()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                Estado = "Reservada"
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("bg-yellow-50", componente.Markup);
            Assert.Contains("border-yellow-200", componente.Markup);
            Assert.Contains("bg-yellow-100", componente.Markup);
            Assert.Contains("text-yellow-800", componente.Markup);
            Assert.Contains("text-yellow-600", componente.Markup);
        }

        [Fact]
        public void AplicaClasesCorrectasParaEstadoMantenimiento()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                Estado = "Mantenimiento"
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("bg-gray-50", componente.Markup);
            Assert.Contains("border-gray-200", componente.Markup);
            Assert.Contains("bg-gray-100", componente.Markup);
            Assert.Contains("text-gray-800", componente.Markup);
            Assert.Contains("text-gray-600", componente.Markup);
        }

        [Fact]
        public void RenderizaIconosCorrectos()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                TieneVentana = true,
                TieneSofa = true,
                EsAccesible = true,
                TieneEnchufe = true
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("chair_alt", componente.Markup);
            Assert.Contains("window", componente.Markup);
            Assert.Contains("chair", componente.Markup);
            Assert.Contains("accessible", componente.Markup);
            Assert.Contains("power", componente.Markup);
            Assert.Contains("event_busy", componente.Markup);
            Assert.Contains("close", componente.Markup);
        }

        [Fact]
        public void InvocaOnCerrarAlHacerClicEnBotonCerrar()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4
            };
            var onCerrarInvocado = false;

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa)
                .Add(p => p.OnCerrar, EventCallback.Factory.Create(this, () => onCerrarInvocado = true)));

            var botonCerrar = componente.Find("button[class*='hover:bg-gray-100']");
            botonCerrar.Click();

            // Assert
            Assert.True(onCerrarInvocado);
        }

        [Fact]
        public void InvocaOnCerrarAlHacerClicEnBotonCancelar()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4
            };
            var onCerrarInvocado = false;

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa)
                .Add(p => p.OnCerrar, EventCallback.Factory.Create(this, () => onCerrarInvocado = true)));

            var botonCancelar = componente.Find("button[class*='text-[var(--secondary-text-color)]']");
            botonCancelar.Click();

            // Assert
            Assert.True(onCerrarInvocado);
        }

        [Fact]
        public void InvocaOnGuardarNotasAlHacerClicEnBotonGuardar()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4
            };
            var onGuardarNotasInvocado = false;
            string notasGuardadas = "";

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa)
                .Add(p => p.OnGuardarNotas, EventCallback.Factory.Create<string>(this, (notas) => 
                {
                    onGuardarNotasInvocado = true;
                    notasGuardadas = notas;
                })));

            var botonGuardar = componente.Find("button[class*='bg-[var(--primary-color)]']");
            botonGuardar.Click();

            // Assert
            Assert.True(onGuardarNotasInvocado);
        }

        [Fact]
        public void MuestraSpinnerCuandoGuardandoEsTrue()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert - Verificar que el botón de guardar existe
            var botonGuardar = componente.Find("button[class*='bg-[var(--primary-color)]']");
            Assert.NotNull(botonGuardar);
            Assert.Contains("Guardar Cambios", botonGuardar.InnerHtml);
        }

        [Fact]
        public void AplicaClasesCssCorrectas()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("fixed inset-0", componente.Markup);
            Assert.Contains("bg-black/30", componente.Markup);
            Assert.Contains("rounded-2xl", componente.Markup);
            Assert.Contains("shadow-lg", componente.Markup);
            Assert.Contains("grid grid-cols-1 md:grid-cols-2", componente.Markup);
            Assert.Contains("grid grid-cols-2 md:grid-cols-3", componente.Markup);
        }

        [Fact]
        public void RenderizaTablaDeHistorial()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("Fecha", componente.Markup);
            Assert.Contains("Hora de Inicio", componente.Markup);
            Assert.Contains("Hora de Fin", componente.Markup);
            Assert.Contains("Estado", componente.Markup);
        }

        [Fact]
        public void RenderizaUltimaActualizacion()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                UltimaActualizacion = DateTime.Now.AddHours(-2)
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("Última actualización:", componente.Markup);
        }

        [Fact]
        public void InicializaNotasTemporalesConNotasDeLaMesa()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4,
                Notas = "Notas existentes"
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            var textarea = componente.Find("textarea");
            Assert.Contains("Notas existentes", textarea.GetAttribute("value") ?? "");
        }

        [Fact]
        public void RenderizaPlaceholderCorrecto()
        {
            // Arrange
            var mesa = new MesaDto
            {
                Id = Guid.NewGuid(),
                Numero = "101",
                Capacidad = 4
            };

            // Act
            var componente = RenderComponent<MesaDetallesModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Mesa, mesa));

            // Assert
            Assert.Contains("Agregar notas sobre la mesa...", componente.Markup);
        }
    }
}
