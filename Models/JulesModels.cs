using System.Text.Json.Serialization;

namespace JulesPanel.Models;

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

    // Derived helper
    public string Id => Name.Contains('/') ? Name.Split('/').Last() : Name;

    public bool IsArchived => State?.Equals("ARCHIVED", StringComparison.OrdinalIgnoreCase) == true;
    public bool IsWorking => State?.Contains("WORKING", StringComparison.OrdinalIgnoreCase) == true
                          || State?.Contains("RUNNING", StringComparison.OrdinalIgnoreCase) == true;
    public bool NeedsPlanApproval => State?.Contains("PLAN", StringComparison.OrdinalIgnoreCase) == true
                                  && State?.Contains("WAITING", StringComparison.OrdinalIgnoreCase) == true;
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
