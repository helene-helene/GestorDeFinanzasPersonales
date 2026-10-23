using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GestorFinanzas.Web.Data.Repositories;
using GestorFinanzas.Web.Infrastructure;
using GestorFinanzas.Web.Models;
using GestorFinanzas.Web.Models.Entities;

namespace GestorFinanzas.Web.Services.IA;

// Integración con un servicio de IA compatible con OpenAI Chat Completions.
// Si en la configuración (sección "IA") no hay Endpoint y ApiKey, o si el servicio no responde, se usa un
// clasificador local por palabras clave: la aplicación sigue funcionando sin exponer claves.
// La descripción del gasto solo sale de la aplicación cuando el servicio externo está configurado.
public sealed class ServicioIA : IServicioIA
{
    private const string ProveedorExterno = "Servicio de IA";
    private const string ProveedorLocal = "Clasificador local de respaldo";
    private const string ProveedorAnalisis = "Análisis financiero";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuracion;
    private readonly ILogger<ServicioIA> _logger;
    private readonly ICategoriaRepository _categorias;
    private readonly IResumenRepository _resumen;
    private readonly IPresupuestoService _presupuestos;
    private readonly IMetaAhorroService _metas;
    private readonly ICurrentUserService _usuarioActual;

    public ServicioIA(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuracion,
        ILogger<ServicioIA> logger,
        ICategoriaRepository categorias,
        IResumenRepository resumen,
        IPresupuestoService presupuestos,
        IMetaAhorroService metas,
        ICurrentUserService usuarioActual)
    {
        _httpClientFactory = httpClientFactory;
        _configuracion = configuracion;
        _logger = logger;
        _categorias = categorias;
        _resumen = resumen;
        _presupuestos = presupuestos;
        _metas = metas;
        _usuarioActual = usuarioActual;
    }

    // ------------------------------------------------------------ Clasificación de gastos

    public async Task<ClasificacionIA?> ClasificarGastoAsync(string descripcion, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(descripcion)) return null;

        var categorias = await _categorias.ListarAsync(TipoMovimiento.Gasto);
        if (categorias.Count == 0) return null;

        var texto = descripcion.Trim();
        if (texto.Length > 200) texto = texto[..200];

        var externa = await ClasificarConServicioExternoAsync(texto, categorias, cancellationToken);
        return externa ?? ClasificarLocalmente(texto, categorias);
    }

    private async Task<ClasificacionIA?> ClasificarConServicioExternoAsync(
        string descripcion, IReadOnlyList<Categoria> categorias, CancellationToken cancellationToken)
    {
        var endpoint = _configuracion["IA:Endpoint"]?.Trim();
        var apiKey = _configuracion["IA:ApiKey"]?.Trim();
        var modelo = _configuracion["IA:Model"]?.Trim();

        if (string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(apiKey)) return null;
        if (string.IsNullOrWhiteSpace(modelo)) modelo = "gpt-4o-mini";

        try
        {
            var nombres = string.Join(", ", categorias.Select(c => c.Nombre));
            var pregunta = $"Clasifica este gasto en una sola categoría de esta lista: {nombres}. " +
                           $"Descripción: {descripcion}. Responde únicamente JSON con las propiedades category y reason.";

            var cuerpo = new
            {
                model = modelo,
                temperature = 0.1,
                messages = new[]
                {
                    new { role = "system", content = "Eres un clasificador de gastos. No inventes categorías." },
                    new { role = "user", content = pregunta }
                },
                response_format = new { type = "json_object" }
            };

            using var peticion = new HttpRequestMessage(HttpMethod.Post, endpoint);
            peticion.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            peticion.Content = new StringContent(JsonSerializer.Serialize(cuerpo), Encoding.UTF8, "application/json");

            using var respuesta = await _httpClientFactory.CreateClient("ia").SendAsync(peticion, cancellationToken);
            if (!respuesta.IsSuccessStatusCode)
            {
                _logger.LogWarning("El servicio de IA respondió con HTTP {Codigo}; se usa el clasificador local.", (int)respuesta.StatusCode);
                return null;
            }

            using var documento = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(cancellationToken));
            var contenido = documento.RootElement.GetProperty("choices")[0]
                .GetProperty("message").GetProperty("content").GetString() ?? "{}";

            var resultado = JsonSerializer.Deserialize<RespuestaClasificacion>(
                contenido, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var elegida = categorias.FirstOrDefault(c =>
                string.Equals(c.Nombre, resultado?.Category?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (elegida is null) return null;   // el servicio inventó una categoría: se usa el respaldo local

            var motivo = string.IsNullOrWhiteSpace(resultado?.Reason)
                ? "Clasificación generada por el servicio de IA."
                : resultado!.Reason!.Trim();

            return new ClasificacionIA(elegida.Id, elegida.Nombre, motivo, ProveedorExterno);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException
                                      or KeyNotFoundException or IndexOutOfRangeException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "No fue posible consultar el servicio de IA; se usa el clasificador local.");
            return null;
        }
    }

    // Palabras clave por categoría (minúsculas, sin acentos). Gana la primera coincidencia.
    private static readonly (string Categoria, string[] Claves)[] ReglasLocales =
    {
        ("Materiales de estudio", new[] { "libro", "libros", "cuaderno", "fotocopia", "fotocopias", "copias", "impresion", "papeleria", "lapiz", "calculadora", "folder", "tinta" }),
        ("Educación", new[] { "matricula", "colegiatura", "curso", "universidad", "arancel", "inscripcion", "clase", "clases", "diplomado", "examen" }),
        ("Transporte", new[] { "uber", "taxi", "bus", "ruta", "pasaje", "pasajes", "gasolina", "combustible", "parqueo", "estacionamiento", "transporte" }),
        ("Salud", new[] { "farmacia", "medicina", "medicinas", "medico", "doctor", "consulta", "dentista", "salud", "pastillas", "laboratorio" }),
        ("Entretenimiento", new[] { "cine", "juego", "juegos", "videojuego", "netflix", "spotify", "musica", "concierto", "fiesta", "boliche" }),
        ("Servicios", new[] { "luz", "internet", "telefono", "recarga", "alquiler", "renta", "wifi", "cable", "servicio", "servicios" }),
        ("Ropa y compras", new[] { "ropa", "zapatos", "camisa", "pantalon", "playera", "compras", "regalo", "tienda" }),
        ("Gastos hormiga", new[] { "cafe", "snack", "snacks", "golosina", "golosinas", "chicle", "dulce", "dulces", "helado", "gaseosa", "refresco", "galletas" }),
        ("Alimentación", new[] { "almuerzo", "cena", "desayuno", "pizza", "pupusa", "pupusas", "comida", "restaurante", "super", "supermercado", "mercado", "hamburguesa", "pollo", "tacos" })
    };

    private static ClasificacionIA ClasificarLocalmente(string descripcion, IReadOnlyList<Categoria> categorias)
    {
        var palabras = Separar(Normalizar(descripcion));

        foreach (var (nombre, claves) in ReglasLocales)
        {
            var clave = claves.FirstOrDefault(k => palabras.Any(p => p == k || (k.Length >= 5 && p.StartsWith(k))));
            if (clave is null) continue;

            var categoria = categorias.FirstOrDefault(c => string.Equals(c.Nombre, nombre, StringComparison.OrdinalIgnoreCase));
            if (categoria is not null)
            {
                return new ClasificacionIA(categoria.Id, categoria.Nombre,
                    $"La descripción menciona «{clave}», propio de {categoria.Nombre}.", ProveedorLocal);
            }
        }

        var otros = categorias.FirstOrDefault(c => string.Equals(c.Nombre, "Otros gastos", StringComparison.OrdinalIgnoreCase))
                    ?? categorias[0];

        return new ClasificacionIA(otros.Id, otros.Nombre,
            "No se encontró una palabra clave conocida; se sugiere una categoría general.", ProveedorLocal);
    }

    private static string Normalizar(string texto)
    {
        var descompuesto = texto.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(descompuesto.Length);
        foreach (var c in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark) sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string[] Separar(string texto) =>
        texto.Split(new[] { ' ', ',', '.', ';', ':', '-', '_', '/', '(', ')', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

    // ------------------------------------------------------------ Recomendaciones

    public async Task<IReadOnlyList<RecomendacionGenerada>> GenerarRecomendacionesAsync(CancellationToken cancellationToken = default)
    {
        var usuarioId = _usuarioActual.UsuarioId;
        var hoy = DateTime.Today;
        var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);

        var gastosDelMes = await _resumen.GastosPorCategoriaAsync(usuarioId, inicioMes, inicioMes.AddMonths(1));
        var presupuestos = await _presupuestos.ObtenerResumenAsync();
        var metas = await _metas.ListarAsync();

        var recomendaciones = new List<RecomendacionGenerada>();
        var avisadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var linea in presupuestos.Lineas.Where(l => l.Limite > 0 && l.Porcentaje >= 80))
        {
            avisadas.Add(linea.Categoria);
            recomendaciones.Add(new RecomendacionGenerada(
                "Presupuesto",
                $"Revisa tus gastos de {linea.Categoria}",
                $"Has usado el {linea.Porcentaje:0}% del límite ({Formato.Dinero(linea.Gastado)} de {Formato.Dinero(linea.Limite)}). " +
                "Considera pausar los gastos no esenciales hasta que termine el período.",
                linea.Superado ? "Alta" : "Media", null, ProveedorAnalisis));
        }

        var principal = gastosDelMes.FirstOrDefault();
        if (principal is not null && principal.Total > 0 && !avisadas.Contains(principal.Nombre))
        {
            var ahorro = Math.Round(principal.Total * 0.10m, 2);
            recomendaciones.Add(new RecomendacionGenerada(
                "Hábito",
                $"Observa tu categoría principal: {principal.Nombre}",
                $"Es tu mayor gasto de este mes con {Formato.Dinero(principal.Total)}. Reducirla un 10% liberaría cerca de {Formato.Dinero(ahorro)}.",
                "Media", ahorro, ProveedorAnalisis));
        }

        foreach (var meta in metas.Where(m => !m.Completa))
        {
            var semanas = meta.FechaLimite.HasValue ? Math.Max(1, (meta.FechaLimite.Value.Date - hoy).Days / 7) : 4;
            var porSemana = Math.Round(meta.Faltante / semanas, 2);
            var plazo = meta.FechaLimite.HasValue ? $" antes del {Formato.Fecha(meta.FechaLimite.Value)}" : string.Empty;

            recomendaciones.Add(new RecomendacionGenerada(
                "Meta",
                $"Impulsa tu meta: {meta.Nombre}",
                $"Ahorra aproximadamente {Formato.Dinero(porSemana)} por semana para llegar a tu objetivo{plazo}.",
                "Media", porSemana, ProveedorAnalisis));
        }

        if (recomendaciones.Count == 0)
        {
            recomendaciones.Add(new RecomendacionGenerada(
                "Bienestar",
                "Mantén el hábito de registrar tus gastos",
                "Aún no hay suficiente información para una alerta. Registra tus movimientos y define presupuestos para recibir recomendaciones personalizadas.",
                "Baja", null, ProveedorAnalisis));
        }

        return recomendaciones.Take(6).ToList();
    }

    private sealed class RespuestaClasificacion
    {
        [JsonPropertyName("category")] public string? Category { get; set; }
        [JsonPropertyName("reason")] public string? Reason { get; set; }
    }
}
