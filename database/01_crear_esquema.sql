/* ============================================================================
   Gestor de Finanzas Personales - Esquema de base de datos (SQL Server)
   Equivale a la migración InitialCreate de Entity Framework Core.

   Úsalo SOLO si prefieres crear la base de datos a mano desde SSMS.
   Si ejecutas la aplicación con 'AplicarMigracionesAlIniciar': true, no lo necesitas.
   Este script registra la migración en __EFMigrationsHistory, así que la aplicación
   no intentará crear las tablas por segunda vez.
   ============================================================================ */

IF DB_ID(N'GestorFinanzasDB') IS NULL
    CREATE DATABASE [GestorFinanzasDB];
GO

USE [GestorFinanzasDB];
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

CREATE TABLE [Categorias] (
    [Id] int NOT NULL IDENTITY,
    [Icono] nvarchar(50) NULL,
    [Nombre] nvarchar(60) NOT NULL,
    [Tipo] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_Categorias] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Categorias_Tipo] CHECK ([Tipo] IN ('Ingreso','Gasto'))
);
GO

CREATE TABLE [Usuarios] (
    [Id] int NOT NULL IDENTITY,
    [Contrasenia] nvarchar(256) NOT NULL,
    [Correo] nvarchar(150) NOT NULL,
    [FechaRegistro] datetime NOT NULL DEFAULT (GETDATE()),
    [Nombre] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Cuentas] (
    [Id] int NOT NULL IDENTITY,
    [Moneda] nvarchar(10) NOT NULL,
    [NombreCuenta] nvarchar(100) NOT NULL,
    [SaldoActual] decimal(12,2) NOT NULL DEFAULT 0.0,
    [TipoCuenta] nvarchar(30) NOT NULL,
    [UsuarioId] int NOT NULL,
    CONSTRAINT [PK_Cuentas] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Cuentas_TipoCuenta] CHECK ([TipoCuenta] IN ('Ahorro','Corriente','Efectivo','Tarjeta')),
    CONSTRAINT [FK_Cuentas_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [MetasAhorro] (
    [Id] int NOT NULL IDENTITY,
    [FechaLimite] date NULL,
    [MontoActual] decimal(12,2) NOT NULL DEFAULT 0.0,
    [MontoObjetivo] decimal(12,2) NOT NULL,
    [NombreMeta] nvarchar(100) NOT NULL,
    [UsuarioId] int NOT NULL,
    CONSTRAINT [PK_MetasAhorro] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_MetasAhorro_MontoObjetivo] CHECK ([MontoObjetivo] > 0),
    CONSTRAINT [CK_MetasAhorro_MontoActual] CHECK ([MontoActual] >= 0),
    CONSTRAINT [FK_MetasAhorro_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Notificaciones] (
    [Id] int NOT NULL IDENTITY,
    [FechaEnvio] datetime NOT NULL,
    [Leida] bit NOT NULL DEFAULT CAST(0 AS bit),
    [Mensaje] nvarchar(250) NOT NULL,
    [Tipo] nvarchar(30) NOT NULL,
    [UsuarioId] int NOT NULL,
    CONSTRAINT [PK_Notificaciones] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notificaciones_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Presupuestos] (
    [Id] int NOT NULL IDENTITY,
    [CategoriaId] int NOT NULL,
    [FechaFin] date NOT NULL,
    [FechaInicio] date NOT NULL,
    [MontoLimite] decimal(12,2) NOT NULL,
    [Periodo] nvarchar(20) NOT NULL,
    [UsuarioId] int NOT NULL,
    CONSTRAINT [PK_Presupuestos] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Presupuestos_MontoLimite] CHECK ([MontoLimite] > 0),
    CONSTRAINT [CK_Presupuestos_Fechas] CHECK ([FechaFin] >= [FechaInicio]),
    CONSTRAINT [FK_Presupuestos_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Presupuestos_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [RecomendacionesIA] (
    [Id] int NOT NULL IDENTITY,
    [Aplicada] bit NOT NULL DEFAULT CAST(0 AS bit),
    [Descripcion] nvarchar(300) NOT NULL,
    [FechaGeneracion] datetime NOT NULL,
    [TipoRecomendacion] nvarchar(50) NOT NULL,
    [UsuarioId] int NOT NULL,
    CONSTRAINT [PK_RecomendacionesIA] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RecomendacionesIA_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Transacciones] (
    [Id] int NOT NULL IDENTITY,
    [CategoriaId] int NOT NULL,
    [CuentaId] int NOT NULL,
    [Descripcion] nvarchar(200) NULL,
    [Fecha] datetime NOT NULL,
    [Monto] decimal(12,2) NOT NULL,
    [Tipo] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_Transacciones] PRIMARY KEY ([Id]),
    CONSTRAINT [CK_Transacciones_Monto] CHECK ([Monto] > 0),
    CONSTRAINT [CK_Transacciones_Tipo] CHECK ([Tipo] IN ('Ingreso','Gasto')),
    CONSTRAINT [FK_Transacciones_Categorias_CategoriaId] FOREIGN KEY ([CategoriaId]) REFERENCES [Categorias] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Transacciones_Cuentas_CuentaId] FOREIGN KEY ([CuentaId]) REFERENCES [Cuentas] ([Id]) ON DELETE CASCADE
);
GO

SET IDENTITY_INSERT [Categorias] ON;
INSERT INTO [Categorias] ([Id], [Icono], [Nombre], [Tipo])
VALUES
    (1, N'bi-cup-hot', N'Alimentación', N'Gasto'),
    (2, N'bi-bus-front', N'Transporte', N'Gasto'),
    (3, N'bi-heart-pulse', N'Salud', N'Gasto'),
    (4, N'bi-controller', N'Entretenimiento', N'Gasto'),
    (5, N'bi-book', N'Materiales de estudio', N'Gasto'),
    (6, N'bi-mortarboard', N'Educación', N'Gasto'),
    (7, N'bi-wifi', N'Servicios', N'Gasto'),
    (8, N'bi-bag', N'Ropa y compras', N'Gasto'),
    (9, N'bi-cookie', N'Gastos hormiga', N'Gasto'),
    (10, N'bi-three-dots', N'Otros gastos', N'Gasto'),
    (11, N'bi-house-heart', N'Mesada o apoyo familiar', N'Ingreso'),
    (12, N'bi-award', N'Beca', N'Ingreso'),
    (13, N'bi-briefcase', N'Trabajo de medio tiempo', N'Ingreso'),
    (14, N'bi-cash-coin', N'Otros ingresos', N'Ingreso');
SET IDENTITY_INSERT [Categorias] OFF;
GO

CREATE UNIQUE INDEX [IX_Categorias_Nombre_Tipo] ON [Categorias] ([Nombre], [Tipo]);
GO
CREATE UNIQUE INDEX [IX_Usuarios_Correo] ON [Usuarios] ([Correo]);
GO
CREATE UNIQUE INDEX [IX_Cuentas_UsuarioId_NombreCuenta] ON [Cuentas] ([UsuarioId], [NombreCuenta]);
GO
CREATE INDEX [IX_MetasAhorro_UsuarioId] ON [MetasAhorro] ([UsuarioId]);
GO
CREATE INDEX [IX_Notificaciones_UsuarioId_Leida] ON [Notificaciones] ([UsuarioId], [Leida]);
GO
CREATE INDEX [IX_Presupuestos_CategoriaId] ON [Presupuestos] ([CategoriaId]);
GO
CREATE UNIQUE INDEX [IX_Presupuestos_UsuarioId_CategoriaId_FechaInicio] ON [Presupuestos] ([UsuarioId], [CategoriaId], [FechaInicio]);
GO
CREATE INDEX [IX_RecomendacionesIA_UsuarioId] ON [RecomendacionesIA] ([UsuarioId]);
GO
CREATE INDEX [IX_Transacciones_CategoriaId] ON [Transacciones] ([CategoriaId]);
GO
CREATE INDEX [IX_Transacciones_CuentaId_Fecha] ON [Transacciones] ([CuentaId], [Fecha]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261014120000_InitialCreate', N'8.0.11');
GO

PRINT N'Esquema de GestorFinanzasDB creado correctamente.';
GO
