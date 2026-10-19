using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestorFinanzas.Web.Data.Seed;

/// <summary>
/// Carga un usuario de demostración con cuentas, movimientos, presupuestos, metas, recomendaciones
/// y notificaciones. Las fechas son relativas a hoy para que el historial siempre se vea reciente.
/// Es seguro llamarlo varias veces: si el usuario demo ya existe no hace nada.
/// </summary>
public static class DbSeeder
{
    public static async Task CargarDatosDemoAsync(AppDbContext contexto)
    {
        if (await contexto.Usuarios.AnyAsync(u => u.Correo == DatosDemo.Correo))
        {
            return;
        }

        var categorias = await contexto.Categorias.AsNoTracking().ToDictionaryAsync(c => c.Nombre);
        var hoy = DateTime.Today;
        var ahora = DateTime.Now;

        // ---- Usuario (la contraseña se guarda con hash, igual que lo hará el login)
        var usuario = new Usuario
        {
            Nombre = DatosDemo.Nombre,
            Correo = DatosDemo.Correo,
            FechaRegistro = ahora
        };
        usuario.Contrasenia = new PasswordHasher<Usuario>().HashPassword(usuario, DatosDemo.Contrasenia);

        // ---- Cuentas (SaldoActual arranca con el saldo inicial y luego se le aplican los movimientos)
        var efectivo = new Cuenta { Usuario = usuario, NombreCuenta = "Efectivo", TipoCuenta = TipoCuenta.Efectivo, SaldoActual = 50m, Moneda = "USD" };
        var ahorro = new Cuenta { Usuario = usuario, NombreCuenta = "Cuenta de ahorro", TipoCuenta = TipoCuenta.Ahorro, SaldoActual = 150m, Moneda = "USD" };
        var tarjeta = new Cuenta { Usuario = usuario, NombreCuenta = "Tarjeta de débito", TipoCuenta = TipoCuenta.Tarjeta, SaldoActual = 60m, Moneda = "USD" };
        contexto.Cuentas.AddRange(efectivo, ahorro, tarjeta);

        // ---- Movimientos: (cuenta, categoría, monto, días atrás, descripción)
        var movimientos = new (Cuenta Cuenta, string Categoria, decimal Monto, int DiasAtras, string Descripcion)[]
        {
            (efectivo, "Mesada o apoyo familiar", 100.00m, 20, "Mesada de la quincena"),
            (efectivo, "Transporte", 1.50m, 1, "Bus a la universidad"),
            (efectivo, "Alimentación", 3.25m, 2, "Almuerzo en la cafetería"),
            (efectivo, "Gastos hormiga", 1.25m, 3, "Café y pan dulce"),
            (efectivo, "Transporte", 1.50m, 4, "Bus de regreso a casa"),
            (efectivo, "Materiales de estudio", 4.00m, 6, "Fotocopias de la guía de laboratorio"),
            (efectivo, "Alimentación", 5.50m, 9, "Pupusas con compañeros"),
            (efectivo, "Entretenimiento", 6.00m, 12, "Cine"),
            (efectivo, "Transporte", 7.50m, 15, "Recarga de pasajes del mes"),

            (ahorro, "Beca", 80.00m, 28, "Pago mensual de la beca"),
            (ahorro, "Educación", 25.00m, 18, "Cuota de laboratorio"),
            (ahorro, "Trabajo de medio tiempo", 60.00m, 10, "Tutoría de matemática"),
            (ahorro, "Servicios", 12.99m, 8, "Internet del mes"),
            (ahorro, "Salud", 9.80m, 5, "Farmacia"),

            (tarjeta, "Gastos hormiga", 2.75m, 2, "Bebida energética"),
            (tarjeta, "Alimentación", 8.90m, 7, "Comida a domicilio"),
            (tarjeta, "Ropa y compras", 22.00m, 11, "Camisa"),
            (tarjeta, "Otros ingresos", 15.00m, 13, "Venta de libros usados"),
            (tarjeta, "Entretenimiento", 9.99m, 14, "Suscripción de streaming"),
        };

        foreach (var m in movimientos)
        {
            var categoria = categorias[m.Categoria];
            contexto.Transacciones.Add(new Transaccion
            {
                Cuenta = m.Cuenta,
                CategoriaId = categoria.Id,
                Monto = m.Monto,
                Tipo = categoria.Tipo,
                Fecha = hoy.AddDays(-m.DiasAtras),
                Descripcion = m.Descripcion
            });

            m.Cuenta.SaldoActual += categoria.Tipo == TipoMovimiento.Ingreso ? m.Monto : -m.Monto;
        }

        // ---- Presupuestos del mes actual
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
        var finMes = inicioMes.AddMonths(1).AddDays(-1);
        foreach (var (nombre, limite) in new[] { ("Alimentación", 60m), ("Transporte", 25m), ("Entretenimiento", 20m) })
        {
            contexto.Presupuestos.Add(new Presupuesto
            {
                Usuario = usuario,
                CategoriaId = categorias[nombre].Id,
                MontoLimite = limite,
                Periodo = PeriodoPresupuesto.Mensual,
                FechaInicio = inicioMes,
                FechaFin = finMes
            });
        }

        // ---- Metas de ahorro
        contexto.MetasAhorro.AddRange(
            new MetaAhorro { Usuario = usuario, NombreMeta = "Fondo de emergencia", MontoObjetivo = 300m, MontoActual = 120m, FechaLimite = hoy.AddDays(90) },
            new MetaAhorro { Usuario = usuario, NombreMeta = "Laptop para clases", MontoObjetivo = 600m, MontoActual = 80m, FechaLimite = hoy.AddMonths(8) });

        // ---- Recomendaciones y notificaciones de ejemplo
        contexto.RecomendacionesIA.AddRange(
            new RecomendacionIA
            {
                Usuario = usuario,
                TipoRecomendacion = "Gastos hormiga",
                Descripcion = "Tus compras pequeñas (café, bebidas y snacks) suman varios dólares al mes. Prueba llevar tu propia merienda dos días por semana.",
                FechaGeneracion = ahora.AddDays(-1),
                Aplicada = false
            },
            new RecomendacionIA
            {
                Usuario = usuario,
                TipoRecomendacion = "Ahorro",
                Descripcion = "Si apartas $10 de cada mesada podrías completar tu fondo de emergencia dos meses antes.",
                FechaGeneracion = ahora.AddDays(-3),
                Aplicada = false
            });

        contexto.Notificaciones.AddRange(
            new Notificacion
            {
                Usuario = usuario,
                Tipo = "Presupuesto",
                Mensaje = "Ya usaste el 80% de tu presupuesto de Alimentación este mes.",
                FechaEnvio = ahora.AddHours(-5),
                Leida = false
            },
            new Notificacion
            {
                Usuario = usuario,
                Tipo = "Meta",
                Mensaje = "Vas por el 40% de tu meta Fondo de emergencia. ¡Sigue así!",
                FechaEnvio = ahora.AddDays(-2),
                Leida = true
            });

        await contexto.SaveChangesAsync();
    }
}
