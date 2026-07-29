using System.Text.Json;
using JulesPanel.Models;

namespace JulesPanel.Services;

/// <summary>
/// Manages AccountProfile records persisted to accounts.json.
/// Thread-safe for Blazor Server: reads are lock-free against a volatile
/// snapshot; writes use an async semaphore and atomically swap the snapshot.
/// NEVER call .Wait() or .Result on this class — all sync reads are
/// intentionally lock-free to avoid circuit deadlocks.
/// </summary>
public class AccountService
{
    private readonly string _filePath;
    private readonly ILogger<AccountService> _logger;

    // Single writer gate — async only, never blocks the Blazor circuit.
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    // Volatile snapshot: reads never need a lock.
    // Writes atomically replace the reference after modifying a clone.
    private volatile IReadOnlyList<AccountProfile> _snapshot =
        Array.Empty<AccountProfile>();

    public event Action? OnChange;

    public AccountService(IWebHostEnvironment env, ILogger<AccountService> logger)
    {
        _filePath = Path.Combine(env.ContentRootPath, "accounts.json");
        _logger = logger;
        LoadFromDisk();
    }

    // ── Lock-free Reads (safe on Blazor circuit thread) ───────────────────────

    /// <summary>Returns the current account list. Zero allocation, no locks.</summary>
    public IReadOnlyList<AccountProfile> GetAll() => _snapshot;

    /// <summary>Finds an account by ID. Zero allocation, no locks.</summary>
    public AccountProfile? GetById(string? id)
    {
        if (id == null) return null;
        var snap = _snapshot;
        for (int i = 0; i < snap.Count; i++)
            if (snap[i].Id == id) return snap[i];
        return null;
    }

    // ── Async Writes ──────────────────────────────────────────────────────────

    public async Task<AccountProfile> AddAsync(AccountProfile account)
    {
        account.Id = Guid.NewGuid().ToString();
        account.CreatedAt = DateTime.UtcNow;

        await _writeLock.WaitAsync();
        try
        {
            var next = _snapshot.Append(account).ToList();
            await PersistAsync(next);
            _snapshot = next.AsReadOnly();
        }
        finally { _writeLock.Release(); }

        OnChange?.Invoke();
        return account;
    }

    public async Task<bool> UpdateAsync(AccountProfile updated)
    {
        await _writeLock.WaitAsync();
        bool found = false;
        try
        {
            var list = _snapshot.ToList();
            var idx = list.FindIndex(a => a.Id == updated.Id);
            if (idx >= 0)
            {
                list[idx] = updated;
                await PersistAsync(list);
                _snapshot = list.AsReadOnly();
                found = true;
            }
        }
        finally { _writeLock.Release(); }

        if (found) OnChange?.Invoke();
        return found;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        await _writeLock.WaitAsync();
        bool removed = false;
        try
        {
            var list = _snapshot.ToList();
            removed = list.RemoveAll(a => a.Id == id) > 0;
            if (removed)
            {
                await PersistAsync(list);
                _snapshot = list.AsReadOnly();
            }
        }
        finally { _writeLock.Release(); }

        if (removed) OnChange?.Invoke();
        return removed;
    }

    // ── Persistence ───────────────────────────────────────────────────────────

    private void LoadFromDisk()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                _snapshot = Array.Empty<AccountProfile>();
                return;
            }
            var json = File.ReadAllText(_filePath);
            var list = JsonSerializer.Deserialize<List<AccountProfile>>(json, JsonOpts) ?? new();
            _snapshot = list.AsReadOnly();
            _logger.LogInformation("Loaded {Count} accounts from {Path}", list.Count, _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load accounts from disk");
            _snapshot = Array.Empty<AccountProfile>();
        }
    }

    private async Task PersistAsync(List<AccountProfile> list)
    {
        try
        {
            var json = JsonSerializer.Serialize(list, JsonOpts);
            await File.WriteAllTextAsync(_filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save accounts to disk");
        }
    }
}
