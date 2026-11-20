using System;
using System.Text.Json;
using CompanyName.Application.Common.Interfaces;
using CompanyName.Domain.Entities;
using CompanyName.Infrastructure.Persistence;

namespace CompanyName.Infrastructure.Outbox;

internal sealed class OutboxWriter : IOutbox
{
    private readonly TemplateDbContext _context;

    public OutboxWriter(TemplateDbContext context)
    {
        _context = context;
    }

    public void Enqueue<T>(T message)
        where T : class
    {
        var outboxMessage = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            OccurredOnUtc = DateTime.UtcNow,
            Type = OutboxMessageTypeRegistry.GetTypeName<T>(),
            Content = JsonSerializer.Serialize(message, OutboxJson.SerializerOptions),
        };

        _context.OutboxMessages.Add(outboxMessage);
    }
}