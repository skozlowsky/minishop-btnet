var builder = DistributedApplication.CreateBuilder(args);

// infrastructure
var postgres = builder
    .AddPostgres("postgres")
    .WithPgWeb(pgWeb => pgWeb
        .WithHostPort(15432)
        .WithLifetime(ContainerLifetime.Persistent))
    .WithLifetime(ContainerLifetime.Persistent);

var inventoryDb = postgres.AddDatabase("inventoryDb");
var catalogDb = postgres.AddDatabase("catalogDb");
var orderDb = postgres.AddDatabase("orderDb");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithContainerName("rabbitmq.aspire")
    .WithLifetime(ContainerLifetime.Persistent);

// apis

var catalog = builder.AddProject<Projects.Catalog>("catalog")
    .WithReference(catalogDb)
    .WaitFor(catalogDb);

var inventory = builder.AddProject<Projects.Inventory>("inventory")
    .WithReference(inventoryDb)
    .WaitFor(inventoryDb);

var order = builder.AddProject<Projects.Order>("order")
    .WithReference(inventory)
    .WithReference(orderDb)
    .WithReference(rabbitmq)
    .WaitFor(inventory)
    .WaitFor(orderDb)
    .WaitFor(rabbitmq);

var apiGateway = builder.AddProject<Projects.ApiGateway>("apigateway")
    .WithReference(catalog)
    .WithReference(order)
    .WithExternalHttpEndpoints()
    .WaitFor(catalog)
    .WaitFor(order);

// frontend

builder.AddDockerfile("minishopweb", "../Frontend")
    .WithContainerName("minishop.web.aspire")
    .WithHttpEndpoint(3000, 3000)
    .WaitFor(apiGateway)
    .WithExternalHttpEndpoints()
    .WithLifetime(ContainerLifetime.Persistent);

builder.Build().Run();