# Migraciones por Contexto - RestaurantePro

Esta carpeta contiene las migraciones de Entity Framework Core organizadas por contexto de dominio.

## 📁 **Estructura Organizacional**

```
Migrations/
├── Core/                    # Migración principal (todas las entidades)
│   ├── 20250630061428_InitialCreate.cs
│   ├── 20250630061428_InitialCreate.Designer.cs
│   └── RestauranteProDbContextModelSnapshot.cs
├── Comercial/               # Migraciones específicas del contexto Comercial
├── Operaciones/             # Migraciones específicas del contexto Operaciones
├── Inventario/              # Migraciones específicas del contexto Inventario
└── Proveedores/             # Migraciones específicas del contexto Proveedores
```

## 🗄️ **Contexto Actual**

Actualmente utilizamos **RestauranteProDbContext** como contexto único que incluye todas las entidades de todos los dominios:

- **Core**: Productos, Usuarios, Notificaciones, Recetas
- **Comercial**: Clientes, Facturas, TarjetasFidelización
- **Operaciones**: Comandas, Reservaciones, Mesas, Preparaciones
- **Inventario**: Ingredientes, MovimientosInventario, OrdenesCompra
- **Proveedores**: Proveedores, ContactosProveedores

## ✅ **Migración Inicial Creada**

**Fecha**: 30/06/2025  
**Migración**: `20250630061428_InitialCreate`  
**Estado**: ✅ Generada correctamente

### **Tablas Creadas**:
- **AspNetRoles** - Roles de Identity
- **AspNetUsers** - Usuarios de Identity  
- **Clientes** (esquema Comercial)
- **Mesas** (esquema Operaciones)
- **OrdenesCompra** (esquema Inventario)
- **PreparacionesDiarias** (esquema Operaciones)
- **ProductoCategorias** (esquema Core)
- **Productos** (esquema Core)
- **Recetas** (esquema Core)
- **Usuarios** (esquema Core)
- **Notificaciones** (esquema Core)
- **TarjetasFidelizacion** (esquema Comercial)
- **Facturas** (esquema Comercial)
- **Promociones** (esquema Comercial)
- **Comandas** (esquema Operaciones)
- **ItemsComanda** (esquema Operaciones)
- **Reservaciones** (esquema Operaciones)
- **Ingredientes** (esquema Inventario)
- **MovimientosInventario** (esquema Inventario)
- **Proveedores** (esquema Proveedores)
- **ContactosProveedor** (esquema Proveedores)

### **Esquemas Creados**:
- **Core** - Entidades centrales
- **Comercial** - Gestión comercial
- **Operaciones** - Operaciones diarias
- **Inventario** - Control de inventario
- **Proveedores** - Gestión de proveedores

## ⚙️ **Comandos de Migración**

### **Generar Nueva Migración**
```bash
# Para el contexto principal
dotnet ef migrations add [NombreMigration] --context RestauranteProDbContext --output-dir Persistence/Migrations/Core
```

### **Aplicar Migraciones**
```bash
# Aplicar todas las migraciones pendientes
dotnet ef database update --context RestauranteProDbContext
```

### **Eliminar Última Migración**
```bash
dotnet ef migrations remove --context RestauranteProDbContext
```

### **Ver Migraciones Pendientes**
```bash
dotnet ef migrations list --context RestauranteProDbContext
```

## 🔮 **Evolución Futura**

En futuras versiones, podríamos separar en contextos independientes:

- **CoreDbContext** para entidades centrales
- **ComercialDbContext** para gestión comercial
- **OperacionesDbContext** para operaciones diarias
- **InventarioDbContext** para control de inventario
- **ProveedoresDbContext** para gestión de proveedores

### **Ventajas de Separación Futura:**
- ✅ **Escalabilidad** independiente por módulo
- ✅ **Deployment** separado por contexto
- ✅ **Performance** optimizada por dominio
- ✅ **Mantenimiento** simplificado

## 💾 **Base de Datos**

**Servidor**: SQL Server Express (`TATHATA\\SQLEXPRESS`)  
**Base de Datos**: `RestauranteProDb`  
**Esquemas**: Organizados por contexto de dominio

## 📝 **Notas Técnicas**

- Las migraciones están configuradas para usar el esquema `Core` en la tabla de historial
- El `RestauranteProDbContextFactory` permite generar migraciones en tiempo de diseño
- Todas las entidades incluyen auditoría automática (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
- Se implementa soft delete para todas las entidades principales
- Las clases mock en `DesignTimeServices.cs` permiten la generación de migraciones sin dependencias externas 