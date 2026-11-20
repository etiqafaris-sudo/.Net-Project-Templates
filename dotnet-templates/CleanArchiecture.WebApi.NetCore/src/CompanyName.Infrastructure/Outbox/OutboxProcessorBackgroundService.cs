using System.Text.Json;
using CompanyName.Domain.Entities;
using CompanyName.Infrastructure.Persistence;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CompanyName.Infrastructure.Outbox;

internal sealed class OutboxProcessorBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessorBackgroundService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var pollingInterval = TimeSpan.FromSeconds(Math.Max(1, options.Value.PollingIntervalSeconds));

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
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Outbox processing batch failed.");
                await Task.Delay(pollingInterval, stoppingToken);
            }
        }
    }

    private async Task<bool> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TemplateDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IMediator>();

        var opts = options.Value;
        var batchSize = Math.Max(1, opts.BatchSize);
        var utcNow = DateTime.UtcNow;
        var lockUntil = utcNow.AddSeconds(Math.Max(1, opts.LockDurationSeconds));

        var executionStrategy = dbContext.Database.CreateExecutionStrategy();
        var messages = await executionStrategy.ExecuteAsync(async () =>
        {
            await using var claimTransaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var claimed = await ClaimMessagesAsync(dbContext, batchSize, utcNow, cancellationToken);

            if (claimed.Count == 0)
            {
                await claimTransaction.CommitAsync(cancellationToken);
                return claimed;
            }

            foreach (var message in claimed)
            {
                message.LockedUntilUtc = lockUntil;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await claimTransaction.CommitAsync(cancellationToken);
            return claimed;
        });

        if (messages.Count == 0)
        {
            return false;
        }

        var maxAttempts = Math.Max(1, opts.MaxRetryAttempts);
        var retryDelay = TimeSpan.FromSeconds(Math.Max(1, opts.RetryDelaySeconds));
        var maxErrorLength = Math.Max(256, opts.MaxErrorLength);

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
                    logger.LogError(
                        ex,
                        "Outbox message {OutboxMessageId} exceeded max attempts ({MaxAttempts}); marking dead-letter.",
                        message.Id,
                        maxAttempts);
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    message.LockedUntilUtc = null;
                }
                else
                {
                    logger.LogWarning(
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

    private static async Task<List<OutboxMessage>> ClaimMessagesAsync(
        TemplateDbContext dbContext,
        int batchSize,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        return await dbContext.OutboxMessages
            .FromSqlInterpolated($"""
                SELECT TOP ({batchSize})
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
                  AND ([LockedUntilUtc] IS NULL OR [LockedUntilUtc] < {utcNow})
                ORDER BY [OccurredOnUtc]
                """)
            .ToListAsync(cancellationToken);
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}