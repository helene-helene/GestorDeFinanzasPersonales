FINANZAS — ASP.NET Core / .NET 8

REQUISITOS
- Visual Studio 2022 con la carga de trabajo “Desarrollo de ASP.NET y web”.
- SDK de .NET 8.

ABRIR Y EJECUTAR
1. Extrae la carpeta Finanzas en una ruta local, por ejemplo C:\Proyectos\Finanzas.
2. Abre Finanzas.csproj con Visual Studio.
3. Espera la restauración y selecciona Compilar > Compilar solución.
4. Presiona F5.
5. En la página inicial selecciona “Regístrate”, introduce tu nombre, un correo válido y una contraseña de al menos 8 caracteres.
6. Al registrarte se inicia sesión automáticamente. Luego podrás salir y volver a entrar con ese correo y contraseña.

CUENTAS Y SEGURIDAD DEL PROTOTIPO
- El nombre visible proviene del nombre que escribes al registrarte; no se adivina a partir del correo de Gmail.
- Los usuarios se guardan localmente en App_Data/users.json; las contraseñas se almacenan con hash PBKDF2 y sal aleatoria, no en texto plano.
- Cada navegador recibe una cookie de autenticación y las páginas internas requieren sesión.
- Es un prototipo académico local, no debe exponerse a internet ni utilizarse para datos financieros reales sin una base de datos, validaciones, protección CSRF/rate limiting, recuperación de cuenta y una configuración de producción.
- Los movimientos, las metas, los aportes y el presupuesto se guardan localmente en App_Data/finance-data.json y se separan por el identificador del usuario autenticado. El inicio y el presupuesto muestran totales derivados de los datos guardados.
- Los datos se guardan en archivos JSON para facilitar la ejecución del prototipo sin configurar un servidor de base de datos. Para una aplicación publicada conviene migrar a una base de datos real y añadir protección CSRF/rate limiting, recuperación de cuenta y configuración de producción.
