using System.Security.Cryptography;
using System.Text.Json;
using Finanzas.Models;

namespace Finanzas.Services;

// Almacenamiento local para el prototipo. En producción conviene usar una base de datos.
public sealed class UserStore
{
    private readonly string _file;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly JsonSerializerOptions _json = new() { WriteIndented = true };
    public UserStore(IWebHostEnvironment env)
    {
        var folder = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(folder);
        _file = Path.Combine(folder, "users.json");
    }
    private async Task<List<AppUser>> ReadAsync()
    {
        if (!File.Exists(_file)) return new();
        await using var stream = File.OpenRead(_file);
        return await JsonSerializer.DeserializeAsync<List<AppUser>>(stream) ?? new();
    }
    private async Task WriteAsync(List<AppUser> users)
    {
        var temp = _file + ".tmp";
        await using (var stream = File.Create(temp)) await JsonSerializer.SerializeAsync(stream, users, _json);
        File.Move(temp, _file, true);
    }
    public async Task<(bool Ok, string Message, AppUser? User)> RegisterAsync(string name, string email, string password)
    {
        await _gate.WaitAsync();
        try
        {
            var users = await ReadAsync();
            email = email.Trim().ToLowerInvariant();
            if (users.Any(u => u.Email == email)) return (false, "Ya existe una cuenta con ese correo.", null);
            var salt = RandomNumberGenerator.GetBytes(16);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 210_000, HashAlgorithmName.SHA256, 32);
            var user = new AppUser { Name = name.Trim(), Email = email, PasswordSalt = Convert.ToBase64String(salt), PasswordHash = Convert.ToBase64String(hash) };
            users.Add(user); await WriteAsync(users);
            return (true, "Cuenta creada correctamente.", user);
        }
        finally { _gate.Release(); }
    }
    public async Task<AppUser?> ValidateAsync(string email, string password)
    {
        await _gate.WaitAsync();
        try
        {
            var user = (await ReadAsync()).FirstOrDefault(u => u.Email == email.Trim().ToLowerInvariant());
            if (user is null) return null;
            var salt = Convert.FromBase64String(user.PasswordSalt);
            var candidate = Rfc2898DeriveBytes.Pbkdf2(password, salt, 210_000, HashAlgorithmName.SHA256, 32);
            return CryptographicOperations.FixedTimeEquals(candidate, Convert.FromBase64String(user.PasswordHash)) ? user : null;
        }
        finally { _gate.Release(); }
    }
    public async Task<AppUser?> FindByIdAsync(string id)
    {
        await _gate.WaitAsync();
        try { return (await ReadAsync()).FirstOrDefault(u => u.Id == id); }
        finally { _gate.Release(); }
    }
}
