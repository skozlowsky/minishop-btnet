var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .WithOrigins("*")
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseCors("AllowAll");

#if !DEBUG
app.UseHttpsRedirection(); // causes issues with CORS if the frontend doesn't use HTTPS
#endif

app.MapReverseProxy();

app.Run();
