using System.Text.Json.Serialization;

namespace JulesPanel.Models;

// ─── Account Profile ─────────────────────────────────────────────────────────

public class AccountProfile
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; } = string.Empty;

    [JsonPropertyName("color")]
    public string Color { get; set; } = "#a78bfa";

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Initials for avatar
    public string Initials => string.IsNullOrWhiteSpace(Name)
        ? "?"
        : string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2).Select(w => w[0])).ToUpper();
}

// ─── Session ────────────────────────────────────────────────────────────────

public class Session
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;

    [JsonPropertyName("createTime")]
    public DateTimeOffset? CreateTime { get; set; }

    [JsonPropertyName("updateTime")]
    public DateTimeOffset? UpdateTime { get; set; }

    [JsonPropertyName("sourceContext")]
    public SourceContext? SourceContext { get; set; }

    // Multi-Account binding (client-side)
    public string? AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? AccountColor { get; set; }

    // Derived helpers
    public string Id => Name.Contains('/') ? Name.Split('/').Last() : Name;
    public string ShortId => Id.Length > 8 ? Id[..8] : Id;

    public string DisplayTitle => !string.IsNullOrWhiteSpace(Title)
        ? Title
        : (!string.IsNullOrWhiteSpace(Prompt) ? CleanPromptHeadline(Prompt) : $"جلسه {ShortId}");

    public string CleanPrompt => !string.IsNullOrWhiteSpace(Prompt) ? Prompt.Trim() : string.Empty;

    public string RepositoryDisplay => SourceContext?.Source?.Replace("sources/", "") ?? string.Empty;
    public string BranchDisplay => SourceContext?.GithubRepoContext?.StartingBranch ?? string.Empty;

    public string FormattedTime => CreateTime?.ToLocalTime().ToString("yyyy/MM/dd HH:mm") ?? "—";
    public string FormattedTimeShort => CreateTime?.ToLocalTime().ToString("MM/dd HH:mm") ?? "—";

    public bool IsArchived => State?.Equals("ARCHIVED", StringComparison.OrdinalIgnoreCase) == true;

    public bool IsWorking => State?.Contains("WORKING", StringComparison.OrdinalIgnoreCase) == true
                          || State?.Contains("RUNNING", StringComparison.OrdinalIgnoreCase) == true
                          || State?.Equals("IN_PROGRESS", StringComparison.OrdinalIgnoreCase) == true
                          || State?.Equals("PLANNING", StringComparison.OrdinalIgnoreCase) == true
                          || State?.Equals("QUEUED", StringComparison.OrdinalIgnoreCase) == true;

    public bool NeedsPlanApproval => (State?.Contains("PLAN", StringComparison.OrdinalIgnoreCase) == true
                                   && State?.Contains("WAITING", StringComparison.OrdinalIgnoreCase) == true)
                                   || State?.Equals("AWAITING_USER_FEEDBACK", StringComparison.OrdinalIgnoreCase) == true;

    public bool IsCompleted => State?.Equals("COMPLETED", StringComparison.OrdinalIgnoreCase) == true
                            || State?.Equals("DONE", StringComparison.OrdinalIgnoreCase) == true;

    public bool IsPaused => State?.Equals("PAUSED", StringComparison.OrdinalIgnoreCase) == true;

    public bool IsFailed => State?.Contains("FAIL", StringComparison.OrdinalIgnoreCase) == true
                         || State?.Contains("ERROR", StringComparison.OrdinalIgnoreCase) == true
                         || State?.Equals("CANCELLED", StringComparison.OrdinalIgnoreCase) == true;

    public string NormalizedStatusCategory
    {
        get
        {
            if (NeedsPlanApproval) return "AWAITING_FEEDBACK";
            if (IsWorking) return "WORKING";
            if (IsCompleted) return "COMPLETED";
            if (IsPaused) return "PAUSED";
            if (IsFailed) return "FAILED";
            return "OTHER";
        }
    }

    public string PersianStatusLabel
    {
        get
        {
            if (NeedsPlanApproval) return "منتظر تأیید شما";
            if (IsWorking) return "در حال کار";
            if (IsCompleted) return "تکمیل شده";
            if (IsPaused) return "متوقف شده";
            if (IsFailed) return "خطا / ناموفق";
            return State ?? "نامشخص";
        }
    }

    public string StatusBadgeClass
    {
        get
        {
            if (NeedsPlanApproval) return "warning";
            if (IsWorking) return "working";
            if (IsCompleted) return "done";
            if (IsPaused) return "archived";
            if (IsFailed) return "error";
            return "idle";
        }
    }

    private static string CleanPromptHeadline(string prompt)
    {
        var firstLine = prompt.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? prompt;
        if (firstLine.Length > 70)
            return firstLine[..67] + "...";
        return firstLine;
    }
}

public class SourceContext
{
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("githubRepoContext")]
    public GithubRepoContext? GithubRepoContext { get; set; }
}

public class GithubRepoContext
{
    [JsonPropertyName("startingBranch")]
    public string StartingBranch { get; set; } = string.Empty;
}

// ─── Activity ────────────────────────────────────────────────────────────────

public class Activity
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("originator")]
    public string Originator { get; set; } = string.Empty;  // "agent" or "user"

    [JsonPropertyName("createTime")]
    public DateTimeOffset? CreateTime { get; set; }

    [JsonPropertyName("agentMessaged")]
    public AgentMessaged? AgentMessaged { get; set; }

    [JsonPropertyName("userMessaged")]
    public UserMessaged? UserMessaged { get; set; }

    [JsonPropertyName("plan")]
    public Plan? Plan { get; set; }

    // Helper: get the text content regardless of source
    public string? Text => AgentMessaged?.AgentMessage ?? UserMessaged?.UserMessage;
    public bool IsAgent => Originator?.ToLower() == "agent";
    public bool IsUser  => Originator?.ToLower() == "user";
    public bool HasContent => Text != null || Plan != null;
}

public class AgentMessaged
{
    [JsonPropertyName("agentMessage")]
    public string? AgentMessage { get; set; }
}

public class UserMessaged
{
    [JsonPropertyName("userMessage")]
    public string? UserMessage { get; set; }
}

public class Plan
{
    [JsonPropertyName("steps")]
    public List<PlanStep> Steps { get; set; } = new();

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}

public class PlanStep
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("state")]
    public string State { get; set; } = string.Empty;
}

// ─── Source ──────────────────────────────────────────────────────────────────

public class JulesSource
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("githubRepo")]
    public GithubRepo? GithubRepo { get; set; }

    // Computed display name from owner/repo
    public string DisplayName => GithubRepo != null
        ? $"{GithubRepo.Owner}/{GithubRepo.Repo}"
        : Name;
}

public class GithubRepo
{
    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;

    [JsonPropertyName("repo")]
    public string Repo { get; set; } = string.Empty;

    [JsonPropertyName("isPrivate")]
    public bool IsPrivate { get; set; }

    [JsonPropertyName("defaultBranch")]
    public BranchInfo? DefaultBranch { get; set; }

    [JsonPropertyName("branches")]
    public List<BranchInfo> Branches { get; set; } = new();

    public string DefaultBranchName => DefaultBranch?.DisplayName ?? "main";
}

public class BranchInfo
{
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;
}

// ─── API Requests & Responses ────────────────────────────────────────────────

public class CreateSessionRequest
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("sourceContext")]
    public SourceContext? SourceContext { get; set; }

    [JsonPropertyName("requirePlanApproval")]
    public bool RequirePlanApproval { get; set; } = true;

    [JsonPropertyName("automationMode")]
    public string? AutomationMode { get; set; }
}

public class SendMessageRequest
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;
}

public class ListSessionsResponse
{
    [JsonPropertyName("sessions")]
    public List<Session> Sessions { get; set; } = new();

    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}

public class ListActivitiesResponse
{
    [JsonPropertyName("activities")]
    public List<Activity> Activities { get; set; } = new();

    [JsonPropertyName("nextPageToken")]
    public string? NextPageToken { get; set; }
}

public class ListSourcesResponse
{
    [JsonPropertyName("sources")]
    public List<JulesSource> Sources { get; set; } = new();
}
