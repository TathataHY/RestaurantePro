using RestaurantePro.Domain.Core.Base;
using RestaurantePro.Domain.Inventario.Enums;
using RestaurantePro.Domain.Inventario.Events;

namespace RestaurantePro.Domain.Inventario.Entities
{
    /// <summary>
    /// Entidad que representa un ingrediente en el inventario
    /// </summary>
    public class Ingrediente : EntityBase
    {
        /// <summary>
        /// Nombre del ingrediente
        /// </summary>
        public string Nombre { get; private set; }
        
        /// <summary>
        /// Unidad de medida del ingrediente
        /// </summary>
        public UnidadMedida UnidadMedida { get; private set; }
        
        /// <summary>
        /// Stock mu00ednimo recomendado del ingrediente
        /// </summary>
        public decimal StockMinimo { get; private set; }
        
        /// <summary>
        /// Stock actual del ingrediente
        /// </summary>
        public decimal Stock { get; private set; }
        
        /// <summary>
        /// Indica si el ingrediente estu00e1 activo en el sistema
        /// </summary>
        public bool EstaActivo { get; private set; }
        
        // Constructor privado para EF Core
        private Ingrediente() { }
        
        /// <summary>
        /// Crea una nueva instancia de ingrediente
        /// </summary>
        /// <param name="nombre">Nombre del ingrediente</param>
        /// <param name="unidadMedida">Unidad de medida del ingrediente</param>
        /// <param name="stockMinimo">Stock mu00ednimo recomendado</param>
        /// <returns>Una nueva instancia de Ingrediente</returns>
        /// <exception cref="ArgumentException">Si los datos no son vu00e1lidos</exception>
        public static Ingrediente Crear(string nombre, UnidadMedida unidadMedida, decimal stockMinimo)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre no puede estar vacu00edo", nameof(nombre));
                
            if (stockMinimo < 0)
                throw new ArgumentException("El stock mu00ednimo no puede ser negativo", nameof(stockMinimo));
                
            var ingrediente = new Ingrediente
            {
                Nombre = nombre,
                UnidadMedida = unidadMedida,
                StockMinimo = stockMinimo,
                Stock = 0,
                EstaActivo = true
            };
            
            ingrediente.AddDomainEvent(new IngredienteCreadoEvent(ingrediente.Id, nombre));
            
            return ingrediente;
        }
        
        /// <summary>
        /// Incrementa el stock del ingrediente
        /// </summary>
        /// <param name="cantidad">Cantidad a incrementar</param>
        /// <exception cref="ArgumentException">Si la cantidad es negativa</exception>
        public void IncrementarStock(decimal cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));
                
            Stock += cantidad;
            MarkAsModified();
            
            AddDomainEvent(new StockActualizadoEvent(Id, Nombre, Stock));
        }
        
        /// <summary>
        /// Decrementa el stock del ingrediente
        /// </summary>
        /// <param name="cantidad">Cantidad a decrementar</param>
        /// <exception cref="ArgumentException">Si la cantidad es negativa o mayor al stock actual</exception>
        public void DecrementarStock(decimal cantidad)
        {
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor que cero", nameof(cantidad));
                
            if (cantidad > Stock)
                throw new ArgumentException("No hay suficiente stock disponible", nameof(cantidad));
                
            Stock -= cantidad;
            MarkAsModified();
            
            if (Stock < StockMinimo)
            {
                AddDomainEvent(new StockBajoMinimoEvent(Id, Nombre, Stock, StockMinimo));
            }
            
            AddDomainEvent(new StockActualizadoEvent(Id, Nombre, Stock));
        }
        
        /// <summary>
        /// Actualiza el stock mu00ednimo del ingrediente
        /// </summary>
        /// <param name="nuevoStockMinimo">Nuevo valor de stock mu00ednimo</param>
        /// <exception cref="ArgumentException">Si el valor es negativo</exception>
        public void ActualizarStockMinimo(decimal nuevoStockMinimo)
        {
            if (nuevoStockMinimo < 0)
                throw new ArgumentException("El stock mu00ednimo no puede ser negativo", nameof(nuevoStockMinimo));
                
            StockMinimo = nuevoStockMinimo;
            MarkAsModified();
            
            if (Stock < StockMinimo)
            {
                AddDomainEvent(new StockBajoMinimoEvent(Id, Nombre, Stock, StockMinimo));
            }
        }
        
        /// <summary>
        /// Desactiva el ingrediente
        /// </summary>
        public void Desactivar()
        {
            if (!EstaActivo)
                return;
                
            EstaActivo = false;
            MarkAsModified();
            
            AddDomainEvent(new IngredienteDesactivadoEvent(Id, Nombre));
        }
        
        /// <summary>
        /// Activa el ingrediente
        /// </summary>
        public void Activar()
        {
            if (EstaActivo)
                return;
                
            EstaActivo = true;
            MarkAsModified();
            
            AddDomainEvent(new IngredienteActivadoEvent(Id, Nombre));
        }
    }
}
