namespace Ninjadog.Templates.CrudWebAPI.Template;

/// <summary>
/// This template generates the wwwroot/index.html landing page for the Web API project.
/// The page displays project information, links to Swagger UI, API documentation,
/// and lists all available entity endpoints.
/// </summary>
public class IndexPageTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "index";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        var config = ninjadogSettings.Config;
        var entities = ninjadogSettings.Entities.FromKeys();
        const string fileName = "index.html";

        var entityListItems = string.Join(
            "\n",
            entities.Select(e =>
            {
                var st = e.StringTokens;
                return $"                    <li><a href=\"{st.ModelEndpoint}\">{st.ModelsHumanized}</a> &mdash; <code>{st.ModelEndpoint}</code></li>";
            }));

        var content =
            $$"""
              <!DOCTYPE html>
              <html lang="en">
              <head>
                  <meta charset="utf-8" />
                  <meta name="viewport" content="width=device-width, initial-scale=1" />
                  <title>{{config.Name}}</title>
                  <style>
                      * { margin: 0; padding: 0; box-sizing: border-box; }
                      body {
                          font-family: system-ui, -apple-system, sans-serif;
                          background: #0f172a;
                          color: #e2e8f0;
                          min-height: 100vh;
                          display: flex;
                          align-items: center;
                          justify-content: center;
                      }
                      .container { max-width: 640px; width: 100%; padding: 2rem; }
                      h1 { font-size: 2rem; font-weight: 700; margin-bottom: 0.25rem; }
                      .version { color: #94a3b8; font-size: 0.875rem; margin-bottom: 1rem; }
                      .description { color: #cbd5e1; margin-bottom: 2rem; line-height: 1.6; }
                      h2 { font-size: 1.125rem; font-weight: 600; margin-bottom: 0.75rem; color: #f8fafc; }
                      .links {
                          display: flex;
                          gap: 0.75rem;
                          margin-bottom: 2rem;
                          flex-wrap: wrap;
                      }
                      .links a {
                          display: inline-flex;
                          align-items: center;
                          gap: 0.5rem;
                          padding: 0.5rem 1rem;
                          border-radius: 0.5rem;
                          background: #1e293b;
                          color: #38bdf8;
                          text-decoration: none;
                          font-size: 0.875rem;
                          border: 1px solid #334155;
                          transition: background 0.15s;
                      }
                      .links a:hover { background: #334155; }
                      .endpoints ul {
                          list-style: none;
                          display: flex;
                          flex-direction: column;
                          gap: 0.375rem;
                      }
                      .endpoints li { font-size: 0.875rem; }
                      .endpoints a { color: #38bdf8; text-decoration: none; }
                      .endpoints a:hover { text-decoration: underline; }
                      .endpoints code {
                          background: #1e293b;
                          padding: 0.125rem 0.375rem;
                          border-radius: 0.25rem;
                          font-size: 0.8125rem;
                          color: #94a3b8;
                      }
                      .footer {
                          margin-top: 2.5rem;
                          padding-top: 1.5rem;
                          border-top: 1px solid #1e293b;
                          font-size: 0.75rem;
                          color: #64748b;
                      }
                      .footer a { color: #64748b; text-decoration: underline; }
                  </style>
              </head>
              <body>
                  <div class="container">
                      <h1>{{config.Name}}</h1>
                      <p class="version">v{{config.Version}}</p>
                      <p class="description">{{config.Description}}</p>

                      <div class="links">
                          <a href="/swagger">Swagger UI</a>
                          <a href="/swagger/v1/swagger.json">OpenAPI Spec</a>
                          <a href="/cs-client">C# Client</a>
                          <a href="/ts-client">TypeScript Client</a>
                      </div>

                      <div class="endpoints">
                          <h2>Endpoints</h2>
                          <ul>
              {{entityListItems}}
                          </ul>
                      </div>

                      <div class="footer">
                          Built with <a href="https://github.com/Atypical-Consulting/Ninjadog" target="_blank" rel="noopener noreferrer">Ninjadog</a>
                      </div>
                  </div>
              </body>
              </html>
              """;

        return CreateNinjadogContentFile(fileName, content, false);
    }
}
