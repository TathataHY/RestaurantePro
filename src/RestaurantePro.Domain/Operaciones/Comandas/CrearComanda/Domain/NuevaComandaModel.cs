using System;
using System.Collections.Generic;
using RestaurantePro.Domain.Core.Base.Entities;

namespace RestaurantePro.Domain.Operaciones.Comandas.CrearComanda.Domain
{
    /// <summary>
    /// Modelo de dominio específico para la operación de crear comanda
    /// </summary>
    public class NuevaComandaModel
    {
        public int MesaId { get; }
        public int UsuarioId { get; }
        public List<NuevoProductoComanda> Productos { get; }
        public string Notas { get; }
        
        private NuevaComandaModel(int mesaId, int usuarioId, List<NuevoProductoComanda> productos, string notas)
        {
            MesaId = mesaId;
            UsuarioId = usuarioId;
            Productos = productos;
            Notas = notas;
        }
        
        public static NuevaComandaModel Crear(int mesaId, int usuarioId, List<NuevoProductoComanda> productos, string notas)
        {
            if (mesaId <= 0)
                throw new ArgumentException("Mesa inválida", nameof(mesaId));
                
            if (usuarioId <= 0)
                throw new ArgumentException("Usuario inválido", nameof(usuarioId));
                
            if (productos == null || productos.Count == 0)
                throw new ArgumentException("Debe incluir al menos un producto", nameof(productos));
                
            return new NuevaComandaModel(mesaId, usuarioId, productos, notas);
        }
    }
    
    public class NuevoProductoComanda
    {
        public int ProductoId { get; }
        public int Cantidad { get; }
        public string Notas { get; }
        public List<PersonalizacionProducto> Personalizaciones { get; }
        
        public NuevoProductoComanda(int productoId, int cantidad, string notas, List<PersonalizacionProducto> personalizaciones = null)
        {
            if (productoId <= 0)
                throw new ArgumentException("Producto inválido", nameof(productoId));
                
            if (cantidad <= 0)
                throw new ArgumentException("Cantidad debe ser mayor a cero", nameof(cantidad));
                
            ProductoId = productoId;
            Cantidad = cantidad;
            Notas = notas;
            Personalizaciones = personalizaciones ?? new List<PersonalizacionProducto>();
        }
    }
    
    public class PersonalizacionProducto
    {
        public string Tipo { get; }
        public string Valor { get; }
        
        public PersonalizacionProducto(string tipo, string valor)
        {
            if (string.IsNullOrWhiteSpace(tipo))
                throw new ArgumentException("Tipo de personalización requerido", nameof(tipo));
                
            Tipo = tipo;
            Valor = valor;
        }
    }
}
