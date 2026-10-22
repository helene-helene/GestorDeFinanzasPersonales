using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Finanzas.Services;
using Finanzas.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AllowAnonymousToPage("/Index");
});
builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<FinanceStore>();
builder.Services.AddHttpClient("ai", client => client.Timeout = TimeSpan.FromSeconds(15));
builder.Services.AddScoped<IArtificialIntelligenceService, ArtificialIntelligenceService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Index";
    options.Cookie.Name = "Finanzas.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});
builder.Services.AddAuthorization();
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Error"); app.UseHsts(); }
app.UseHttpsRedirection(); app.UseStaticFiles(); app.UseRouting(); app.UseAuthentication(); app.UseAuthorization();
app.MapPost("/api/register", async (RegisterRequest req, UserStore store, HttpContext ctx) =>
{
    if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password)) return Results.BadRequest(new { message = "Completa todos los campos." });
    if (req.Name.Trim().Length > 80 || req.Password.Length < 8 || req.Email.Length > 200 || !req.Email.Contains('@')) return Results.BadRequest(new { message = "Escribe un nombre válido, un correo válido y una contraseña de al menos 8 caracteres." });
    var result = await store.RegisterAsync(req.Name, req.Email, req.Password);
    if (!result.Ok) return Results.Conflict(new { message = result.Message });
    await SignIn(ctx, result.User!);
    return Results.Ok(new { message = result.Message, name = result.User!.Name, email = result.User.Email });
}).AllowAnonymous();
app.MapPost("/api/login", async (LoginRequest req, UserStore store, HttpContext ctx) =>
{
    var user = await store.ValidateAsync(req.Email ?? "", req.Password ?? "");
    if (user is null) return Results.Unauthorized();
    await SignIn(ctx, user);
    return Results.Ok(new { name = user.Name, email = user.Email });
}).AllowAnonymous();
app.MapGet("/api/me", async (ClaimsPrincipal principal, UserStore store) =>
{
    var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (id is null) return Results.Unauthorized();
    var user = await store.FindByIdAsync(id);
    return user is null ? Results.Unauthorized() : Results.Ok(new { name = user.Name, email = user.Email, createdAt = user.CreatedAt });
}).RequireAuthorization();
app.MapPost("/api/logout", async (HttpContext ctx) => { await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); return Results.Ok(new { ok = true }); }).RequireAuthorization();

// API financiera: todos los datos quedan aislados por el usuario autenticado.
string? CurrentUserId(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.NameIdentifier);
app.MapGet("/api/finance", async (ClaimsPrincipal user, FinanceStore store) =>
{
    var id = CurrentUserId(user); if (id is null) return Results.Unauthorized();
    return Results.Ok(await store.GetAsync(id));
}).RequireAuthorization();
app.MapPost("/api/ai/classify", async (AiClassifyRequest req, ClaimsPrincipal user, IArtificialIntelligenceService ai, CancellationToken cancellationToken) =>
{
    if (CurrentUserId(user) is null) return Results.Unauthorized();
    if (string.IsNullOrWhiteSpace(req.Description)) return Results.BadRequest(new { message = "Escribe una descripción para clasificar el gasto." });
    var result = await ai.ClassifyExpenseAsync(req.Description, req.Note, cancellationToken);
    return Results.Ok(result);
}).RequireAuthorization();
app.MapGet("/api/ai/recommendations", async (ClaimsPrincipal user, FinanceStore store, IArtificialIntelligenceService ai, CancellationToken cancellationToken) =>
{
    var id = CurrentUserId(user); if (id is null) return Results.Unauthorized();
    var data = await store.GetAsync(id);
    return Results.Ok(new { recommendations = await ai.BuildRecommendationsAsync(data, cancellationToken) });
}).RequireAuthorization();

app.MapPost("/api/movements", async (MovementRequest req, ClaimsPrincipal user, FinanceStore store) =>
{
    var id = CurrentUserId(user); if (id is null) return Results.Unauthorized();
    if (req.Amount <= 0 || string.IsNullOrWhiteSpace(req.Description) || (req.Kind != "Gasto" && req.Kind != "Ingreso")) return Results.BadRequest(new { message = "Revisa el tipo, monto y descripción." });
    var movement = new FinanceMovement { Kind = req.Kind, Amount = req.Amount, Description = req.Description.Trim(), Category = string.IsNullOrWhiteSpace(req.Category) ? "Gastos personales" : req.Category.Trim(), Note = req.Note?.Trim() ?? "", Date = DateTime.UtcNow };
    await store.UpdateAsync(id, data => { data.Movements.Add(movement); return true; });
    return Results.Ok(movement);
}).RequireAuthorization();
app.MapPost("/api/goals", async (GoalRequest req, ClaimsPrincipal user, FinanceStore store) =>
{
    var id = CurrentUserId(user); if (id is null) return Results.Unauthorized();
    if (string.IsNullOrWhiteSpace(req.Name) || req.Target <= 0 || req.Saved < 0 || req.Saved > req.Target) return Results.BadRequest(new { message = "Revisa el nombre y los montos de la meta." });
    var goal = new FinanceGoal { Name = req.Name.Trim(), Target = req.Target, Saved = req.Saved, DueDate = req.DueDate };
    await store.UpdateAsync(id, data => { data.Goals.Add(goal); return true; });
    return Results.Ok(goal);
}).RequireAuthorization();
app.MapPost("/api/goals/{goalId}/contributions", async (string goalId, ContributionRequest req, ClaimsPrincipal user, FinanceStore store) =>
{
    var id = CurrentUserId(user); if (id is null) return Results.Unauthorized();
    if (req.Amount <= 0) return Results.BadRequest(new { message = "El aporte debe ser mayor que cero." });
    var result = await store.UpdateAsync(id, data => { var goal = data.Goals.FirstOrDefault(g => g.Id == goalId); if (goal is null) return (Found: false, Goal: (FinanceGoal?)null); goal.Saved = Math.Min(goal.Target, goal.Saved + req.Amount); return (Found: true, Goal: (FinanceGoal?)goal); });
    return result.Found ? Results.Ok(result.Goal) : Results.NotFound(new { message = "No se encontró esa meta." });
}).RequireAuthorization();
app.MapPut("/api/budget", async (BudgetRequest req, ClaimsPrincipal user, FinanceStore store) =>
{
    var id = CurrentUserId(user); if (id is null) return Results.Unauthorized();
    if (req.MonthlyBudget < 0 || req.CategoryBudgets is null || req.CategoryBudgets.Any(x => string.IsNullOrWhiteSpace(x.Key) || x.Value < 0)) return Results.BadRequest(new { message = "Los presupuestos no pueden ser negativos." });
    await store.UpdateAsync(id, data => { data.MonthlyBudget = req.MonthlyBudget; data.CategoryBudgets = new Dictionary<string, decimal>(req.CategoryBudgets, StringComparer.OrdinalIgnoreCase); return true; });
    return Results.Ok(new { message = "Presupuesto guardado." });
}).RequireAuthorization();
app.MapRazorPages();
app.Run();
static Task SignIn(HttpContext ctx, Finanzas.Models.AppUser user)
{
    var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id), new Claim(ClaimTypes.Name, user.Name), new Claim(ClaimTypes.Email, user.Email) };
    return ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
}
record RegisterRequest(string? Name, string? Email, string? Password);
record LoginRequest(string? Email, string? Password);

record MovementRequest(string? Kind, decimal Amount, string? Description, string? Category, string? Note);
record GoalRequest(string? Name, decimal Target, decimal Saved, DateTime? DueDate);
record ContributionRequest(decimal Amount);
record BudgetRequest(decimal MonthlyBudget, Dictionary<string, decimal>? CategoryBudgets);
record AiClassifyRequest(string? Description, string? Note);
