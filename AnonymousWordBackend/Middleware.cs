namespace AnonymousWordBackend;

public class Middleware(
    ILogger<Middleware> logger,
    RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Prevent app indexing by search engines.
        // @see https://developers.google.com/search/docs/crawling-indexing/block-indexing
        context.Response.Headers.Append("X-Robots-Tag", "noindex");
        
        await next(context);
        
        logger.LogInformation(
            "{} {}: {}",
            context.Response.StatusCode,
            context.Request.Method,
            context.Request.Path);
    }
}