namespace TeckTalentManagement.Application.OneOnOne;

public record MeetingListItemDto(
    Guid Id,
    Guid TalentId,
    string TalentName,
    Guid ManagerId,
    string ManagerName,
    DateOnly ConductedAt,
    string Format,
    string? NotesSummary,
    int ActionItemCount,
    int PendingActionItemCount,
    bool ShareWithMdc,
    DateTimeOffset CreatedAt
);

public record ActionItemDto(
    Guid Id,
    string Description,
    string Status,
    DateTimeOffset? CompletedAt,
    DateTimeOffset CreatedAt
);

public record MeetingDetailDto(
    Guid Id,
    Guid TalentId,
    string TalentName,
    Guid ManagerId,
    string ManagerName,
    DateOnly ConductedAt,
    string Format,
    string? WillTopics,
    string? CanTopics,
    string? MustTopics,
    string? CurrentSituation,
    string? FreeText,
    bool ShareWithMdc,
    IReadOnlyList<ActionItemDto> ActionItems,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public record MeetingListResponse(
    IReadOnlyList<MeetingListItemDto> Items,
    int TotalCount,
    int Page,
    int Limit
);

public record SharedMeetingSummaryDto(
    Guid Id,
    DateOnly ConductedAt,
    string? NotesSummary
);

public record OneOnOneSummaryDto(
    Guid TalentId,
    string TalentName,
    DateOnly? LastMeetingAt,
    int TotalMeetingCount,
    IReadOnlyList<SharedMeetingSummaryDto> SharedMeetings
);

public record CreateMeetingRequest(
    Guid TalentId,
    DateOnly ConductedAt,
    string Format,
    string? WillTopics,
    string? CanTopics,
    string? MustTopics,
    string? CurrentSituation,
    string? FreeText,
    bool ShareWithMdc,
    IReadOnlyList<string>? ActionItems
);

public record UpdateMeetingRequest(
    DateOnly ConductedAt,
    string Format,
    string? WillTopics,
    string? CanTopics,
    string? MustTopics,
    string? CurrentSituation,
    string? FreeText,
    bool ShareWithMdc,
    IReadOnlyList<string>? ActionItems
);

public record AddActionItemRequest(string Description);

public record UpdateActionItemStatusRequest(string Status);
