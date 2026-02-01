# Order Service - DDD Microservice with CQRS

.NET Core 8.0 microservice implementing Domain-Driven Design (DDD) with CQRS, MediatR, Repository Pattern, and Domain Events for Order Management.

## ??? Architecture

This project follows Clean Architecture principles with clear separation of concerns:

```
OrderService/
??? OrderService.Domain/         # Core business logic (no dependencies)
??? OrderService.Application/    # Use cases, commands, queries
??? OrderService.Infrastructure/ # Data access, EF Core, repositories
??? OrderService.API/           # REST API endpoints, Swagger
```

### Key Patterns Implemented

- **Domain-Driven Design (DDD)**: Aggregate roots, entities, value objects
- **CQRS**: Separate command and query responsibilities
- **Repository Pattern**: Abstract data access
- **Unit of Work**: Transaction management
- **Domain Events**: Event-driven architecture within the domain
- **MediatR**: Command/Query/Event handlers
- **Pipeline Behaviors**: Cross-cutting concerns (logging, validation)

## ?? Features

### Domain Model

- **Order Aggregate**: Root entity managing order lifecycle
- **OrderItem Entity**: Line items within an order
- **Value Objects**: Money, Address, OrderStatus
- **Domain Events**: OrderPlaced, OrderConfirmed, OrderShipped, OrderCancelled
- **Business Rules**: Enforced within the domain model

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/orders` | Create a new order |
| GET | `/api/orders/{id}` | Get order by ID |
| GET | `/api/orders/customer/{customerId}` | Get orders by customer |
| PUT | `/api/orders/{id}/confirm` | Confirm an order |
| PUT | `/api/orders/{id}/cancel` | Cancel an order |
| PUT | `/api/orders/{id}/ship` | Ship an order |

## ?? Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code
- (Optional) SQL Server for production use

### Installation

1. **Clone the repository**
   ```bash
   cd OrderService
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the API**
   ```bash
   cd OrderService.API
   dotnet run
   ```

5. **Access Swagger UI**
   
   Open your browser and navigate to:
   ```
   https://localhost:7xxx/swagger
   ```
   (Replace xxx with the actual port shown in the console)

## ?? Usage Examples

### Create an Order

```bash
POST /api/orders
Content-Type: application/json

{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "shippingAddress": {
    "street": "123 Main St",
    "city": "New York",
    "state": "NY",
    "zipCode": "10001",
    "country": "USA"
  },
  "items": [
    {
      "productId": "7fa85f64-5717-4562-b3fc-2c963f66afa6",
      "productName": "Product A",
      "unitPrice": 29.99,
      "currency": "USD",
      "quantity": 2
    }
  ]
}
```

### Get Order by ID

```bash
GET /api/orders/{orderId}
```

### Confirm an Order

```bash
PUT /api/orders/{orderId}/confirm
```

### Cancel an Order

```bash
PUT /api/orders/{orderId}/cancel
Content-Type: application/json

{
  "reason": "Customer requested cancellation"
}
```

### Ship an Order

```bash
PUT /api/orders/{orderId}/ship
Content-Type: application/json

{
  "trackingNumber": "1Z999AA10123456784"
}
```

## ??? Project Structure Details

### Domain Layer (`OrderService.Domain`)

**Pure domain logic with no external dependencies**

- `Common/`: Base classes (Entity, AggregateRoot, ValueObject)
- `Aggregates/`: Order aggregate root
- `Entities/`: OrderItem entity
- `ValueObjects/`: Money, Address, OrderStatus
- `Events/`: Domain event definitions
- `Exceptions/`: Domain-specific exceptions
- `Repositories/`: Repository interfaces

### Application Layer (`OrderService.Application`)

**Use cases and application logic**

- `Commands/`: Create, Confirm, Cancel, Ship order commands and handlers
- `Queries/`: Get order by ID, Get orders by customer
- `DTOs/`: Data transfer objects for API
- `EventHandlers/`: Domain event handlers
- `Behaviors/`: MediatR pipeline behaviors (logging, validation)

### Infrastructure Layer (`OrderService.Infrastructure`)

**Technical implementations**

- `Persistence/`: EF Core DbContext and configurations
- `Repositories/`: Repository and Unit of Work implementations
- **Domain Event Dispatching**: Events dispatched after successful SaveChanges

### API Layer (`OrderService.API`)

**REST API and configuration**

- `Controllers/`: REST endpoints
- `Program.cs`: Dependency injection, middleware, Swagger
- Exception handling middleware
- CORS configuration

## ?? Configuration

### Database Options

**Development (In-Memory)**
The project uses EF Core In-Memory database by default for easy development.

**Production (SQL Server)**
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "OrderServiceDb": "Server=.;Database=OrderServiceDb;Trusted_Connection=True;"
  }
}
```

And modify `Program.cs`:
```csharp
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderServiceDb")));
```

### Logging

Logging is configured in `appsettings.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "OrderService": "Debug"
    }
  }
}
```

## ?? Domain Events

Domain events are automatically dispatched after successful persistence:

1. **OrderPlacedDomainEvent**: When an order is created
2. **OrderConfirmedDomainEvent**: When an order is confirmed
3. **OrderShippedDomainEvent**: When an order is shipped
4. **OrderCancelledDomainEvent**: When an order is cancelled

Event handlers are registered in MediatR and executed asynchronously.

## ?? Business Rules

### Order Creation
- Must have at least one item
- Customer ID and shipping address are required

### Order Confirmation
- Can only confirm orders in "Pending" status

### Order Cancellation
- Cannot cancel orders that are "Shipped" or "Delivered"
- Cancellation reason is required

### Order Shipping
- Can only ship orders in "Confirmed" status
- Tracking number is required

## ?? Testing

### Manual Testing with Swagger

1. Start the application
2. Navigate to Swagger UI
3. Use the interactive interface to test all endpoints

### Testing Workflow

1. **Create an Order** - POST `/api/orders`
2. **Verify Order** - GET `/api/orders/{id}`
3. **Confirm Order** - PUT `/api/orders/{id}/confirm`
4. **Ship Order** - PUT `/api/orders/{id}/ship`

## ?? NuGet Packages

### Domain
- No external dependencies (pure C#)

### Application
- `MediatR` - Command/Query/Event handling

### Infrastructure
- `Microsoft.EntityFrameworkCore` - ORM
- `Microsoft.EntityFrameworkCore.SqlServer` - SQL Server provider
- `Microsoft.EntityFrameworkCore.InMemory` - In-memory provider
- `MediatR` - Event dispatching

### API
- `Swashbuckle.AspNetCore` - Swagger/OpenAPI
- `Microsoft.EntityFrameworkCore.Design` - EF Core tools

## ??? Development Guidelines

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
4. Register handler in `Program.cs`

## ?? Error Handling

The API returns standardized Problem Details for errors:

- **400 Bad Request**: Validation errors, business rule violations
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Unexpected errors

Example error response:
```json
{
  "title": "Domain Validation Error",
  "status": 400,
  "detail": "Cannot confirm an order in Shipped status"
}
```

## ?? Monitoring and Logging

All operations are logged using `ILogger`:
- Command/Query execution
- Domain events raised and handled
- Errors and exceptions

Check console output or configure logging providers (Serilog, Application Insights, etc.)

## ?? Deployment

### Docker (Optional)

Create a `Dockerfile` in the API project:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["OrderService.API/OrderService.API.csproj", "OrderService.API/"]
COPY ["OrderService.Application/OrderService.Application.csproj", "OrderService.Application/"]
COPY ["OrderService.Infrastructure/OrderService.Infrastructure.csproj", "OrderService.Infrastructure/"]
COPY ["OrderService.Domain/OrderService.Domain.csproj", "OrderService.Domain/"]
RUN dotnet restore "OrderService.API/OrderService.API.csproj"
COPY . .
WORKDIR "/src/OrderService.API"
RUN dotnet build "OrderService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderService.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.API.dll"]
```

## ?? Resources

- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## ?? License

This project is provided as a sample implementation for educational purposes.

## ?? Contributing

This is a demonstration project showcasing DDD best practices. Feel free to use it as a template for your own projects.

## ?? Support

For questions or issues, please refer to the inline code documentation and XML comments.

---

**Built with ?? using ASP.NET Core 8.0, Domain-Driven Design, and CQRS**
