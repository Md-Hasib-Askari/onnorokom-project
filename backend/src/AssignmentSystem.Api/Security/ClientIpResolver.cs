using System.Net;

namespace AssignmentSystem.Api.Security;

public static class ClientIpResolver
{
    public static string Resolve(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();
        var candidate = forwardedFor.Split(',')[0].Trim();
        if (IPAddress.TryParse(candidate, out _))
        {
            return candidate;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
