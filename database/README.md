# Base de datos

Motor: **SQL Server** (2019 o más reciente). Nombre de la base: `GestorFinanzasDB`.

## Opción A (recomendada): que la aplicación la cree sola

Con `BaseDeDatos:AplicarMigracionesAlIniciar` en `true` (valor por defecto en `appsettings.json`), al arrancar la
aplicación se crea la base de datos, se aplican las migraciones y se cargan las categorías iniciales.
Con `BaseDeDatos:CargarDatosDemo` en `true` también se crea el usuario de demostración.
No hay que ejecutar ningún script.

## Opción B: crearla a mano desde SSMS

Ejecuta los scripts en este orden:

| Orden | Archivo | Qué hace |
|---|---|---|
| 1 | `01_crear_esquema.sql` | Crea la base de datos, las 8 tablas, restricciones, índices y las categorías iniciales. Registra la migración `InitialCreate` para que la aplicación no intente crear las tablas otra vez. |
| 2 | `02_datos_demo.sql` | Carga el usuario de demostración con cuentas, movimientos, presupuestos, metas, recomendaciones y notificaciones. |
| 3 | `03_consultas_utiles.sql` | Consultas para revisar los datos y comprobar reglas (no modifica nada). |

> Usa **una** de las dos opciones sobre la misma base. Si ya la creó la aplicación y quieres empezar de cero,
> elimina la base `GestorFinanzasDB` desde SSMS y vuelve a empezar.

## Usuario de demostración

- Correo: `demo@estudiante.udb.edu.sv`
- Contraseña: `Demo1234!`

Es solo para desarrollo y pruebas. En un despliegue real se desactiva con `BaseDeDatos:CargarDatosDemo = false`.
