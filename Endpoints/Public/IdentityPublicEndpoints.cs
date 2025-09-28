using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SimpleIdentity.DTOs;
using SimpleIdentity.Encryption;
using SimpleIdentity.Models;

namespace SimpleIdentity.Endpoints.Public;

public static class IdentityPublicEndpoints
{
    public static void RegisterIdentityPublicEndpoints(this WebApplication app)
    {
        var publicUsersGroup = app.MapGroup("/users").WithMetadata(new EndpointRequiresAuth { IsAnonymous = true });

        // Endpoints added here will be public accessible!

        publicUsersGroup.MapPost("/register", UserRegister.Handler);

        publicUsersGroup.MapPost("/login", UserLogin.Handler);


    }




}
