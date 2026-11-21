using System.Text.Json;
using CompanyName.Domain.Common;
using CompanyName.Domain.Entities;
using CompanyName.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CompanyName.Infrastructure.Persistence.Interceptors;

internal sealed class InsertOutboxMessagesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        InsertOutboxMessages(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        InsertOutboxMessages(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void InsertOutboxMessages(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var outboxMessages = context.ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                var domainEvents = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();
                return domainEvents;
            })
            .Select(domainEvent => new OutboxMessage
            {
                Id = domainEvent.Id,
                OccurredOnUtc = domainEvent.OccurredOnUtc,
                Type = domainEvent.GetType().FullName!,
                Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType(), OutboxJson.SerializerOptions),
            })
            .ToList();

        if (outboxMessages.Count == 0)
        {
            return;
        }

        context.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}