using Catalog.Abstractions;
using MediatR;

namespace Catalog.Features.GetProductsByCategory;

public class GetProductsByCategoryEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories/{categoryId}/products", async (
                Guid categoryId,
                [AsParameters] PaginationParams pagination,
                IMediator mediator) =>
            {
                var result = await mediator.Send(
                    new GetProductsByCategoryQuery(categoryId, pagination.Page, pagination.PageSize));
                return Results.Ok(result.Value);
            })
            .WithName("GetProductsByCategory")
            .WithTags("Products")
            .Produces<PagedResult<ProductDto>>();
    }
}