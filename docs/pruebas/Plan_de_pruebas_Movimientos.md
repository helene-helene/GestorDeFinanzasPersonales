# Plan de pruebas — Módulo Registrar movimiento

Completa las columnas **Resultado obtenido** y **Evidencia** al ejecutar cada caso y guarda las capturas de pantalla en `docs/pruebas/capturas/` con el nombre del caso (por ejemplo `MOV-03.png`).

**Preparación:** base de datos con los datos de demostración cargados y la aplicación en ejecución (ver `README.md`).

**Saldos iniciales del usuario de demostración** (antes de cualquier prueba):

| Cuenta | Tipo | Saldo |
|---|---|---|
| Efectivo | Efectivo | $119.50 |
| Cuenta de ahorro | Ahorro | $242.21 |
| Tarjeta de débito | Tarjeta | $31.36 |

> Los casos se ejecutan en orden: algunos usan el saldo que dejó el anterior.

## A. Registrar

| ID | Caso | Pasos | Resultado esperado | Resultado obtenido | Evidencia |
|---|---|---|---|---|---|
| MOV-01 | Registrar un gasto válido | Movimientos → Registrar. Tipo Gasto, cuenta Efectivo, categoría Alimentación, monto 19.50, fecha de hoy, descripción «Prueba». Guardar. | Mensaje de éxito. Aparece primero en el historial. Saldo de Efectivo: $100.00. | | |
| MOV-02 | Registrar un ingreso válido | Tipo Ingreso, cuenta Cuenta de ahorro, categoría Beca, monto 50.00. Guardar. | Mensaje de éxito. Saldo de Cuenta de ahorro: $292.21. | | |
| MOV-03 | Las categorías cambian según el tipo | Alternar entre Ingreso y Gasto en el formulario. | Con Gasto solo se ven categorías de gasto; con Ingreso, solo de ingreso. La categoría elegida se limpia si no corresponde. | | |
| MOV-04 | Campos obligatorios | Guardar con cuenta, categoría, monto y fecha vacíos. | Mensajes de validación en cada campo. No se guarda nada. | | |
| MOV-05 | Monto cero o negativo | Monto 0 y luego -5. | Mensaje «El monto debe ser mayor que 0.» No se guarda. | | |
| MOV-06 | Más de 2 decimales | Monto 1.234. | Se rechaza con el mensaje de máximo 2 decimales. | | |
| MOV-07 | Fecha futura | Fecha de mañana (se puede forzar desde las herramientas del navegador). | Se rechaza: «La fecha del movimiento no puede ser futura.» | | |
| MOV-08 | Descripción muy larga | Descripción de más de 200 caracteres. | El campo no admite más de 200 caracteres o aparece el mensaje de validación. | | |
| MOV-09 | Saldo insuficiente | Gasto de 500.00 en Efectivo. | Se rechaza: la cuenta no tiene saldo suficiente. El saldo no cambia. | | |
| MOV-10 | Tarjeta admite saldo negativo | Gasto de 50.00 en Tarjeta de débito. | Se guarda. Saldo de la tarjeta: -$18.64. | | |

## B. Consultar

| ID | Caso | Pasos | Resultado esperado | Resultado obtenido | Evidencia |
|---|---|---|---|---|---|
| MOV-11 | Historial y totales | Abrir Movimientos. | Lista ordenada del más reciente al más antiguo, 10 por página. Se muestran ingresos, gastos y balance. | | |
| MOV-12 | Paginación | Pulsar «Siguiente» y «Anterior». | Cambia la página y se conservan los filtros. | | |
| MOV-13 | Filtro por tipo | Filtrar por Gasto. | Solo gastos. Los totales cambian: ingresos en $0.00. | | |
| MOV-14 | Filtro por cuenta y categoría | Cuenta Efectivo y categoría Transporte. | Solo los movimientos que cumplen ambas condiciones. | | |
| MOV-15 | Filtro por fechas | Desde hace 10 días hasta hoy. | Solo movimientos de ese rango, incluyendo el día «hasta». | | |
| MOV-16 | Búsqueda por texto | Buscar «bus». | Aparecen los movimientos cuya descripción o categoría contiene el texto. | | |
| MOV-17 | Sin resultados | Buscar «zzzz». | Mensaje «No hay movimientos que coincidan con los filtros.» | | |

## C. Editar

| ID | Caso | Pasos | Resultado esperado | Resultado obtenido | Evidencia |
|---|---|---|---|---|---|
| MOV-18 | Editar el monto | Editar el gasto de MOV-01 y cambiar el monto a 10.00. | Mensaje de éxito. Saldo de Efectivo: $109.50. | | |
| MOV-19 | Cambiar de cuenta | Editar ese mismo gasto y pasarlo a Cuenta de ahorro. | Efectivo vuelve a $119.50 y Cuenta de ahorro baja $10.00 (de $292.21 a $282.21). | | |
| MOV-20 | Cambiar el tipo | Editar un gasto a Ingreso con una categoría de ingreso. | El saldo se ajusta con el efecto contrario. | | |
| MOV-21 | Edición que deja saldo negativo | Editar un gasto de Efectivo subiéndolo a 500.00. | Se rechaza por saldo insuficiente. El saldo no cambia. | | |

## D. Eliminar

| ID | Caso | Pasos | Resultado esperado | Resultado obtenido | Evidencia |
|---|---|---|---|---|---|
| MOV-22 | Confirmación | Pulsar eliminar en un movimiento. | Pantalla de confirmación con los datos del movimiento. «Cancelar» regresa sin borrar. | | |
| MOV-23 | Eliminar un gasto | Confirmar la eliminación del gasto de MOV-19. | Desaparece del historial. El saldo de la cuenta se restaura. | | |
| MOV-24 | Eliminar un ingreso que dejaría saldo negativo | Registrar un ingreso de 100 en Efectivo, gastar casi todo y luego intentar eliminar ese ingreso. | Se rechaza con mensaje de saldo insuficiente. | | |

## E. Seguridad y errores

| ID | Caso | Pasos | Resultado esperado | Resultado obtenido | Evidencia |
|---|---|---|---|---|---|
| MOV-25 | Movimiento inexistente | Abrir `/Movimientos/Edit/99999`. | Error 404. | | |
| MOV-26 | Token antifalsificación | Enviar el formulario sin el token (herramientas del navegador). | El servidor responde 400 y no guarda. | | |

## F. Base de datos

| ID | Caso | Pasos | Resultado esperado | Resultado obtenido | Evidencia |
|---|---|---|---|---|---|
| MOV-27 | Restricción de monto | En SSMS: insertar una transacción con `Monto = 0`. | SQL Server rechaza por la restricción `CK_Transacciones_Monto`. | | |
| MOV-28 | Restricción de tipo | En SSMS: insertar una transacción con `Tipo = 'Otro'`. | SQL Server rechaza por `CK_Transacciones_Tipo`. | | |
| MOV-29 | Correo único | En SSMS: insertar dos usuarios con el mismo correo. | El segundo falla por el índice `IX_Usuarios_Correo`. | | |
| MOV-30 | Categoría con movimientos | En SSMS: borrar una categoría usada por una transacción. | Falla por la llave foránea (no se elimina). | | |
| MOV-31 | Integridad tipo-categoría | Ejecutar la consulta 6 de `database/03_consultas_utiles.sql`. | Devuelve 0 filas. | | |
| MOV-32 | Migraciones | Eliminar la base y arrancar la aplicación. | Se crea la base, las 8 tablas, las 14 categorías y el usuario demo. | | |
