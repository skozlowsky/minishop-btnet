using Catalog.Abstractions;
using MediatR;

namespace Catalog.Features.GetProducts;

public class GetProductsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async (
                [AsParameters] GetProductsQuery query,
                IMediator mediator) =>
            {
                var result = await mediator.Send(query);
                return Results.Ok(result.Value);
            })
            .WithName("GetProducts")
            .WithTags("Products")
            .Produces<PagedResult<ProductDto>>();
    }
}