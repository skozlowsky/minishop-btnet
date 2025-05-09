using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient("inventoryClient",
    client => client.BaseAddress = new("http://where_should_i_do_request?"));

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Shop management API");
});

app.UseHttpsRedirection();

app.MapGet("/api/stock", async (IHttpClientFactory clientFactory, CancellationToken cancellationToken) =>
    {
        var inventoryClient = clientFactory.CreateClient("inventoryClient");

        return await inventoryClient.GetFromJsonAsync<dynamic>("/api/inventory?page=1&pageSize=1000000", cancellationToken);
    })
    .WithName("GetStock");

app.Run();