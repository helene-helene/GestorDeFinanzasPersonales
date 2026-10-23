// Pantalla de movimientos: filtra las categorías según el tipo y sugiere una categoría con la IA.
document.addEventListener('DOMContentLoaded', function () {
    var radios = document.querySelectorAll('input[name="Tipo"]');
    var selectCategoria = document.getElementById('CategoriaId');
    if (!selectCategoria || radios.length === 0) {
        return;
    }

    // ---- Muestra solo las categorías que corresponden al tipo elegido (Ingreso o Gasto).
    function aplicarFiltro() {
        var marcado = document.querySelector('input[name="Tipo"]:checked');
        var tipo = marcado ? marcado.value : null;
        var seleccionValida = false;

        selectCategoria.querySelectorAll('option[data-tipo]').forEach(function (opcion) {
            var visible = opcion.dataset.tipo === tipo;
            opcion.hidden = !visible;
            opcion.disabled = !visible;
            if (visible && opcion.selected) {
                seleccionValida = true;
            }
        });

        if (!seleccionValida) {
            selectCategoria.value = '';
        }
    }

    radios.forEach(function (radio) {
        radio.addEventListener('change', aplicarFiltro);
    });
    aplicarFiltro();

    // ---- Sugerencia inteligente: la IA propone la categoría a partir de la descripción del gasto.
    var caja = document.getElementById('sugerenciaIA');
    var descripcion = document.getElementById('Descripcion');
    var textoCategoria = document.getElementById('sugerenciaCategoria');
    var textoMotivo = document.getElementById('sugerenciaMotivo');
    var botonAplicar = document.getElementById('aplicarSugerencia');
    if (!caja || !descripcion || !textoCategoria || !textoMotivo || !botonAplicar) {
        return;
    }

    var url = caja.dataset.url;
    var ultima = null;
    var temporizador = null;
    var numeroPeticion = 0;

    function tipoActual() {
        var marcado = document.querySelector('input[name="Tipo"]:checked');
        return marcado ? marcado.value : null;
    }

    function ocultar() {
        caja.hidden = true;
        ultima = null;
    }

    function consultar() {
        var texto = descripcion.value.trim();
        if (tipoActual() !== 'Gasto' || texto.length < 3) {
            ocultar();
            return;
        }

        var campoToken = document.querySelector('input[name="__RequestVerificationToken"]');
        var esta = ++numeroPeticion;

        fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': campoToken ? campoToken.value : ''
            },
            body: JSON.stringify({ descripcion: texto })
        })
            .then(function (respuesta) { return respuesta.ok ? respuesta.json() : null; })
            .then(function (resultado) {
                if (esta !== numeroPeticion) {
                    return;   // llegó una respuesta vieja: se ignora
                }
                if (!resultado || !resultado.encontrada) {
                    ocultar();
                    return;
                }
                ultima = resultado;
                textoCategoria.textContent = resultado.categoria;
                textoMotivo.textContent = resultado.motivo + ' (' + resultado.proveedor + ')';
                caja.hidden = false;
            })
            .catch(ocultar);
    }

    descripcion.addEventListener('input', function () {
        clearTimeout(temporizador);
        temporizador = setTimeout(consultar, 500);
    });
    radios.forEach(function (radio) {
        radio.addEventListener('change', consultar);
    });

    botonAplicar.addEventListener('click', function () {
        if (!ultima) {
            return;
        }
        selectCategoria.value = String(ultima.categoriaId);
        selectCategoria.dispatchEvent(new Event('change', { bubbles: true }));
    });
});
