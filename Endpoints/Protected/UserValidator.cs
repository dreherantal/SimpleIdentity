using System;

namespace SimpleIdentity.Endpoints.Protected;

public class UserValidator
{
    public static void Handler(HttpContext context)
    {
        int? UserId = (int?)context.Items["UserId"];


            System.Console.WriteLine("Validated UserId: " + UserId);

            TypedResults.Ok(UserId);
    }
}
