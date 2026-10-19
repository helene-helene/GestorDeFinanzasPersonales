// Muestra solo las categorías que corresponden al tipo elegido (Ingreso o Gasto).
document.addEventListener('DOMContentLoaded', function () {
    var radios = document.querySelectorAll('input[name="Tipo"]');
    var selectCategoria = document.getElementById('CategoriaId');
    if (!selectCategoria || radios.length === 0) {
        return;
    }

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
});
