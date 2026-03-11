// Copyright (c) 2020-2024 Atypical Consulting SRL. All rights reserved.
// Atypical Consulting SRL licenses this file to you under the Proprietary license.
// See the LICENSE file in the project root for full license information.

using Microsoft.Extensions.DependencyInjection;
using Ninjadog.CLI.Commands;
using Ninjadog.CLI.Utilities;
using Ninjadog.Engine;
using Ninjadog.Engine.Core.DomainEvents;
using Ninjadog.Engine.Infrastructure;
using Ninjadog.Templates.CrudWebAPI.Setup;

SpectreWriteHelpers.WriteNinjadog();

var registrations = new ServiceCollection();
registrations.AddDomainEventDispatcher();
registrations.AddInfrastructure();
registrations.AddSingleton<INinjadogEngineFactory, NinjadogEngineFactory>();
registrations.AddSingleton<NinjadogTemplateManifest, CrudTemplateManifest>();

const string settingsFileName = "ninjadog.json";
var settingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), settingsFileName);

if (Ninjadog.CLI.CliInvocation.RequiresProjectSettings(args))
{
    if (File.Exists(settingsFilePath))
    {
        var json = File.ReadAllText(settingsFilePath);
        var settings = NinjadogSettings.FromJsonString(json);
        registrations.AddSingleton(settings);
    }
    else
    {
        AnsiConsole.MarkupLine($"[yellow]Warning:[/] No {settingsFileName} found in the current directory. Run [green]ninjadog init[/] first.");
        AnsiConsole.MarkupLine("[yellow]Using default settings.[/]");
        registrations.AddSingleton<NinjadogSettings>(new Ninjadog.Settings.Extensions.NinjadogInitialSettings());
    }
}

var registrar = new Ninjadog.CLI.Infrastructure.TypeRegistrar(registrations);

var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.PropagateExceptions();

#if DEBUG
    config.ValidateExamples();
#endif

    config.AddCommand<InitCommand>("init")
        .WithDescription("Initializes a new Ninjadog project.")
        .WithExample(["init"]);

    config.AddCommand<BuildCommand>("build")
        .WithDescription("Builds and compiles the project.")
        .WithExample(["build"]);

    config.AddCommand<AddEntityCommand>("add-entity")
        .WithDescription("Adds a new entity to the ninjadog.json file.")
        .WithExample(["add-entity", "Product"]);

    config.AddCommand<ValidateCommand>("validate")
        .WithDescription("Validates a ninjadog.json configuration file.")
        .WithExample(["validate"])
        .WithExample(["validate", "--file", "path/to/ninjadog.json"])
        .WithExample(["validate", "--strict"]);

    config.AddCommand<UiCommand>("ui")
        .WithDescription("Opens a web-based configuration builder.")
        .WithExample(["ui"]);
});

try
{
    return await app.RunAsync(args);
}
catch (Exception ex)
{
    WriteException(ex, ExceptionFormats.ShortenEverything);
    return -99;
}
