using RestaurantePro.Domain.Core.Productos.Entities;
using RestaurantePro.Domain.Core.Usuarios.Entities;

namespace RestaurantePro.Domain.Core.Services
{
    public interface ICoreServiceFacade
    {
        Task<Result<Producto>> CrearProducto(string nombre, string descripcion, decimal precio, Guid categoriaId, string? imagenUrl, CancellationToken cancellationToken);
        
        Task<Result<Producto>> ActualizarProducto(Guid id, string nombre, string descripcion, decimal precio, Guid categoriaId, string? imagenUrl, CancellationToken cancellationToken);
        
        Task<Result<Receta>> AsignarRecetaAProducto(Guid productoId, Dictionary<Guid, decimal> ingredientes, CancellationToken cancellationToken);
        
        Task<Result<Dictionary<Guid, decimal>>> ObtenerIngredientesParaProductoAsync(Guid productoId, CancellationToken cancellationToken);
        
        Task<Result<bool>> VerificarDisponibilidadIngredientesAsync(Guid productoId, int cantidad, CancellationToken cancellationToken);
        
        Task<Result<decimal>> CalcularCostoRecetaAsync(Guid productoId, CancellationToken cancellationToken);
        
        Task<Result<Usuario>> CrearUsuario(string nombreUsuario, string email, string rol, CancellationToken cancellationToken);
    }
}