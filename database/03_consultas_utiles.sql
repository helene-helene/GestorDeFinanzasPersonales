/* ============================================================================
   Gestor de Finanzas Personales - Consultas útiles
   Sirven para revisar los datos y para comprobar las reglas del módulo de movimientos.
   ============================================================================ */

USE [GestorFinanzasDB];
GO

-- 1) Historial completo de movimientos (lo más reciente primero)
SELECT t.[Id], t.[Fecha], t.[Tipo], t.[Monto], cat.[Nombre] AS [Categoria],
       cu.[NombreCuenta] AS [Cuenta], u.[Nombre] AS [Usuario], t.[Descripcion]
FROM [Transacciones] t
JOIN [Cuentas] cu   ON cu.[Id] = t.[CuentaId]
JOIN [Usuarios] u   ON u.[Id] = cu.[UsuarioId]
JOIN [Categorias] cat ON cat.[Id] = t.[CategoriaId]
ORDER BY t.[Fecha] DESC, t.[Id] DESC;
GO

-- 2) Saldo actual de cada cuenta
SELECT u.[Nombre] AS [Usuario], cu.[NombreCuenta], cu.[TipoCuenta], cu.[SaldoActual]
FROM [Cuentas] cu
JOIN [Usuarios] u ON u.[Id] = cu.[UsuarioId]
ORDER BY u.[Nombre], cu.[NombreCuenta];
GO

-- 3) Gastos por categoría en el mes actual
SELECT cat.[Nombre] AS [Categoria], COUNT(*) AS [Movimientos], SUM(t.[Monto]) AS [TotalGastado]
FROM [Transacciones] t
JOIN [Categorias] cat ON cat.[Id] = t.[CategoriaId]
WHERE t.[Tipo] = N'Gasto'
  AND t.[Fecha] >= DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)
GROUP BY cat.[Nombre]
ORDER BY [TotalGastado] DESC;
GO

-- 4) Ingresos contra gastos por mes
SELECT FORMAT(t.[Fecha], 'yyyy-MM') AS [Mes],
       SUM(CASE WHEN t.[Tipo] = N'Ingreso' THEN t.[Monto] ELSE 0 END) AS [Ingresos],
       SUM(CASE WHEN t.[Tipo] = N'Gasto' THEN t.[Monto] ELSE 0 END)   AS [Gastos]
FROM [Transacciones] t
GROUP BY FORMAT(t.[Fecha], 'yyyy-MM')
ORDER BY [Mes] DESC;
GO

-- 5) Presupuesto del mes contra lo gastado (útil para el módulo de presupuestos y las alertas)
SELECT cat.[Nombre] AS [Categoria], p.[MontoLimite],
       ISNULL(SUM(t.[Monto]), 0) AS [Gastado],
       CAST(ISNULL(SUM(t.[Monto]), 0) * 100.0 / p.[MontoLimite] AS decimal(6,2)) AS [PorcentajeUsado]
FROM [Presupuestos] p
JOIN [Categorias] cat ON cat.[Id] = p.[CategoriaId]
JOIN [Cuentas] cu ON cu.[UsuarioId] = p.[UsuarioId]
LEFT JOIN [Transacciones] t ON t.[CuentaId] = cu.[Id]
                           AND t.[CategoriaId] = p.[CategoriaId]
                           AND t.[Tipo] = N'Gasto'
                           AND t.[Fecha] BETWEEN p.[FechaInicio] AND p.[FechaFin]
GROUP BY cat.[Nombre], p.[MontoLimite];
GO

-- 6) Comprobación de integridad: movimientos cuyo tipo no coincide con el de su categoría
--    (debe devolver 0 filas)
SELECT t.[Id], t.[Tipo] AS [TipoMovimiento], cat.[Tipo] AS [TipoCategoria]
FROM [Transacciones] t
JOIN [Categorias] cat ON cat.[Id] = t.[CategoriaId]
WHERE t.[Tipo] <> cat.[Tipo];
GO
