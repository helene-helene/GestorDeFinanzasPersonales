# Docker

El archivo `docker-compose.yml` está en la raíz del proyecto y el `Dockerfile` en `src/GestorFinanzas.Web`.

Para levantar el sistema:

1. Copiar `.env.example` como `.env` y cambiar `DB_PASSWORD` por una contraseña propia, con al menos 8 caracteres, mayúsculas, minúsculas, números y un símbolo.
2. En la raíz del proyecto, ejecutar `docker compose up --build`.
3. Abrir http://localhost:8080 en el navegador.

Se levantan dos contenedores. `gestor-db` ejecuta SQL Server 2022 en el puerto 1434 y guarda los datos en el volumen `sqlserver_data`. `gestor-web` ejecuta la aplicación en el puerto 8080; espera a que la base de datos esté lista y al arrancar aplica las migraciones.

Si se llenan `IA_ENDPOINT`, `IA_API_KEY` e `IA_MODEL` en el `.env`, la sugerencia de categoría usa ese servicio de IA; si se dejan vacíos, se usa el clasificador local.

Comandos útiles:

```
docker compose down            # detener
docker compose down -v         # detener y borrar la base de datos
docker compose logs -f webapp  # ver los registros de la aplicación
```
