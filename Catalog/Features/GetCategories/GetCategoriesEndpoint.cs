using Catalog.Abstractions;
using MediatR;

namespace Catalog.Features.GetCategories;

public class GetCategoriesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/categories", async (
                IMediator mediator) =>
            {
                var result = await mediator.Send(new GetCategoriesQuery());
                return Results.Ok(result.Value);
            })
            .WithName("GetCategories")
            .WithTags("Categories")
            .Produces<List<CategoryDto>>();
    }
}