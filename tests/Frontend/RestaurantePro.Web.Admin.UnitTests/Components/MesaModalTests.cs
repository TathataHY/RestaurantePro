using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class MesaModalTests : TestContext
    {
        [Fact]
        public void RenderizaEstructuraBasica()
        {
            // Arrange
            var crearModel = new CrearMesaRequest
            {
                Numero = 101,
                Capacidad = 4,
                Zona = "Salón Principal",
                Tipo = "Estándar"
            };

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.Contains("Crear Mesa", componente.Markup);
            Assert.Contains("Gestiona la información y configuración de las mesas", componente.Markup);
            Assert.Contains("Información Básica", componente.Markup);
            Assert.Contains("Configuración Avanzada", componente.Markup);
        }

        [Fact]
        public void RenderizaTituloCorrectoParaCrear()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.Contains("Crear Mesa", componente.Markup);
            Assert.DoesNotContain("Editar Mesa", componente.Markup);
        }

        [Fact]
        public void RenderizaTituloCorrectoParaEditar()
        {
            // Arrange
            var editarModel = new ActualizarMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, true)
                .Add(p => p.EditarModel, editarModel));

            // Assert
            Assert.Contains("Editar Mesa", componente.Markup);
            Assert.DoesNotContain("Crear Mesa", componente.Markup);
        }

        [Fact]
        public void RenderizaFormularioConCamposObligatorios()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.Contains("Número de Mesa", componente.Markup);
            Assert.Contains("Capacidad", componente.Markup);
            Assert.Contains("Zona/Ubicación", componente.Markup);
            Assert.Contains("Tipo de Mesa", componente.Markup);
            Assert.Contains("Estado Inicial", componente.Markup);
        }

        [Fact]
        public void RenderizaCamposOpcionales()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.Contains("Descripción (Opcional)", componente.Markup);
            Assert.Contains("Notas Internas", componente.Markup);
        }

        [Fact]
        public void RenderizaCaracteristicasEspeciales()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.Contains("Características Especiales", componente.Markup);
            Assert.Contains("Ventana", componente.Markup);
            Assert.Contains("Sofa", componente.Markup);
            Assert.Contains("Accesible", componente.Markup);
            Assert.Contains("Enchufe", componente.Markup);
        }

        [Fact]
        public void RenderizaOpcionesDeZona()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.Contains("Salón Principal", componente.Markup);
            Assert.Contains("Terraza", componente.Markup);
            Assert.Contains("Barra", componente.Markup);
            Assert.Contains("Privado", componente.Markup);
            Assert.Contains("VIP", componente.Markup);
        }

        [Fact]
        public void RenderizaOpcionesDeTipo()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.Contains("Estándar", componente.Markup);
            Assert.Contains("Alta", componente.Markup);
            Assert.Contains("Baja", componente.Markup);
            Assert.Contains("Booth", componente.Markup);
            Assert.Contains("VIP", componente.Markup);
        }

        [Fact]
        public void RenderizaOpcionesDeEstado()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.Contains("Disponible", componente.Markup);
            Assert.Contains("Reservada", componente.Markup);
            Assert.Contains("Ocupada", componente.Markup);
            Assert.Contains("Mantenimiento", componente.Markup);
        }

        [Fact]
        public void RenderizaBotonGuardarCorrectoParaCrear()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.Contains("Guardar Mesa", componente.Markup);
            Assert.DoesNotContain("Actualizar Mesa", componente.Markup);
        }

        [Fact]
        public void RenderizaBotonGuardarCorrectoParaEditar()
        {
            // Arrange
            var editarModel = new ActualizarMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, true)
                .Add(p => p.EditarModel, editarModel));

            // Assert
            Assert.Contains("Actualizar Mesa", componente.Markup);
            Assert.DoesNotContain("Guardar Mesa", componente.Markup);
        }

        [Fact]
        public void MuestraSpinnerCuandoGuardandoEsTrue()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel)
                .Add(p => p.Guardando, true));

            // Assert
            Assert.Contains("animate-spin", componente.Markup);
        }

        [Fact]
        public void MuestraErrorCuandoErrorNoEstaVacio()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();
            var error = "Error de validación";

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel)
                .Add(p => p.Error, error));

            // Assert
            Assert.Contains(error, componente.Markup);
            Assert.Contains("bg-red-100", componente.Markup);
        }

        [Fact]
        public void NoMuestraErrorCuandoErrorEstaVacio()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel)
                .Add(p => p.Error, string.Empty));

            // Assert
            Assert.DoesNotContain("bg-red-100", componente.Markup);
        }

        [Fact]
        public void RenderizaIconosCorrectos()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.Contains("table_restaurant", componente.Markup);
            Assert.Contains("close", componente.Markup);
            Assert.Contains("tag", componente.Markup);
            Assert.Contains("groups", componente.Markup);
            Assert.Contains("room_service", componente.Markup);
            Assert.Contains("table_bar", componente.Markup);
            Assert.Contains("toggle_on", componente.Markup);
            Assert.Contains("cloud_upload", componente.Markup);
        }

        [Fact]
        public void RenderizaValidaciones()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.NotNull(componente.FindComponent<DataAnnotationsValidator>());
        }

        [Fact]
        public void InvocaOnCerrarAlHacerClicEnBotonCerrar()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();
            var onCerrarInvocado = false;

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel)
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
            var crearModel = new CrearMesaRequest();
            var onCerrarInvocado = false;

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel)
                .Add(p => p.OnCerrar, EventCallback.Factory.Create(this, () => onCerrarInvocado = true)));

            var botonCancelar = componente.Find("button[type='button']");
            botonCancelar.Click();

            // Assert
            Assert.True(onCerrarInvocado);
        }

        [Fact]
        public void InvocaOnGuardarAlEnviarFormulario()
        {
            // Arrange
            var crearModel = new CrearMesaRequest
            {
                Numero = 101,
                Capacidad = 4,
                Zona = "Salón Principal",
                Tipo = "Estándar"
            };
            var onGuardarInvocado = false;

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel)
                .Add(p => p.OnGuardar, EventCallback.Factory.Create(this, () => onGuardarInvocado = true)));

            var form = componente.Find("form");
            form.Submit();

            // Assert
            Assert.True(onGuardarInvocado);
        }

        [Fact]
        public void ConvierteCorrectamenteEditarModelACrearRequest()
        {
            // Arrange
            var editarModel = new ActualizarMesaRequest
            {
                Numero = 201,
                Capacidad = 6,
                Zona = "Terraza",
                Tipo = "VIP",
                Estado = "Reservada",
                Descripcion = "Mesa con vista al mar",
                Notas = "Requiere reserva previa",
                TieneVentana = true,
                TieneSofa = false,
                EsAccesible = true,
                TieneEnchufe = true
            };

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, true)
                .Add(p => p.EditarModel, editarModel));

            // Assert
            // Verificamos que los valores se muestran correctamente en el formulario
            Assert.Contains("201", componente.Markup);
            Assert.Contains("6", componente.Markup);
            Assert.Contains("Terraza", componente.Markup);
            Assert.Contains("VIP", componente.Markup);
            Assert.Contains("Reservada", componente.Markup);
            Assert.Contains("Mesa con vista al mar", componente.Markup);
            Assert.Contains("Requiere reserva previa", componente.Markup);
        }

        [Fact]
        public void RenderizaAreaDeSubidaDeArchivos()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.Contains("Foto de la Mesa", componente.Markup);
            Assert.Contains("Sube un archivo", componente.Markup);
            Assert.Contains("PNG, JPG, GIF hasta 10MB", componente.Markup);
            Assert.Contains("file-upload", componente.Markup);
        }

        [Fact]
        public void AplicaClasesCssCorrectas()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.Contains("fixed inset-0", componente.Markup);
            Assert.Contains("bg-black/50", componente.Markup);
            Assert.Contains("rounded-2xl", componente.Markup);
            Assert.Contains("shadow-[0_8px_30px_rgb(0,0,0,0.12)]", componente.Markup);
            Assert.Contains("grid grid-cols-1 md:grid-cols-2", componente.Markup);
        }

        [Fact]
        public void RenderizaCamposConPlaceholdersCorrectos()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel));

            // Assert
            Assert.Contains("Ej: 101", componente.Markup);
            Assert.Contains("Ej: 4", componente.Markup);
            Assert.Contains("Ej: Mesa cerca de la ventana con buena iluminación", componente.Markup);
            Assert.Contains("Anotaciones solo para el personal", componente.Markup);
        }

        [Fact]
        public void DeshabilitaBotonGuardarCuandoGuardandoEsTrue()
        {
            // Arrange
            var crearModel = new CrearMesaRequest();

            // Act
            var componente = RenderComponent<MesaModal>(parameters => parameters
                .Add(p => p.Mostrar, true)
                .Add(p => p.Editando, false)
                .Add(p => p.CrearModel, crearModel)
                .Add(p => p.Guardando, true));

            // Assert
            var botonGuardar = componente.Find("button[type='submit']");
            Assert.NotNull(botonGuardar.GetAttribute("disabled"));
        }
    }
}
