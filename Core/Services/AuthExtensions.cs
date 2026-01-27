using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.ResponseCompression;

namespace Core.Services;

public class RequireMfa : IAuthorizationRequirement { }
public class RequireMfaHandler : AuthorizationHandler<RequireMfa>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RequireMfa requirement)
    {
        ArgumentNullException.ThrowIfNull(context);

        ArgumentNullException.ThrowIfNull(requirement);

        var amrClaim =
            context.User.Claims.FirstOrDefault(t => t.Type == "amr");

        if (amrClaim != null && amrClaim.Value == "mfa")
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

}
public class CustomCompressionProvider : ICompressionProvider
{
    public string EncodingName => "webCompressor";
    public bool SupportsFlush => true;

    public Stream CreateStream(Stream outputStream)
    {
        // Create a custom compression stream wrapper here
        return outputStream;
    }
}
