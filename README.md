# Gestor de Finanzas Personales

Aplicación web para que los estudiantes universitarios registren sus ingresos y gastos, controlen sus presupuestos y den seguimiento a sus metas de ahorro, con ayuda de inteligencia artificial para clasificar los gastos y recomendar formas de ahorrar.

Proyecto de cátedra de Desarrollo de Aplicaciones con Software Propietario (DSP404), Universidad Don Bosco, Fase II.

## Integrantes

- Helene Irene España Medina (EM260140): Docker, propuesta de Azure y módulo de Presupuesto.
- Carlos Alberto Gómez Flores (GF261954): base de datos y módulo de Movimientos.
- Jocelyn Alejandra Gutiérrez Lemus (GL261095): arquitectura, inicio de sesión, Mi perfil y seguridad.
- Helen Lisbeth Mejía Cea (MC260893): pantallas de Inicio y Metas, diseño y repositorio.
- Marlon Joel Mendoza Gálvez (MG260672): inteligencia artificial y notificaciones.

## Tecnologías

ASP.NET Core MVC (.NET 8), Entity Framework Core 8, SQL Server, CSS propio con el diseño de Figma, jQuery Validation, Docker y Docker Compose.

## Cómo ejecutarlo

Con Visual Studio: abrir `GestorFinanzas.sln`, elegir el perfil https y presionar F5. La primera vez se crea sola la base de datos `GestorFinanzasDB`. El sistema se abre en https://localhost:7080.

Con Docker: copiar `.env.example` como `.env`, poner una contraseña segura en `DB_PASSWORD` y ejecutar:

```
docker compose up --build
```

Luego abrir http://localhost:8080.

Para entrar se puede usar la cuenta de demostración `demo@estudiante.udb.edu.sv` con la contraseña `Demo1234!`, o crear una cuenta nueva desde Regístrate.

Los pasos completos están en el Manual de Instalación.

## Carpetas

- `src`: código fuente de la aplicación.
- `database`: scripts SQL para crear la base de datos y cargar datos de demostración.
- `docker`: instrucciones para ejecutar el sistema en contenedores.
- `docs`: manuales, propuesta de Azure, avance técnico y evidencias de pruebas.
