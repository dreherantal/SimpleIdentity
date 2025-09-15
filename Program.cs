using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SimpleIdentity.DTOs;
using SimpleIdentity.Encryption;
using SimpleIdentity.Middlewares;
using SimpleIdentity.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});


builder.Services.AddDbContext<UsersDbContext>(options =>
      options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
app.UseEndpoints(e => {});


app.MapGet("/validate", (HttpContext context) =>
{

    int? UserId = (int?)context.Items["UserId"];

    System.Console.WriteLine("Validated UserId: " + UserId);
    return Results.Ok();

})
//.AddEndpointFilter<AuthEndpoint>()
.WithOpenApi();


app.MapPost("/register", async (UsersDbContext db, RegisterDTO registerDTO) =>
{
    var user = DTOTools.RegisterDTOtoUser(registerDTO);
    db.Users.Add(user);

    try
    {
        await db.SaveChangesAsync();
    }
    catch (DbUpdateException x) when (x.InnerException is SqlException inner && (inner.Number == 2627 || inner.Number == 2601))
    {

        var result = await db.Users.FirstOrDefaultAsync(x => x.Email == user.Email);

        if (result != null)
        {
            return Results.Conflict($"Email '{user.Email}' already registered. Please use another one to register.");
        }

    }
    catch
    {
        throw;

    }
    return Results.Created();

}
)
.WithMetadata(new EndpointRequiresAuth { IsAnonymous = true });

app.MapPost("/login", async (UsersDbContext db, LoginDTO loginDTO) =>
{
    var result = await db.Users.FirstOrDefaultAsync(x => x.Email == loginDTO.Email.ToLower());

    if (result == null)
    {
        return Results.NotFound($"Email not registered.");
    }

    if (SecretHasher.Verify(loginDTO.Password, result.PasswordHash))
    {

        return Results.Json(new { bearer = $"{TokenHasher.CreateJWT(result.Id)}" });

    }
    else
    {
        return Results.Unauthorized();
    }

}
)
.WithMetadata(new EndpointRequiresAuth { IsAnonymous = true });

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