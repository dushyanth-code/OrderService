using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.BackgroundServices;

public class OutboxProcessorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxProcessorService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(5);
    private const int MaxRetryCount = 3;

    public OutboxProcessorService(
        IServiceProvider serviceProvider,
        ILogger<OutboxProcessorService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Outbox Processor Service stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var eventPublisher = scope.ServiceProvider.GetRequiredService<IOutboxEventPublisher>();

        var unprocessedMessages = await dbContext.OutboxMessages
            .Where(m => !m.IsProcessed && m.RetryCount < MaxRetryCount)
            .OrderBy(m => m.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        if (!unprocessedMessages.Any())
        {
            return;
        }

        _logger.LogInformation("Processing {Count} outbox messages", unprocessedMessages.Count);

        foreach (var message in unprocessedMessages)
        {
            try
            {
                _logger.LogDebug("Publishing event {EventType} for aggregate {AggregateId}", 
                    message.EventType, message.AggregateId);

                //TODO--Publish to topic
                //await PublishAsync(message.EventType, message.EventData, cancellationToken);

                message.MarkAsProcessed();
                
                _logger.LogDebug("Successfully published event {EventType} for aggregate {AggregateId}", 
                    message.EventType, message.AggregateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to publish event {EventType} for aggregate {AggregateId}. Retry count: {RetryCount}", 
                    message.EventType, message.AggregateId, message.RetryCount);

                message.MarkAsFailed(ex.Message);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Processed {Count} outbox messages", unprocessedMessages.Count);
    }
}
