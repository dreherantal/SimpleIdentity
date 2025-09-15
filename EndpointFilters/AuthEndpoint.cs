using SimpleIdentity.Encryption;

namespace SimpleIdentity.EndpointFilters;

public class AuthEndpoint : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next

    )
    {

        var endpointMetadataCollection = context.HttpContext.GetEndpoint()?.Metadata;
        bool? IsLoginAnonymous = endpointMetadataCollection?.GetMetadata<EndpointRequiresAuth>()?.IsAnonymous;
        Console.WriteLine(IsLoginAnonymous);

        if (IsLoginAnonymous is true && IsLoginAnonymous is not null)
        {
            return await next(context);

        }


        Console.WriteLine("auth endpoint executing (not anonymous login)");

        string? auth = context.HttpContext.Request.Headers.Authorization;

        if (auth != null)
        {
            string bearer = auth.Replace("Bearer ", "");

            JWTValidationResult ValidationResult = TokenHasher.ValidateJWT(bearer);

            if (ValidationResult.IsValid == true)
            {

                context.HttpContext.Items["UserId"] = ValidationResult.UserID;


                return await next(context);

            }
            else
            {
                return Results.Problem(detail: ValidationResult.ErrorMessage, title: "JWT Validation error.", statusCode: 403);
            }
        }
        return Results.Problem(detail: "Authorization header missing.", title: "Authorization error", statusCode: 403);


    }
}
