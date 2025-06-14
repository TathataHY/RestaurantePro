using RestaurantePro.Domain.Core.SharedKernel.Interfaces;
using RestaurantePro.Domain.Proveedores.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantePro.Domain.Proveedores.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de contactos de proveedores
    /// </summary>
    public interface IContactoProveedorRepository : IRepository<ContactoProveedor>
    {
        /// <summary>
        /// Obtiene todos los contactos de un proveedor
        /// </summary>
        /// <param name="proveedorId">ID del proveedor</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de contactos del proveedor</returns>
        Task<IEnumerable<ContactoProveedor>> ObtenerPorProveedorAsync(Guid proveedorId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca contactos por nombre o cargo
        /// </summary>
        /// <param name="termino">Término de búsqueda</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de contactos que coinciden con el término</returns>
        Task<IEnumerable<ContactoProveedor>> BuscarPorNombreOCargoAsync(string termino, CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca contactos por email
        /// </summary>
        /// <param name="email">Email a buscar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Contacto encontrado o null si no existe</returns>
        Task<ContactoProveedor?> BuscarPorEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Busca contactos por teléfono
        /// </summary>
        /// <param name="telefono">Teléfono a buscar</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Lista de contactos que coinciden con el teléfono</returns>
        Task<IEnumerable<ContactoProveedor>> BuscarPorTelefonoAsync(string telefono, CancellationToken cancellationToken = default);
    }
} 