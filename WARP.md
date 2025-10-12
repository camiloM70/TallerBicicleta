# WARP.md

Este archivo proporciona orientación a WARP (warp.dev) para trabajar con código en este repositorio.

## Descripción del Proyecto

TallerBicicleta es una aplicación ASP.NET Core Blazor Server construida con .NET 8. La aplicación utiliza el modo de renderizado Interactive Server de Blazor para una experiencia de aplicación web responsiva del lado del servidor.

## Stack Tecnológico

- **Framework**: ASP.NET Core 8.0 con Blazor Server
- **Lenguaje**: C#
- **Frontend**: Componentes Blazor con Bootstrap 5
- **Renderizado**: Componentes Interactive Server
- **Sistema de Build**: MSBuild/.NET CLI

## Comandos de Desarrollo

### Construcción y Ejecución
```powershell
# Construir la solución
dotnet build

# Ejecutar la aplicación (modo desarrollo)
dotnet run --project TallerBicicleta

# Ejecutar con perfil de lanzamiento específico
dotnet run --project TallerBicicleta --launch-profile https

# Limpiar artefactos de build
dotnet clean

# Restaurar paquetes NuGet
dotnet restore
```

### Pruebas
```powershell
# Ejecutar todas las pruebas (si se agregan proyectos de prueba)
dotnet test

# Ejecutar pruebas con cobertura (si se agregan proyectos de prueba)
dotnet test --collect:"XPlat Code Coverage"
```

### Publicación
```powershell
# Publicar para producción
dotnet publish TallerBicicleta -c Release -o publish

# Publicar para runtime específico
dotnet publish TallerBicicleta -c Release -r win-x64 --self-contained
```

## Arquitectura y Estructura

### Estructura del Proyecto
- **TallerBicicleta.sln**: Archivo de solución de Visual Studio
- **TallerBicicleta/**: Proyecto principal de la aplicación
  - **Components/**: Componentes Blazor organizados por propósito
    - **Pages/**: Componentes de página enrutables (directiva `@page`)
      - **Admin/**: Páginas del panel de administración
        - `AdminDashboard.razor` - Panel principal del administrador
        - `AdminGanancias.razor` - Resumen de ingresos y estadísticas
        - `Productos.razor` - Gestión de productos
        - `Servicios.razor` - Gestión de servicios
      - **Cliente/**: Páginas del área del cliente
        - `ClienteDashboard.razor` - Panel principal del cliente
        - `SolicitarServicio.razor` - Formulario de solicitud de servicio
        - `MisSolicitudes.razor` - Lista de solicitudes del cliente
        - `MisFacturas.razor` - Historial de facturas
        - `FacturaDetalle.razor` - Detalle de factura específica
        - `Abonar.razor` - Formulario de abono/pago
        - `PagoExitoso.razor` - Confirmación de pago exitoso
        - `PagoCancelado.razor` - Notificación de pago cancelado
      - **Empleado/**: Páginas del área del empleado
        - `EmpleadoDashboard.razor` - Panel principal del empleado
        - `Empleado.razor` - Información del empleado
        - `MisServicios.razor` - Servicios asignados al empleado
        - `ServiciosDisponibles.razor` - Lista de servicios disponibles
        - `GenerarFactura.razor` - Formulario para generar facturas
    - **Layout/**: Componentes de diseño (MainLayout, NavMenu)
    - **App.razor**: Componente raíz con estructura de documento HTML
    - **Routes.razor**: Configuración del enrutador
    - **_Imports.razor**: Declaraciones using globales para todos los componentes
  - **wwwroot/**: Recursos web estáticos (CSS, imágenes, archivos del lado cliente)
  - **Program.cs**: Punto de entrada de la aplicación y configuración de servicios

### Patrones Arquitectónicos Clave

**Modo Interactive Server de Blazor**: La aplicación utiliza el modo de renderizado `InteractiveServer`, lo que significa:
- Las actualizaciones de UI se manejan del lado del servidor con comunicación SignalR en tiempo real
- Los componentes mantienen el estado en el servidor
- La comunicación cliente-servidor es automática a través del framework de Blazor

**Arquitectura Basada en Componentes**: 
- Las páginas son componentes Blazor con directivas `@page`
- Los layouts definen estructura común de UI entre páginas
- Los componentes pueden contener tanto markup como código C# en bloques `@code`
- Las importaciones globales en `_Imports.razor` proporcionan espacios de nombres comunes a todos los componentes

**Inyección de Dependencias**: Los servicios se configuran en `Program.cs` usando el contenedor DI integrado:
- `AddRazorComponents()` registra servicios de componentes Blazor
- `AddInteractiveServerComponents()` habilita la interactividad del lado del servidor

### Puertos de Desarrollo
- **HTTP**: localhost:5234
- **HTTPS**: localhost:7183
- **IIS Express**: localhost:37831 (HTTP), localhost:44376 (HTTPS)

### Rutas de la Aplicación

#### Área de Administración (`/admin/`)
- `/admin/dashboard` - Panel principal del administrador
- `/admin/ganancias` - Resumen de ingresos y estadísticas
- `/admin/productos` - Gestión de productos
- `/admin/servicios` - Gestión de servicios

#### Área del Cliente (`/cliente/`)
- `/cliente/dashboard` - Panel principal del cliente
- `/cliente/solicitar-servicio` - Formulario de solicitud de servicio
- `/cliente/mis-solicitudes` - Lista de solicitudes del cliente
- `/cliente/mis-facturas` - Historial de facturas
- `/cliente/factura-detalle` - Detalle de factura específica
- `/cliente/abonar` - Formulario de abono/pago
- `/cliente/pago-exitoso` - Confirmación de pago exitoso
- `/cliente/pago-cancelado` - Notificación de pago cancelado

#### Área del Empleado (`/empleado/`)
- `/empleado/dashboard` - Panel principal del empleado
- `/empleado/info` - Información del empleado
- `/empleado/mis-servicios` - Servicios asignados al empleado
- `/empleado/servicios-disponibles` - Lista de servicios disponibles
- `/empleado/generar-factura` - Formulario para generar facturas

## Guías de Desarrollo de Componentes

### Creando Nuevas Páginas
- Agregar archivos `.razor` a `Components/Pages/`
- Usar la directiva `@page "/ruta"` para hacer componentes enrutables
- Seguir las convenciones de nomenclatura existentes (PascalCase)

### Agregando Nuevos Componentes
- Colocar componentes reutilizables en `Components/` (crear subdirectorios según sea necesario)
- Componentes específicos de página van en `Components/Pages/`
- Componentes de layout van en `Components/Layout/`

### Estilos
- Bootstrap 5 está incluido por defecto
- Los estilos específicos de componente se pueden agregar como archivos `.razor.css` (aislamiento CSS)
- Los estilos globales van en `wwwroot/app.css`

### Gestión de Estado
- Usar bloques `@code` para estado a nivel de componente
- Para estado compartido, considerar implementar servicios y registrarlos en `Program.cs`
- El estado del lado del servidor se mantiene automáticamente con el modo Interactive Server

## Convenciones de Nomenclatura de Archivos

- **Componentes**: PascalCase con extensión `.razor` (ej., `Counter.razor`)
- **CSS de Componentes**: Coincidir con el nombre del componente con `.razor.css` (ej., `MainLayout.razor.css`)
- **Configuración**: camelCase para archivos JSON (ej., `appsettings.json`)

## Configuración de Entornos

- **Desarrollo**: Usa `appsettings.Development.json` 
- **Producción**: Usa `appsettings.json`
- El entorno se controla por la variable `ASPNETCORE_ENVIRONMENT`
- Los perfiles de lanzamiento se definen en `Properties/launchSettings.json`
