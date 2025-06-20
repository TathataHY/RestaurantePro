using Microsoft.Extensions.Logging;
using RestaurantePro.Application.Operaciones.Mesas.Interfaces;
using RestaurantePro.Application.Operaciones.Mesas.DTOs;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Interfaces;
using RestaurantePro.Domain.Operaciones.Reservaciones.Mesas.Enums;
using RestaurantePro.Domain.Core.SharedKernel.Results;

namespace RestaurantePro.Infrastructure.Services
{
    /// <summary>
    /// Implementación de IMesaService que actúa como adapter del repositorio de dominio
    /// </summary>
    public class MesaService : IMesaService
    {
        private readonly IMesaRepository _mesaRepository;
        private readonly ILogger<MesaService> _logger;

        public MesaService(
            IMesaRepository mesaRepository,
            ILogger<MesaService> logger)
        {
            _mesaRepository = mesaRepository ?? throw new ArgumentNullException(nameof(mesaRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Libera una mesa específica
        /// </summary>
        public async Task<Result<LiberarMesaResult>> LiberarMesaAsync(Guid mesaId, Guid usuarioId, CancellationToken cancellationToken)
        {
            try
            {
                if (mesaId == Guid.Empty)
                    return Result.Failure<LiberarMesaResult>("El ID de la mesa no puede estar vacío");

                if (usuarioId == Guid.Empty)
                    return Result.Failure<LiberarMesaResult>("El ID del usuario no puede estar vacío");

                // Obtener la mesa
                var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
                if (mesa == null)
                {
                    _logger.LogWarning("⚠️ Mesa con ID {MesaId} no encontrada", mesaId);
                    return Result.Failure<LiberarMesaResult>($"Mesa con ID {mesaId} no encontrada");
                }

                // Verificar que se pueda liberar
                if (mesa.Estado == EstadoMesa.Disponible)
                {
                    _logger.LogInformation("ℹ️ Mesa {MesaId} ya está disponible", mesaId);
                    return Result.Success(new LiberarMesaResult
                    {
                        MesaId = mesaId,
                        EstadoAnterior = mesa.Estado.ToString(),
                        Estado = "Disponible",
                        Mensaje = "La mesa ya estaba disponible",
                        Exitoso = true
                    });
                }

                // Liberar mesa
                mesa.MarcarComoDisponible();
                await _mesaRepository.ActualizarAsync(mesa);
                await _mesaRepository.GuardarCambiosAsync();

                _logger.LogInformation("✅ Mesa {MesaId} liberada exitosamente por usuario {UsuarioId}", mesaId, usuarioId);

                return Result.Success(new LiberarMesaResult
                {
                    MesaId = mesaId,
                    EstadoAnterior = "Ocupada", // Simplificado
                    Estado = "Disponible",
                    Mensaje = "Mesa liberada exitosamente",
                    Exitoso = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al liberar mesa {MesaId}", mesaId);
                return Result.Failure<LiberarMesaResult>($"Error interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Cambia el estado de una mesa
        /// </summary>
        public async Task<Result<MesaDto>> CambiarEstadoMesaAsync(Guid mesaId, string nuevoEstado, Guid usuarioId, CancellationToken cancellationToken)
        {
            try
            {
                if (mesaId == Guid.Empty)
                    return Result.Failure<MesaDto>("El ID de la mesa no puede estar vacío");

                if (string.IsNullOrWhiteSpace(nuevoEstado))
                    return Result.Failure<MesaDto>("El nuevo estado no puede estar vacío");

                if (usuarioId == Guid.Empty)
                    return Result.Failure<MesaDto>("El ID del usuario no puede estar vacío");

                // Obtener la mesa
                var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
                if (mesa == null)
                {
                    _logger.LogWarning("⚠️ Mesa con ID {MesaId} no encontrada", mesaId);
                    return Result.Failure<MesaDto>($"Mesa con ID {mesaId} no encontrada");
                }

                // Cambiar estado según el valor
                switch (nuevoEstado.ToLowerInvariant())
                {
                    case "disponible":
                        mesa.MarcarComoDisponible();
                        break;
                    case "ocupada":
                        mesa.MarcarComoOcupada();
                        break;
                    case "reservada":
                        mesa.MarcarComoReservada();
                        break;
                    case "fueradeservicio":
                        mesa.MarcarComoFueraDeServicio("Cambiado desde servicio de aplicación");
                        break;
                    default:
                        return Result.Failure<MesaDto>($"Estado '{nuevoEstado}' no válido");
                }

                await _mesaRepository.ActualizarAsync(mesa);
                await _mesaRepository.GuardarCambiosAsync();

                _logger.LogInformation("✅ Estado de mesa {MesaId} cambiado a {NuevoEstado} por usuario {UsuarioId}", 
                    mesaId, nuevoEstado, usuarioId);

                // Mapear a DTO
                var mesaDto = new MesaDto
                {
                    Id = mesa.Id,
                    Numero = mesa.Numero.ToString(),
                    Capacidad = mesa.Capacidad,
                    Estado = mesa.Estado.ToString(),
                    Zona = mesa.Ubicacion,
                    UltimaActualizacion = mesa.FechaActualizacion ?? DateTime.Now
                };

                return Result.Success(mesaDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cambiar estado de mesa {MesaId} a {NuevoEstado}", mesaId, nuevoEstado);
                return Result.Failure<MesaDto>($"Error interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Asigna una mesa a un cliente o grupo
        /// </summary>
        public async Task<Result<MesaDto>> AsignarMesaAsync(Guid mesaId, Guid? clienteId, Guid usuarioId, int numeroPersonas, CancellationToken cancellationToken)
        {
            try
            {
                if (mesaId == Guid.Empty)
                    return Result.Failure<MesaDto>("El ID de la mesa no puede estar vacío");

                if (usuarioId == Guid.Empty)
                    return Result.Failure<MesaDto>("El ID del usuario no puede estar vacío");

                if (numeroPersonas <= 0)
                    return Result.Failure<MesaDto>("El número de personas debe ser mayor a cero");

                // Obtener la mesa
                var mesa = await _mesaRepository.ObtenerPorIdAsync(mesaId);
                if (mesa == null)
                {
                    _logger.LogWarning("⚠️ Mesa con ID {MesaId} no encontrada", mesaId);
                    return Result.Failure<MesaDto>($"Mesa con ID {mesaId} no encontrada");
                }

                // Verificar disponibilidad
                if (mesa.Estado != EstadoMesa.Disponible)
                {
                    return Result.Failure<MesaDto>($"La mesa está {mesa.Estado} y no puede ser asignada");
                }

                // Verificar capacidad
                if (mesa.Capacidad < numeroPersonas)
                {
                    return Result.Failure<MesaDto>($"La mesa tiene capacidad para {mesa.Capacidad} personas, pero se requiere para {numeroPersonas}");
                }

                // Asignar mesa
                mesa.MarcarComoOcupada();
                await _mesaRepository.ActualizarAsync(mesa);
                await _mesaRepository.GuardarCambiosAsync();

                _logger.LogInformation("✅ Mesa {MesaId} asignada a cliente {ClienteId} para {NumeroPersonas} personas por usuario {UsuarioId}", 
                    mesaId, clienteId, numeroPersonas, usuarioId);

                // Mapear a DTO
                var mesaDto = new MesaDto
                {
                    Id = mesa.Id,
                    Numero = mesa.Numero.ToString(),
                    Capacidad = mesa.Capacidad,
                    Estado = mesa.Estado.ToString(),
                    ClienteId = clienteId,
                    Zona = mesa.Ubicacion,
                    UltimaActualizacion = mesa.FechaActualizacion ?? DateTime.Now
                };

                return Result.Success(mesaDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al asignar mesa {MesaId} a cliente {ClienteId}", mesaId, clienteId);
                return Result.Failure<MesaDto>($"Error interno: {ex.Message}");
            }
        }
    }
} 