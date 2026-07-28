using System.Text.Json;
using JulesPanel.Models;

namespace JulesPanel.Services;

/// <summary>
/// Manages AccountProfile records — persisted to accounts.json alongside appsettings.
/// Thread-safe for Blazor Server concurrent access.
/// </summary>
public class AccountService
{
    private readonly string _filePath;
    private readonly ILogger<AccountService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private List<AccountProfile> _accounts = new();

    public AccountService(IWebHostEnvironment env, ILogger<AccountService> logger)
    {
        _filePath = Path.Combine(env.ContentRootPath, "accounts.json");
        _logger = logger;
        LoadFromDisk();
    }

    // ── Read ──────────────────────────────────────────────────────────────────

    public IReadOnlyList<AccountProfile> GetAll() => _accounts.AsReadOnly();

    public AccountProfile? GetById(string id) =>
        _accounts.FirstOrDefault(a => a.Id == id);

    // ── Write ─────────────────────────────────────────────────────────────────

    public async Task<AccountProfile> AddAsync(AccountProfile account)
    {
        account.Id = Guid.NewGuid().ToString();
        account.CreatedAt = DateTime.UtcNow;
        await _lock.WaitAsync();
        try
        {
            _accounts.Add(account);
            await SaveToDiskAsync();
        }
        finally { _lock.Release(); }
        return account;
    }

    public async Task<bool> UpdateAsync(AccountProfile updated)
    {
        await _lock.WaitAsync();
        try
        {
            var idx = _accounts.FindIndex(a => a.Id == updated.Id);
            if (idx < 0) return false;
            _accounts[idx] = updated;
            await SaveToDiskAsync();
            return true;
        }
        finally { _lock.Release(); }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        await _lock.WaitAsync();
        try
        {
            var removed = _accounts.RemoveAll(a => a.Id == id) > 0;
            if (removed) await SaveToDiskAsync();
            return removed;
        }
        finally { _lock.Release(); }
    }

    // ── Persistence ───────────────────────────────────────────────────────────

    private void LoadFromDisk()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                _accounts = new List<AccountProfile>();
                return;
            }
            var json = File.ReadAllText(_filePath);
            _accounts = JsonSerializer.Deserialize<List<AccountProfile>>(json, JsonOpts) ?? new();
            _logger.LogInformation("Loaded {Count} accounts from {Path}", _accounts.Count, _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load accounts from disk");
            _accounts = new List<AccountProfile>();
        }
    }

    private async Task SaveToDiskAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_accounts, JsonOpts);
            await File.WriteAllTextAsync(_filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save accounts to disk");
        }
    }
}
