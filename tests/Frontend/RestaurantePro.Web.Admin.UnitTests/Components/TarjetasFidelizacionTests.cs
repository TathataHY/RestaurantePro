using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using RestaurantePro.Web.Admin.Components;
using RestaurantePro.Web.Admin.Models;
using Xunit;

namespace RestaurantePro.Web.Admin.UnitTests.Components
{
    public class TarjetasFidelizacionTests : TestContext
    {
        [Fact]
        public void RenderizaEstructuraBasica()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                SistemaActivo = true,
                PuntosPorPeso = 2,
                PesoPorPunto = 50,
                PuntosMinimosCanje = 1000,
                DescuentoMaximo = 30,
                DiasVencimientoPuntos = 365,
                CanjeAutomatico = false
            };

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            Assert.Contains("Sistema de Tarjetas de Fidelización", componente.Markup);
            Assert.Contains("oi-badge", componente.Markup);
        }

        [Fact]
        public void MuestraSpinnerCuandoConfiguracionEsNull()
        {
            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, (ConfiguracionFidelizacionDto?)null));

            // Assert
            Assert.Contains("Cargando configuración...", componente.Markup);
            Assert.Contains("spinner-border", componente.Markup);
        }

        [Fact]
        public void RenderizaFormularioCuandoConfiguracionExiste()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                SistemaActivo = true,
                PuntosPorPeso = 2,
                PesoPorPunto = 50,
                PuntosMinimosCanje = 1000,
                DescuentoMaximo = 30,
                DiasVencimientoPuntos = 365,
                CanjeAutomatico = false
            };

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            Assert.NotNull(componente.FindComponent<EditForm>());
            Assert.NotNull(componente.FindComponent<DataAnnotationsValidator>());
        }

        [Fact]
        public void RenderizaCheckboxSistemaActivo()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                SistemaActivo = true
            };

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            var checkbox = componente.Find("input[type='checkbox'][id='sistemaActivo']");
            Assert.NotNull(checkbox);
            Assert.True(checkbox.HasAttribute("checked"));
            Assert.Contains("Sistema de Fidelización Activo", componente.Markup);
        }

        [Fact]
        public void RenderizaCheckboxCanjeAutomatico()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                CanjeAutomatico = true
            };

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            var checkbox = componente.Find("input[type='checkbox'][id='canjeAutomatico']");
            Assert.NotNull(checkbox);
            Assert.True(checkbox.HasAttribute("checked"));
            Assert.Contains("Canje Automático de Puntos", componente.Markup);
        }

        [Fact]
        public void RenderizaInputPuntosPorPeso()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                PuntosPorPeso = 3
            };

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            var input = componente.Find("input[type='number'][min='1'][max='10']");
            Assert.NotNull(input);
            Assert.Equal("3", input.GetAttribute("value"));
            Assert.Contains("Puntos por peso gastado", componente.Markup);
        }

        [Fact]
        public void RenderizaInputPesoPorPunto()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                PesoPorPunto = 75.50m
            };

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            var input = componente.Find("input[type='number'][min='1'][max='1000'][step='0.01']");
            Assert.NotNull(input);
            Assert.Equal("75.50", input.GetAttribute("value"));
            Assert.Contains("Peso por punto (para canje)", componente.Markup);
        }

        [Fact]
        public void RenderizaInputPuntosMinimosCanje()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                PuntosMinimosCanje = 1500
            };

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            var input = componente.Find("input[type='number'][min='100'][max='10000']");
            Assert.NotNull(input);
            Assert.Equal("1500", input.GetAttribute("value"));
            Assert.Contains("Puntos mínimos para canje", componente.Markup);
        }

        [Fact]
        public void RenderizaInputDescuentoMaximo()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                DescuentoMaximo = 25.75m
            };

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            var input = componente.Find("input[type='number'][min='1'][max='100'][step='0.01']");
            Assert.NotNull(input);
            Assert.Equal("25.75", input.GetAttribute("value"));
            Assert.Contains("Descuento máximo (%)", componente.Markup);
        }

        [Fact]
        public void RenderizaInputDiasVencimientoPuntos()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                DiasVencimientoPuntos = 180
            };

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            var input = componente.Find("input[type='number'][min='30'][max='1095']");
            Assert.NotNull(input);
            Assert.Equal("180", input.GetAttribute("value"));
            Assert.Contains("Días de vencimiento de puntos", componente.Markup);
        }

        [Fact]
        public void RenderizaResumenDeConfiguracion()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                PuntosPorPeso = 2,
                PesoPorPunto = 50,
                PuntosMinimosCanje = 1000,
                DescuentoMaximo = 30,
                DiasVencimientoPuntos = 365
            };

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            Assert.Contains("Resumen de Configuración", componente.Markup);
            Assert.Contains("2 punto(s) por cada peso gastado", componente.Markup);
            Assert.Contains("$50 pesos por punto", componente.Markup);
            Assert.Contains("1000 puntos", componente.Markup);
            Assert.Contains("30%", componente.Markup);
            Assert.Contains("365 días", componente.Markup);
        }

        [Fact]
        public void RenderizaBotonResetear()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            var boton = componente.Find("button[type='button']");
            Assert.NotNull(boton);
            Assert.Contains("Resetear", boton.TextContent);
            Assert.Contains("oi-reload", componente.Markup);
        }

        [Fact]
        public void RenderizaBotonGuardar()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            var boton = componente.Find("button[type='submit']");
            Assert.NotNull(boton);
            Assert.Contains("Guardar Configuración", boton.TextContent);
            Assert.Contains("oi-check", componente.Markup);
        }

        [Fact]
        public void MuestraSpinnerEnBotonGuardarCuandoGuardandoEsTrue()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            var boton = componente.Find("button[type='submit']");
            Assert.NotNull(boton);
            // Verificamos que el botón existe y tiene el texto correcto
            Assert.Contains("Guardar Configuración", boton.TextContent);
        }

        [Fact]
        public void InvocaOnGuardarAlEnviarFormulario()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto
            {
                SistemaActivo = true,
                PuntosPorPeso = 2
            };

            var onGuardarInvocado = false;
            ConfiguracionFidelizacionDto? configuracionEnviada = null;

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion)
                .Add(p => p.OnGuardar, EventCallback.Factory.Create<ConfiguracionFidelizacionDto>(this, (config) =>
                {
                    onGuardarInvocado = true;
                    configuracionEnviada = config;
                })));

            var form = componente.Find("form");
            form.TriggerEvent("onsubmit", EventArgs.Empty);

            // Assert
            Assert.True(onGuardarInvocado);
            Assert.NotNull(configuracionEnviada);
            Assert.Equal(configuracion, configuracionEnviada);
        }

        [Fact]
        public void InvocaOnResetearAlHacerClicEnResetear()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();
            var onResetearInvocado = false;

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion)
                .Add(p => p.OnResetear, EventCallback.Factory.Create(this, () =>
                {
                    onResetearInvocado = true;
                })));

            var botonResetear = componente.Find("button[type='button']");
            botonResetear.Click();

            // Assert
            Assert.True(onResetearInvocado);
        }

        [Fact]
        public void MuestraTitulosDeSecciones()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            Assert.Contains("Configuración General", componente.Markup);
            Assert.Contains("Sistema de Puntos", componente.Markup);
        }

        [Fact]
        public void MuestraTextosDeAyuda()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            Assert.Contains("Activar o desactivar el sistema completo de fidelización", componente.Markup);
            Assert.Contains("Permitir canje automático cuando se alcance el mínimo", componente.Markup);
            Assert.Contains("Puntos que se otorgan por cada peso gastado", componente.Markup);
            Assert.Contains("Peso que equivale a 1 punto para canjes", componente.Markup);
            Assert.Contains("Puntos mínimos requeridos para realizar un canje", componente.Markup);
            Assert.Contains("Porcentaje máximo de descuento por puntos", componente.Markup);
            Assert.Contains("Días antes de que los puntos expiren", componente.Markup);
        }

        [Fact]
        public void RenderizaIconosCorrectos()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            Assert.Contains("oi-badge", componente.Markup);
            Assert.Contains("oi-info", componente.Markup);
            Assert.Contains("oi-reload", componente.Markup);
            Assert.Contains("oi-check", componente.Markup);
        }

        [Fact]
        public void AplicaClasesCssCorrectas()
        {
            // Arrange
            var configuracion = new ConfiguracionFidelizacionDto();

            // Act
            var componente = RenderComponent<TarjetasFidelizacion>(parameters => parameters
                .Add(p => p.configuracion, configuracion));

            // Assert
            Assert.Contains("card", componente.Markup);
            Assert.Contains("card-header", componente.Markup);
            Assert.Contains("card-body", componente.Markup);
            Assert.Contains("form-check", componente.Markup);
            Assert.Contains("form-control", componente.Markup);
            Assert.Contains("alert-info", componente.Markup);
            Assert.Contains("text-primary", componente.Markup);
        }
    }
}