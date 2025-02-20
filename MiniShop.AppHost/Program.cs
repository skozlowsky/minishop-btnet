var builder = DistributedApplication.CreateBuilder(args);

// parameters
var usernameDb = builder.AddParameter("usernameDb", "postgres", secret: true);
var passwordDb = builder.AddParameter("passwordDb", "postgres", secret: true);

// infrastructure

var postgres = builder
    .AddPostgres("postgres", usernameDb, passwordDb)
    .WithContainerName("minishop.database.aspire")
    //.WithDataVolume("minishop-db")
    .WithPgWeb(pgWeb => pgWeb
        .WithHostPort(15432)
        .WithContainerName("minishop.database.webconsole.aspire")
        .WithLifetime(ContainerLifetime.Persistent))
    .WithLifetime(ContainerLifetime.Persistent);

var inventoryDb = postgres.AddDatabase("inventoryDb", "inventory");
var catalogDb = postgres.AddDatabase("catalogDb", "catalog");
var orderDb = postgres.AddDatabase("orderDb", "order");

var maildev = builder.AddContainer("maildev", "maildev/maildev")
    .WithContainerName("maildev.aspire")
    .WithEndpoint(1025, 1025, name: "smtp")
    .WithHttpEndpoint(1080, 1080)
    .WithLifetime(ContainerLifetime.Persistent);

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithContainerName("rabbitmq.aspire")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent);

var redis = builder.AddRedis("redis")
    .WithContainerName("redis.aspire")
    .WithLifetime(ContainerLifetime.Persistent);

// apis

var catalog = builder.AddProject<Projects.Catalog>("catalog")
    .WithReference(catalogDb)
    .WithReference(redis)
    .WaitFor(catalogDb)
    .WaitFor(redis);

var inventory = builder.AddProject<Projects.Inventory>("inventory")
    .WithReference(inventoryDb)
    .WaitFor(inventoryDb);

var notification = builder.AddProject<Projects.Notification>("notification")
    .WithReference(maildev.GetEndpoint("smtp"))
    .WithReference(rabbitmq)
    .WaitFor(maildev)
    .WaitFor(rabbitmq);

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
    //.WithEnvironment("services__apigateway__http__0", "http://localhost:5001")
    //.WithReference(apiGateway)
    .WaitFor(apiGateway)
    .WithExternalHttpEndpoints()
    .WithLifetime(ContainerLifetime.Persistent);

// builder.AddNpmApp("minishopweb", "../Frontend")
//     .WithReference(apiGateway)
//     .WaitFor(apiGateway)
//     .WithEnvironment("BROWSER", "none")
//     .WithHttpEndpoint(env: "VITE_PORT")
//     .PublishAsDockerFile();

builder.Build().Run();