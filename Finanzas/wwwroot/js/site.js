// Funciones del sitio Finanzas. Los datos se consultan y guardan en el servidor por cuenta.
document.addEventListener("DOMContentLoaded", () => {
    const $ = (s, root = document) => root.querySelector(s);
    const $$ = (s, root = document) => [...root.querySelectorAll(s)];
    const money = n => new Intl.NumberFormat("es-SV", { style: "currency", currency: "USD" }).format(Number(n || 0));
    const esc = value => String(value ?? "").replace(/[&<>"']/g, c => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]));
    const message = (node, text) => { if (node) node.textContent = text; };
    async function api(url, options = {}) {
        const response = await fetch(url, { credentials: "same-origin", ...options, headers: { "Content-Type": "application/json", ...(options.headers || {}) } });
        if ((response.status === 401 && url !== "/api/login") || response.redirected) { window.location.href = "/Index"; throw new Error("Inicia sesión nuevamente."); }
        const data = await response.json().catch(() => ({}));
        if (!response.ok) throw new Error(data.message || "No se pudo completar la operación.");
        return data;
    }

    // Registro e inicio de sesión validan las cuentas guardadas en el servidor.
    const loginForm = $("#loginForm"), registerForm = $("#registerForm");
    $("#registerLink")?.addEventListener("click", e => { e.preventDefault(); loginForm.hidden = true; registerForm.hidden = false; message($("#registerMessage"), ""); });
    $("#loginLink")?.addEventListener("click", e => { e.preventDefault(); registerForm.hidden = true; loginForm.hidden = false; message($("#authMessage"), ""); });
    loginForm?.addEventListener("submit", async e => {
        e.preventDefault(); message($("#authMessage"), "Verificando datos…");
        try { await api("/api/login", { method: "POST", body: JSON.stringify({ email: $("#email").value, password: $("#password").value }) }); window.location.href = "/Dashboard"; }
        catch (err) { message($("#authMessage"), err.message === "Failed to fetch" ? "No se pudo conectar con la aplicación." : err.message); }
    });
    registerForm?.addEventListener("submit", async e => {
        e.preventDefault(); message($("#registerMessage"), "Creando cuenta…");
        try { await api("/api/register", { method: "POST", body: JSON.stringify({ name: $("#registerName").value, email: $("#registerEmail").value, password: $("#registerPassword").value }) }); window.location.href = "/Dashboard"; }
        catch (err) { message($("#registerMessage"), err.message === "Failed to fetch" ? "No se pudo conectar con la aplicación." : err.message); }
    });

    // Cargar perfil real en todas las páginas protegidas.
    if (!loginForm && !registerForm) {
        api("/api/me").then(user => {
            const initials = user.name.trim().split(/\s+/).slice(0, 2).map(x => x[0]).join("").toUpperCase();
            if ($("#headerAvatar")) $("#headerAvatar").textContent = initials || "?";
            if ($("#headerUserName")) $("#headerUserName").textContent = user.name;
            if ($("#profileAvatar")) $("#profileAvatar").textContent = initials || "?";
            if ($("#profileName")) $("#profileName").textContent = user.name;
            if ($("#profileEmail")) $("#profileEmail").textContent = user.email;
            if ($("#accountEmail")) $("#accountEmail").textContent = user.email;
            if ($("#memberSince")) $("#memberSince").textContent = new Date(user.createdAt).toLocaleDateString("es-SV", { month: "long", year: "numeric" });
            if ($("#dashboardGreeting")) $("#dashboardGreeting").textContent = `Hola, ${user.name.split(/\s+/)[0]}`;
        }).catch(() => {});
    }
    async function logout(e) { e.preventDefault(); try { await fetch("/api/logout", { method: "POST" }); } finally { window.location.href = "/Index"; } }
    $("#logoutLink")?.addEventListener("click", logout);
    $("#profileLogout")?.addEventListener("click", logout);

    // Formularios de movimientos: el registro se guarda de forma persistente y separado por cuenta.
    $$(".segment").forEach(btn => btn.addEventListener("click", () => { $$(".segment").forEach(b => b.classList.remove("active")); btn.classList.add("active"); if ($("#movementKind")) $("#movementKind").value = btn.dataset.kind || "Gasto"; }));
    $$(".category-choice").forEach(btn => btn.addEventListener("click", () => { $$(".category-choice").forEach(b => b.classList.remove("selected")); btn.classList.add("selected"); if ($("#category")) $("#category").value = btn.dataset.category || "Gastos personales"; }));
    $("#applySuggestion")?.addEventListener("click", () => { $$(".category-choice").forEach(b => b.classList.toggle("selected", b.dataset.category === "Comida")); $("#category").value = "Comida"; });
    $("#movementForm")?.addEventListener("submit", async e => {
        e.preventDefault(); const amount = Number($("#amount").value), description = $("#description").value.trim();
        try { await api("/api/movements", { method: "POST", body: JSON.stringify({ kind: $("#movementKind").value, amount, description, category: $("#category").value, note: $("#note").value }) });
            message($("#movementMessage"), "Movimiento guardado correctamente."); e.target.reset(); $("#movementKind").value = "Gasto"; $$(".segment").forEach(b => b.classList.toggle("active", b.dataset.kind === "Gasto")); $$(".category-choice").forEach(b => b.classList.toggle("selected", b.dataset.category === "Comida")); $("#category").value = "Comida"; await loadFinance();
        } catch (err) { message($("#movementMessage"), err.message); }
    });

    // Creación y aportes a metas guardadas por usuario.
    $("#goalForm")?.addEventListener("submit", async e => {
        e.preventDefault(); const target = Number($("#goalTarget").value), saved = Number($("#goalSaved").value);
        try { await api("/api/goals", { method: "POST", body: JSON.stringify({ name: $("#goalName").value.trim(), target, saved, dueDate: $("#goalDate").value || null }) }); window.location.href = "/Goals"; }
        catch (err) { message($("#goalFormMessage"), err.message); }
    });

    // Presupuesto mensual y límites por categoría.
    $("#editBudget")?.addEventListener("click", async () => {
        try {
            const current = await api("/api/finance");
            const raw = prompt("Presupuesto mensual en USD:", String(current.monthlyBudget || "")); if (raw === null) return;
            const monthlyBudget = Number(raw); if (!Number.isFinite(monthlyBudget) || monthlyBudget < 0) { alert("Escribe un monto válido, igual o mayor que cero."); return; }
            const categories = current.categoryBudgets || {};
            for (const name of ["Comida", "Transporte", "Gastos personales", "Libros"]) {
                const cat = prompt(`Límite para ${name} en USD (déjalo vacío para mantenerlo):`, categories[name] ?? "");
                if (cat === null) break;
                if (cat.trim() !== "") { const value = Number(cat); if (!Number.isFinite(value) || value < 0) { alert("El límite debe ser un monto válido."); return; } categories[name] = value; }
            }
            await api("/api/budget", { method: "PUT", body: JSON.stringify({ monthlyBudget, categoryBudgets: categories }) }); await loadFinance(); alert("Presupuesto guardado.");
        } catch (err) { alert(err.message); }
    });
    $$(".contribute-btn").forEach(btn => btn.addEventListener("click", async () => {
        const goals = await api("/api/finance").catch(err => { message($("#goalMessage"), err.message); return null; }); if (!goals) return;
        const goal = goals.goals.find(g => g.name === btn.dataset.goal); if (!goal) { message($("#goalMessage"), "No se encontró la meta. Crea una meta para comenzar."); return; }
        const raw = prompt(`¿Cuánto quieres aportar a “${goal.name}”? (USD)`); if (raw === null) return;
        try { await api(`/api/goals/${encodeURIComponent(goal.id)}/contributions`, { method: "POST", body: JSON.stringify({ amount: Number(raw) }) }); await loadFinance(); message($("#goalMessage"), "Aporte guardado."); }
        catch (err) { message($("#goalMessage"), err.message); }
    }));

    function renderGoals(goals) {
        const grid = $("#goalsGrid"); if (!grid) return;
        if (!goals.length) { grid.innerHTML = '<p class="muted">Todavía no tienes metas. Usa “Nueva meta” para crear la primera.</p>'; return; }
        grid.innerHTML = goals.map(g => { const pct = g.target > 0 ? Math.min(100, g.saved / g.target * 100) : 0; return `<article class="goal-card"><div class="goal-top"><div class="goal-emoji goal-laptop">◎</div><span class="pill">${pct >= 100 ? "Completada" : "En progreso"}</span></div><h2>${esc(g.name)}</h2><div class="goal-amount"><strong>${money(g.saved)}</strong><span>de ${money(g.target)}</span></div><div class="mini-track thick"><i style="width:${pct}%;background:#c96b4b"></i></div><div class="line-foot"><span>${pct.toFixed(0)}% completado</span><span>${g.dueDate ? "Hasta " + new Date(g.dueDate).toLocaleDateString("es-SV") : "Sin fecha límite"}</span></div><button class="btn btn-outline btn-full contribute-btn" data-goal-id="${esc(g.id)}" data-goal="${esc(g.name)}" ${pct >= 100 ? "disabled" : ""}>＋ Añadir ahorro</button></article>`; }).join("");
        $$(".contribute-btn[data-goal-id]").forEach(btn => btn.addEventListener("click", async () => { const raw = prompt(`¿Cuánto quieres aportar a “${btn.dataset.goal}”? (USD)`); if (raw === null) return; try { await api(`/api/goals/${encodeURIComponent(btn.dataset.goalId)}/contributions`, { method: "POST", body: JSON.stringify({ amount: Number(raw) }) }); await loadFinance(); message($("#goalMessage"), "Aporte guardado."); } catch (err) { message($("#goalMessage"), err.message); } }));
    }
    function renderBudget(data, spent) {
        if (!$("#budgetMonthly")) return;
        const budget = Number(data.monthlyBudget || 0), pct = budget ? Math.min(100, spent / budget * 100) : 0;
        $("#budgetMonthly").textContent = money(budget); $("#budgetSpent").textContent = money(spent); $("#budgetRemaining").textContent = money(Math.max(0, budget - spent)); $("#budgetPercent").textContent = `${pct.toFixed(1)}% del presupuesto`;
        const spentBy = {}; data.movements.filter(m => m.kind === "Gasto").forEach(m => spentBy[m.category] = (spentBy[m.category] || 0) + Number(m.amount));
        const limits = data.categoryBudgets || {}; const names = [...new Set([...Object.keys(limits), ...Object.keys(spentBy)])];
        $("#budgetCategoryCount").textContent = `${names.length} categorías`;
        $("#budgetLines").innerHTML = names.length ? names.map(name => { const total = spentBy[name] || 0, limit = Number(limits[name] || 0), p = limit ? Math.min(100, total / limit * 100) : 0; return `<div class="budget-line"><div class="budget-line-top"><span class="category-icon">${esc(name.slice(0,2).toUpperCase())}</span><div class="budget-line-title"><strong>${esc(name)}</strong><small>${money(total)} gastados${limit ? " de " + money(limit) : " · sin límite definido"}</small></div><strong>${limit ? money(limit) : "—"}</strong></div><div class="mini-track thick"><i style="width:${p}%;background:${p >= 90 ? "#b95b3d" : "#c96b4b"}"></i></div><div class="line-foot"><span>${limit ? p.toFixed(0) + "% utilizado" : "Define un límite desde Nuevo presupuesto"}</span><span>${limit ? "Quedan " + money(Math.max(0, limit-total)) : ""}</span></div></div>`; }).join("") : '<p class="muted">Registra movimientos y define tus límites para verlos aquí.</p>';
        const notice = $("#budgetNotice"), over = names.find(n => limits[n] > 0 && (spentBy[n] || 0) >= limits[n] * .9); if (notice) { notice.hidden = !over; if (over) { $("#budgetNoticeTitle").textContent = `Atención con ${over}`; $("#budgetNoticeText").textContent = `Has utilizado ${money(spentBy[over] || 0)} de ${money(limits[over])}.`; } }
    }
    async function loadFinance() {
        if (!$("#dashBalance") && !$("#budgetMonthly") && !$("#goalsGrid") && !$("#movementCount")) return;
        try {
            const d = await api("/api/finance"), movements = d.movements || [], goals = d.goals || [], month = new Date();
            const current = movements.filter(m => { const date = new Date(m.date); return date.getMonth() === month.getMonth() && date.getFullYear() === month.getFullYear(); });
            const spent = current.filter(m => m.kind === "Gasto").reduce((s,m) => s + Number(m.amount), 0), income = current.filter(m => m.kind === "Ingreso").reduce((s,m) => s + Number(m.amount), 0), budget = Number(d.monthlyBudget || 0), remaining = income - spent;
            if ($("#dashBalance")) { $("#dashBalance").textContent = money(remaining); $("#dashSpent").textContent = money(spent); $("#dashBudget").textContent = money(budget); $("#dashBudgetFoot").textContent = money(budget); const pct = budget ? Math.min(100, spent/budget*100) : 0; $("#dashProgress").style.width = pct+"%"; $("#dashPercent").textContent = pct.toFixed(0)+"% utilizado"; $("#dashExpenseStat").textContent = money(spent); $("#dashIncomeStat").textContent = money(income); $("#dashGoalsStat").textContent = `${goals.length} metas`; $("#dashboardTip").textContent = budget ? `Has utilizado ${pct.toFixed(0)}% de tu presupuesto mensual.` : "Define tu presupuesto mensual y registra tus movimientos para tener un resumen más útil.";
                const cats = {}; current.filter(m => m.kind === "Gasto").forEach(m => cats[m.category] = (cats[m.category]||0)+Number(m.amount)); const entries=Object.entries(cats).sort((a,b)=>b[1]-a[1]); $("#dashboardCategories").innerHTML = entries.length ? entries.map(([name,total])=>`<div class="category-row"><span class="category-icon">${esc(name.slice(0,2).toUpperCase())}</span><div class="category-info"><div><strong>${esc(name)}</strong><span>${money(total)}</span></div><div class="mini-track"><i style="width:${spent?Math.min(100,total/spent*100):0}%;background:#c96b4b"></i></div></div></div>`).join("") : '<p class="muted">Todavía no tienes gastos registrados.</p>'; }
            if ($("#movementCount")) { $("#movementCount").textContent = String(movements.length); $("#spentTotal").textContent = money(movements.filter(m=>m.kind==="Gasto").reduce((s,m)=>s+Number(m.amount),0)); }
            renderBudget(d, spent); renderGoals(goals);
        } catch (err) { if ($("#movementMessage")) message($("#movementMessage"), err.message); }
    }
    loadFinance();

    // Acciones de interfaz sencillas.
    $("#markRead")?.addEventListener("click", () => { $$(".notification-item").forEach(n => n.classList.remove("unread")); $$(".unread-dot").forEach(n => n.remove()); $("#markRead").textContent = "Todo leído ✓"; });
    $$(".apply-recommendation").forEach(btn => btn.addEventListener("click", () => { btn.textContent = "Aplicada ✓"; btn.classList.add("button-done"); btn.disabled = true; }));
    $("#editProfile")?.addEventListener("click", () => message($("#profileMessage"), "El perfil muestra los datos de la cuenta con la que iniciaste sesión."));
    $("#changePassword")?.addEventListener("click", () => alert("La función para cambiar contraseña se añadirá en una siguiente versión."));
    $("#helpSupport")?.addEventListener("click", () => alert("Ayuda y soporte de Finanzas."));
});
