using Bunit;
using Microsoft.AspNetCore.Components;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class TarjetasFidelizacionTests : TestContext
    {
        [Fact]
        public void Renderizar_ConConfiguracionNull_DeberiaMostrarSpinner()
        {
            // Arrange
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, (ConfiguracionFidelizacionDto?)null));

            // Act & Assert
            Assert.Contains("Cargando configuración...", componente.Markup);
            Assert.Contains("spinner-border", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarFormulario()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                SistemaActivo = true,
                PuntosPorPeso = 2,
                PesoPorPunto = 50m,
                PuntosMinimosCanje = 500,
                DescuentoMaximo = 25m,
                DiasVencimientoPuntos = 180,
                CanjeAutomatico = false
            };

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.NotNull(componente.Find("form"));
            Assert.Contains("Sistema de Tarjetas de Fidelización", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarTitulo()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("Sistema de Tarjetas de Fidelización", componente.Markup);
            Assert.Contains("oi-badge", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarCheckboxes()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                SistemaActivo = true,
                CanjeAutomatico = false
            };

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.NotNull(componente.Find("#sistemaActivo"));
            Assert.NotNull(componente.Find("#canjeAutomatico"));
            Assert.Contains("Sistema de Fidelización Activo", componente.Markup);
            Assert.Contains("Canje Automático de Puntos", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarInputsNumericos()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("Puntos por peso gastado", componente.Markup);
            Assert.Contains("Peso por punto (para canje)", componente.Markup);
            Assert.Contains("Puntos mínimos para canje", componente.Markup);
            Assert.Contains("Descuento máximo (%)", componente.Markup);
            Assert.Contains("Días de vencimiento de puntos", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarResumen()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                PuntosPorPeso = 3,
                PesoPorPunto = 75m,
                PuntosMinimosCanje = 750,
                DescuentoMaximo = 30m,
                DiasVencimientoPuntos = 200
            };

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("Resumen de Configuración", componente.Markup);
            Assert.Contains("3 punto(s) por cada peso gastado", componente.Markup);
            Assert.Contains("$75 pesos por punto", componente.Markup);
            Assert.Contains("750 puntos", componente.Markup);
            Assert.Contains("30%", componente.Markup);
            Assert.Contains("200 días", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarBotones()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.NotNull(componente.Find("button[type=\"button\"]"));
            Assert.NotNull(componente.Find("button[type=\"submit\"]"));
            Assert.Contains("Resetear", componente.Markup);
            Assert.Contains("Guardar Configuración", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarEstructuraCard()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.NotNull(componente.Find(".card"));
            Assert.NotNull(componente.Find(".card-header"));
            Assert.NotNull(componente.Find(".card-body"));
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarSecciones()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("Configuración General", componente.Markup);
            Assert.Contains("Sistema de Puntos", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarFormTexts()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("Activar o desactivar el sistema completo de fidelización", componente.Markup);
            Assert.Contains("Permitir canje automático cuando se alcance el mínimo", componente.Markup);
            Assert.Contains("Puntos que se otorgan por cada peso gastado", componente.Markup);
            Assert.Contains("Peso que equivale a 1 punto para canjes", componente.Markup);
            Assert.Contains("Puntos mínimos requeridos para realizar un canje", componente.Markup);
            Assert.Contains("Porcentaje máximo de descuento por puntos", componente.Markup);
            Assert.Contains("Días antes de que los puntos expiren", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarIconos()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("oi-badge", componente.Markup);
            Assert.Contains("oi-info", componente.Markup);
            Assert.Contains("oi-reload", componente.Markup);
            Assert.Contains("oi-check", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarClasesCorrectas()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("text-primary", componente.Markup);
            Assert.Contains("alert-info", componente.Markup);
            Assert.Contains("btn-secondary", componente.Markup);
            Assert.Contains("btn-primary", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarInputsConAtributosCorrectos()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            var puntosPorPesoInput = componente.Find("input[type=\"number\"][min=\"1\"][max=\"10\"]");
            Assert.NotNull(puntosPorPesoInput);

            var pesoPorPuntoInput = componente.Find("input[type=\"number\"][min=\"1\"][max=\"1000\"]");
            Assert.NotNull(pesoPorPuntoInput);

            var puntosMinimosInput = componente.Find("input[type=\"number\"][min=\"100\"][max=\"10000\"]");
            Assert.NotNull(puntosMinimosInput);

            var descuentoMaximoInput = componente.Find("input[type=\"number\"][min=\"1\"][max=\"100\"]");
            Assert.NotNull(descuentoMaximoInput);

            var diasVencimientoInput = componente.Find("input[type=\"number\"][min=\"30\"][max=\"1095\"]");
            Assert.NotNull(diasVencimientoInput);
        }


        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarEstructuraCompleta()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            var card = componente.Find(".card");
            Assert.NotNull(card);

            var cardHeader = componente.Find(".card-header");
            Assert.NotNull(cardHeader);

            var cardBody = componente.Find(".card-body");
            Assert.NotNull(cardBody);

            var form = componente.Find("form");
            Assert.NotNull(form);

            var row = componente.Find(".row");
            Assert.NotNull(row);

            var alert = componente.Find(".alert");
            Assert.NotNull(alert);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarValoresPorDefecto()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("1 punto(s) por cada peso gastado", componente.Markup);
            Assert.Contains("$100 pesos por punto", componente.Markup);
            Assert.Contains("1000 puntos", componente.Markup);
            Assert.Contains("50%", componente.Markup);
            Assert.Contains("365 días", componente.Markup);
        }

        [Fact]
        public void Renderizar_ConConfiguracion_DeberiaMostrarFormularioCompleto()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                SistemaActivo = true,
                CanjeAutomatico = true,
                PuntosPorPeso = 5,
                PesoPorPunto = 20m,
                PuntosMinimosCanje = 2000,
                DescuentoMaximo = 75m,
                DiasVencimientoPuntos = 730
            };

            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Act & Assert
            Assert.Contains("Sistema de Tarjetas de Fidelización", componente.Markup);
            Assert.Contains("Configuración General", componente.Markup);
            Assert.Contains("Sistema de Puntos", componente.Markup);
            Assert.Contains("Resumen de Configuración", componente.Markup);
            Assert.Contains("5 punto(s) por cada peso gastado", componente.Markup);
            Assert.Contains("$20 pesos por punto", componente.Markup);
            Assert.Contains("2000 puntos", componente.Markup);
            Assert.Contains("75%", componente.Markup);
            Assert.Contains("730 días", componente.Markup);
        }
    }
}
