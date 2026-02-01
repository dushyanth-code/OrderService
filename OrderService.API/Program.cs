using System.Reflection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrderService.Application.Behaviors;
using OrderService.Application.EventHandlers;
using OrderService.Domain.Events;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.BackgroundServices;
using OrderService.Infrastructure.Persistence;
using OrderService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<OrderDbContext>(options =>
{
    //options.UseSqlServer(builder.Configuration.GetConnectionString("OrderServiceDb"));  
   
    options.UseInMemoryDatabase("OrderServiceDb");
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(OrderService.Application.Commands.CreateOrderCommand).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(OutboxProcessorService).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);

    //Register pipeline behaviors -Dushyanth
    //cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    //cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddScoped<INotificationHandler<OrderPlacedDomainEvent>, OrderPlacedEventHandler>();
builder.Services.AddScoped<INotificationHandler<OrderConfirmedDomainEvent>, OrderConfirmedEventHandler>();
builder.Services.AddScoped<INotificationHandler<OrderCancelledDomainEvent>, OrderCancelledEventHandler>();
builder.Services.AddScoped<INotificationHandler<OrderShippedDomainEvent>, OrderShippedEventHandler>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IOutboxEventPublisher, OutboxEventPublisher>();
builder.Services.AddHostedService<OutboxProcessorService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Order Service API",
        Version = "v1",
        Description = "A DDD based Project (Dushyanth)",
        Contact = new OpenApiContact
        {
            Name = "Order Service Team",
            Email = "orderservice@example.com"
        }
    });

    // Enable XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Order Service API v1");
        options.RoutePrefix = "swagger";
    });
}

// Global exception handling middleware
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        
        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError(error.Error, "Unhandled exception occurred");
            
            await context.Response.WriteAsJsonAsync(new
            {
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = app.Environment.IsDevelopment() ? error.Error.Message : "An unexpected error occurred"
            });
        }
    });
});

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    
    // For SQL Server: Apply migrations
    //await dbContext.Database.MigrateAsync();
    
    // For In-Memory Database
     await dbContext.Database.EnsureDeletedAsync();
     await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
