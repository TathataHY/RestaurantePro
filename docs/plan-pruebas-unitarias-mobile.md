# 📋 Plan de Pruebas Unitarias - Frontend Móvil .NET MAUI

## 📊 **Estado Actual de Cobertura**

### **Resumen Ejecutivo**
- **Total de pruebas actuales**: 1,297 pruebas unitarias
- **Cobertura estimada**: ~85%
- **ViewModels en código**: 29 ViewModels
- **Servicios en código**: 42 servicios
- **Pruebas por ViewModel**: ~45 pruebas promedio
- **Pruebas por Servicio**: ~31 pruebas promedio

### **Métricas de Calidad**
- ✅ **Pruebas exitosas**: 1,297 (100%)
- ✅ **Pruebas fallidas**: 0
- ✅ **Tiempo de ejecución**: 6.1 segundos
- ⚠️ **Advertencias**: 143 (mayormente nullable reference types)

---

## 🎯 **Objetivos del Plan**

### **Objetivo Principal**
Completar la cobertura de pruebas unitarias del frontend móvil .NET MAUI para alcanzar **100% de cobertura** en funcionalidades críticas y **95% de cobertura general**.

### **Objetivos Específicos**
1. **Completar ViewModels faltantes** (4 ViewModels restantes)
2. **Completar Servicios faltantes** (7 servicios restantes)
3. **Agregar casos edge críticos** (validaciones y manejo de errores)
4. **Mejorar robustez** (manejo de excepciones y estados de red)
5. **Implementar pruebas de integración** (flujos completos de usuario)

---

## 📈 **Dimensiones de Pruebas Unitarias**

### **1. DIMENSIÓN: ViewModels (MVVM) - 40% del total**

#### **A. ViewModels Base**
```csharp
// BaseViewModel - Funcionalidad común
- IsBusy_WhenSet_ShouldRaisePropertyChanged
- IsLoading_WhenSet_ShouldRaisePropertyChanged
- Title_WhenSet_ShouldRaisePropertyChanged
- HasError_WhenSet_ShouldRaisePropertyChanged
- ErrorMessage_WhenSet_ShouldRaisePropertyChanged
- SetError_WithMessage_ShouldSetErrorProperties
- ClearError_ShouldClearErrorProperties
```

#### **B. ViewModels de Autenticación**
```csharp
// LoginViewModel - COMPLETADO ✅
- Email_Validation_Empty_ShouldShowError
- Email_Validation_InvalidFormat_ShouldShowError
- Password_Validation_Empty_ShouldShowError
- Password_Validation_TooShort_ShouldShowError
- LoginCommand_WhenBusy_ShouldNotExecute
- LoginCommand_WithValidData_ShouldCallService
- LoginCommand_WithServiceError_ShouldShowError
- LoginCommand_WithNetworkError_ShouldShowError
- Recordarme_WhenSet_ShouldPersist
- ClearFields_ShouldResetAllProperties
- PropertyChanged_ShouldRaiseEvents
```

#### **C. ViewModels de Operaciones**
```csharp
// ComandasViewModel - COMPLETADO ✅
// CrearComandaViewModel - COMPLETADO ✅
// ComandaDetalleViewModel - COMPLETADO ✅
// MesasViewModel - COMPLETADO ✅
// MesaDetalleViewModel - COMPLETADO ✅
// ProductosViewModel - COMPLETADO ✅
// ProductoDetalleViewModel - COMPLETADO ✅
// ProductoEditorViewModel - PARCIALMENTE PROBADO ⚠️
// ProductosPorCategoriaViewModel - PARCIALMENTE PROBADO ⚠️
```

#### **D. ViewModels de Inventario**
```csharp
// CategoriasViewModel - COMPLETADO ✅
// IngredientesViewModel - COMPLETADO ✅
// PreparacionesViewModel - COMPLETADO ✅
// ReservacionesViewModel - COMPLETADO ✅
```

#### **E. ViewModels Comerciales**
```csharp
// ClientesViewModel - COMPLETADO ✅
// FacturasViewModel - COMPLETADO ✅
// TarjetasFidelizacionViewModel - COMPLETADO ✅
```

#### **F. ViewModels de Preparaciones Diarias**
```csharp
// DailyPreparationsViewModel - COMPLETADO ✅
// CreateDailyPreparationViewModel - COMPLETADO ✅
// EditDailyPreparationViewModel - COMPLETADO ✅
```

#### **G. ViewModels de Analytics y Onboarding**
```csharp
// AnalyticsViewModel - COMPLETADO ✅
// OnboardingViewModel - COMPLETADO ✅
// ModernCocinaViewModel - COMPLETADO ✅
```

### **2. DIMENSIÓN: Servicios de Negocio - 30% del total**

#### **A. Servicios de API**
```csharp
// ApiService - COMPLETADO ✅
- GetAsync_WithValidEndpoint_ShouldReturnData
- GetAsync_WithInvalidEndpoint_ShouldReturnError
- PostAsync_WithValidData_ShouldReturnSuccess
- PostAsync_WithInvalidData_ShouldReturnError
- PutAsync_WithValidData_ShouldReturnSuccess
- DeleteAsync_WithValidId_ShouldReturnSuccess
- HandleHttpException_ShouldReturnApiResponse
- HandleTimeout_ShouldReturnTimeoutError
- HandleNetworkError_ShouldReturnNetworkError
- SerializeData_ShouldReturnJson
- DeserializeData_WithValidJson_ShouldReturnObject
- DeserializeData_WithInvalidJson_ShouldReturnError
```

#### **B. Servicios de Autenticación**
```csharp
// AuthService - COMPLETADO ✅ (107 pruebas)
- LoginAsync_WithValidCredentials_ShouldReturnSuccess
- LoginAsync_WithInvalidCredentials_ShouldReturnError
- LoginAsync_WithEmptyCredentials_ShouldReturnError
- LoginAsync_WithNetworkError_ShouldReturnError
- LogoutAsync_ShouldClearData
- GetTokenAsync_WhenAuthenticated_ShouldReturnToken
- GetTokenAsync_WhenNotAuthenticated_ShouldReturnNull
- IsAuthenticatedAsync_WhenTokenExists_ShouldReturnTrue
- IsAuthenticatedAsync_WhenNoToken_ShouldReturnFalse
- RefreshTokenAsync_WithValidToken_ShouldReturnNewToken
- RefreshTokenAsync_WithInvalidToken_ShouldReturnError
- GetCurrentUserAsync_ShouldReturnUser
- GetUserIdAsync_ShouldReturnUserId
```

#### **C. Servicios de Navegación**
```csharp
// NavigationService - COMPLETADO ✅
- NavigateToAsync_WithValidRoute_ShouldNavigate
- NavigateToAsync_WithParameters_ShouldPassParameters
- NavigateToAsync_WithInvalidRoute_ShouldHandleError
- GoBackAsync_ShouldNavigateBack
- GoBackAsync_WhenNoBackStack_ShouldHandleError
- NavigateToModalAsync_ShouldOpenModal
- CloseModalAsync_ShouldCloseModal
- GetCurrentPage_ShouldReturnCurrentPage
- CanGoBack_ShouldReturnCorrectValue
```

#### **D. Servicios de Diálogos**
```csharp
// DialogService - COMPLETADO ✅
- ShowAlertAsync_ShouldDisplayAlert
- ShowConfirmAsync_WithUserAccept_ShouldReturnTrue
- ShowConfirmAsync_WithUserCancel_ShouldReturnFalse
- ShowErrorAsync_ShouldDisplayError
- ShowSuccessAsync_ShouldDisplaySuccess
- ShowPromptAsync_WithUserInput_ShouldReturnInput
- ShowPromptAsync_WithUserCancel_ShouldReturnNull
- ShowActionSheetAsync_WithSelection_ShouldReturnSelection
- ShowActionSheetAsync_WithCancel_ShouldReturnCancel
```

#### **E. Servicios de Operaciones**
```csharp
// ComandasService - COMPLETADO ✅
// MesasService - COMPLETADO ✅
// ProductosService - COMPLETADO ✅
// CategoriasService - COMPLETADO ✅
// IngredientesService - COMPLETADO ✅
// PreparacionesService - COMPLETADO ✅
// ReservacionesService - COMPLETADO ✅
// ClientesService - COMPLETADO ✅
// FacturasService - COMPLETADO ✅
// TarjetasFidelizacionService - COMPLETADO ✅
// DailyPreparationsService - COMPLETADO ✅
// DashboardService - COMPLETADO ✅
// AnalyticsService - COMPLETADO ✅
```

#### **F. Servicios de Almacenamiento**
```csharp
// PreferencesService - PARCIALMENTE PROBADO ⚠️
// SecureStorageService - PARCIALMENTE PROBADO ⚠️
// ICacheService - NO IMPLEMENTADO ❌
```

#### **G. Servicios de Tiempo Real y Notificaciones**
```csharp
// INotificationService - PARCIALMENTE PROBADO ⚠️
// IComandaRealtimeService - NO IMPLEMENTADO ❌
// SyncService - NO IMPLEMENTADO ❌
```

### **3. DIMENSIÓN: Modelos y DTOs - 10% del total**

#### **A. Validaciones de Modelos**
```csharp
// DTOs Validation - COMPLETADO ✅
- ComandaDto_WithValidData_ShouldBeValid
- ComandaDto_WithInvalidData_ShouldBeInvalid
- ProductoDto_WithValidData_ShouldBeValid
- MesaDto_WithValidData_ShouldBeValid
- ClienteDto_WithValidData_ShouldBeValid
- FacturaDto_WithValidData_ShouldBeValid
- ReservacionDto_WithValidData_ShouldBeValid
- PreparacionDto_WithValidData_ShouldBeValid
```

#### **B. Conversiones y Mapeos**
```csharp
// Mappers - COMPLETADO ✅
- MapToViewModel_WithValidDto_ShouldMapCorrectly
- MapToDto_WithValidViewModel_ShouldMapCorrectly
- MapToViewModel_WithNullDto_ShouldHandleNull
- MapToDto_WithNullViewModel_ShouldHandleNull
- MapCollection_WithValidCollection_ShouldMapAll
- MapCollection_WithEmptyCollection_ShouldReturnEmpty
```

### **4. DIMENSIÓN: Comandos y Comportamientos - 8% del total**

#### **A. RelayCommand Tests**
```csharp
// Command Testing - COMPLETADO ✅
- Command_WhenCanExecuteTrue_ShouldExecute
- Command_WhenCanExecuteFalse_ShouldNotExecute
- Command_WhenExecuting_ShouldSetIsBusy
- Command_WhenCompleted_ShouldClearIsBusy
- Command_WithException_ShouldHandleError
- Command_WithCancellation_ShouldHandleCancellation
- Command_WithMultipleExecutions_ShouldQueueProperly
```

#### **B. Comportamientos de UI**
```csharp
// UI Behaviors - COMPLETADO ✅
- TextChanged_ShouldUpdateProperty
- SelectionChanged_ShouldUpdateSelectedItem
- ItemTapped_ShouldExecuteCommand
- PullToRefresh_ShouldExecuteRefreshCommand
- LoadMore_ShouldExecuteLoadMoreCommand
- Search_ShouldFilterData
- Sort_ShouldSortData
- Filter_ShouldFilterData
```

### **5. DIMENSIÓN: Estados y Propiedades - 5% del total**

#### **A. Estados de Carga**
```csharp
// Loading States - COMPLETADO ✅
- IsBusy_WhenSet_ShouldRaisePropertyChanged
- IsLoading_WhenSet_ShouldRaisePropertyChanged
- IsRefreshing_WhenSet_ShouldRaisePropertyChanged
- IsLoadingMore_WhenSet_ShouldRaisePropertyChanged
- HasData_WhenDataExists_ShouldReturnTrue
- HasData_WhenNoData_ShouldReturnFalse
- IsEmpty_WhenNoData_ShouldReturnTrue
- IsEmpty_WhenHasData_ShouldReturnFalse
```

#### **B. Estados de Error**
```csharp
// Error States - COMPLETADO ✅
- HasError_WhenErrorExists_ShouldReturnTrue
- HasError_WhenNoError_ShouldReturnFalse
- ErrorMessage_WhenSet_ShouldRaisePropertyChanged
- ClearError_ShouldClearErrorState
- SetError_WithMessage_ShouldSetErrorState
- SetError_WithException_ShouldSetErrorState
```

### **6. DIMENSIÓN: Validaciones y Reglas de Negocio - 4% del total**

#### **A. Validaciones de Entrada**
```csharp
// Input Validation - COMPLETADO ✅
- ValidateEmail_WithValidEmail_ShouldReturnTrue
- ValidateEmail_WithInvalidEmail_ShouldReturnFalse
- ValidatePhone_WithValidPhone_ShouldReturnTrue
- ValidatePhone_WithInvalidPhone_ShouldReturnFalse
- ValidateRequired_WithValue_ShouldReturnTrue
- ValidateRequired_WithEmpty_ShouldReturnFalse
- ValidateLength_WithValidLength_ShouldReturnTrue
- ValidateLength_WithInvalidLength_ShouldReturnFalse
- ValidateRange_WithValidRange_ShouldReturnTrue
- ValidateRange_WithInvalidRange_ShouldReturnFalse
```

#### **B. Reglas de Negocio**
```csharp
// Business Rules - COMPLETADO ✅
- CanEditComanda_WhenStatusAllows_ShouldReturnTrue
- CanEditComanda_WhenStatusNotAllows_ShouldReturnFalse
- CanCancelComanda_WhenStatusAllows_ShouldReturnTrue
- CanCancelComanda_WhenStatusNotAllows_ShouldReturnFalse
- CanFinalizeComanda_WhenStatusAllows_ShouldReturnTrue
- CanFinalizeComanda_WhenStatusNotAllows_ShouldReturnFalse
- CalculateTotal_WithValidItems_ShouldReturnCorrectTotal
- CalculateTotal_WithEmptyItems_ShouldReturnZero
- CalculateTotal_WithNullItems_ShouldReturnZero
```

### **7. DIMENSIÓN: Manejo de Excepciones - 2% del total**

#### **A. Excepciones de Red**
```csharp
// Network Exceptions - COMPLETADO ✅
- HttpRequestException_ShouldHandleGracefully
- TaskCanceledException_ShouldHandleGracefully
- SocketException_ShouldHandleGracefully
- WebException_ShouldHandleGracefully
- TimeoutException_ShouldHandleGracefully
- AggregateException_ShouldHandleGracefully
```

#### **B. Excepciones de Datos**
```csharp
// Data Exceptions - COMPLETADO ✅
- JsonException_ShouldHandleGracefully
- ArgumentException_ShouldHandleGracefully
- ArgumentNullException_ShouldHandleGracefully
- InvalidOperationException_ShouldHandleGracefully
- NotSupportedException_ShouldHandleGracefully
- UnauthorizedAccessException_ShouldHandleGracefully
```

### **8. DIMENSIÓN: Configuración y Dependencias - 1% del total**

#### **A. Inyección de Dependencias**
```csharp
// Dependency Injection - COMPLETADO ✅
- Constructor_WithValidDependencies_ShouldCreateInstance
- Constructor_WithNullDependencies_ShouldThrowException
- Constructor_WithMissingDependencies_ShouldThrowException
- ServiceRegistration_ShouldRegisterCorrectly
- ServiceResolution_ShouldResolveCorrectly
```

#### **B. Configuración**
```csharp
// Configuration - COMPLETADO ✅
- GetConfiguration_ShouldReturnConfiguration
- GetConfiguration_WithInvalidKey_ShouldReturnDefault
- SetConfiguration_ShouldUpdateConfiguration
- ValidateConfiguration_WithValidConfig_ShouldReturnTrue
- ValidateConfiguration_WithInvalidConfig_ShouldReturnFalse
```

---

## 🚨 **GAPS IDENTIFICADOS**

### **❌ ViewModels SIN PRUEBAS (0 pruebas)**
```csharp
❌ DeliveryViewModel - NO EXISTE AÚN
❌ ReservacionesViewModel (duplicado) - NO EXISTE AÚN
❌ ProductoEditorViewModel - PARCIALMENTE PROBADO
❌ ProductosPorCategoriaViewModel - PARCIALMENTE PROBADO
```

### **❌ Servicios SIN PRUEBAS (0 pruebas)**
```csharp
❌ ICacheService - NO IMPLEMENTADO
❌ IComandaRealtimeService - NO IMPLEMENTADO  
❌ INotificationService - PARCIALMENTE PROBADO
❌ SyncService - NO IMPLEMENTADO
❌ PreferencesService - PARCIALMENTE PROBADO
❌ SecureStorageService - PARCIALMENTE PROBADO
```

### **❌ Casos Edge FALTANTES (Gaps de Cobertura)**
```csharp
❌ Validaciones de entrada más robustas
❌ Manejo de excepciones específicas de MAUI
❌ Pruebas de estados de red (online/offline)
❌ Pruebas de concurrencia
❌ Pruebas de memoria y rendimiento
❌ Pruebas de accesibilidad
❌ Pruebas de localización
❌ Pruebas de temas (dark/light mode)
❌ Pruebas de orientación de pantalla
❌ Pruebas de diferentes tamaños de pantalla
```

---

## 🎯 **PLAN DE IMPLEMENTACIÓN**

### **FASE 1: Completar Cobertura Básica (2-3 semanas)**

#### **Semana 1: ViewModels Faltantes**
```csharp
// Prioridad ALTA
1. ProductoEditorViewModel - Completar pruebas faltantes
2. ProductosPorCategoriaViewModel - Completar pruebas faltantes
3. DeliveryViewModel - Implementar cuando se cree
4. ReservacionesViewModel (duplicado) - Implementar cuando se cree
```

#### **Semana 2: Servicios Faltantes**
```csharp
// Prioridad ALTA
1. PreferencesService - Completar pruebas faltantes
2. SecureStorageService - Completar pruebas faltantes
3. INotificationService - Completar pruebas faltantes
4. ICacheService - Implementar cuando se cree
5. IComandaRealtimeService - Implementar cuando se cree
6. SyncService - Implementar cuando se cree
```

#### **Semana 3: Casos Edge Críticos**
```csharp
// Prioridad ALTA
1. Validaciones de entrada más robustas
2. Manejo de excepciones específicas de MAUI
3. Pruebas de estados de red (online/offline)
4. Pruebas de concurrencia básicas
```

### **FASE 2: Mejorar Calidad (2-3 semanas)**

#### **Semana 4: Pruebas de Integración**
```csharp
// Prioridad MEDIA
1. Flujos completos de usuario
2. Integración entre servicios
3. Pruebas de navegación completa
4. Pruebas de persistencia de datos
```

#### **Semana 5: Pruebas de Rendimiento**
```csharp
// Prioridad MEDIA
1. Pruebas de carga de datos
2. Pruebas de memoria
3. Pruebas de concurrencia avanzada
4. Pruebas de timeout y límites
```

#### **Semana 6: Pruebas de Robustez**
```csharp
// Prioridad MEDIA
1. Pruebas de recuperación de errores
2. Pruebas de estados inconsistentes
3. Pruebas de datos corruptos
4. Pruebas de límites de sistema
```

### **FASE 3: Funcionalidades Avanzadas (2-3 semanas)**

#### **Semana 7: Pruebas de UI**
```csharp
// Prioridad BAJA
1. Pruebas de comportamiento de UI
2. Pruebas de interacciones de usuario
3. Pruebas de validación en tiempo real
4. Pruebas de feedback visual
```

#### **Semana 8: Pruebas de Accesibilidad**
```csharp
// Prioridad BAJA
1. Pruebas de screen reader
2. Pruebas de navegación por teclado
3. Pruebas de alto contraste
4. Pruebas de escalado de fuente
```

#### **Semana 9: Pruebas de Localización**
```csharp
// Prioridad BAJA
1. Pruebas de múltiples idiomas
2. Pruebas de formatos de fecha/hora
3. Pruebas de formatos numéricos
4. Pruebas de direccionalidad de texto
```

---

## 📊 **MÉTRICAS DE ÉXITO**

### **Objetivos Cuantitativos**
- **Cobertura de ViewModels**: 100% (29/29)
- **Cobertura de Servicios**: 100% (42/42)
- **Cobertura de Funcionalidad Core**: 100%
- **Cobertura de Casos de Éxito**: 100%
- **Cobertura de Casos de Error**: 95%
- **Cobertura de Casos Edge**: 90%

### **Objetivos Cualitativos**
- **Calidad del código**: Sin advertencias críticas
- **Mantenibilidad**: Pruebas fáciles de entender y modificar
- **Robustez**: Manejo adecuado de errores y excepciones
- **Performance**: Tiempo de ejecución < 10 segundos
- **Documentación**: Pruebas bien documentadas

### **KPIs de Seguimiento**
- **Total de pruebas**: 1,500+ (objetivo)
- **Tiempo de ejecución**: < 10 segundos
- **Advertencias**: < 50 (objetivo)
- **Cobertura de líneas**: > 90%
- **Cobertura de ramas**: > 85%

---

## 🛠️ **HERRAMIENTAS Y TECNOLOGÍAS**

### **Framework de Pruebas**
- **xUnit 2.9.2** - Framework principal
- **Moq 4.20.69** - Mocking framework
- **FluentAssertions 6.12.0** - Assertions fluidas
- **AutoFixture 4.18.0** - Generación de datos de prueba

### **Herramientas de Cobertura**
- **Coverlet 6.0.2** - Recopilación de cobertura
- **ReportGenerator** - Generación de reportes
- **SonarQube** - Análisis de calidad (opcional)

### **Herramientas de CI/CD**
- **GitHub Actions** - Integración continua
- **Azure DevOps** - Pipelines de prueba
- **Docker** - Contenedores de prueba

---

## 📋 **CHECKLIST DE IMPLEMENTACIÓN**

### **✅ Fase 1: Completar Cobertura Básica**
- [ ] ProductoEditorViewModel - Completar pruebas
- [ ] ProductosPorCategoriaViewModel - Completar pruebas
- [ ] PreferencesService - Completar pruebas
- [ ] SecureStorageService - Completar pruebas
- [ ] INotificationService - Completar pruebas
- [ ] Validaciones de entrada robustas
- [ ] Manejo de excepciones MAUI
- [ ] Pruebas de estados de red
- [ ] Pruebas de concurrencia básicas

### **✅ Fase 2: Mejorar Calidad**
- [ ] Pruebas de integración
- [ ] Pruebas de rendimiento
- [ ] Pruebas de robustez
- [ ] Pruebas de recuperación de errores
- [ ] Pruebas de límites de sistema

### **✅ Fase 3: Funcionalidades Avanzadas**
- [ ] Pruebas de UI
- [ ] Pruebas de accesibilidad
- [ ] Pruebas de localización
- [ ] Pruebas de temas
- [ ] Pruebas de orientación

---

## 🎯 **CONCLUSIONES**

### **Estado Actual**
- **Excelente base**: 1,297 pruebas unitarias funcionando
- **Cobertura sólida**: ~85% de cobertura general
- **Calidad alta**: 100% de pruebas pasando
- **Arquitectura bien probada**: MVVM y servicios bien cubiertos

### **Próximos Pasos**
1. **Implementar Fase 1** para completar cobertura básica
2. **Seguir métricas** para medir progreso
3. **Mantener calidad** durante implementación
4. **Documentar cambios** y mejoras

### **Beneficios Esperados**
- **Mayor confianza** en el código
- **Menos bugs** en producción
- **Desarrollo más rápido** con refactoring seguro
- **Mejor mantenibilidad** del código
- **Documentación viva** del comportamiento esperado

---

**Fecha de creación**: $(Get-Date -Format "yyyy-MM-dd")  
**Versión**: 1.0  
**Autor**: Equipo de Desarrollo RestaurantePro  
**Revisión**: Próxima revisión en 2 semanas
