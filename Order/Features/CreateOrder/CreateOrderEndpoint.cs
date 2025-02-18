using MediatR;
using Order.Abstractions;
using Order.Metrics;

namespace Order.Features.CreateOrder;

public class CreateOrderEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (
                CreateOrderCommand command,
                IMediator mediator,
                OrderMetrics metrics) =>
            {
                var result = await mediator.Send(command);

                if (result.IsSuccess)
                {
                    var items = command
                        .Items
                        .Select(orderItem => 
                            (productId: $"{orderItem.ProductId:D}", quantity: orderItem.Quantity))
                        .ToArray();
                    metrics.ProductsSold(items);
                }

                return result.IsSuccess 
                    ? Results.Ok(new { OrderId = result.Value }) 
                    : Results.BadRequest(result.Error);
            })
            .WithName("CreateOrder")
            .WithTags("Orders")
            .Produces<Guid>()
            .ProducesValidationProblem()
            .WithOpenApi();
    }
}