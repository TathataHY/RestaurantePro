/*
 * Implementación del patrón Result/Notification (Julio 2025)
 * 
 * Esta clase ha sido adaptada para utilizar los patrones Result y Notification.
 * Se ha reemplazado la clase ResultadoStockBajoPolicy por Result<StockBajoPolicyData>.
 * 
 * Características implementadas:
 * - Los métodos devuelven Task<Result<StockBajoPolicyData>>
 * - Se utiliza _notificationManager para validaciones y acumulación de errores
 * - Se convierten excepciones a errores en Result
 * - Se retorna Result.Success o Result.Failure según corresponda
 * - Se usa la clase StockBajoPolicyData para contener los datos de resultado
 */

namespace RestaurantePro.Domain.Inventario.Policies
{
    /// <summary>
    /// Implementación de la política de stock bajo
    /// </summary>
    public class StockBajoPolicy : IStockBajoPolicy
    {
        private readonly IIngredienteRepository _ingredienteRepository;
        private readonly IServicioNotificacionesInventario _servicioNotificaciones;
        private readonly IVerificadorStock _verificadorStock;
        private readonly IDateTimeService _dateTimeService;
        private readonly INotificationManager _notificationManager;
        
        // Ponderaciones para el cálculo de prioridades
        private const int PONDERACION_ROTACION = 40;
        private const int PONDERACION_TEMPORADA = 30;
        private const int PONDERACION_STOCK = 20;
        private const int PONDERACION_COSTO = 10;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public StockBajoPolicy(
            IIngredienteRepository ingredienteRepository,
            IServicioNotificacionesInventario servicioNotificaciones,
            IVerificadorStock verificadorStock,
            IDateTimeService dateTimeService,
            INotificationManager notificationManager)
        {
            _ingredienteRepository = ingredienteRepository ?? throw new ArgumentNullException(nameof(ingredienteRepository));
            _servicioNotificaciones = servicioNotificaciones ?? throw new ArgumentNullException(nameof(servicioNotificaciones));
            _verificadorStock = verificadorStock ?? throw new ArgumentNullException(nameof(verificadorStock));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
            _notificationManager = notificationManager ?? throw new ArgumentNullException(nameof(notificationManager));
        }
        
        /// <inheritdoc />
        public async Task<Result<StockBajoPolicyData>> EjecutarPolicy(CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                var resultado = new StockBajoPolicyData();
                
                // Priorizar ingredientes según rotación y temporada actual
                var resultadoPriorizacion = await PriorizarIngredientesParaReposicion(true, cancellationToken);
                if (!resultadoPriorizacion.Succeeded)
                {
                    return resultadoPriorizacion;
                }
                
                // Si no hay ingredientes con stock bajo, no hay acciones
                if (!resultadoPriorizacion.Value.IngredientesPriorizados.Any())
                {
                    return Result.Success(resultado);
                }
                
                // Transferir los ingredientes priorizados al resultado
                resultado.IngredientesPriorizados.AddRange(resultadoPriorizacion.Value.IngredientesPriorizados);
                
                // Enviar notificaciones para cada ingrediente con stock bajo, ordenados por prioridad
                foreach (var ingredientePriorizado in resultado.IngredientesPriorizados)
                {
                    try
                    {
                        var notificacionId = await _servicioNotificaciones.NotificarStockBajo(
                            ingredientePriorizado.IngredienteId,
                            ingredientePriorizado.Nombre,
                            ingredientePriorizado.Stock,
                            ingredientePriorizado.StockMinimo);
                            
                        resultado.Notificaciones.Add(notificacionId);
                    }
                    catch (Exception ex)
                    {
                        _notificationManager.AddError(
                            $"Error al notificar stock bajo para ingrediente {ingredientePriorizado.Nombre}: {ex.Message}", 
                            "NotificarStockBajo");
                    }
                }
                
                // Generar órdenes de compra automáticas
                var resultadoVerificacion = await _verificadorStock.VerificarYGenerarOrdenesCompraAsync(cancellationToken);
                
                // Registrar las órdenes generadas en el resultado
                if (resultadoVerificacion.Succeeded)
                {
                    foreach (var orden in resultadoVerificacion.Value.OrdenesGeneradas)
                    {
                        resultado.OrdenesCompraGeneradas.Add(orden.Id);
                        
                        try
                        {
                            // Notificar sobre la orden de compra generada
                            await _servicioNotificaciones.NotificarOrdenCompraGenerada(
                                orden.Id,
                                orden.ProveedorId,
                                "Proveedor"); // Idealmente, obtendríamos el nombre real del proveedor
                        }
                        catch (Exception ex)
                        {
                            _notificationManager.AddError(
                                $"Error al notificar orden generada {orden.Id}: {ex.Message}",
                                "NotificarOrdenCompra");
                        }
                    }
                }
                
                if (_notificationManager.HasErrors)
                {
                    return _notificationManager.ToResult(resultado);
                }
                
                return Result.Success(resultado);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al ejecutar política de stock bajo: {ex.Message}", "EjecutarPolicy");
                return _notificationManager.ToResult<StockBajoPolicyData>(new StockBajoPolicyData());
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<StockBajoPolicyData>> EjecutarPolicyParaIngrediente(Guid ingredienteId, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            // Validar parámetros
            _notificationManager.Require(ingredienteId != Guid.Empty, "El ID del ingrediente no puede estar vacío", "IngredienteId");
            
            if (_notificationManager.HasErrors)
            {
                return _notificationManager.ToResult<StockBajoPolicyData>(null);
            }
            
            try
            {
                var resultado = new StockBajoPolicyData();
                
                // Obtener el ingrediente
                var ingrediente = await _ingredienteRepository.ObtenerPorIdAsync(ingredienteId, cancellationToken);
                
                if (ingrediente == null)
                {
                    return Result.Failure<StockBajoPolicyData>($"No existe un ingrediente con el ID {ingredienteId}");
                }
                
                // Verificar si el stock está por debajo del mínimo
                if (ingrediente.Stock >= ingrediente.StockMinimo)
                {
                    return Result.Success(resultado); // El stock no está bajo, no hay acciones a tomar
                }
                
                // Calcular prioridad del ingrediente
                int prioridad = CalcularPrioridadIngrediente(
                    ingrediente.Rotacion, 
                    ingrediente.Temporada, 
                    ingrediente.Stock, 
                    ingrediente.StockMinimo,
                    ingrediente.CostoPromedio,
                    _dateTimeService.Now);
                
                var ingredientePriorizado = new IngredientePriorizado(
                    ingrediente.Id,
                    ingrediente.Nombre,
                    prioridad,
                    ingrediente.Rotacion,
                    ingrediente.Temporada,
                    ingrediente.Stock,
                    ingrediente.StockMinimo);
                    
                resultado.IngredientesPriorizados.Add(ingredientePriorizado);
                
                try
                {
                    // Enviar notificación de stock bajo
                    var notificacionId = await _servicioNotificaciones.NotificarStockBajo(
                        ingrediente.Id,
                        ingrediente.Nombre,
                        ingrediente.Stock,
                        ingrediente.StockMinimo);
                        
                    resultado.Notificaciones.Add(notificacionId);
                }
                catch (Exception ex)
                {
                    _notificationManager.AddError(
                        $"Error al notificar stock bajo para ingrediente {ingrediente.Nombre}: {ex.Message}", 
                        "NotificarStockBajo");
                }
                
                // Verificar si tiene proveedor principal
                if (!ingrediente.ProveedorPrincipalId.HasValue)
                {
                    if (_notificationManager.HasErrors)
                    {
                        return _notificationManager.ToResult(resultado);
                    }
                    
                    return Result.Success(resultado); // No se puede generar orden sin proveedor
                }
                
                // Intentar generar una orden de compra para este ingrediente
                var resultadoVerificacion = await _verificadorStock.VerificarYGenerarOrdenesCompraAsync(cancellationToken);
                
                // Registrar las órdenes generadas en el resultado
                if (resultadoVerificacion.Succeeded)
                {
                    foreach (var orden in resultadoVerificacion.Value.OrdenesGeneradas)
                    {
                        if (orden.Items.Any(i => i.IngredienteId == ingredienteId))
                        {
                            resultado.OrdenesCompraGeneradas.Add(orden.Id);
                            
                            try
                            {
                                // Notificar sobre la orden de compra generada
                                await _servicioNotificaciones.NotificarOrdenCompraGenerada(
                                    orden.Id,
                                    orden.ProveedorId,
                                    "Proveedor"); // Idealmente, obtendríamos el nombre real del proveedor
                            }
                            catch (Exception ex)
                            {
                                _notificationManager.AddError(
                                    $"Error al notificar orden generada {orden.Id}: {ex.Message}",
                                    "NotificarOrdenCompra");
                            }
                        }
                    }
                }
                
                if (_notificationManager.HasErrors)
                {
                    return _notificationManager.ToResult(resultado);
                }
                
                return Result.Success(resultado);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al ejecutar política para ingrediente: {ex.Message}", "EjecutarPolicyParaIngrediente");
                return _notificationManager.ToResult<StockBajoPolicyData>(new StockBajoPolicyData());
            }
        }
        
        /// <inheritdoc />
        public async Task<Result<StockBajoPolicyData>> PriorizarIngredientesParaReposicion(bool considerarTemporadaActual = true, CancellationToken cancellationToken = default)
        {
            _notificationManager.CreateNewNotification();
            
            try
            {
                var resultado = new StockBajoPolicyData();
                
                // Obtener los ingredientes con stock bajo
                var ingredientes = await _ingredienteRepository.ObtenerConStockBajoAsync();
                
                if (!ingredientes.Any())
                {
                    return Result.Success(resultado); // No hay ingredientes con stock bajo
                }
                
                // Fecha actual para cálculos de temporada
                var fechaActual = _dateTimeService.Now;
                
                // Calcular prioridades y crear lista de ingredientes priorizados
                foreach (var ingrediente in ingredientes)
                {
                    try
                    {
                        // Calcular prioridad del ingrediente
                        int prioridad = CalcularPrioridadIngrediente(
                            ingrediente.Rotacion, 
                            ingrediente.Temporada, 
                            ingrediente.Stock, 
                            ingrediente.StockMinimo,
                            ingrediente.CostoPromedio,
                            fechaActual,
                            considerarTemporadaActual);
                        
                        var ingredientePriorizado = new IngredientePriorizado(
                            ingrediente.Id,
                            ingrediente.Nombre,
                            prioridad,
                            ingrediente.Rotacion,
                            ingrediente.Temporada,
                            ingrediente.Stock,
                            ingrediente.StockMinimo);
                            
                        resultado.IngredientesPriorizados.Add(ingredientePriorizado);
                    }
                    catch (Exception ex)
                    {
                        _notificationManager.AddError(
                            $"Error al priorizar ingrediente {ingrediente.Nombre}: {ex.Message}", 
                            "PriorizarIngrediente");
                    }
                }
                
                // Ordenar por prioridad (mayor a menor)
                resultado.IngredientesPriorizados.Sort((a, b) => b.Prioridad.CompareTo(a.Prioridad));
                
                if (_notificationManager.HasErrors)
                {
                    return _notificationManager.ToResult(resultado);
                }
                
                return Result.Success(resultado);
            }
            catch (Exception ex)
            {
                _notificationManager.AddError($"Error al priorizar ingredientes: {ex.Message}", "PriorizarIngredientes");
                return _notificationManager.ToResult<StockBajoPolicyData>(new StockBajoPolicyData());
            }
        }
        
        /// <summary>
        /// Calcula la prioridad de un ingrediente basado en varios factores
        /// </summary>
        /// <param name="rotacion">Nivel de rotación del ingrediente</param>
        /// <param name="temporada">Temporada del ingrediente</param>
        /// <param name="stock">Stock actual</param>
        /// <param name="stockMinimo">Stock mínimo</param>
        /// <param name="costoPromedio">Costo promedio</param>
        /// <param name="fechaActual">Fecha actual para cálculos de temporada</param>
        /// <param name="considerarTemporadaActual">Si se debe considerar la temporada actual</param>
        /// <returns>Valor de prioridad (mayor número = mayor prioridad)</returns>
        private int CalcularPrioridadIngrediente(
            RotacionIngrediente rotacion, 
            TemporadaIngrediente temporada, 
            decimal stock, 
            decimal stockMinimo,
            decimal costoPromedio,
            DateTime fechaActual,
            bool considerarTemporadaActual = true)
        {
            // 1. Calcular puntuación por rotación (0-100)
            int puntuacionRotacion = rotacion switch
            {
                RotacionIngrediente.Critica => 100,
                RotacionIngrediente.Alta => 75,
                RotacionIngrediente.Media => 50,
                RotacionIngrediente.Baja => 25,
                _ => 0
            };
            
            // 2. Calcular puntuación por temporada (0-100)
            int puntuacionTemporada = 50; // Valor por defecto medio
            
            if (considerarTemporadaActual)
            {
                // Determinar la temporada actual
                TemporadaIngrediente temporadaActual = ObtenerTemporadaActual(fechaActual);
                
                // Si el ingrediente está en temporada, aumentar su prioridad
                if (temporada == temporadaActual)
                {
                    puntuacionTemporada = 100;
                }
                else if (temporada == TemporadaIngrediente.TodoElAño)
                {
                    puntuacionTemporada = 70;
                }
                else
                {
                    // Fuera de temporada tiene menor prioridad
                    puntuacionTemporada = 30;
                }
            }
            
            // 3. Calcular puntuación por nivel de stock (0-100)
            // Cuanto más cerca esté de 0 el stock, mayor será la puntuación
            decimal porcentajeStock = stockMinimo > 0 ? (stock / stockMinimo) * 100 : 100;
            int puntuacionStock = 100 - (int)Math.Min(100, Math.Max(0, porcentajeStock));
            
            // 4. Puntuación por costo (ingredientes más baratos primero, para optimizar flujo de caja)
            // Asumimos un máximo arbitrario de 1000 para normalizar
            int puntuacionCosto = costoPromedio > 0 
                ? 100 - (int)Math.Min(100, (costoPromedio / 1000) * 100) 
                : 50; // Si no tiene costo asignado, valor neutro
            
            // Calcular prioridad ponderada
            int prioridad = 
                (puntuacionRotacion * PONDERACION_ROTACION +
                puntuacionTemporada * PONDERACION_TEMPORADA +
                puntuacionStock * PONDERACION_STOCK +
                puntuacionCosto * PONDERACION_COSTO) / 100;
                
            return prioridad;
        }
        
        /// <summary>
        /// Determina la temporada actual basada en la fecha
        /// </summary>
        private TemporadaIngrediente ObtenerTemporadaActual(DateTime fecha)
        {
            // Ajustado para hemisferio sur
            int mes = fecha.Month;
            
            return mes switch
            {
                >= 9 and <= 11 => TemporadaIngrediente.Primavera,
                12 or 1 or 2 => TemporadaIngrediente.Verano,
                >= 3 and <= 5 => TemporadaIngrediente.Otoño,
                >= 6 and <= 8 => TemporadaIngrediente.Invierno,
                _ => TemporadaIngrediente.TodoElAño  // Por si acaso, aunque nunca debería llegar aquí
            };
        }

        /// <inheritdoc />
        public async Task<Result<StockBajoPolicyData>> EjecutarAsync(CancellationToken cancellationToken = default)
        {
            // Este método es simplemente un alias de EjecutarPolicy
            return await EjecutarPolicy(cancellationToken);
        }
    }
} 