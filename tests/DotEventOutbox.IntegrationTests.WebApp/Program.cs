using DotEventOutbox.IntegrationTests.WebApp;
using Microsoft.EntityFrameworkCore;
using DotEventOutbox;

var builder = WebApplication.CreateBuilder(args);

// Register the application's DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"));
});

// Register domain event handlers using MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));


// Register DotEventOutbox services
builder.Services.AddDotEventOutbox(builder.Configuration,
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("AppDb")));

var app = builder.Build();

app.Run();

public partial class Program;


