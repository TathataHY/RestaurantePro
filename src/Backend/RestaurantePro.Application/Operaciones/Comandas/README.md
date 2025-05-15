# Vertical Slices en la Capa Application

Este directorio contiene los casos de uso para el módulo de Comandas, organizados siguiendo el patrón de Vertical Slices.

## Estructura de Vertical Slices

Cada caso de uso (slice) está organizado en su propia carpeta:

```
Comandas/
├── CrearComanda/
│   ├── CrearComandaCommand.cs      # Comando con datos de entrada
│   ├── CrearComandaValidator.cs    # Validador para el comando
│   └── CrearComandaHandler.cs      # Manejador que implementa la lógica
│
├── ActualizarEstadoComanda/
│   ├── ActualizarEstadoComandaCommand.cs
│   ├── ActualizarEstadoComandaValidator.cs
│   └── ActualizarEstadoComandaHandler.cs
│
├── AgregarProductoComanda/
│   ├── AgregarProductoComandaCommand.cs
│   ├── AgregarProductoComandaValidator.cs
│   └── AgregarProductoComandaHandler.cs
```

## Patrones utilizados

1. **CQRS (Command Query Responsibility Segregation)**: 
   - Commands para operaciones que modifican datos
   - Queries para operaciones que solo leen datos

2. **Mediator Pattern**:
   - Utiliza MediatR para desacoplar el envío de comandos de su ejecución
   - Permite agregar comportamientos transversales (logging, validación, etc.)

3. **Validation Pipeline**:
   - Cada comando tiene su propio validador con FluentValidation
   - Las validaciones se ejecutan automáticamente antes del handler

4. **Result Pattern**:
   - Retorno estandarizado para operaciones
   - Manejo uniforme de errores y excepciones

## Flujo típico de ejecución

1. La UI/API envía un comando a través del mediador
2. El pipeline de MediatR ejecuta la validación
3. Si la validación es exitosa, el comando se pasa al handler
4. El handler:
   - Recupera entidades del dominio a través de repositorios
   - Ejecuta la lógica de negocio llamando a métodos del dominio
   - Persiste los cambios
   - Publica eventos de dominio
   - Retorna un Result con el resultado

## Beneficios de esta estructura

1. **Alta cohesión**: Todo el código para una funcionalidad está junto
2. **Bajo acoplamiento**: Cada slice es independiente de los demás
3. **Fácil de mantener**: Los cambios están localizados
4. **Desarrollo en paralelo**: Diferentes desarrolladores pueden trabajar en diferentes slices
5. **Testing simplificado**: Cada slice puede probarse de forma aislada

## Consejos para implementar nuevos Vertical Slices

1. Crear una nueva carpeta con el nombre de la funcionalidad
2. Implementar el Command/Query con los datos de entrada
3. Implementar el Validator con las reglas de validación
4. Implementar el Handler con la lógica de negocio 