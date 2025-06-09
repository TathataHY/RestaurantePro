using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Common.Models;
using RestaurantePro.Application.Operaciones.Comandas.DTOs;
using RestaurantePro.Domain.Core.SharedKernel.Results;
using RestaurantePro.Domain.Operaciones.Comandas.Entities;
using RestaurantePro.Domain.Operaciones.Comandas.Enums;
using RestaurantePro.Domain.Operaciones.Comandas.Interfaces;
using RestaurantePro.Domain.Operaciones.Preparaciones.Services;

namespace RestaurantePro.Application.Operaciones.Comandas.Commands.AgregarItemComanda;

/// <summary>
/// Handler para agregar un producto a una comanda existente
/// Gestiona las reglas de negocio para modificación de comandas
/// </summary>
public class AgregarItemComandaHandler : IRequestHandler<AgregarItemComandaCommand, Result<ComandaDto>>
{
    private readonly IComandaRepository _comandaRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AgregarItemComandaHandler> _logger;
    private readonly IServicioPreparaciones _servicioPreparaciones;

    public AgregarItemComandaHandler(
        IComandaRepository comandaRepository,
        IMapper mapper,
        ILogger<AgregarItemComandaHandler> logger,
        IServicioPreparaciones servicioPreparaciones)
    {
        _comandaRepository = comandaRepository;
        _mapper = mapper;
        _logger = logger;
        _servicioPreparaciones = servicioPreparaciones;
    }

    public async Task<Result<ComandaDto>> Handle(AgregarItemComandaCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("🛒 Iniciando proceso de agregar item a comanda {ComandaId} - Producto: {ProductoId} x{Cantidad}", 
            request.ComandaId, request.ProductoId, request.Cantidad);

        try
        {
            // 1. Buscar la comanda existente
            var comanda = await _comandaRepository.ObtenerPorIdAsync(request.ComandaId);
            if (comanda == null)
            {
                _logger.LogWarning("❌ Comanda no encontrada: {ComandaId}", request.ComandaId);
                return Result.Failure<ComandaDto>("La comanda especificada no existe");
            }

            // 2. Verificar que la comanda se puede modificar
            if (!PuedeModificarComanda(comanda))
            {
                _logger.LogWarning("🚫 Intento de modificar comanda en estado no permitido. ComandaId: {ComandaId}, Estado: {Estado}", 
                    request.ComandaId, comanda.Estado);
                return Result.Failure<ComandaDto>($"No se puede agregar items a una comanda en estado '{comanda.Estado}'");
            }

            // 3. Verificar disponibilidad en preparaciones si es un producto preparado
            var disponibilidadResult = await _servicioPreparaciones.VerificarDisponibilidadAsync(request.ProductoId, request.Cantidad);
            
            // Si hay disponibilidad de preparaciones, se consumirá después de agregar el producto
            bool usarPreparacion = disponibilidadResult.Succeeded && disponibilidadResult.Value;

            // 4. Agregar el producto usando el método del dominio
            var itemResult = await AgregarProductoAComanda(comanda, request);
            if (!itemResult.Succeeded)
            {
                return Result.Failure<ComandaDto>(itemResult.Error);
            }

            // 5. Si hay preparaciones disponibles, consumirlas
            if (usarPreparacion)
            {
                var consumoResult = await _servicioPreparaciones.ConsumirPreparacionAsync(request.ProductoId, request.Cantidad);
                if (!consumoResult.Succeeded)
                {
                    _logger.LogWarning("⚠️ No se pudo consumir preparación. Error: {Error}", consumoResult.Error);
                    // No fallamos toda la operación si no se puede consumir la preparación
                    // Solo registramos la advertencia
                }
                else
                {
                    _logger.LogInformation("✅ Preparación consumida exitosamente. Producto: {ProductoId}, Cantidad: {Cantidad}", 
                        request.ProductoId, request.Cantidad);
                }
            }

            // 6. Guardar los cambios
            await _comandaRepository.ActualizarAsync(comanda);
            await _comandaRepository.GuardarCambiosAsync();

            // 7. Mapear y retornar el resultado
            var comandaDto = _mapper.Map<ComandaDto>(comanda);

            _logger.LogInformation("✅ Item agregado exitosamente a comanda {ComandaId}. Nuevo total: ${Total:F2}", 
                request.ComandaId, comandaDto.Total);

            return Result.Success(comandaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error al agregar item a comanda {ComandaId}", request.ComandaId);
            return Result.Failure<ComandaDto>($"Error al procesar la solicitud: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si una comanda se puede modificar según su estado actual
    /// </summary>
    private static bool PuedeModificarComanda(Comanda comanda)
    {
        return comanda.Estado == EstadoComanda.Creada || comanda.Estado == EstadoComanda.EnProceso;
    }

    /// <summary>
    /// Agrega un producto a la comanda usando los métodos del dominio
    /// </summary>
    private async Task<Result> AgregarProductoAComanda(Comanda comanda, AgregarItemComandaCommand request)
    {
        try
        {
            // Agregar el producto a la comanda
            var item = comanda.AgregarItem(
                request.ProductoId,
                request.NombreProducto,
                request.Cantidad,
                request.PrecioUnitario,
                request.Observaciones);

            // Agregar personalizaciones si existen
            if (request.Personalizaciones != null && request.Personalizaciones.Any())
            {
                foreach (var personalizacion in request.Personalizaciones)
                {
                    switch (personalizacion.Tipo)
                    {
                        case "Extra":
                            comanda.AgregarPersonalizacionExtra(
                                item.Id,
                                personalizacion.IngredienteId,
                                personalizacion.NombreIngrediente,
                                personalizacion.Cantidad,
                                personalizacion.PrecioAdicional);
                            break;
                        case "Quitar":
                            comanda.AgregarPersonalizacionQuitar(
                                item.Id,
                                personalizacion.IngredienteId,
                                personalizacion.NombreIngrediente);
                            break;
                        case "Sustituir":
                            if (!personalizacion.IngredienteSustitucionId.HasValue)
                            {
                                return Result.Failure("Se requiere el ingrediente de sustitución para personalizaciones de tipo 'Sustituir'");
                            }
                            
                            comanda.AgregarPersonalizacionSustituir(
                                item.Id,
                                personalizacion.IngredienteId,
                                personalizacion.NombreIngrediente,
                                personalizacion.IngredienteSustitucionId.Value,
                                personalizacion.NombreIngredienteSustitucion ?? string.Empty,
                                personalizacion.Cantidad,
                                personalizacion.PrecioAdicional);
                            break;
                        default:
                            return Result.Failure($"Tipo de personalización no soportado: {personalizacion.Tipo}");
                    }
                }
            }

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Error al agregar producto a comanda: {Error}", ex.Message);
            return Result.Failure(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Error de validación al agregar producto: {Error}", ex.Message);
            return Result.Failure(ex.Message);
        }
    }
} 
