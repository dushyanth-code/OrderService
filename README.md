# Order Service - DDD Microservice with CQRS

.NET Core 8.0 microservice implementing Domain-Driven Design (DDD) with CQRS, MediatR, Repository Pattern, Domain Events, and Transactional Outbox Pattern for Order Management.

## Architecture

This project follows Clean Architecture principles with clear separation of concerns:

```
OrderService/
 OrderService.Domain/         # Core business logic (no dependencies)
 OrderService.Application/    # Use cases, commands, queries
 OrderService.Infrastructure/ # Data access, EF Core, repositories
 OrderService.API/           # REST API endpoints, Swagger
```

### Key Patterns Implemented

- **Domain-Driven Design (DDD)**: Aggregate roots, entities, value objects
- **CQRS**: Separate command and query responsibilities
- **Repository Pattern**: Abstract data access
- **Unit of Work**: Transaction management
- **Domain Events**: Event-driven architecture within the domain
- **Transactional Outbox Pattern**: Reliable event publishing
- **MediatR**: Command/Query/Event handlers
- **Pipeline Behaviors**: Cross-cutting concerns (logging, validation)

## ✨ Features

### Domain Model

- **Order Aggregate**: Root entity managing order lifecycle
- **OrderItem Entity**: Line items within an order
- **Value Objects**: Money, Address, OrderStatus
- **Domain Events**: OrderPlaced, OrderConfirmed, OrderShipped, OrderCancelled
- **Business Rules**: Enforced within the domain model

### Transactional Outbox Pattern

The service implements the **Transactional Outbox Pattern** to ensure reliable event publishing and maintain data consistency across distributed systems.

#### How It Works

1. **Domain Events Raised**: When business operations occur (e.g., order placed), domain events are added to the aggregate root
2. **Transactional Storage**: Events are persisted to the `OutboxMessages` table in the same database transaction as the business data
3. **Background Processing**: A background service (`OutboxProcessorService`) continuously polls and processes unprocessed events
4. **Event Publishing**: Events are published to external message brokers (RabbitMQ, Kafka, Azure Service Bus, etc.)
5. **Guaranteed Delivery**: Implements retry logic (max 3 attempts) with automatic failure handling

#### Outbox Message Structure

```csharp
public class OutboxMessage
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }          // Order ID
    public string EventType { get; set; }          // Event type name
    public string EventData { get; set; }          // Serialized JSON payload
    public DateTime CreatedAt { get; set; }        // When event was created
    public DateTime? ProcessedAt { get; set; }     // When successfully published
    public bool IsProcessed { get; set; }          // Processing status
    public int RetryCount { get; set; }            // Number of retry attempts
    public string? Error { get; set; }             // Last error message
}
```

### Background Service - OutboxProcessorService

The **OutboxProcessorService** is a hosted background service that runs continuously alongside the API to ensure reliable event delivery.

#### Service Configuration

```csharp
public class OutboxProcessorService : BackgroundService
{
    private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(5);
    private const int MaxRetryCount = 3;
    
    // Processes up to 10 messages per batch
    private const int BatchSize = 10;
}
```

#### Key Features

**1. Continuous Polling**
- Runs as an `IHostedService` in the background
- Polls the `OutboxMessages` table every **5 seconds**
- Does not block API request processing

**2. Batch Processing**
- Processes up to **10 unprocessed events** per batch
- Orders events by creation time (FIFO - First In, First Out)
- Efficient database queries with filtering on `IsProcessed` and `RetryCount`

**3. Retry Logic**
- Maximum **3 retry attempts** for failed events
- Events exceeding max retries are marked as failed
- Error messages are logged for troubleshooting

**4. Transactional Safety**
- Each batch is processed in a separate service scope
- Database context per batch ensures proper transaction isolation
- Successful processing updates are persisted immediately

**5. Comprehensive Logging**
- Logs service start/stop events
- Logs batch processing information (count, event types)
- Logs individual event success/failure with details
- Logs retry attempts with error context


#### Service Lifecycle

**Startup**
```csharp
// Registered in Program.cs
builder.Services.AddHostedService<OutboxProcessorService>();

// Automatically starts when application starts
_logger.LogInformation("Outbox Processor Service started");
```

**Runtime**
- Continuously processes events while application is running
- Gracefully handles exceptions without crashing
- Respects cancellation tokens for clean shutdown

**Shutdown**
- Stops processing when application stops
- Completes current batch before shutting down
- Logs shutdown event

#### Integration with Message Brokers

The service uses the `IOutboxEventPublisher` interface for publishing events:

```csharp
public interface IOutboxEventPublisher
{
    Task PublishAsync(string eventType, string eventData, CancellationToken cancellationToken);
}
```

**Current Implementation**: Stub implementation (events marked as processed but not published to actual broker)

**Production Implementation**: Replace with actual message broker client

```csharp
// Example: RabbitMQ Integration
public class RabbitMQEventPublisher : IOutboxEventPublisher
{
    private readonly IConnection _connection;
    
    public async Task PublishAsync(string eventType, string eventData, CancellationToken cancellationToken)
    {
        using var channel = _connection.CreateModel();
        
        var properties = channel.CreateBasicProperties();
        properties.ContentType = "application/json";
        properties.Type = eventType;
        
        var body = Encoding.UTF8.GetBytes(eventData);
        
        channel.BasicPublish(
            exchange: "orders",
            routingKey: eventType,
            basicProperties: properties,
            body: body
        );
        
        await Task.CompletedTask;
    }
}
```

#### Monitoring & Troubleshooting

**Log Messages to Monitor**:
```
[Information] Outbox Processor Service started
[Information] Processing 5 outbox messages
[Debug] Publishing event OrderPlacedDomainEvent for aggregate {id}
[Debug] Successfully published event OrderPlacedDomainEvent for aggregate {id}
[Information] Processed 5 outbox messages
[Error] Failed to publish event OrderPlacedDomainEvent for aggregate {id}. Retry count: 1
```

**Health Checks**:
- Monitor outbox table size: Large number of unprocessed events indicates publishing issues
- Check retry counts: Events with high retry counts need investigation
- Review error messages: Identify patterns in failures

**Common Issues**:
- **Message broker unavailable**: Events will accumulate and retry automatically
- **Serialization errors**: Check event data format and schema
- **Max retries exceeded**: Manual intervention required for dead-letter events

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/orders` | Create a new order |
| GET | `/api/orders/{id}` | Get order by ID |
| GET | `/api/orders/customer/{customerId}` | Get orders by customer |
| GET | `/api/orders/{id}/events` | Get order outbox events |
| PUT | `/api/orders/{id}/confirm` | Confirm an order |
| PUT | `/api/orders/{id}/cancel` | Cancel an order |
| PUT | `/api/orders/{id}/ship` | Ship an order |

## 🔄 Domain Events & Background Processing

### Event Lifecycle with Outbox Pattern

```
API Request ──> Command Handler ──> Aggregate
                                       │
                                       ├─> Raise Domain Event
                                       │
                                       ▼
                                  Unit of Work
                                       │
                                       ├─> Save Order (Transaction Start)
                                       ├─> Save OutboxMessage (Same Transaction)
                                       │
                                       ▼
                                  Transaction Commit ✓
                                       │
                                       ▼
                            [Event Safely Stored]
                                       │
        ┌──────────────────────────────┘
        │
        ▼
OutboxProcessorService (Background)
        │
        ├─> Query Unprocessed Events
        ├─> Publish to Message Broker
        └─> Mark as Processed
```

### Domain Events

Domain events are automatically stored in the outbox table during the same database transaction:

1. **OrderPlacedDomainEvent**: When an order is created
   - Contains: OrderId, CustomerId, TotalAmount
   
2. **OrderConfirmedDomainEvent**: When an order is confirmed
   - Contains: OrderId, ConfirmationTimestamp
   
3. **OrderShippedDomainEvent**: When an order is shipped
   - Contains: OrderId, TrackingNumber
   
4. **OrderCancelledDomainEvent**: When an order is cancelled
   - Contains: OrderId, Reason

Event handlers in the Application layer write these events to the outbox table, ensuring reliable delivery to external systems.

### Monitoring Outbox Events

Check the status of events for any order:

```bash
GET /api/orders/{orderId}/events
```

**Response Example**:
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "eventType": "OrderPlacedDomainEvent",
    "createdAt": "2024-02-01T10:30:00Z",
    "processedAt": "2024-02-01T10:30:05Z",
    "isProcessed": true,
    "retryCount": 0,
    "error": null
  },
  {
    "id": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "eventType": "OrderConfirmedDomainEvent",
    "createdAt": "2024-02-01T10:35:00Z",
    "processedAt": "2024-02-01T10:35:05Z",
    "isProcessed": true,
    "retryCount": 0,
    "error": null
  }
]
```

## Configuration

### Background Service Configuration

Modify polling interval and retry settings in `OutboxProcessorService.cs`:

```csharp
// Adjust processing frequency
private readonly TimeSpan _processingInterval = TimeSpan.FromSeconds(5);

// Change maximum retry attempts
private const int MaxRetryCount = 3;

// Modify batch size
.Take(10)  // Process 10 messages per batch
```

**Production Recommendations**:
- **High Volume Systems**: Reduce interval to 1-2 seconds, increase batch size
- **Low Volume Systems**: Increase interval to 10-30 seconds to reduce database load
- **Critical Systems**: Lower max retry count, implement dead-letter queue

## 🧪 Testing

### Manual Testing with Swagger

1. Start the application
2. Navigate to Swagger UI
3. Use the interactive interface to test all endpoints

### Testing Workflow with Outbox Pattern

1. **Create an Order** - POST `/api/orders`
2. **Immediately Check Events** - GET `/api/orders/{id}/events`
   - Should show `isProcessed: false`
3. **Wait 5-10 Seconds** (for background processor)
4. **Check Events Again** - GET `/api/orders/{id}/events`
   - Should show `isProcessed: true` with `processedAt` timestamp
5. **Confirm Order** - PUT `/api/orders/{id}/confirm`
6. **Verify New Event** - GET `/api/orders/{id}/events`
   - Should show both OrderPlaced and OrderConfirmed events

### Testing Background Service

**Monitor Logs**:
```bash
dotnet run --project OrderService.API

# Look for these log entries:
[Information] Outbox Processor Service started
[Information] Processing {Count} outbox messages
[Information] Processed {Count} outbox messages
```

**Simulate Failure**:
1. Temporarily break the `IOutboxEventPublisher` implementation
2. Create an order
3. Watch logs show retry attempts
4. Event should have `retryCount` incremented
5. Fix the publisher
6. Event should be processed on next attempt

## Development Guidelines

### Adding New Commands

1. Create command record in `Application/Commands/`
2. Create handler implementing `IRequestHandler<TCommand, TResult>`
3. MediatR automatically discovers and registers the handler

### Adding New Queries

1. Create query record in `Application/Queries/`
2. Create handler implementing `IRequestHandler<TQuery, TResult>`

### Adding New Domain Events

1. Define event in `Domain/Events/` implementing `IDomainEvent`
2. Raise event in aggregate using `AddDomainEvent()`
3. Create handler in `Application/EventHandlers/` implementing `INotificationHandler<TEvent>`
4. Handler should save event to outbox via domain event handler
5. Register handler in `Program.cs`
6. Background service will automatically pick up and process the event

### Implementing Production Message Broker

Replace the stub `OutboxEventPublisher` with your message broker client:

```csharp
// 1. Install NuGet package (e.g., RabbitMQ.Client)
// 2. Implement IOutboxEventPublisher
// 3. Register in Program.cs
builder.Services.AddScoped<IOutboxEventPublisher, RabbitMQEventPublisher>();

// 4. Configure connection settings in appsettings.json
{
  "MessageBroker": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest"
  }
}
```

## 📊 Monitoring & Observability

### Key Metrics

**Outbox Health**:
- Unprocessed message count: `SELECT COUNT(*) FROM OutboxMessages WHERE IsProcessed = false`
- Failed messages: `SELECT COUNT(*) FROM OutboxMessages WHERE RetryCount >= 3`
- Processing lag: Average time between `CreatedAt` and `ProcessedAt`

**Background Service**:
- Processing rate: Events processed per minute
- Success rate: Percentage of successful first-attempt publishes
- Retry rate: Percentage of events requiring retries

### Alerting Recommendations

- ⚠️ **Alert** if unprocessed message count > 100
- 🚨 **Critical** if unprocessed messages older than 5 minutes
- ⚠️ **Alert** if failed message count > 10
- 🚨 **Critical** if background service stops running

---

**Built with  ASP.NET Core 8.0, Domain-Driven Design, CQRS, and Transactional Outbox Pattern**

**Author**: Dushyanth - Order Service Team (orderservice@example.com)
