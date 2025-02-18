using Catalog.Abstractions;
using Catalog.Metrics;
using MediatR;

namespace Catalog.Features.GetProduct;

public class GetProductEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id}", async (
                Guid id,
                IMediator mediator,
                ProductMetrics metrics) =>
            {
                var result = await mediator.Send(new GetProductQuery(id));
                metrics.ProductDisplayed($"{id:D}");
                return result.IsSuccess ? Results.Ok(result.Value) : Results.NotFound();
            })
            .WithName("GetProduct")
            .WithTags("Products")
            .Produces<ProductDetailsDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}