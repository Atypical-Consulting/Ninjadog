namespace Ninjadog.Templates.CrudWebAPI.Template.Middleware;

/// <summary>
/// This template generates the RequestCorrelationMiddleware.cs file that adds
/// correlation IDs to every request for structured logging and distributed tracing.
/// </summary>
public class RequestCorrelationMiddlewareTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "RequestCorrelationMiddleware";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        const string fileName = "RequestCorrelationMiddleware.cs";

        var content =
            $$"""

              using Serilog.Context;

              {{WriteFileScopedNamespace($"{rootNamespace}.Middleware")}}

              /// <summary>
              /// Middleware that assigns a unique correlation ID to each HTTP request.
              /// The correlation ID is pushed into the Serilog log context so every log entry
              /// written during the request includes it automatically.
              /// It is also returned in the X-Correlation-Id response header for client-side tracing.
              /// </summary>
              public class RequestCorrelationMiddleware
              {
                  private const string CorrelationIdHeader = "X-Correlation-Id";
                  private readonly RequestDelegate _next;

                  public RequestCorrelationMiddleware(RequestDelegate next)
                  {
                      _next = next;
                  }

                  public async Task InvokeAsync(HttpContext context)
                  {
                      var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                          ?? Guid.NewGuid().ToString("D");

                      context.Response.OnStarting(() =>
                      {
                          context.Response.Headers[CorrelationIdHeader] = correlationId;
                          return Task.CompletedTask;
                      });

                      using (LogContext.PushProperty("CorrelationId", correlationId))
                      {
                          await _next(context);
                      }
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
