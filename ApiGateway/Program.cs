var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddHttpForwarderWithServiceDiscovery();

builder.Services.AddCors();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseCors(c => c
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());


app.UseRouting();
app.MapReverseProxy();

app.Run();