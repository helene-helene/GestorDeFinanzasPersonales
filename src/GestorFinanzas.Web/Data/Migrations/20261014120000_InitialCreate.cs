using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestorFinanzas.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                    Icono = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                    table.CheckConstraint("CK_Categorias_Tipo", "[Tipo] IN ('Ingreso','Gasto')");
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                    Contrasenia = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cuentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                    Moneda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    NombreCuenta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SaldoActual = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    TipoCuenta = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuentas", x => x.Id);
                    table.CheckConstraint("CK_Cuentas_TipoCuenta", "[TipoCuenta] IN ('Ahorro','Corriente','Efectivo','Tarjeta')");
                    table.ForeignKey(
                        name: "FK_Cuentas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MetasAhorro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                    FechaLimite = table.Column<DateTime>(type: "date", nullable: true),
                    MontoActual = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValue: 0m),
                    MontoObjetivo = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    NombreMeta = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetasAhorro", x => x.Id);
                    table.CheckConstraint("CK_MetasAhorro_MontoObjetivo", "[MontoObjetivo] > 0");
                    table.CheckConstraint("CK_MetasAhorro_MontoActual", "[MontoActual] >= 0");
                    table.ForeignKey(
                        name: "FK_MetasAhorro_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                    FechaEnvio = table.Column<DateTime>(type: "datetime", nullable: false),
                    Leida = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Mensaje = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Presupuestos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "date", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "date", nullable: false),
                    MontoLimite = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Periodo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Presupuestos", x => x.Id);
                    table.CheckConstraint("CK_Presupuestos_MontoLimite", "[MontoLimite] > 0");
                    table.CheckConstraint("CK_Presupuestos_Fechas", "[FechaFin] >= [FechaInicio]");
                    table.ForeignKey(
                        name: "FK_Presupuestos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Presupuestos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecomendacionesIA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                    Aplicada = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "datetime", nullable: false),
                    TipoRecomendacion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecomendacionesIA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecomendacionesIA_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transacciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                            .Annotation("SqlServer:Identity", "1, 1"),
                    CategoriaId = table.Column<int>(type: "int", nullable: false),
                    CuentaId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacciones", x => x.Id);
                    table.CheckConstraint("CK_Transacciones_Monto", "[Monto] > 0");
                    table.CheckConstraint("CK_Transacciones_Tipo", "[Tipo] IN ('Ingreso','Gasto')");
                    table.ForeignKey(
                        name: "FK_Transacciones_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transacciones_Cuentas_CuentaId",
                        column: x => x.CuentaId,
                        principalTable: "Cuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categorias",
                columns: new[] { "Id", "Icono", "Nombre", "Tipo" },
                values: new object[,]
                {
                    { 1, "bi-cup-hot", "Alimentación", "Gasto" },
                    { 2, "bi-bus-front", "Transporte", "Gasto" },
                    { 3, "bi-heart-pulse", "Salud", "Gasto" },
                    { 4, "bi-controller", "Entretenimiento", "Gasto" },
                    { 5, "bi-book", "Materiales de estudio", "Gasto" },
                    { 6, "bi-mortarboard", "Educación", "Gasto" },
                    { 7, "bi-wifi", "Servicios", "Gasto" },
                    { 8, "bi-bag", "Ropa y compras", "Gasto" },
                    { 9, "bi-cookie", "Gastos hormiga", "Gasto" },
                    { 10, "bi-three-dots", "Otros gastos", "Gasto" },
                    { 11, "bi-house-heart", "Mesada o apoyo familiar", "Ingreso" },
                    { 12, "bi-award", "Beca", "Ingreso" },
                    { 13, "bi-briefcase", "Trabajo de medio tiempo", "Ingreso" },
                    { 14, "bi-cash-coin", "Otros ingresos", "Ingreso" },
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nombre_Tipo",
                table: "Categorias",
                columns: new[] { "Nombre", "Tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cuentas_UsuarioId_NombreCuenta",
                table: "Cuentas",
                columns: new[] { "UsuarioId", "NombreCuenta" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MetasAhorro_UsuarioId",
                table: "MetasAhorro",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioId_Leida",
                table: "Notificaciones",
                columns: new[] { "UsuarioId", "Leida" });

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_CategoriaId",
                table: "Presupuestos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Presupuestos_UsuarioId_CategoriaId_FechaInicio",
                table: "Presupuestos",
                columns: new[] { "UsuarioId", "CategoriaId", "FechaInicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RecomendacionesIA_UsuarioId",
                table: "RecomendacionesIA",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_CategoriaId",
                table: "Transacciones",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_CuentaId_Fecha",
                table: "Transacciones",
                columns: new[] { "CuentaId", "Fecha" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Transacciones");

            migrationBuilder.DropTable(
                name: "RecomendacionesIA");

            migrationBuilder.DropTable(
                name: "Presupuestos");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "MetasAhorro");

            migrationBuilder.DropTable(
                name: "Cuentas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
