# Gestor de Finanzas Personales

Aplicación web para que estudiantes universitarios registren sus ingresos y gastos, controlen presupuestos y den seguimiento a metas de ahorro, con apoyo de inteligencia artificial.
Proyecto de cátedra DSP404 (Desarrollo de Aplicaciones con Software Propietario) · Universidad Don Bosco.

Tecnologías: ASP.NET Core MVC (.NET 8) · Entity Framework Core 8 · SQL Server · Docker · Azure.

> Este README cubre la base del proyecto: estructura, base de datos y módulo de movimientos. El equipo lo amplía con el resto de módulos.

## Qué incluye hasta ahora

- Modelo de datos completo (8 entidades), relaciones, restricciones, índices y migración inicial.
- 14 categorías iniciales y datos de demostración.
- Arquitectura en capas (Controllers → Services → Repositories → Entity Framework Core).
- Módulo **Registrar movimiento**: historial con filtros y paginación, y CRUD de ingresos y gastos con actualización automática del saldo.

## Requisitos

| Herramienta | Versión |
|---|---|
| Visual Studio 2022 o 2026 con la carga de trabajo «ASP.NET y desarrollo web» | Cualquier edición (Community sirve) |
| SDK de .NET | 8.0 o más reciente |
| SQL Server | 2019 o más reciente (Developer o Express), o LocalDB |
| SSMS | Opcional, para revisar la base de datos |

## Cómo abrir y ejecutar

### Con Visual Studio

1. Descomprime el proyecto y abre `GestorFinanzas.sln`.
2. Visual Studio restaura los paquetes NuGet solo (necesita internet la primera vez).
3. Revisa la cadena de conexión en `src/GestorFinanzas.Web/appsettings.json`. Por defecto apunta a `Server=localhost` con autenticación de Windows. Si usas otra instancia, cámbiala (opciones en `docs/Manual_Tecnico_Estructura_y_BD.md`, sección 2.9).
4. Elige el perfil **https** y presiona **F5**.
5. Al arrancar, la aplicación crea la base `GestorFinanzasDB`, aplica las migraciones y carga los datos de demostración. Se abre el navegador en la pantalla de inicio. Entra a **Movimientos**.

### Con la terminal

```
cd src/GestorFinanzas.Web
dotnet run --launch-profile http
```

Abre `http://localhost:5080`.

### Si prefieres crear la base a mano

Ejecuta en SSMS los scripts de `database/` en orden (`01`, `02`) y pon `BaseDeDatos:AplicarMigracionesAlIniciar` en `false`. Detalles en `database/README.md`.

## Usuario de demostración

Mientras no esté integrado el login, la aplicación usa siempre este usuario:

- Correo: `demo@estudiante.udb.edu.sv`
- Contraseña: `Demo1234!`

## Solución de problemas

| Síntoma | Qué revisar |
|---|---|
| La aplicación tarda y luego falla al iniciar | SQL Server no está en ejecución o la cadena de conexión no coincide con tu instancia. Revisa el servicio «SQL Server» y el valor de `Server`. |
| `Login failed for user` | Con autenticación de Windows, tu usuario necesita permisos en esa instancia. O bien usa una cadena con usuario y contraseña de SQL. |
| `A connection was successfully established... certificate chain` | Falta `TrustServerCertificate=True` en la cadena de conexión. |
| No se ven estilos ni íconos | Bootstrap se carga desde internet (CDN); revisa la conexión. |
| «There is already an object named...» | Ejecutaste los scripts SQL y además la aplicación creó las tablas. Elimina la base y usa solo una de las dos opciones. |

## Estructura

```
GestorFinanzas.sln
src/GestorFinanzas.Web/   Aplicación ASP.NET Core MVC
docs/                     Manual técnico, diagrama ER y plan de pruebas
database/                 Scripts SQL
docker/                   Archivos de Docker
```

El detalle de cada carpeta está en `docs/Manual_Tecnico_Estructura_y_BD.md`.

## Guía para el resto del equipo

Cómo conectar los demás módulos con lo que ya existe:

| Módulo | Qué usar |
|---|---|
| Login y perfil | La tabla `Usuarios` y su `Contrasenia` (guardar con `PasswordHasher<Usuario>`). Para que el resto de la aplicación sepa quién está conectado, crear una implementación de `ICurrentUserService` que lea el Id de los claims y cambiar **una línea** en `Program.cs` (`AddScoped<ICurrentUserService, ...>`). |
| Presupuestos y metas | Las entidades `Presupuesto` y `MetaAhorro` ya existen con su configuración. Para consultar datos, crear su repositorio en `Data/Repositories` y su servicio en `Services`, siguiendo el patrón de `Transaccion`. |
| IA y notificaciones | Las entidades `RecomendacionIA` y `Notificacion` ya existen. Para disparar una alerta al registrar un gasto, el punto de enganche es `TransaccionService.CrearAsync`. |
| Interfaz | `Views/Shared/_Layout.cshtml` y `wwwroot/css/site.css` son provisionales. Reemplázalos con la identidad visual de Figma. |
| Docker | La cadena de conexión se sobrescribe con la variable de entorno `ConnectionStrings__Default`. Con `BaseDeDatos__AplicarMigracionesAlIniciar=true` la aplicación crea la base sola al arrancar, y reintenta mientras SQL Server termina de iniciar. Para producción, `BaseDeDatos__CargarDatosDemo=false`. |

### Cambios en la base de datos

Si alguien modifica una entidad o su configuración: `dotnet ef migrations add NombreDelCambio` desde `src/GestorFinanzas.Web`, revisar el archivo generado y subirlo al repositorio. Nunca editar una migración que ya subió otra persona. Si el cambio altera tablas, avisar al equipo y actualizar `database/01_crear_esquema.sql` (que equivale a las migraciones).

### Flujo con Git

- Una rama por persona o módulo (`feature/movimientos`, `feature/ia`, ...) y pull request hacia `main`.
- Nadie hace push directo a `main`.
- Commits pequeños y con mensaje claro: sirven como evidencia de trabajo colaborativo.
