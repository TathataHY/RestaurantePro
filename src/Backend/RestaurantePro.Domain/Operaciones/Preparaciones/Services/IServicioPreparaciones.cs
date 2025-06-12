using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Preparaciones.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Operaciones.Preparaciones.Services;

/// <summary>
/// Servicio de dominio para gestionar preparaciones diarias
/// </summary>
public interface IServicioPreparaciones
{
    /// <summary>
    /// Prepara un producto con una cantidad específica
    /// </summary>
    /// <param name="productoId">ID del producto a preparar</param>
    /// <param name="cantidad">Cantidad a preparar</param>
    /// <param name="chefId">ID del chef que realiza la preparación</param>
    /// <param name="fechaVencimiento">Fecha de vencimiento opcional</param>
    /// <param name="observaciones">Observaciones adicionales</param>
    /// <returns>La preparación creada</returns>
    Task<Result<PreparacionDiaria>> PrepararProductoAsync(
        Guid productoId, 
        int cantidad, 
        Guid chefId,
        DateTime fechaVencimiento,
        string? observaciones = null);

    /// <summary>
    /// Verifica si hay suficiente cantidad preparada de un producto
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <param name="cantidadRequerida">Cantidad requerida</param>
    /// <param name="preparacionId">ID de una preparación específica (opcional)</param>
    /// <returns>True si hay suficiente cantidad disponible</returns>
    Task<Result<bool>> VerificarDisponibilidadAsync(
        Guid productoId, 
        int cantidadRequerida,
        Guid? preparacionId = null);

    /// <summary>
    /// Consume una cantidad específica de un producto preparado
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <param name="cantidad">Cantidad a consumir</param>
    /// <returns>Resultado de la operación</returns>
    Task<Result> ConsumirPreparacionAsync(Guid productoId, int cantidad);

    /// <summary>
    /// Obtiene todas las preparaciones del día actual
    /// </summary>
    /// <returns>Lista de preparaciones</returns>
    Task<Result<List<PreparacionDiaria>>> ObtenerPreparacionesDelDiaAsync();

    /// <summary>
    /// Obtiene las preparaciones de un producto específico
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>Lista de preparaciones del producto</returns>
    Task<Result<List<PreparacionDiaria>>> ObtenerPreparacionesPorProductoAsync(Guid productoId);

    /// <summary>
    /// Marca las preparaciones vencidas automáticamente
    /// </summary>
    /// <returns>Número de preparaciones marcadas como vencidas</returns>
    Task<Result<int>> MarcarVencidasAsync();

    /// <summary>
    /// Obtiene las preparaciones que están por vencer
    /// </summary>
    /// <param name="horasAnticipacion">Horas de anticipación para la alerta</param>
    /// <returns>Lista de preparaciones por vencer</returns>
    Task<Result<List<PreparacionDiaria>>> ObtenerPreparacionesPorVencerAsync(int horasAnticipacion = 2); 

    /// <summary>
    /// Marca una preparación como disponible para consumo
    /// </summary>
    /// <param name="preparacionId">ID de la preparación</param>
    /// <returns>Resultado de la operación</returns>
    Task<Result> MarcarComoDisponibleAsync(Guid preparacionId);

    /// <summary>
    /// Agrega cantidad adicional a una preparación existente
    /// </summary>
    /// <param name="preparacionId">ID de la preparación</param>
    /// <param name="cantidadAdicional">Cantidad adicional a agregar</param>
    /// <returns>Resultado de la operación</returns>
    Task<Result> AgregarCantidadAsync(Guid preparacionId, int cantidadAdicional);

    /// <summary>
    /// Obtiene estadísticas de preparaciones del día
    /// </summary>
    /// <returns>Estadísticas de preparaciones</returns>
    Task<Result<EstadisticasPreparaciones>> ObtenerEstadisticasDelDiaAsync();
}

/// <summary>
/// Estadísticas de preparaciones diarias
/// </summary>
public class EstadisticasPreparaciones
{
    public int TotalPreparaciones { get; set; }
    public int PreparacionesDisponibles { get; set; }
    public int PreparacionesAgotadas { get; set; }
    public int PreparacionesVencidas { get; set; }
    public int PreparacionesPorVencer { get; set; }
    public decimal PorcentajeEficiencia { get; set; }
    public int CantidadTotalPreparada { get; set; }
    public int CantidadTotalConsumida { get; set; }
    public int CantidadDesperdiciada { get; set; }
} 