using UnitConversion.Api.Extensions;
using UnitConversion.Api.Middleware;
using UnitConversion.Application;
using UnitConversion.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddSwaggerDocumentation();

var app = builder.Build();

app.UseExceptionHandler();

// Swagger stays on in every environment so the API is easy to try out locally.
app.UseSwaggerDocumentation();

app.MapControllers();

app.Run();

// Lets the integration tests start the app through WebApplicationFactory.
public partial class Program;
