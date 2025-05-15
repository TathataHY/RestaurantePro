# Modelo de Datos RestaurantePro

Este documento describe las entidades principales del sistema RestaurantePro y sus relaciones.

## Diagrama Entidad-Relación

*Nota: Se incluirá un diagrama ER completo en formato imagen en este documento cuando esté disponible.*

## Entidades Principales

### Usuario

Representa a los usuarios del sistema con diferentes roles.

```csharp
public class Usuario
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string PasswordHash { get; set; }
    public string Rol { get; set; } // Admin, Mesero, Cocinero
    public bool Activo { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    
    // Relaciones
    public virtual ICollection<Comanda> Comandas { get; set; }
}
```

### Mesa

Representa las mesas físicas del restaurante.

```csharp
public class Mesa
{
    public int Id { get; set; }
    public string Numero { get; set; }
    public int Capacidad { get; set; }
    public string Estado { get; set; } // Libre, Ocupada, Reservada
    public string Ubicacion { get; set; }
    public bool Activa { get; set; }
    
    // Relaciones
    public virtual ICollection<Comanda> Comandas { get; set; }
    public virtual ICollection<Reservacion> Reservaciones { get; set; }
}
```

### Categoria

Categorías para organizar los productos del menú.

```csharp
public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int Orden { get; set; }
    public bool Activa { get; set; }
    
    // Relaciones
    public virtual ICollection<Producto> Productos { get; set; }
}
```

### Producto

Representa los platos o productos disponibles en el menú.

```csharp
public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public string ImagenUrl { get; set; }
    public bool Disponible { get; set; }
    public int? TiempoPreparacion { get; set; } // En minutos
    public int CategoriaId { get; set; }
    
    // Relaciones
    public virtual Categoria Categoria { get; set; }
    public virtual ICollection<ComandaDetalle> ComandaDetalles { get; set; }
    public virtual ICollection<ProductoIngrediente> Ingredientes { get; set; }
}
```

### Comanda

Representa un pedido realizado en una mesa.

```csharp
public class Comanda
{
    public int Id { get; set; }
    public DateTime FechaHora { get; set; }
    public int MesaId { get; set; }
    public Guid MeseroId { get; set; }
    public string Estado { get; set; } // Pendiente, EnPreparacion, Lista, Entregada, Pagada, Cancelada
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
    public string Observaciones { get; set; }
    
    // Relaciones
    public virtual Mesa Mesa { get; set; }
    public virtual Usuario Mesero { get; set; }
    public virtual ICollection<ComandaDetalle> Detalles { get; set; }
    public virtual ICollection<Pago> Pagos { get; set; }
}
```

### ComandaDetalle

Representa cada producto incluido en una comanda.

```csharp
public class ComandaDetalle
{
    public int Id { get; set; }
    public int ComandaId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public string Estado { get; set; } // Pendiente, EnPreparacion, Listo, Entregado, Cancelado
    public string Observaciones { get; set; }
    public DateTime? TiempoPreparacion { get; set; }
    
    // Relaciones
    public virtual Comanda Comanda { get; set; }
    public virtual Producto Producto { get; set; }
}
```

### Pago

Registra los pagos realizados para las comandas.

```csharp
public class Pago
{
    public int Id { get; set; }
    public int ComandaId { get; set; }
    public string MetodoPago { get; set; } // Efectivo, Tarjeta, etc.
    public decimal Monto { get; set; }
    public DateTime FechaHora { get; set; }
    public string Referencia { get; set; }
    public Guid UsuarioId { get; set; }
    
    // Relaciones
    public virtual Comanda Comanda { get; set; }
    public virtual Usuario Usuario { get; set; }
}
```

### Ingrediente

Representa los ingredientes utilizados en los productos.

```csharp
public class Ingrediente
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Unidad { get; set; } // kg, l, unidad, etc.
    public decimal StockActual { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal PrecioCompra { get; set; }
    public bool Activo { get; set; }
    
    // Relaciones
    public virtual ICollection<ProductoIngrediente> Productos { get; set; }
    public virtual ICollection<InventarioMovimiento> Movimientos { get; set; }
}
```

### ProductoIngrediente

Relación entre productos e ingredientes (tabla intermedia).

```csharp
public class ProductoIngrediente
{
    public int ProductoId { get; set; }
    public int IngredienteId { get; set; }
    public decimal Cantidad { get; set; }
    
    // Relaciones
    public virtual Producto Producto { get; set; }
    public virtual Ingrediente Ingrediente { get; set; }
}
```

### InventarioMovimiento

Registra los movimientos de entrada y salida de ingredientes.

```csharp
public class InventarioMovimiento
{
    public int Id { get; set; }
    public int IngredienteId { get; set; }
    public string TipoMovimiento { get; set; } // Entrada, Salida, Ajuste
    public decimal Cantidad { get; set; }
    public DateTime FechaHora { get; set; }
    public string Referencia { get; set; }
    public Guid UsuarioId { get; set; }
    
    // Relaciones
    public virtual Ingrediente Ingrediente { get; set; }
    public virtual Usuario Usuario { get; set; }
}
```

## Relaciones Principales

1. Un **Usuario** puede crear muchas **Comandas** (1:N)
2. Una **Mesa** puede tener muchas **Comandas** a lo largo del tiempo (1:N)
3. Una **Comanda** contiene muchos **DetallesComanda** (1:N)
4. Un **Producto** puede estar en muchos **DetallesComanda** (1:N)
5. Una **Categoria** contiene muchos **Productos** (1:N)
6. Un **Producto** utiliza varios **Ingredientes** a través de la tabla **ProductoIngrediente** (N:M)
7. Un **Ingrediente** tiene muchos **MovimientosInventario** (1:N)
8. Una **Comanda** puede tener varios **Pagos** (1:N)

## Consideraciones

1. Se utilizarán enumeraciones para los estados de las entidades (Mesa, Comanda, ComandaDetalle).
2. Las relaciones se configurarán en el contexto de Entity Framework Core.
3. Se implementarán validaciones en el nivel de dominio para garantizar la integridad de los datos.
4. Se considerará la auditoría de cambios para entidades críticas. 