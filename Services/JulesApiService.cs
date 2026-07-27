using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using JulesPanel.Models;

namespace JulesPanel.Services;

public class JulesApiService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<JulesApiService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public JulesApiService(HttpClient http, IConfiguration config, ILogger<JulesApiService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
        ConfigureClient();
    }

    private void ConfigureClient()
    {
        var apiKey = _config["Jules:ApiKey"] ?? string.Empty;
        _http.BaseAddress = new Uri(_config["Jules:BaseUrl"] ?? "https://jules.googleapis.com/v1alpha/");
        _http.DefaultRequestHeaders.Remove("x-goog-api-key");
        _http.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
    }

    // ── Sessions ─────────────────────────────────────────────────────────────

    public async Task<List<Session>> ListSessionsAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.GetAsync("sessions?pageSize=50", ct);
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("ListSessions response: {Content}", content);
            var result = JsonSerializer.Deserialize<ListSessionsResponse>(content, JsonOptions);
            return result?.Sessions ?? new List<Session>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list sessions");
            return new List<Session>();
        }
    }

    public async Task<Session?> GetSessionAsync(string sessionId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.GetAsync($"sessions/{ExtractId(sessionId)}", ct);
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync(ct);
            return JsonSerializer.Deserialize<Session>(content, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get session {SessionId}", sessionId);
            return null;
        }
    }

    public async Task<Session?> CreateSessionAsync(CreateSessionRequest request, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(request, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync("sessions", content, ct);
            resp.EnsureSuccessStatusCode();
            var respContent = await resp.Content.ReadAsStringAsync(ct);
            _logger.LogInformation("CreateSession response: {Content}", respContent);
            return JsonSerializer.Deserialize<Session>(respContent, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create session");
            return null;
        }
    }

    public async Task<bool> SendMessageAsync(string sessionId, string prompt, CancellationToken ct = default)
    {
        try
        {
            var request = new SendMessageRequest { Prompt = prompt };
            var json = JsonSerializer.Serialize(request, JsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync($"sessions/{ExtractId(sessionId)}:sendMessage", content, ct);
            resp.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<bool> ApprovePlanAsync(string sessionId, CancellationToken ct = default)
    {
        try
        {
            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync($"sessions/{ExtractId(sessionId)}:approvePlan", content, ct);
            resp.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve plan for session {SessionId}", sessionId);
            return false;
        }
    }

    // ── Activities ────────────────────────────────────────────────────────────

    // Strip the "sessions/" prefix if present so we don't double it in the URL
    private static string ExtractId(string nameOrId) =>
        nameOrId.StartsWith("sessions/", StringComparison.OrdinalIgnoreCase)
            ? nameOrId["sessions/".Length..]
            : nameOrId;

    public async Task<List<Activity>> ListActivitiesAsync(string sessionId, CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.GetAsync($"sessions/{ExtractId(sessionId)}/activities?pageSize=100", ct);
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync(ct);
            _logger.LogDebug("ListActivities response: {Content}", content);
            var result = JsonSerializer.Deserialize<ListActivitiesResponse>(content, JsonOptions);
            return result?.Activities ?? new List<Activity>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list activities for session {SessionId}", sessionId);
            return new List<Activity>();
        }
    }

    // ── Sources ───────────────────────────────────────────────────────────────

    public async Task<List<JulesSource>> ListSourcesAsync(CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.GetAsync("sources", ct);
            resp.EnsureSuccessStatusCode();
            var content = await resp.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<ListSourcesResponse>(content, JsonOptions);
            return result?.Sources ?? new List<JulesSource>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list sources");
            return new List<JulesSource>();
        }
    }

    // ── API Key Management ────────────────────────────────────────────────────

    public string GetCurrentApiKey() => _config["Jules:ApiKey"] ?? string.Empty;

    public void UpdateApiKey(string newKey)
    {
        _config["Jules:ApiKey"] = newKey;
        _http.DefaultRequestHeaders.Remove("x-goog-api-key");
        _http.DefaultRequestHeaders.Add("x-goog-api-key", newKey);
    }
}
