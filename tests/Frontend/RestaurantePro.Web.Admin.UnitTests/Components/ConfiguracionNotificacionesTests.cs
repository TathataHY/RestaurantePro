using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class ConfiguracionNotificacionesTests : TestContext
    {
        [Fact]
        public void Renderizar_ConConfiguracionNull_DeberiaMostrarSpinner()
        {
            // Arrange
            var componente = RenderComponent<ConfiguracionNotificaciones>();

            // Act & Assert
            Assert.NotNull(componente.Find(".spinner-border"));
            Assert.Contains("Cargando configuración...", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarFormulario()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto
            {
                NotificacionesEmail = true,
                NotificacionesSistema = false,
                NotificacionesStockBajo = true,
                NotificacionesVencimiento = false,
                NotificacionesReservaciones = true,
                NotificacionesComandas = false,
                DiasAntesVencimiento = 7,
                StockMinimoAlerta = 10
            };

            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
            Assert.NotNull(componente.FindComponent<EditForm>());
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTitulo()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("Configuración de Notificaciones", componente.Markup);
            Assert.NotNull(componente.Find("i.oi-bell"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCheckboxesNotificaciones()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.NotNull(componente.Find("#notifEmail"));
            Assert.NotNull(componente.Find("#notifSistema"));
            Assert.NotNull(componente.Find("#notifStock"));
            Assert.NotNull(componente.Find("#notifVencimiento"));
            Assert.NotNull(componente.Find("#notifReservaciones"));
            Assert.NotNull(componente.Find("#notifComandas"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarLabelsNotificaciones()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("Notificaciones por Email", componente.Markup);
            Assert.Contains("Notificaciones del Sistema", componente.Markup);
            Assert.Contains("Alertas de Stock Bajo", componente.Markup);
            Assert.Contains("Alertas de Vencimiento", componente.Markup);
            Assert.Contains("Notificaciones de Reservaciones", componente.Markup);
            Assert.Contains("Notificaciones de Comandas", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCamposNumericos()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            var diasInput = componente.Find("input[type='number'][min='1'][max='30']");
            var stockInput = componente.Find("input[type='number'][min='1'][max='100']");
            
            Assert.NotNull(diasInput);
            Assert.NotNull(stockInput);
            Assert.Contains("Días antes del vencimiento para alertar", componente.Markup);
            Assert.Contains("Stock mínimo para alerta", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarBotones()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.NotNull(componente.Find("button[type='button']"));
            Assert.NotNull(componente.Find("button[type='submit']"));
            Assert.Contains("Resetear", componente.Markup);
            Assert.Contains("Guardar Configuración", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarIconos()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.NotNull(componente.Find("i.oi-bell"));
            Assert.NotNull(componente.Find("i.oi-reload"));
            Assert.NotNull(componente.Find("i.oi-check"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarSecciones()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("Tipos de Notificaciones", componente.Markup);
            Assert.Contains("Configuración de Alertas", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTextosAyuda()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("Número de días antes del vencimiento para enviar alertas", componente.Markup);
            Assert.Contains("Cantidad mínima de stock para activar alertas", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarDataAnnotationsValidator()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.NotNull(componente.FindComponent<DataAnnotationsValidator>());
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarValoresCorrectos()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto
            {
                NotificacionesEmail = true,
                NotificacionesSistema = false,
                NotificacionesStockBajo = true,
                NotificacionesVencimiento = false,
                NotificacionesReservaciones = true,
                NotificacionesComandas = false,
                DiasAntesVencimiento = 14,
                StockMinimoAlerta = 5
            };

            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            var emailCheckbox = componente.Find("#notifEmail");
            var sistemaCheckbox = componente.Find("#notifSistema");
            var stockCheckbox = componente.Find("#notifStock");
            var vencimientoCheckbox = componente.Find("#notifVencimiento");
            var reservacionesCheckbox = componente.Find("#notifReservaciones");
            var comandasCheckbox = componente.Find("#notifComandas");

            Assert.True(emailCheckbox.HasAttribute("checked"));
            Assert.False(sistemaCheckbox.HasAttribute("checked"));
            Assert.True(stockCheckbox.HasAttribute("checked"));
            Assert.False(vencimientoCheckbox.HasAttribute("checked"));
            Assert.True(reservacionesCheckbox.HasAttribute("checked"));
            Assert.False(comandasCheckbox.HasAttribute("checked"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarValoresPorDefecto()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            var diasInput = componente.Find("input[type='number'][min='1'][max='30']");
            var stockInput = componente.Find("input[type='number'][min='1'][max='100']");

            Assert.Equal("7", diasInput.GetAttribute("value"));
            Assert.Equal("10", stockInput.GetAttribute("value"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraCorrecta()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
            Assert.NotNull(componente.Find(".row"));
            Assert.NotNull(componente.Find(".col-md-6"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarClasesCSS()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.NotNull(componente.Find(".form-check"));
            Assert.NotNull(componente.Find(".form-check-input"));
            Assert.NotNull(componente.Find(".form-check-label"));
            Assert.NotNull(componente.Find(".form-control"));
            Assert.NotNull(componente.Find(".form-label"));
            Assert.NotNull(componente.Find(".form-text"));
            Assert.NotNull(componente.Find(".btn"));
            Assert.NotNull(componente.Find(".btn-primary"));
            Assert.NotNull(componente.Find(".btn-secondary"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarTextosDescriptivos()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("Tipos de Notificaciones", componente.Markup);
            Assert.Contains("Configuración de Alertas", componente.Markup);
            Assert.Contains("Días antes del vencimiento para alertar", componente.Markup);
            Assert.Contains("Stock mínimo para alerta", componente.Markup);
        }

        [Fact]
        public void Renderizar_DeberiaMostrarAtributosInput()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            var diasInput = componente.Find("input[type='number'][min='1'][max='30']");
            var stockInput = componente.Find("input[type='number'][min='1'][max='100']");

            Assert.Equal("number", diasInput.GetAttribute("type"));
            Assert.Equal("1", diasInput.GetAttribute("min"));
            Assert.Equal("30", diasInput.GetAttribute("max"));

            Assert.Equal("number", stockInput.GetAttribute("type"));
            Assert.Equal("1", stockInput.GetAttribute("min"));
            Assert.Equal("100", stockInput.GetAttribute("max"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarCheckboxesConIds()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.NotNull(componente.Find("#notifEmail"));
            Assert.NotNull(componente.Find("#notifSistema"));
            Assert.NotNull(componente.Find("#notifStock"));
            Assert.NotNull(componente.Find("#notifVencimiento"));
            Assert.NotNull(componente.Find("#notifReservaciones"));
            Assert.NotNull(componente.Find("#notifComandas"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarLabelsConFor()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            var labels = componente.FindAll("label[for]");
            Assert.Equal(6, labels.Count);
            
            Assert.NotNull(componente.Find("label[for='notifEmail']"));
            Assert.NotNull(componente.Find("label[for='notifSistema']"));
            Assert.NotNull(componente.Find("label[for='notifStock']"));
            Assert.NotNull(componente.Find("label[for='notifVencimiento']"));
            Assert.NotNull(componente.Find("label[for='notifReservaciones']"));
            Assert.NotNull(componente.Find("label[for='notifComandas']"));
        }

        [Fact]
        public void Renderizar_DeberiaMostrarEstructuraCompleta()
        {
            // Arrange
            var configuracion = new ConfiguracionNotificacionesDto();
            var componente = RenderComponent<ConfiguracionNotificaciones>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            // Verificar estructura principal
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
            
            // Verificar formulario
            Assert.NotNull(componente.Find("form"));
            Assert.NotNull(componente.FindComponent<EditForm>());
            
            // Verificar columnas
            var columnas = componente.FindAll(".col-md-6");
            Assert.Equal(2, columnas.Count);
            
            // Verificar checkboxes
            var checkboxes = componente.FindAll("input[type='checkbox']");
            Assert.Equal(6, checkboxes.Count);
            
            // Verificar inputs numéricos
            var inputsNumericos = componente.FindAll("input[type='number']");
            Assert.Equal(2, inputsNumericos.Count);
            
            // Verificar botones
            var botones = componente.FindAll("button");
            Assert.Equal(2, botones.Count);
        }
    }
}
