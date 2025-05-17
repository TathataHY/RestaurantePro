namespace RestaurantePro.Domain.Comercial.Policies
{
    using RestaurantePro.Domain.Comercial.Clientes.Interfaces;
    using RestaurantePro.Domain.Comercial.Clientes.Entities;
    using RestaurantePro.Domain.Comercial.Clientes.Enums;
    using RestaurantePro.Domain.Comercial.Services;
    using RestaurantePro.Domain.Core.SharedKernel.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementación de la política de clientes frecuentes que analiza los patrones
    /// de consumo y actualiza los niveles de fidelización automáticamente
    /// </summary>
    public class ClientesFrecuentesPolicy : IClientesFrecuentesPolicy
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly ITarjetaFidelizacionRepository _tarjetaRepository;
        private readonly IServicioFidelizacion _servicioFidelizacion;
        private readonly IDateTimeService _dateTimeService;
        
        // Umbrales para determinar los niveles según la cantidad de visitas
        private const int UMBRAL_PLATA = 10;
        private const int UMBRAL_ORO = 20;
        private const int UMBRAL_PLATINO = 30;
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ClientesFrecuentesPolicy(
            IClienteRepository clienteRepository,
            ITarjetaFidelizacionRepository tarjetaRepository,
            IServicioFidelizacion servicioFidelizacion,
            IDateTimeService dateTimeService)
        {
            _clienteRepository = clienteRepository ?? throw new ArgumentNullException(nameof(clienteRepository));
            _tarjetaRepository = tarjetaRepository ?? throw new ArgumentNullException(nameof(tarjetaRepository));
            _servicioFidelizacion = servicioFidelizacion ?? throw new ArgumentNullException(nameof(servicioFidelizacion));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }
        
        /// <inheritdoc />
        public async Task<ResultadoClientesFrecuentesPolicy> EjecutarPolicy(CancellationToken cancellationToken = default)
        {
            var resultado = new ResultadoClientesFrecuentesPolicy();
            
            // Obtener todos los clientes activos con información de visitas
            var clientes = await _clienteRepository.ObtenerClientesActivosConVisitasAsync(cancellationToken);
            
            if (!clientes.Any())
            {
                return resultado; // No hay clientes que procesar
            }
            
            // Procesar cada cliente
            foreach (var cliente in clientes)
            {
                var resultadoCliente = await ProcesarCliente(cliente, cancellationToken);
                
                // Agregar los resultados de este cliente al resultado general
                if (resultadoCliente.ClientesActualizados.Any())
                {
                    resultado.ClientesActualizados.AddRange(resultadoCliente.ClientesActualizados);
                }
                
                if (resultadoCliente.TarjetasCreadas.Any())
                {
                    resultado.TarjetasCreadas.AddRange(resultadoCliente.TarjetasCreadas);
                }
            }
            
            return resultado;
        }
        
        /// <inheritdoc />
        public async Task<ResultadoClientesFrecuentesPolicy> EjecutarPolicyParaCliente(Guid clienteId, CancellationToken cancellationToken = default)
        {
            var resultado = new ResultadoClientesFrecuentesPolicy();
            
            // Obtener el cliente específico
            var cliente = await _clienteRepository.ObtenerPorIdAsync(clienteId, cancellationToken);
            
            if (cliente == null)
            {
                throw new ArgumentException($"No existe un cliente con el ID {clienteId}", nameof(clienteId));
            }
            
            if (!cliente.EstaActivo)
            {
                return resultado; // No procesamos clientes inactivos
            }
            
            // Procesar el cliente
            var resultadoCliente = await ProcesarCliente(cliente, cancellationToken);
            
            // Agregar los resultados al resultado general
            if (resultadoCliente.ClientesActualizados.Any())
            {
                resultado.ClientesActualizados.AddRange(resultadoCliente.ClientesActualizados);
            }
            
            if (resultadoCliente.TarjetasCreadas.Any())
            {
                resultado.TarjetasCreadas.AddRange(resultadoCliente.TarjetasCreadas);
            }
            
            return resultado;
        }
        
        /// <summary>
        /// Procesa un cliente y actualiza su nivel de fidelización según sus visitas
        /// </summary>
        private async Task<ResultadoClientesFrecuentesPolicy> ProcesarCliente(Cliente cliente, CancellationToken cancellationToken)
        {
            var resultado = new ResultadoClientesFrecuentesPolicy();
            
            // Obtener la tarjeta de fidelización del cliente
            var tarjeta = await _tarjetaRepository.ObtenerTarjetaActivaPorClienteIdAsync(cliente.Id, cancellationToken);
            
            // Si no tiene tarjeta, crear una
            if (tarjeta == null)
            {
                tarjeta = TarjetaFidelizacion.Crear(
                    cliente.Id,
                    $"TF-{DateTime.Now:yyyyMMdd}-{cliente.Id.ToString().Substring(0, 8)}"
                );
                tarjeta.Activar();
                
                await _tarjetaRepository.AgregarAsync(tarjeta, cancellationToken);
                resultado.TarjetasCreadas.Add(tarjeta.Id);
            }
            
            // Determinar el nivel que debería tener según sus visitas
            NivelFidelizacion nivelSegunVisitas = DeterminarNivelSegunVisitas(cliente.CantidadVisitas);
            
            // Si el nivel actual es diferente del que debería tener, actualizarlo
            if (tarjeta.NivelFidelizacion != nivelSegunVisitas)
            {
                tarjeta.ActualizarNivel(nivelSegunVisitas);
                await _tarjetaRepository.ActualizarAsync(tarjeta, cancellationToken);
                resultado.ClientesActualizados.Add(cliente.Id);
            }
            
            return resultado;
        }
        
        /// <summary>
        /// Determina el nivel de fidelización que corresponde según la cantidad de visitas
        /// </summary>
        private NivelFidelizacion DeterminarNivelSegunVisitas(int cantidadVisitas)
        {
            if (cantidadVisitas >= UMBRAL_PLATINO)
            {
                return NivelFidelizacion.Platino;
            }
            else if (cantidadVisitas >= UMBRAL_ORO)
            {
                return NivelFidelizacion.Oro;
            }
            else if (cantidadVisitas >= UMBRAL_PLATA)
            {
                return NivelFidelizacion.Plata;
            }
            else
            {
                return NivelFidelizacion.Basico;
            }
        }
    }
} 