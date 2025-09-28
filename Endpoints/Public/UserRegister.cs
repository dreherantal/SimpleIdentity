using System;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SimpleIdentity.DTOs;
using SimpleIdentity.Models;

namespace SimpleIdentity.Endpoints.Public;

public class UserRegister
{
    public static async Task<Results<Created, Conflict<string>>> Handler(UsersDbContext db, RegisterDTO registerDTO)
    {
        var user = DTOTools.RegisterDTOtoUser(registerDTO);
        await db.Users.AddAsync(user);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException x) when (x.InnerException is SqlException inner && (inner.Number == 2627 || inner.Number == 2601))
        {

            var result = await db.Users.FirstOrDefaultAsync(x => x.Email == user.Email);

            if (result != null)
            {
                return TypedResults.Conflict($"Email '{user.Email}' already registered. Please use another one to register.");
            }

        }

        return TypedResults.Created();

    }
}
