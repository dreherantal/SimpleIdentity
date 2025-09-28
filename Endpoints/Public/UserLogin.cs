using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SimpleIdentity.DTOs;
using SimpleIdentity.Encryption;
using SimpleIdentity.Models;

namespace SimpleIdentity.Endpoints.Public;

public class UserLogin
{
    public static async Task<Results<UnauthorizedHttpResult, JsonHttpResult<Object>>> Handler(UsersDbContext db, LoginDTO loginDTO)
    {
        var result = await db.Users.FirstOrDefaultAsync(x => x.Email == loginDTO.Email.ToLower());

        if ((result is not null) && SecretHasher.Verify(loginDTO.Password, result.PasswordHash))
        {
            object _bearer = new { bearer = $"{TokenHasher.CreateJWT(result.Id)}" };
            return TypedResults.Json(_bearer);

        }
        else
        {
            return TypedResults.Unauthorized();
        }
    }
}
