/* ============================================================================
   Gestor de Finanzas Personales - Datos de demostración
   Crea el usuario demo con cuentas, movimientos, presupuestos, metas,
   recomendaciones y notificaciones. Las fechas son relativas a hoy.

   Usuario: demo@estudiante.udb.edu.sv      Contraseña: Demo1234!
   Es seguro ejecutarlo varias veces: si el usuario ya existe no cambia nada.
   Requiere haber ejecutado antes 01_crear_esquema.sql (o haber iniciado la aplicación).
   ============================================================================ */

USE [GestorFinanzasDB];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM [Usuarios] WHERE [Correo] = N'demo@estudiante.udb.edu.sv')
BEGIN
    PRINT N'El usuario de demostración ya existe. No se hizo ningún cambio.';
    RETURN;
END;

DECLARE @hoy date = CAST(GETDATE() AS date);
DECLARE @ahora datetime = GETDATE();
DECLARE @inicioMes date = DATEFROMPARTS(YEAR(@hoy), MONTH(@hoy), 1);
DECLARE @usuarioId int, @efectivoId int, @ahorroId int, @tarjetaId int;

BEGIN TRANSACTION;

-- Usuario (la contraseña está guardada con hash, no en texto plano)
INSERT INTO [Usuarios] ([Nombre], [Correo], [Contrasenia], [FechaRegistro])
VALUES (N'Estudiante Demo', N'demo@estudiante.udb.edu.sv', N'AQAAAAIAAYagAAAAEAKasfX80pzoRJ9hQGKCHszi18ITcu3n+2K9ppRudGSM1cxYMRT52Wz2aEd0dEDM4g==', @ahora);
SET @usuarioId = SCOPE_IDENTITY();

-- Cuentas con su saldo inicial (los movimientos se suman después)
INSERT INTO [Cuentas] ([UsuarioId], [NombreCuenta], [TipoCuenta], [SaldoActual], [Moneda])
VALUES (@usuarioId, N'Efectivo', N'Efectivo', 50.00, N'USD');
SET @efectivoId = SCOPE_IDENTITY();
INSERT INTO [Cuentas] ([UsuarioId], [NombreCuenta], [TipoCuenta], [SaldoActual], [Moneda])
VALUES (@usuarioId, N'Cuenta de ahorro', N'Ahorro', 150.00, N'USD');
SET @ahorroId = SCOPE_IDENTITY();
INSERT INTO [Cuentas] ([UsuarioId], [NombreCuenta], [TipoCuenta], [SaldoActual], [Moneda])
VALUES (@usuarioId, N'Tarjeta de débito', N'Tarjeta', 60.00, N'USD');
SET @tarjetaId = SCOPE_IDENTITY();

-- Movimientos
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @efectivoId, c.[Id], 100.00, c.[Tipo], DATEADD(DAY, -20, @hoy), N'Mesada de la quincena'
FROM [Categorias] c WHERE c.[Nombre] = N'Mesada o apoyo familiar';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @efectivoId, c.[Id], 1.50, c.[Tipo], DATEADD(DAY, -1, @hoy), N'Bus a la universidad'
FROM [Categorias] c WHERE c.[Nombre] = N'Transporte';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @efectivoId, c.[Id], 3.25, c.[Tipo], DATEADD(DAY, -2, @hoy), N'Almuerzo en la cafetería'
FROM [Categorias] c WHERE c.[Nombre] = N'Alimentación';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @efectivoId, c.[Id], 1.25, c.[Tipo], DATEADD(DAY, -3, @hoy), N'Café y pan dulce'
FROM [Categorias] c WHERE c.[Nombre] = N'Gastos hormiga';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @efectivoId, c.[Id], 1.50, c.[Tipo], DATEADD(DAY, -4, @hoy), N'Bus de regreso a casa'
FROM [Categorias] c WHERE c.[Nombre] = N'Transporte';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @efectivoId, c.[Id], 4.00, c.[Tipo], DATEADD(DAY, -6, @hoy), N'Fotocopias de la guía de laboratorio'
FROM [Categorias] c WHERE c.[Nombre] = N'Materiales de estudio';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @efectivoId, c.[Id], 5.50, c.[Tipo], DATEADD(DAY, -9, @hoy), N'Pupusas con compañeros'
FROM [Categorias] c WHERE c.[Nombre] = N'Alimentación';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @efectivoId, c.[Id], 6.00, c.[Tipo], DATEADD(DAY, -12, @hoy), N'Cine'
FROM [Categorias] c WHERE c.[Nombre] = N'Entretenimiento';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @efectivoId, c.[Id], 7.50, c.[Tipo], DATEADD(DAY, -15, @hoy), N'Recarga de pasajes del mes'
FROM [Categorias] c WHERE c.[Nombre] = N'Transporte';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @ahorroId, c.[Id], 80.00, c.[Tipo], DATEADD(DAY, -28, @hoy), N'Pago mensual de la beca'
FROM [Categorias] c WHERE c.[Nombre] = N'Beca';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @ahorroId, c.[Id], 25.00, c.[Tipo], DATEADD(DAY, -18, @hoy), N'Cuota de laboratorio'
FROM [Categorias] c WHERE c.[Nombre] = N'Educación';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @ahorroId, c.[Id], 60.00, c.[Tipo], DATEADD(DAY, -10, @hoy), N'Tutoría de matemática'
FROM [Categorias] c WHERE c.[Nombre] = N'Trabajo de medio tiempo';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @ahorroId, c.[Id], 12.99, c.[Tipo], DATEADD(DAY, -8, @hoy), N'Internet del mes'
FROM [Categorias] c WHERE c.[Nombre] = N'Servicios';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @ahorroId, c.[Id], 9.80, c.[Tipo], DATEADD(DAY, -5, @hoy), N'Farmacia'
FROM [Categorias] c WHERE c.[Nombre] = N'Salud';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @tarjetaId, c.[Id], 2.75, c.[Tipo], DATEADD(DAY, -2, @hoy), N'Bebida energética'
FROM [Categorias] c WHERE c.[Nombre] = N'Gastos hormiga';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @tarjetaId, c.[Id], 8.90, c.[Tipo], DATEADD(DAY, -7, @hoy), N'Comida a domicilio'
FROM [Categorias] c WHERE c.[Nombre] = N'Alimentación';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @tarjetaId, c.[Id], 22.00, c.[Tipo], DATEADD(DAY, -11, @hoy), N'Camisa'
FROM [Categorias] c WHERE c.[Nombre] = N'Ropa y compras';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @tarjetaId, c.[Id], 15.00, c.[Tipo], DATEADD(DAY, -13, @hoy), N'Venta de libros usados'
FROM [Categorias] c WHERE c.[Nombre] = N'Otros ingresos';
INSERT INTO [Transacciones] ([CuentaId], [CategoriaId], [Monto], [Tipo], [Fecha], [Descripcion])
SELECT @tarjetaId, c.[Id], 9.99, c.[Tipo], DATEADD(DAY, -14, @hoy), N'Suscripción de streaming'
FROM [Categorias] c WHERE c.[Nombre] = N'Entretenimiento';

-- Saldo actual = saldo inicial + ingresos - gastos
UPDATE c
SET c.[SaldoActual] = c.[SaldoActual] + ISNULL(x.[Variacion], 0)
FROM [Cuentas] c
OUTER APPLY (
    SELECT SUM(CASE WHEN t.[Tipo] = N'Ingreso' THEN t.[Monto] ELSE -t.[Monto] END) AS [Variacion]
    FROM [Transacciones] t WHERE t.[CuentaId] = c.[Id]
) x
WHERE c.[UsuarioId] = @usuarioId;

-- Presupuestos del mes actual
INSERT INTO [Presupuestos] ([UsuarioId], [CategoriaId], [MontoLimite], [Periodo], [FechaInicio], [FechaFin])
SELECT @usuarioId, c.[Id], 60.00, N'Mensual', @inicioMes, EOMONTH(@inicioMes) FROM [Categorias] c WHERE c.[Nombre] = N'Alimentación';
INSERT INTO [Presupuestos] ([UsuarioId], [CategoriaId], [MontoLimite], [Periodo], [FechaInicio], [FechaFin])
SELECT @usuarioId, c.[Id], 25.00, N'Mensual', @inicioMes, EOMONTH(@inicioMes) FROM [Categorias] c WHERE c.[Nombre] = N'Transporte';
INSERT INTO [Presupuestos] ([UsuarioId], [CategoriaId], [MontoLimite], [Periodo], [FechaInicio], [FechaFin])
SELECT @usuarioId, c.[Id], 20.00, N'Mensual', @inicioMes, EOMONTH(@inicioMes) FROM [Categorias] c WHERE c.[Nombre] = N'Entretenimiento';

-- Metas de ahorro
INSERT INTO [MetasAhorro] ([UsuarioId], [NombreMeta], [MontoObjetivo], [MontoActual], [FechaLimite])
VALUES (@usuarioId, N'Fondo de emergencia', 300.00, 120.00, DATEADD(DAY, 90, @hoy)),
       (@usuarioId, N'Laptop para clases', 600.00, 80.00, DATEADD(MONTH, 8, @hoy));

-- Recomendaciones y notificaciones de ejemplo
INSERT INTO [RecomendacionesIA] ([UsuarioId], [TipoRecomendacion], [Descripcion], [FechaGeneracion], [Aplicada])
VALUES (@usuarioId, N'Gastos hormiga', N'Tus compras pequeñas (café, bebidas y snacks) suman varios dólares al mes. Prueba llevar tu propia merienda dos días por semana.', DATEADD(DAY, -1, @ahora), 0),
       (@usuarioId, N'Ahorro', N'Si apartas $10 de cada mesada podrías completar tu fondo de emergencia dos meses antes.', DATEADD(DAY, -3, @ahora), 0);

INSERT INTO [Notificaciones] ([UsuarioId], [Tipo], [Mensaje], [FechaEnvio], [Leida])
VALUES (@usuarioId, N'Presupuesto', N'Ya usaste el 80% de tu presupuesto de Alimentación este mes.', DATEADD(HOUR, -5, @ahora), 0),
       (@usuarioId, N'Meta', N'Vas por el 40% de tu meta Fondo de emergencia. ¡Sigue así!', DATEADD(DAY, -2, @ahora), 1);

COMMIT TRANSACTION;
PRINT N'Datos de demostración cargados correctamente.';
GO
