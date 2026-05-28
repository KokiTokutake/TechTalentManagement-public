using Microsoft.EntityFrameworkCore;
using TechTalentManagement.Application.Notification;

namespace TechTalentManagement.Infrastructure.Persistence.Repositories;

public class NotificationRepository(AppDbContext db) : INotificationRepository
{
    public async Task<IReadOnlyList<Domain.Entities.Notification>> GetByUserIdAsync(
        Guid userId, bool unreadOnly, CancellationToken ct)
    {
        var query = db.Notifications.Where(n => n.UserId == userId);
        if (unreadOnly)
            query = query.Where(n => !n.IsRead);
        return await query.OrderByDescending(n => n.CreatedAt).ToListAsync(ct);
    }

    public Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct) =>
        db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead, ct);

    public Task<Domain.Entities.Notification?> FindByIdAsync(Guid id, CancellationToken ct) =>
        db.Notifications.FindAsync([id], ct).AsTask();

    public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken ct)
    {
        var notifications = await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var n in notifications)
            n.MarkAsRead();

        await db.SaveChangesAsync(ct);
        return notifications.Count;
    }

    public async Task AddAsync(Domain.Entities.Notification notification, CancellationToken ct) =>
        await db.Notifications.AddAsync(notification, ct);

    public Task SaveChangesAsync(CancellationToken ct) =>
        db.SaveChangesAsync(ct);
}
