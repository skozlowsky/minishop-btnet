namespace Order.Middleware;

public static class Extensions
{
    public static IApplicationBuilder UseUserEmailAuthentication(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserEmailAuthenticationMiddleware>();
    }
}