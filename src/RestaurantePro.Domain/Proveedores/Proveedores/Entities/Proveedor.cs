using RestaurantePro.Domain.Common;
using System;
using System.Collections.Generic;

namespace RestaurantePro.Domain.Entities
{
    public class Proveedor : BaseEntity
    {
        public string Nombre { get; set; }
        public string NombreContacto { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string CodigoPostal { get; set; }
        public string Pais { get; set; }
        public string RFC { get; set; }
        public string InformacionBancaria { get; set; }
        public int DiasCredito { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimaOrden { get; set; }
        public string Observaciones { get; set; }
        
        public virtual ICollection<OrdenCompra> OrdenesCompra { get; set; } = new List<OrdenCompra>();
        public virtual ICollection<ProveedorCategoria> CategoriasProveidas { get; set; } = new List<ProveedorCategoria>();
        public virtual ICollection<ProveedorIngrediente> IngredientesProveidos { get; set; } = new List<ProveedorIngrediente>();
    }
} 