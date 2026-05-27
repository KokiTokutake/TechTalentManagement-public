using TechTalentManagement.Application.Common;
using TechTalentManagement.Domain.Enums;

namespace TechTalentManagement.Application.Notification;

// ── DTOs ──────────────────────────────────────────────────────────────────

public record NotificationDto(
    Guid Id,
    string Type,
    string Title,
    string Body,
    string? LinkUrl,
    bool IsRead,
    DateTimeOffset? ReadAt,
    DateTimeOffset CreatedAt);

public record UnreadCountResponse(int Count);

public record ReadNotificationResponse(Guid Id, bool IsRead, DateTimeOffset? ReadAt);

public record ReadAllResponse(int UpdatedCount);

// ── Interfaces ────────────────────────────────────────────────────────────

public interface INotificationService
{
    Task<Result<IReadOnlyList<NotificationDto>>> GetListAsync(
        Guid requesterId, Guid userId, bool unreadOnly, CancellationToken ct = default);
    Task<Result<UnreadCountResponse>> GetUnreadCountAsync(
        Guid requesterId, Guid userId, CancellationToken ct = default);
    Task<Result<ReadNotificationResponse>> MarkAsReadAsync(
        Guid requesterId, Guid notificationId, CancellationToken ct = default);
    Task<Result<ReadAllResponse>> MarkAllAsReadAsync(
        Guid requesterId, Guid userId, CancellationToken ct = default);
    Task CreateAsync(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        string? linkUrl = null,
        CancellationToken ct = default);
}

public interface INotificationRepository
{
    Task<IReadOnlyList<Domain.Entities.Notification>> GetByUserIdAsync(
        Guid userId, bool unreadOnly, CancellationToken ct);
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct);
    Task<Domain.Entities.Notification?> FindByIdAsync(Guid id, CancellationToken ct);
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken ct);
    Task AddAsync(Domain.Entities.Notification notification, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

// ── Service ───────────────────────────────────────────────────────────────

public class NotificationService(INotificationRepository repo) : INotificationService
{
    public async Task<Result<IReadOnlyList<NotificationDto>>> GetListAsync(
        Guid requesterId, Guid userId, bool unreadOnly, CancellationToken ct = default)
    {
        if (requesterId != userId)
            return Result<IReadOnlyList<NotificationDto>>.Forbidden("自分の通知のみ参照できます。");

        var notifications = await repo.GetByUserIdAsync(userId, unreadOnly, ct);
        var dtos = notifications
            .Select(n => new NotificationDto(
                n.Id, n.Type.ToString(), n.Title, n.Body, n.LinkUrl,
                n.IsRead, n.ReadAt, n.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<NotificationDto>>.Ok(dtos);
    }

    public async Task<Result<UnreadCountResponse>> GetUnreadCountAsync(
        Guid requesterId, Guid userId, CancellationToken ct = default)
    {
        if (requesterId != userId)
            return Result<UnreadCountResponse>.Forbidden("自分の未読数のみ取得できます。");

        var count = await repo.GetUnreadCountAsync(userId, ct);
        return Result<UnreadCountResponse>.Ok(new UnreadCountResponse(count));
    }

    public async Task<Result<ReadNotificationResponse>> MarkAsReadAsync(
        Guid requesterId, Guid notificationId, CancellationToken ct = default)
    {
        var notification = await repo.FindByIdAsync(notificationId, ct);
        if (notification is null)
            return Result<ReadNotificationResponse>.NotFound("通知が見つかりません。");

        if (notification.UserId != requesterId)
            return Result<ReadNotificationResponse>.Forbidden("自分の通知のみ既読化できます。");

        notification.MarkAsRead();
        await repo.SaveChangesAsync(ct);

        return Result<ReadNotificationResponse>.Ok(
            new ReadNotificationResponse(notification.Id, notification.IsRead, notification.ReadAt));
    }

    public async Task<Result<ReadAllResponse>> MarkAllAsReadAsync(
        Guid requesterId, Guid userId, CancellationToken ct = default)
    {
        if (requesterId != userId)
            return Result<ReadAllResponse>.Forbidden("自分の通知のみ一括既読化できます。");

        var updatedCount = await repo.MarkAllAsReadAsync(userId, ct);
        return Result<ReadAllResponse>.Ok(new ReadAllResponse(updatedCount));
    }

    public async Task CreateAsync(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        string? linkUrl = null,
        CancellationToken ct = default)
    {
        var notification = Domain.Entities.Notification.Create(userId, type, title, body, linkUrl);
        await repo.AddAsync(notification, ct);
        await repo.SaveChangesAsync(ct);
    }
}
