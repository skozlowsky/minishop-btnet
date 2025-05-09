# Mini Shop

## Zadanie 1

- utwórz projekt AppHost
- dodaj referencje projektów (ApiGateway, Catalog, Inventory, Order) do AppHost oraz dodaj projekty za pomocą:
  - ```csharp
    var service = builder.AddProject<Projects.YourService>("projectResourceName");
    ```
- utwórz instancje postgresa używając dodając nuget `<PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.2.1" />` do apphost, oraz zdefiniuj go używając buildera w projekcie AppHost
  - ```csharp
    var postgresResource = builder.AddPostgres("postgresResourceName");
    ```
  - do zdefiniowanego postgresa dodaj
    ```csharp
    .WithPgWeb(pgWeb => pgWeb
        .WithHostPort(15432)
        .WithLifetime(ContainerLifetime.Persistent))
    ```
  - możesz też użyć `.WithLifetime(ContainerLifetime.Persistent))` aby kontener sie nie restetowal się miedzy kolejnymi sesjami debuga
- stwórz trzy bazy `inventoryDb`, `catalogDb`, `orderDb` o dokladnie takich nazwach
  - ```csharp
    var dbResource = yourPostgresDatabase.AddDatabase("dbResourceName");
    ```
- dodaj instancje Rabbita za pomocą odpowiedniego pakietu
  - poszukaj odpowiedniej integracji hostingowej **Azure.Hosting.***
- za pomocą `.WithReference(<nazwa_resourcu_bazy>)` dodaj referencje:
  - inventoryDb -> inventoryService
  - catalogDb -> catalogService
  - orderDb -> orderService
  - rabbitmq -> orderService
  - inventoryService -> orderService (order manipuluje stanem magazynu)
  - catalogService -> apiGateway
  - orderService -> apiGateway
- do projektu `apiGateway` dodaj `.WithExternalHttpEndpoints()`
- ostatecznie dodaj frontend :)
```csharp
builder.AddDockerfile("minishopweb", "../Frontend")
    .WithHttpEndpoint(3000, 3000)
    .WaitFor(apiGateway)
    .WithExternalHttpEndpoints()
    .WithLifetime(ContainerLifetime.Persistent);
```

<details>
<summary>Sciąga:</summary>

Postgres:
```csharp
var postgres = builder
    .AddPostgres("postgres")
    .WithPgWeb(pgWeb => pgWeb
        .WithHostPort(15432)
        .WithLifetime(ContainerLifetime.Persistent))
    .WithLifetime(ContainerLifetime.Persistent);
```

Bazy:
```csharp
var inventoryDb = postgres.AddDatabase("inventoryDb");
var catalogDb = postgres.AddDatabase("catalogDb");
var orderDb = postgres.AddDatabase("orderDb");
```

RabbitMq:
```xml
<PackageReference Include="Aspire.Hosting.RabbitMQ" Version="9.2.1" />
```

```csharp
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithContainerName("rabbitmq.aspire")
    .WithLifetime(ContainerLifetime.Persistent);
```

Serwisy:
```csharp
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
```

ApiGateway:
```csharp
var apiGateway = builder.AddProject<Projects.ApiGateway>("apigateway")
    .WithReference(catalog)
    .WithReference(order)
    .WithExternalHttpEndpoints()
    .WaitFor(catalog)
    .WaitFor(order);

```

</details>

## Test data

CatalogDB:
```sql
INSERT INTO "Categories" ("Id", "Name", "Description") VALUES
    ('f47ac10b-58cc-4372-a567-0e02b2c3d479', 'Electronics', 'Electronic devices and accessories'),
    ('f47ac10b-58cc-4372-a567-0e02b2c3d480', 'Books', 'Physical and digital books'),
    ('f47ac10b-58cc-4372-a567-0e02b2c3d481', 'Home & Garden', 'Home improvement and garden supplies'),
    ('f47ac10b-58cc-4372-a567-0e02b2c3d482', 'Sports', 'Sports equipment and accessories');

-- Products
INSERT INTO "Products" ("Id", "Name", "Description", "Price", "IsActive", "CategoryId", "Tags") VALUES
    ('a47ac10b-58cc-4372-a567-0e02b2c3d479', 'Smartphone X', 'Latest generation smartphone with advanced features', 999.99, true, 'f47ac10b-58cc-4372-a567-0e02b2c3d479', ARRAY['electronics', 'mobile', 'smartphone']),
    ('a47ac10b-58cc-4372-a567-0e02b2c3d480', 'Laptop Pro', 'Professional laptop for demanding users', 1499.99, true, 'f47ac10b-58cc-4372-a567-0e02b2c3d479', ARRAY['electronics', 'computer', 'laptop']),
    ('a47ac10b-58cc-4372-a567-0e02b2c3d481', 'Python Programming', 'Comprehensive guide to Python programming', 49.99, true, 'f47ac10b-58cc-4372-a567-0e02b2c3d480', ARRAY['programming', 'education', 'software']),
    ('a47ac10b-58cc-4372-a567-0e02b2c3d482', 'Garden Tools Set', 'Complete set of essential garden tools', 129.99, true, 'f47ac10b-58cc-4372-a567-0e02b2c3d481', ARRAY['garden', 'tools', 'outdoor']),
    ('a47ac10b-58cc-4372-a567-0e02b2c3d483', 'Tennis Racket Pro', 'Professional grade tennis racket', 199.99, true, 'f47ac10b-58cc-4372-a567-0e02b2c3d482', ARRAY['sports', 'tennis', 'equipment']),
    ('a47ac10b-58cc-4372-a567-0e02b2c3d484', 'Tablet Y', 'Compact tablet for entertainment', 299.99, false, 'f47ac10b-58cc-4372-a567-0e02b2c3d479', ARRAY['electronics', 'tablet', 'mobile']);
```
InventoryDb:
```sql
-- Items
INSERT INTO "Items" ("Id", "Sku", "Name", "AvailableQuantity", "ReservedQuantity", "Price", "CreatedAt", "UpdatedAt") VALUES
    ('a47ac10b-58cc-4372-a567-0e02b2c3d479', 'PHONE-001', 'Smartphone X - 128GB Black', 50, 5, 999.99, '2024-02-18 10:00:00+00', '2024-02-18 10:00:00+00'),
    ('a47ac10b-58cc-4372-a567-0e02b2c3d480', 'LAPTOP-001', 'Laptop Pro - 16GB/512GB', 20, 2, 1499.99, '2024-02-18 10:00:00+00', '2024-02-18 10:00:00+00'),
    ('a47ac10b-58cc-4372-a567-0e02b2c3d481', 'BOOK-001', 'Python Programming - Hardcover', 100, 10, 49.99, '2024-02-18 10:00:00+00', '2024-02-18 10:00:00+00'),
    ('a47ac10b-58cc-4372-a567-0e02b2c3d482', 'GARDEN-001', 'Garden Tools Set - Basic', 30, 0, 129.99, '2024-02-18 10:00:00+00', '2024-02-18 10:00:00+00'),
    ('a47ac10b-58cc-4372-a567-0e02b2c3d483', 'TENNIS-001', 'Tennis Racket Pro - Adult', 25, 2, 199.99, '2024-02-18 10:00:00+00', '2024-02-18 10:00:00+00'),
    ('a47ac10b-58cc-4372-a567-0e02b2c3d484', 'TABLET-001', 'Tablet Y - 64GB', 0, 0, 299.99, '2024-02-18 10:00:00+00', '2024-02-18 10:00:00+00');

```