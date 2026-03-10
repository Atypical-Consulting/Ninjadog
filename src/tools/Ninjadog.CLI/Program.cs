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
using Ninjadog.Templates.CrudWebAPI.UseCases.TodoApp;

SpectreWriteHelpers.WriteNinjadog();

var registrations = new ServiceCollection();
registrations.AddDomainEventDispatcher();
registrations.AddInfrastructure();
registrations.AddSingleton<INinjadogEngineFactory, NinjadogEngineFactory>();
registrations.AddSingleton<NinjadogTemplateManifest, CrudTemplateManifest>();
registrations.AddSingleton<NinjadogSettings, TodoAppSettings>();
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

    config.AddCommand<NinjadogCommand>("ninjadog")
        .WithDescription("Generates a new Ninjadog project.")
        .WithExample(["ninjadog"]);
});

try
{
    return app.Run(args);
}
catch (Exception ex)
{
    WriteException(ex, ExceptionFormats.ShortenEverything);
    return -99;
}
