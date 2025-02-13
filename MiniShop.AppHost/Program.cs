using System.ComponentModel;
using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// infrastructure

var postgres = builder.AddPostgres("postgres")
    .WithContainerName("minishop.database.aspire")
    .WithDataVolume("minishop-db")
    .WithPgWeb(pgWeb => pgWeb
        .WithHostPort(15432)
        .WithContainerName("minishop.database.webconsole.aspire")
        .WithLifetime(ContainerLifetime.Persistent))
    .WithLifetime(ContainerLifetime.Persistent);

var inventoryDb = postgres.AddDatabase("InventoryDb", "inventory");
var catalogDb = postgres.AddDatabase("CatalogDb", "catalog");
var orderDb = postgres.AddDatabase("OrderDb", "order");

var maildev = builder.AddContainer("maildev", "maildev/maildev")
    .WithContainerName("maildev.aspire")
    .WithEndpoint(1025, 1025, name: "smtp")
    .WithHttpEndpoint(1080, 1080)
    .WithLifetime(ContainerLifetime.Persistent);

var rabbitmq = builder.AddContainer("rabbitmq", "rabbitmq", "3-management")
    .WithContainerName("rabbitmq.aspire")
    .WithEndpoint(5672, 5672, scheme: "amqp")
    .WithHttpEndpoint(15672, 15672)
    //.WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
    //.WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest") 
    .WithLifetime(ContainerLifetime.Persistent);

// apis

var catalog = builder.AddProject<Projects.Catalog>("catalog")
    .WithReference(catalogDb);

var inventory = builder.AddProject<Projects.Inventory>("inventory")
    .WithReference(inventoryDb);

var notification = builder.AddProject<Projects.Notification>("notification")
    .WithReference(maildev.GetEndpoint("smtp"))
    .WithReference(rabbitmq.GetEndpoint("amqp"));

var order = builder.AddProject<Projects.Order>("order")
    .WithReference(inventory)
    .WithReference(orderDb)
    .WithReference(rabbitmq.GetEndpoint("amqp"));

var apiGateway = builder.AddProject<Projects.ApiGateway>("apigateway")
    .WithReference(catalog)
    .WithReference(order)
    .WithExternalHttpEndpoints();

// frontend

builder.AddDockerfile("minishopweb", "../Frontend")
    .WithContainerName("minishop.web.aspire")
    .WithHttpEndpoint(3000, 3000)
    .WithReference(apiGateway)
    .WaitFor(apiGateway)
    .WithExternalHttpEndpoints()
    .WithLifetime(ContainerLifetime.Persistent);

builder.Build().Run();
