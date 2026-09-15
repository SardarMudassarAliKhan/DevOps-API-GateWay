using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Add ocelot.json to configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// 2. Register Ocelot
builder.Services.AddOcelot(builder.Configuration);

// 3. Register Swagger for Ocelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);

builder.Services.AddControllers();

var app = builder.Build();

// 4. Configure Swagger UI for Ocelot
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerForOcelotUI(opt =>
    {
        opt.PathToSwaggerGenerator = "/swagger/docs";
    });
}

app.UseRouting();

// 5. Add Ocelot to the pipeline
await app.UseOcelot();

app.Run();