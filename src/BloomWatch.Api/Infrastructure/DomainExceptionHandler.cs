using BloomWatch.SharedKernel;
using Microsoft.AspNetCore.Diagnostics;

namespace BloomWatch.Api.Infrastructure;

/// <summary>
/// Catches unhandled <see cref="DomainException"/> instances that escape endpoint try-catch
/// blocks and maps them to appropriate HTTP problem responses.
/// <para>
/// This handler acts as a safety net so that any domain exception that is <em>not</em>
/// explicitly caught in an endpoint still produces a structured JSON error body
/// instead of a 500 Internal Server Error.
/// </para>
/// </summary>
internal sealed class DomainExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainException)
            return false;

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(
            new { error = domainException.Message },
            cancellationToken);

        return true;
    }
}
