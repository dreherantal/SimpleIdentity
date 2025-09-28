using System;
using Microsoft.AspNetCore.Http.HttpResults;

namespace SimpleIdentity.Endpoints.Protected;

public static class IdentityProtectedEndpoints
{
    public static void RegisterIdentityProtectedEndpoints(this WebApplication app)
    {
        var protectedUsersGroup = app.MapGroup("/users");

        protectedUsersGroup.MapGet("/validate", UserValidator.Handler);


    }



}
