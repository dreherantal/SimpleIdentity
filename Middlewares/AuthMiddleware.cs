using SimpleIdentity.Encryption;

namespace SimpleIdentity.Middlewares;

public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthMiddleware> _logger;

    public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {


            _logger.LogInformation("Incoming request: {Method} {Path}", context.Request.Method, context.Request.Path);

            var endpointMetadataCollection = context.GetEndpoint()?.Metadata;
            bool? IsLoginAnonymous = endpointMetadataCollection?.GetMetadata<EndpointRequiresAuth>()?.IsAnonymous;

            if (IsLoginAnonymous is true && IsLoginAnonymous is not null)
            {
                _logger.LogInformation("Endpoint is public");
                await _next(context);
            }
            else
            {
                _logger.LogInformation("Endpoint is protected");

                string? headerAuth = context.Request.Headers.Authorization;

                if (headerAuth != null)
                {
                    string bearer = headerAuth.Replace("Bearer ", "");

                    JWTValidationResult ValidationResult = TokenHasher.ValidateJWT(bearer);

                    if (ValidationResult.IsValid)
                    {
                        context.Items["UserId"] = ValidationResult.UserID;

                        await _next(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync("Unauthorized");

                        _logger.LogWarning("An error occured while validating JWT: " + ValidationResult.ErrorMessage);
                    }
                }
                else
                {
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Unauthorized");
                    _logger.LogWarning("Authorization header is missing.");
                }

            }

        


    }
}
