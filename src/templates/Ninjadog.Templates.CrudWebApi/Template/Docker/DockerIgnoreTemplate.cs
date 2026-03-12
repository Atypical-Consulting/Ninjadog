namespace Ninjadog.Templates.CrudWebAPI.Template.Docker;

/// <summary>
/// This template generates a .dockerignore file for the Web API project.
/// </summary>
public class DockerIgnoreTemplate : NinjadogTemplate
{
    /// <inheritdoc />
    public override string Name => "DockerIgnore";

    /// <inheritdoc />
    public override NinjadogContentFile GenerateOne(NinjadogSettings ninjadogSettings)
    {
        const string content =
            """
            **/.dockerignore
            **/.git
            **/.gitignore
            **/.vs
            **/.vscode
            **/bin
            **/obj
            **/.rider
            **/node_modules
            **/*.user
            **/*.suo
            """;

        return CreateNinjadogContentFile(".dockerignore", content, false);
    }
}
