using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Commands;
using OrderService.Application.DTOs;
using OrderService.Application.Queries;
using OrderService.Domain.Exceptions;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        try
        {
            var command = new CreateOrderCommand
            {
                CustomerId = dto.CustomerId,
                ShippingAddress = dto.ShippingAddress,
                Items = dto.Items
            };

            var result = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetOrderById), new { id = result.Id }, result);
        }
        catch (OrderDomainException ex)
        {
            _logger.LogWarning(ex, "Domain validation error creating order");
            return BadRequest(new ProblemDetails
            {
                Title = "Domain Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while creating the order",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        try
        {
            var query = new GetOrderByIdQuery { OrderId = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Order Not Found",
                    Detail = $"Order with ID {id} was not found",
                    Status = StatusCodes.Status404NotFound
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving the order",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    [HttpGet("customer/{customerId}")]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrdersByCustomer(Guid customerId)
    {
        try
        {
            var query = new GetOrdersByCustomerQuery { CustomerId = customerId };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving orders for customer {CustomerId}", customerId);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving customer orders",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    [HttpGet("{id}/events")]
    [ProducesResponseType(typeof(List<OutboxMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOrderEvents(Guid id)
    {
        try
        {
            var query = new GetOrderEventsQuery { OrderId = id };
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving events for order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving order events",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}

public record CancelOrderRequest
{
    public string Reason { get; init; } = string.Empty;
}

public record ShipOrderRequest
{
    public string TrackingNumber { get; init; } = string.Empty;
}
