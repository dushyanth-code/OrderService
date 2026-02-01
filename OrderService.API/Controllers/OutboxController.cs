using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.DTOs;
using OrderService.Application.Queries;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OutboxController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OutboxController> _logger;

    public OutboxController(IMediator mediator, ILogger<OutboxController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<OutboxMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetOutboxMessages(
        [FromQuery] bool? isProcessed = null,
        [FromQuery] int? maxResults = 100)
    {
        try
        {
            var query = new GetAllOutboxMessagesQuery 
            { 
                IsProcessed = isProcessed,
                MaxResults = maxResults
            };
            
            var result = await _mediator.Send(query);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving outbox messages");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while retrieving outbox messages",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}
