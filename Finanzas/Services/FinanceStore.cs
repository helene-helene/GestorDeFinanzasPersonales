using System.Text.Json;
using Finanzas.Models;

namespace Finanzas.Services;

// Guarda los datos financieros separados por el identificador autenticado de cada usuario.
public sealed class FinanceStore
{
    private readonly string _file;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _json = new() { WriteIndented = true };
    public FinanceStore(IWebHostEnvironment env)
    {
        var folder = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(folder);
        _file = Path.Combine(folder, "finance-data.json");
    }
    private async Task<Dictionary<string, FinanceData>> ReadAllAsync()
    {
        if (!File.Exists(_file)) return new();
        await using var s = File.OpenRead(_file);
        return await JsonSerializer.DeserializeAsync<Dictionary<string, FinanceData>>(s) ?? new();
    }
    private async Task WriteAllAsync(Dictionary<string, FinanceData> all)
    {
        var temp = _file + ".tmp";
        await using (var s = File.Create(temp)) await JsonSerializer.SerializeAsync(s, all, _json);
        File.Move(temp, _file, true);
    }
    public async Task<FinanceData> GetAsync(string userId)
    {
        await _gate.WaitAsync(); try { var all = await ReadAllAsync(); return all.TryGetValue(userId, out var data) ? data : new FinanceData(); }
        finally { _gate.Release(); }
    }
    public async Task<T> UpdateAsync<T>(string userId, Func<FinanceData, T> update)
    {
        await _gate.WaitAsync();
        try { var all = await ReadAllAsync(); if (!all.TryGetValue(userId, out var data)) all[userId] = data = new FinanceData(); var result = update(data); await WriteAllAsync(all); return result; }
        finally { _gate.Release(); }
    }
}
