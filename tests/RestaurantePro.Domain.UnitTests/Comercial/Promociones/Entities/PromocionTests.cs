using System;
using System.Linq;
using Xunit;
using RestaurantePro.Domain.Comercial.Promociones.Entities;
using RestaurantePro.Domain.Comercial.Promociones.Enums;
using RestaurantePro.Domain.Comercial.Promociones.Events;

namespace RestaurantePro.Domain.UnitTests.Comercial.Promociones.Entities
{
    /// <summary>
    /// Pruebas unitarias para la entidad Promocion
    /// </summary>
    public class PromocionTests
    {
        [Fact]
        public void Crear_ConParametrosValidos_DebeCrearPromocion()
        {
            // Arrange
            var codigo = "PROMO001";
            var nombre = "Promoción de prueba";
            var descripcion = "Descripción de la promoción de prueba";
            var tipo = TipoPromocion.PorcentajeTotal;
            var valorDescuento = 10m;
            var fechaInicio = DateTime.Now.AddDays(1);
            var fechaFin = DateTime.Now.AddDays(30);
            var montoMinimo = 1000m;
            
            // Act
            var promocion = Promocion.Crear(
                codigo,
                nombre,
                descripcion,
                tipo,
                valorDescuento,
                fechaInicio,
                fechaFin,
                montoMinimo);
                
            // Assert
            Assert.NotNull(promocion);
            Assert.Equal(codigo, promocion.Codigo);
            Assert.Equal(nombre, promocion.Nombre);
            Assert.Equal(descripcion, promocion.Descripcion);
            Assert.Equal(tipo, promocion.Tipo);
            Assert.Equal(valorDescuento, promocion.ValorDescuento);
            Assert.Equal(fechaInicio, promocion.FechaInicio);
            Assert.Equal(fechaFin, promocion.FechaFin);
            Assert.Equal(montoMinimo, promocion.MontoMinimo);
            Assert.Equal(EstadoPromocion.Creada, promocion.Estado);
            Assert.Equal(0, promocion.VecesUsada);
            
            // Verificar evento de dominio
            var eventos = promocion.DomainEvents;
            Assert.Single(eventos);
            Assert.IsType<PromocionCreada>(eventos.First());
            
            var evento = (PromocionCreada)eventos.First();
            Assert.Equal(promocion.Id, evento.PromocionId);
            Assert.Equal(codigo, evento.Codigo);
            Assert.Equal(nombre, evento.Nombre);
        }
        
        [Fact]
        public void Crear_ConPorcentajeMayorA100_DebeLanzarExcepcion()
        {
            // Arrange
            var codigo = "PROMO001";
            var nombre = "Promoción de prueba";
            var descripcion = "Descripción de la promoción de prueba";
            var tipo = TipoPromocion.PorcentajeTotal;
            var valorDescuento = 110m; // Porcentaje inválido
            var fechaInicio = DateTime.Now.AddDays(1);
            var fechaFin = DateTime.Now.AddDays(30);
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                Promocion.Crear(
                    codigo,
                    nombre,
                    descripcion,
                    tipo,
                    valorDescuento,
                    fechaInicio,
                    fechaFin));
                    
            Assert.Contains("porcentaje de descuento", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        
        [Fact]
        public void Crear_ConFechaFinAnteriorAInicio_DebeLanzarExcepcion()
        {
            // Arrange
            var codigo = "PROMO001";
            var nombre = "Promoción de prueba";
            var descripcion = "Descripción de la promoción de prueba";
            var tipo = TipoPromocion.PorcentajeTotal;
            var valorDescuento = 10m;
            var fechaInicio = DateTime.Now.AddDays(30);
            var fechaFin = DateTime.Now.AddDays(1); // Anterior a la fecha de inicio
            
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() =>
                Promocion.Crear(
                    codigo,
                    nombre,
                    descripcion,
                    tipo,
                    valorDescuento,
                    fechaInicio,
                    fechaFin));
                    
            Assert.Contains("fecha de inicio", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        
        [Fact]
        public void Activar_PromocionCreada_DebeActivarYGenerarEvento()
        {
            // Arrange
            var promocion = CrearPromocionPrueba();
            Assert.Equal(EstadoPromocion.Creada, promocion.Estado);
            
            // Act
            promocion.Activar();
            
            // Assert
            Assert.Equal(EstadoPromocion.Activa, promocion.Estado);
            
            // Verificar evento de dominio
            var eventos = promocion.DomainEvents.Skip(1).ToList(); // Saltamos el evento de creación
            Assert.Single(eventos);
            Assert.IsType<PromocionEstadoActualizado>(eventos.First());
            
            var evento = (PromocionEstadoActualizado)eventos.First();
            Assert.Equal(promocion.Id, evento.PromocionId);
            Assert.Equal(EstadoPromocion.Creada, evento.EstadoAnterior);
            Assert.Equal(EstadoPromocion.Activa, evento.NuevoEstado);
        }
        
        [Fact]
        public void EstaVigente_PromocionActivaEnFechasValidas_DebeRetornarTrue()
        {
            // Arrange
            var promocion = CrearPromocionPrueba(
                fechaInicio: DateTime.Now.AddDays(-1),
                fechaFin: DateTime.Now.AddDays(10));
                
            promocion.Activar();
            
            // Act
            var resultado = promocion.EstaVigente();
            
            // Assert
            Assert.True(resultado);
        }
        
        [Fact]
        public void EstaVigente_PromocionNoActiva_DebeRetornarFalse()
        {
            // Arrange
            var promocion = CrearPromocionPrueba(
                fechaInicio: DateTime.Now.AddDays(-1),
                fechaFin: DateTime.Now.AddDays(10));
                
            // Sin activar, sigue en estado Creada
            
            // Act
            var resultado = promocion.EstaVigente();
            
            // Assert
            Assert.False(resultado);
        }
        
        [Fact]
        public void RegistrarUso_PromocionActiva_DebeIncrementarVecesUsada()
        {
            // Arrange
            var promocion = CrearPromocionPrueba();
            promocion.Activar();
            
            var clienteId = Guid.NewGuid();
            var comandaId = Guid.NewGuid();
            var montoAplicado = 100m;
            
            // Act
            promocion.RegistrarUso(clienteId, comandaId, montoAplicado);
            
            // Assert
            Assert.Equal(1, promocion.VecesUsada);
            Assert.Contains(clienteId, promocion.ClientesQueUsaronIds);
            
            // Verificar evento de dominio
            var eventos = promocion.DomainEvents.Skip(2).ToList(); // Saltamos eventos de creación y activación
            Assert.Single(eventos);
            Assert.IsType<PromocionUsada>(eventos.First());
            
            var evento = (PromocionUsada)eventos.First();
            Assert.Equal(promocion.Id, evento.PromocionId);
            Assert.Equal(clienteId, evento.ClienteId);
            Assert.Equal(comandaId, evento.ComandaId);
            Assert.Equal(montoAplicado, evento.MontoAplicado);
        }
        
        [Fact]
        public void RegistrarUso_PromocionNoActiva_DebeLanzarExcepcion()
        {
            // Arrange
            var promocion = CrearPromocionPrueba();
            // No activamos la promoción
            
            var clienteId = Guid.NewGuid();
            var comandaId = Guid.NewGuid();
            var montoAplicado = 100m;
            
            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                promocion.RegistrarUso(clienteId, comandaId, montoAplicado));
                
            Assert.Contains("estado", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        
        [Fact]
        public void CalcularDescuento_PorcentajeTotal_DebeCalcularCorrectamente()
        {
            // Arrange
            var promocion = CrearPromocionPrueba(
                tipo: TipoPromocion.PorcentajeTotal,
                valorDescuento: 15m);
                
            promocion.Activar();
            var montoOriginal = 1000m;
            
            // Act
            var descuento = promocion.CalcularDescuento(montoOriginal);
            
            // Assert
            Assert.Equal(150m, descuento); // 15% de 1000 = 150
        }
        
        [Fact]
        public void CalcularDescuento_MontoFijoTotal_DebeCalcularCorrectamente()
        {
            // Arrange
            var promocion = CrearPromocionPrueba(
                tipo: TipoPromocion.MontoFijoTotal,
                valorDescuento: 200m);
                
            promocion.Activar();
            var montoOriginal = 1000m;
            
            // Act
            var descuento = promocion.CalcularDescuento(montoOriginal);
            
            // Assert
            Assert.Equal(200m, descuento);
        }
        
        [Fact]
        public void CalcularDescuento_MontoFijoMayorQueOriginal_DebeRetornarMontoOriginal()
        {
            // Arrange
            var promocion = CrearPromocionPrueba(
                tipo: TipoPromocion.MontoFijoTotal,
                valorDescuento: 1200m);
                
            promocion.Activar();
            var montoOriginal = 1000m;
            
            // Act
            var descuento = promocion.CalcularDescuento(montoOriginal);
            
            // Assert
            Assert.Equal(1000m, descuento); // No puede exceder el monto original
        }
        
        [Fact]
        public void Cancelar_PromocionActiva_DebeCancelarYGenerarEvento()
        {
            // Arrange
            var promocion = CrearPromocionPrueba();
            promocion.Activar();
            var motivo = "Prueba de cancelación";
            
            // Act
            promocion.Cancelar(motivo);
            
            // Assert
            Assert.Equal(EstadoPromocion.Cancelada, promocion.Estado);
            
            // Verificar evento de dominio
            var eventos = promocion.DomainEvents.Skip(2).ToList(); // Saltamos eventos de creación y activación
            Assert.Single(eventos);
            Assert.IsType<PromocionCancelada>(eventos.First());
            
            var evento = (PromocionCancelada)eventos.First();
            Assert.Equal(promocion.Id, evento.PromocionId);
            Assert.Equal(motivo, evento.Motivo);
        }
        
        #region Métodos Auxiliares
        
        private Promocion CrearPromocionPrueba(
            string? codigo = null,
            string? nombre = null,
            string? descripcion = null,
            TipoPromocion? tipo = null,
            decimal? valorDescuento = null,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            decimal? montoMinimo = null)
        {
            return Promocion.Crear(
                codigo ?? "PROMO_TEST",
                nombre ?? "Promoción de Prueba",
                descripcion ?? "Descripción de prueba",
                tipo ?? TipoPromocion.PorcentajeTotal,
                valorDescuento ?? 10m,
                fechaInicio ?? DateTime.Now.AddDays(-1),
                fechaFin ?? DateTime.Now.AddDays(30),
                montoMinimo ?? 0m
            );
        }
        
        #endregion
    }
} 