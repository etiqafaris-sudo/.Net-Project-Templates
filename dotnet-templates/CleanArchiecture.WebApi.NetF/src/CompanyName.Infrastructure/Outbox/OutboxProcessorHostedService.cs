using System.Text.Json;
using CompanyName.Domain.Entities;
using CompanyName.Infrastructure.Persistence;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CompanyName.Infrastructure.Outbox;

internal sealed class OutboxProcessorHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly OutboxOptions _options;
    private readonly ILogger<OutboxProcessorHostedService> _logger;

    public OutboxProcessorHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxOptions> options,
        ILogger<OutboxProcessorHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollingInterval = TimeSpan.FromSeconds(Math.Max(1, _options.PollingIntervalSeconds));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedAny = await ProcessBatchAsync(stoppingToken);
                if (!processedAny)
                {
                    await Task.Delay(pollingInterval, stoppingToken);
                }
            }
            catch (Exception ex) when (!(ex is OperationCanceledException))
            {
                _logger.LogError(ex, "Outbox processing batch failed.");
                await Task.Delay(pollingInterval, stoppingToken);
            }
        }
    }

    private async Task<bool> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TemplateDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IMediator>();

        var batchSize = Math.Max(1, _options.BatchSize);
        var utcNow = DateTime.UtcNow;
        var lockUntil = utcNow.AddSeconds(Math.Max(1, _options.LockDurationSeconds));

        List<OutboxMessage> messages;
        using (var transaction = dbContext.Database.BeginTransaction())
        {
            messages = ClaimMessages(dbContext, batchSize, utcNow);
            if (messages.Count == 0)
            {
                transaction.Commit();
                return false;
            }

            foreach (var message in messages)
            {
                dbContext.OutboxMessages.Attach(message);
                message.LockedUntilUtc = lockUntil;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            transaction.Commit();
        }

        var maxAttempts = Math.Max(1, _options.MaxRetryAttempts);
        var retryDelay = TimeSpan.FromSeconds(Math.Max(1, _options.RetryDelaySeconds));
        var maxErrorLength = Math.Max(256, _options.MaxErrorLength);

        foreach (var message in messages)
        {
            try
            {
                var messageType = OutboxMessageTypeRegistry.Resolve(message.Type);

                var deserialized = JsonSerializer.Deserialize(message.Content, messageType, OutboxJson.SerializerOptions)
                    ?? throw new InvalidOperationException($"Failed to deserialize outbox message {message.Id}.");

                await MediatorNotificationPublisher.PublishAsync(publisher, deserialized, cancellationToken);

                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;
                message.LockedUntilUtc = null;
            }
            catch (Exception ex)
            {
                message.AttemptCount++;
                message.Error = Truncate(ex.ToString(), maxErrorLength);

                if (message.AttemptCount >= maxAttempts)
                {
                    _logger.LogError(
                        ex,
                        "Outbox message {OutboxMessageId} exceeded max attempts ({MaxAttempts}); marking dead-letter.",
                        message.Id,
                        maxAttempts);
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    message.LockedUntilUtc = null;
                }
                else
                {
                    _logger.LogWarning(
                        ex,
                        "Failed to process outbox message {OutboxMessageId} (attempt {AttemptCount}/{MaxAttempts})",
                        message.Id,
                        message.AttemptCount,
                        maxAttempts);
                    message.LockedUntilUtc = DateTime.UtcNow.Add(retryDelay);
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static List<OutboxMessage> ClaimMessages(TemplateDbContext dbContext, int batchSize, DateTime utcNow)
    {
        const string sql = @"
SELECT TOP ({0})
    [Id],
    [Type],
    [Content],
    [OccurredOnUtc],
    [ProcessedOnUtc],
    [Error],
    [LockedUntilUtc],
    [AttemptCount]
FROM [outbox_messages] WITH (UPDLOCK, READPAST, ROWLOCK)
WHERE [ProcessedOnUtc] IS NULL
  AND ([LockedUntilUtc] IS NULL OR [LockedUntilUtc] < {1})
ORDER BY [OccurredOnUtc]";

        return dbContext.Database.SqlQuery<OutboxMessage>(sql, batchSize, utcNow).ToList();
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value.Substring(0, maxLength);
}
