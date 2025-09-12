Te ayudo a identificar qué archivos necesitan pruebas unitarias y cuáles ya las tienen. Voy a hacer un análisis completo de la cobertura actual.


[6 tools called]

## 📋 **ANÁLISIS COMPLETO: ARCHIVOS QUE NECESITAN PRUEBAS UNITARIAS**


[1 tool called]

Basándome en mi análisis exhaustivo, aquí tienes el **estado actual** de las pruebas unitarias del proyecto móvil:

## �� **ESTADÍSTICAS ACTUALES**

- **📁 Archivos totales en Mobile.Core**: 102 archivos
- **✅ Archivos con pruebas**: 37 archivos de prueba
- **❌ Archivos SIN pruebas**: ~65 archivos

---

## 🎯 **ARCHIVOS QUE YA TIENEN PRUEBAS UNITARIAS** ✅

### **�� Services (Bien Cubiertos)**
- ✅ `AuthService` → `AuthServiceTests.cs`
- ✅ `ComandasService` → `ComandasServiceTests.cs`
- ✅ `TarjetasFidelizacionService` → `TarjetasFidelizacionServiceTests.cs`
- ✅ `ProductosService` → `ProductosServiceTests.cs`
- ✅ `MesasService` → `MesasServiceTests.cs`
- ✅ `AnalyticsService` → `AnalyticsServiceTests.cs`
- ✅ `FacturasService` → `FacturasServiceTests.cs`
- ✅ `ClientesService` → `ClientesServiceTests.cs`
- ✅ `IngredientesService` → `IngredientesServiceTests.cs`
- ✅ `PreparacionesService` → `PreparacionesServiceTests.cs`
- ✅ `ReservacionesService` → `ReservacionesServiceTests.cs`
- ✅ `CategoriasService` → `CategoriasServiceTests.cs`

### **🖥️ ViewModels (Bien Cubiertos)**
- ✅ `ComandasViewModel` → `ComandasViewModelTests.cs`
- ✅ `ComandaDetalleViewModel` → `ComandaDetalleViewModelTests.cs`
- ✅ `MesasViewModel` → `MesasViewModelTests.cs`
- ✅ `MesaDetalleViewModel` → `MesaDetalleViewModelTests.cs`
- ✅ `ProductosViewModel` → `ProductosViewModelTests.cs`
- ✅ `ProductoDetalleViewModel` → `ProductoDetalleViewModelTests.cs`
- ✅ `TarjetasFidelizacionViewModel` → `TarjetasFidelizacionViewModelTests.cs`
- ✅ `AnalyticsViewModel` → `AnalyticsViewModelTests.cs`
- ✅ `LoginViewModel` → `LoginViewModelTests.cs`
- ✅ `IngredientesViewModel` → `IngredientesViewModelTests.cs`
- ✅ `PreparacionesViewModel` → `PreparacionesViewModelTests.cs`
- ✅ `ReservacionesViewModel` → `ReservacionesViewModelTests.cs`
- ✅ `CategoriasViewModel` → `CategoriasViewModelTests.cs`
- ✅ `ClientesViewModel` → `ClientesViewModelTests.cs`
- ✅ `FacturasViewModel` → `FacturasViewModelTests.cs`
- ✅ `DailyPreparationsViewModel` → `DailyPreparationsViewModelTests.cs`

---

## ❌ **ARCHIVOS QUE NECESITAN PRUEBAS UNITARIAS**

### **�� PRIORIDAD ALTA - ViewModels Críticos**

#### **1. ViewModels de Operaciones**
- ❌ `CrearComandaViewModel` - **CRÍTICO** (creación de comandas)
- ❌ `ProductoEditorViewModel` - **CRÍTICO** (edición de productos)
- ❌ `ProductosPorCategoriaViewModel` - **CRÍTICO** (navegación por categorías)

#### **2. ViewModels de Preparaciones Diarias**
- ❌ `CreateDailyPreparationViewModel` - **CRÍTICO**
- ❌ `EditDailyPreparationViewModel` - **CRÍTICO**

#### **3. ViewModels de Cocina**
- ❌ `ModernCocinaViewModel` - **CRÍTICO** (operaciones de cocina)

#### **4. ViewModels de Onboarding**
- ❌ `OnboardingViewModel` - **IMPORTANTE** (primera experiencia del usuario)

### **🔧 PRIORIDAD MEDIA - Services de Infraestructura**

#### **1. Services Core**
- ❌ `ApiService` - **IMPORTANTE** (comunicación con backend)
- ❌ `NavigationService` - **IMPORTANTE** (navegación)
- ❌ `DialogService` - **IMPORTANTE** (diálogos)

#### **2. Services de Plataforma**
- ❌ `SecureStorageService` - **IMPORTANTE** (almacenamiento seguro)
- ❌ `PreferencesService` - **IMPORTANTE** (preferencias)
- ❌ `DashboardService` - **IMPORTANTE** (dashboard)
- ❌ `DailyPreparationsService` - **IMPORTANTE**

#### **3. Services de Notificaciones**
- ❌ `NotificationService` - **MEDIO** (notificaciones)

### **�� PRIORIDAD BAJA - Modelos y Utilidades**

#### **1. Modelos DTOs**
- ❌ `ApiResponse.cs` - **BAJO** (modelo simple)
- ❌ `PaginatedList.cs` - **BAJO** (modelo simple)
- ❌ `Result.cs` - **BAJO** (modelo simple)

#### **2. Configuración**
- ❌ `MobileCoreServiceCollection.cs` - **BAJO** (configuración DI)

---

## 🎯 **PLAN DE ACCIÓN RECOMENDADO**

### **📅 FASE 1: CRÍTICOS (Semana 1-2)**
1. **`CrearComandaViewModelTests`** - Flujo principal del negocio
2. **`ProductoEditorViewModelTests`** - Edición de productos
3. **`ModernCocinaViewModelTests`** - De ahí bajo no no no Uy AA Sí Seguro afuera A El A