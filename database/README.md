# Base de datos

El sistema usa SQL Server con una base llamada `GestorFinanzasDB`. Hay dos formas de crearla, y solo se debe usar una sobre la misma base.

La forma recomendada es dejar que la aplicación la cree sola: con la opción `AplicarMigracionesAlIniciar` activada, que viene así por defecto en `appsettings.json`, al arrancar se crea la base, se aplican las migraciones y se cargan las categorías iniciales.

La otra forma es crearla a mano desde SQL Server Management Studio, ejecutando estos scripts en orden:

1. `01_crear_esquema.sql` crea la base, las 8 tablas, sus restricciones e índices, y las categorías iniciales. También registra la migración inicial para que la aplicación no vuelva a crear las tablas.
2. `02_datos_demo.sql` carga el usuario de demostración con cuentas, movimientos, presupuestos, metas, recomendaciones y notificaciones.
3. `03_consultas_utiles.sql` tiene consultas para revisar los datos; no modifica nada.

Si se quiere empezar de cero, se elimina la base `GestorFinanzasDB` desde SQL Server Management Studio y se vuelve a crear con una sola de las dos formas.

Usuario de demostración: `demo@estudiante.udb.edu.sv`, contraseña `Demo1234!`. Es solo para desarrollo y pruebas.
