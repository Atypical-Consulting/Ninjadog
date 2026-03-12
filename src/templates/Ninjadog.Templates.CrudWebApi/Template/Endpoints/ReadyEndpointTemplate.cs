namespace Ninjadog.Templates.CrudWebAPI.Template.Endpoints;

/// <summary>
/// This template generates the Ready endpoint (/ready) for readiness checks.
/// Verifies that the database connection is available.
/// </summary>
public sealed class ReadyEndpointTemplate
    : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "ReadyEndpoint";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var rootNamespace = ninjadogSettings.Config.RootNamespace;
        var ns = $"{rootNamespace}.Endpoints";
        const string fileName = "ReadyEndpoint.cs";

        var content =
            $$"""

              using {{rootNamespace}}.Database;
              using FastEndpoints;
              using Microsoft.AspNetCore.Http;

              {{WriteFileScopedNamespace(ns)}}

              public sealed class ReadyEndpoint(IDbConnectionFactory connectionFactory)
                  : EndpointWithoutRequest
              {
                  public override void Configure()
                  {
                      Get("/ready");
                      AllowAnonymous();
                  }

                  public override async Task HandleAsync(CancellationToken ct)
                  {
                      try
                      {
                          using var connection = await connectionFactory.CreateConnectionAsync();
                          await SendOkAsync(new { Status = "Ready" }, ct);
                      }
                      catch (Exception) when (ct.IsCancellationRequested is false)
                      {
                          ThrowError("Database is not available", StatusCodes.Status503ServiceUnavailable);
                      }
                  }
              }
              """;

        return CreateNinjadogContentFile(fileName, content);
    }
}
