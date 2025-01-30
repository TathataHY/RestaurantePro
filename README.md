# RestaurantePro

RestaurantePro es una aplicación de gestión de restaurantes desarrollada con .NET MAUI. La aplicación permite a los usuarios gestionar comandas, mesas, platos y usuarios, así como generar reportes de ventas.

## Características

- **Gestión de Comandas**: Crear, editar y eliminar comandas. Ver detalles de las comandas y agregar detalles a las mismas.
- **Gestión de Mesas**: Crear, editar y eliminar mesas.
- **Gestión de Platos**: Crear, editar y eliminar platos.
- **Gestión de Usuarios**: Crear, editar y eliminar usuarios.
- **Reportes de Ventas**: Generar reportes de ventas.

## Tecnologías Utilizadas

- .NET MAUI
- CommunityToolkit.Maui
- CommunityToolkit.Mvvm
- SQLite

## Estructura del Proyecto

- **Models**: Contiene las clases de modelo de datos.
- **Services**: Contiene los servicios para acceder a la base de datos y manejar la lógica de negocio.
- **ViewModels**: Contiene los ViewModels para la lógica de presentación.
- **Views**: Contiene las vistas (páginas XAML) de la aplicación.

## Instalación

1. Clona el repositorio:
    ```bash
    git clone https://github.com/tu-usuario/RestaurantePro.git
    ```

2. Navega al directorio del proyecto:
    ```bash
    cd RestaurantePro
    ```

3. Restaura los paquetes NuGet:
    ```bash
    dotnet restore
    ```

4. Compila el proyecto:
    ```bash
    dotnet build
    ```

5. Ejecuta la aplicación:
    ```bash
    dotnet run
    ```

## Uso

### Gestión de Comandas

- **Crear Comanda**: Haz clic en el botón "Agregar Comanda" en la página principal.
- **Editar Comanda**: Desliza una comanda hacia la izquierda y selecciona "Editar".
- **Eliminar Comanda**: Desliza una comanda hacia la izquierda y selecciona "Eliminar".
- **Ver Detalles de Comanda**: Haz clic en una comanda para ver sus detalles y agregar más detalles.

### Gestión de Mesas

- **Crear Mesa**: Navega a la página de mesas y haz clic en "Agregar Mesa".
- **Editar Mesa**: Desliza una mesa hacia la izquierda y selecciona "Editar".
- **Eliminar Mesa**: Desliza una mesa hacia la izquierda y selecciona "Eliminar".

### Gestión de Platos

- **Crear Plato**: Navega a la página de platos y haz clic en "Agregar Plato".
- **Editar Plato**: Desliza un plato hacia la izquierda y selecciona "Editar".
- **Eliminar Plato**: Desliza un plato hacia la izquierda y selecciona "Eliminar".

### Gestión de Usuarios

- **Crear Usuario**: Navega a la página de usuarios y haz clic en "Agregar Usuario".
- **Editar Usuario**: Desliza un usuario hacia la izquierda y selecciona "Editar".
- **Eliminar Usuario**: Desliza un usuario hacia la izquierda y selecciona "Eliminar".

### Reportes de Ventas

- **Generar Reporte**: Navega a la página de reportes y selecciona el rango de fechas para generar el reporte de ventas.

## Contribuciones

Las contribuciones son bienvenidas. Por favor, abre un issue o un pull request para discutir cualquier cambio que desees realizar.

## Licencia

Este proyecto está licenciado bajo la Licencia MIT. Consulta el archivo LICENSE para obtener más detalles.