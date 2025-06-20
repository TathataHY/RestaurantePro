# Seed Data - RestaurantePro

Este directorio contiene todos los datos semilla necesarios para inicializar la base de datos del sistema.

## 📊 **Estado Actual del Desarrollo**

### ✅ **COMPLETADO**
- **Infraestructura Base**: Interface `ISeedData` y `SeedDataRunner` implementados
- **RolesSeeder** (Orden: 100): 7 roles del sistema con permisos granulares
- **UnidadesMedidaSeeder** (Orden: 120): Validación de unidades de medida críticas
- **PermisosSeeder** (Orden: 130): Validación de permisos críticos extraídos de RolesSeeder
- **ConfiguracionSeeder** (Orden: 140): Validación de configuraciones críticas de AppSettings
- **EstadosSeeder** (Orden: 150): Documentación de estados del sistema
- **UsuarioAdminSeeder** (Orden: 200): Usuario administrador inicial con credenciales

### 🎉 **SEEDERS CRÍTICOS COMPLETADOS** - 6/6 ✅

### ⏳ **PENDIENTE**
- **Seeders Demo**: Productos, Ingredientes, Proveedores, Clientes, Mesas
- **Seeders Testing**: Datos específicos para pruebas automatizadas
- **Integración con API**: Configuración en Program.cs

### 🏗️ **ESTADO DE COMPILACIÓN**
- ✅ **Infraestructura**: Compila sin errores
- ✅ **Seeders Críticos**: 6/6 implementados y funcionando
- ✅ **Interface ISeedData**: Correctamente implementada
- ✅ **RestauranteProDbContext**: Integración completa
- 📊 **Advertencias**: 107 warnings (normales, no críticos)

## 📁 **Estructura Organizacional**

```
SeedData/
├── Critical/                # Datos críticos del sistema (OBLIGATORIOS)
│   ├── RolesSeeder.cs      ✅ Roles de usuario del sistema
│   ├── UnidadesMedidaSeeder.cs ✅ Unidades de medida estándar
│   ├── PermisosSeeder.cs   ✅ Validación de permisos críticos del sistema
│   ├── ConfiguracionSeeder.cs ✅ Validación de configuraciones críticas
│   ├── EstadosSeeder.cs    ✅ Estados predefinidos para entidades
│   └── UsuarioAdminSeeder.cs ✅ Usuario administrador inicial
├── Demo/                    # Datos de demostración (OPCIONALES)
│   ├── CategoriasSeeder.cs ⏳ Categorías de productos
│   ├── ProductosSeeder.cs  ⏳ Productos del menú
│   ├── IngredientesSeeder.cs ⏳ Ingredientes básicos
│   ├── ProveedoresSeeder.cs ⏳ Proveedores de ejemplo
│   ├── ClientesSeeder.cs   ⏳ Clientes de prueba
│   ├── MesasSeeder.cs      ⏳ Configuración de mesas
│   └── EscenariosDemoSeeder.cs ⏳ Escenarios completos de demo
├── Testing/                 # Datos específicos para pruebas
│   ├── DatosPruebasUnitarias.cs ⏳
│   ├── DatosPruebasIntegracion.cs ⏳
│   └── DatosRendimiento.cs ⏳
└── Extensions/              # Extensiones y utilidades
    ├── ISeedData.cs        ✅ Interface base para seeders
    └── SeedDataRunner.cs   ✅ Runner principal con manejo de transacciones
```

## 🔧 **Datos Críticos del Sistema**

### **Roles de Usuario**
- **Administrador**: Acceso total al sistema
- **Gerente**: Gestión operativa y reportes
- **Cajero**: Facturación y pagos
- **Mesero**: Atención al cliente y comandas
- **Cocinero**: Preparación de alimentos
- **EncargadoInventario**: Control de stock y compras
- **Empleado**: Acceso básico

### **Estados Predefinidos**
- **Estados de Mesa**: Disponible, Ocupada, Reservada, FueraDeServicio, EnLimpieza
- **Estados de Comanda**: Creada, EnProceso, Lista, Entregada, Finalizada, Cancelada
- **Estados de Reservación**: Pendiente, Confirmada, Cancelada, Completada, NoShow
- **Estados de Factura**: Borrador, Emitida, Pagada, PagadaParcialmente, Anulada, Vencida
- **Estados de Usuario**: Activo, Inactivo, Bloqueado, PendienteConfirmacion, Suspendido

### **Unidades de Medida**
- **Peso**: Kilogramo, Gramo
- **Volumen**: Litro, Mililitro
- **Cantidad**: Unidad, Piezas, Paquete
- **Cocina**: Cucharada, Cucharadita, Taza

### **Tipos de Mesa**
- Interior, Terraza, Privada, Barra, Ventana, VIP, Comunitaria, Alta, Accesible

### **Categorías de Proveedores**
- AlimentosBasicos, Carnes, FrutasVerduras, Lacteos, BebidasNoAlcoholicas, BebidasAlcoholicas, Limpieza, EmpaquesDesechables, Especias, UtensiliosEquipo, Servicios

## 🧪 **Datos de Demostración**

### **Productos del Menú**
- **Entradas**: Nachos, Alitas, Ensaladas
- **Platos Fuertes**: Hamburguesas, Pasta, Carnes
- **Postres**: Helados, Pasteles, Frutas
- **Bebidas**: Refrescos, Jugos, Café, Bebidas Alcohólicas

### **Ingredientes Básicos**
- **Carnes**: Pollo, Res, Cerdo, Pescado
- **Vegetales**: Lechuga, Tomate, Cebolla, Zanahoria
- **Lácteos**: Leche, Queso, Mantequilla, Crema
- **Especias**: Sal, Pimienta, Ajo, Orégano

### **Clientes de Prueba**
- Cliente VIP con tarjeta Premium
- Cliente Regular con tarjeta Estándar
- Cliente Corporativo
- Cliente Frecuente

### **Escenarios de Demo**
- Mesa ocupada con comanda activa
- Reservación confirmada para hoy
- Factura pendiente de pago
- Orden de compra en proceso

## ⚙️ **Configuración de Ejecución**

### **Orden de Ejecución**
1. **Datos Críticos** (siempre primero)
2. **Datos de Demo** (opcional, configurable)
3. **Datos de Testing** (solo en entorno de desarrollo)

### **Variables de Entorno**
```json
{
  "SeedData": {
    "RunCriticalData": true,
    "RunDemoData": false,
    "RunTestingData": false,
    "CreateAdminUser": true,
    "AdminEmail": "admin@restaurantepro.com",
    "AdminPassword": "Admin@123"
  }
}
```

### **Comandos de Ejecución**
```bash
# Solo datos críticos
dotnet run --seed-critical

# Datos críticos + demo
dotnet run --seed-all

# Limpiar y re-sembrar
dotnet run --seed-clean --seed-all
```

## 🚨 **Consideraciones Importantes**

### **Datos Críticos**
- ✅ **Siempre se ejecutan** en producción
- ✅ **Idempotentes** - pueden ejecutarse múltiples veces
- ✅ **Versionados** - mantienen consistencia entre versiones
- ✅ **Validados** - verifican integridad antes de insertar

### **Datos de Demo**
- ⚠️ **Solo en desarrollo/staging** - NUNCA en producción
- ⚠️ **Configurables** - pueden deshabilitarse
- ⚠️ **Realistas** - reflejan casos de uso reales
- ⚠️ **Actualizables** - pueden modificarse sin afectar lógica

### **Datos de Testing**
- 🧪 **Solo en entorno de desarrollo**
- 🧪 **Aislados** - no interfieren con datos reales
- 🧪 **Reproducibles** - mismos datos cada vez
- 🧪 **Completos** - cubren todos los escenarios de prueba

## 🔄 **Flujo de Inicialización**

1. **Verificar Estado**: Comprobar si ya existen datos
2. **Ejecutar Críticos**: Sembrar datos esenciales del sistema
3. **Ejecutar Demo** (opcional): Agregar datos de demostración
4. **Verificar Integridad**: Validar que todos los datos estén correctos
5. **Generar Reporte**: Mostrar resumen de datos sembrados

## 📝 **Notas Técnicas**

- Todos los seeders implementan `ISeedData` interface
- Se usa Transaction para garantizar atomicidad
- Los datos se validan antes de la inserción
- Se genera log detallado de todas las operaciones
- Los IDs son determinísticos para consistencia entre entornos 