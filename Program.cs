using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SimpleIdentity.DTOs;
using SimpleIdentity.Encryption;
using SimpleIdentity.Middlewares;
using SimpleIdentity.Models;
using Serilog;
using SimpleIdentity.Endpoints.Public;
using SimpleIdentity.Endpoints.Protected;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});


builder.Services.AddDbContext<UsersDbContext>(options =>
      options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlServerOptionsAction: sqlOptions =>
      {
          sqlOptions.EnableRetryOnFailure(
          maxRetryCount: 3);
      }));

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.UseRouting();
app.UseMiddleware<AuthMiddleware>();
app.UseEndpoints(e => { });

app.RegisterIdentityPublicEndpoints();
app.RegisterIdentityProtectedEndpoints();

app.Run();

public class JWTValidationResult
{
    public bool IsValid { get; set; } = false;
    public int UserID { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

}

public class EndpointRequiresAuth
{
    public required bool IsAnonymous { get; set; }
}