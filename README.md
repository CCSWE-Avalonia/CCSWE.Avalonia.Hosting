# CCSWE.Avalonia.Hosting

[![Build, test, and publish](https://github.com/CCSWE-Avalonia/CCSWE.Avalonia.Hosting/actions/workflows/dotnet-build-publish-library.yml/badge.svg)](https://github.com/CCSWE-Avalonia/CCSWE.Avalonia.Hosting/actions/workflows/dotnet-build-publish-library.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE.md)

Bootstrap an [Avalonia](https://avaloniaui.net) desktop app on the .NET **Generic Host** — dependency injection,
`IHostedService`/`BackgroundService` lifecycle, configuration, and logging — without the hand-written
Avalonia↔host plumbing. Your `Program.Main` becomes *create a builder, register services, build, run*.

```csharp
[STAThread]
public static void Main(string[] args)
{
    var builder = DesktopApplication.CreateBuilder<App>(args);
    builder.Services.AddSingleton<IMyService, MyService>();
    builder.Services.AddHostedService<MyBackgroundWorker>();
    builder.Build().Run(args);
}

// The Avalonia XAML previewer needs a parameterless AppBuilder-returning method.
public static AppBuilder BuildAvaloniaApp() => DesktopApplication.ConfigureAppBuilder<App>();
```

`Run()` starts the hosted services, runs the Avalonia classic-desktop main loop, and stops the host on exit
(cancelling background services) — the correct lifecycle, by construction. Your `App` receives the built
provider by implementing `IServiceProviderAccessor`:

```csharp
public partial class App : Application, IServiceProviderAccessor
{
    public IServiceProvider? Services { get; private set; }
    IServiceProvider IServiceProviderAccessor.Services { set => Services = value; }

    public override void OnFrameworkInitializationCompleted()
    {
        // Services is set before this runs; resolve view models, register data templates, show the main window…
        base.OnFrameworkInitializationCompleted();
    }
}
```

## Packages

| Package | What it is | Depends on |
| --- | --- | --- |
| **`CCSWE.Avalonia.Hosting`** | The lifetime-agnostic contract (`IServiceProviderAccessor`). | nothing |
| **`CCSWE.Avalonia.Hosting.Desktop`** | `DesktopApplication` / `DesktopApplicationBuilder` / `DesktopApplicationOptions` for the classic desktop lifetime (Windows, Linux, macOS). | `Avalonia.Desktop`, `Microsoft.Extensions.Hosting` |

Both ship their types in the **`CCSWE.Avalonia.Hosting`** namespace (the `.Desktop` suffix is package identity,
not namespace — mirroring `Avalonia.Desktop`), so a consumer needs a single `using CCSWE.Avalonia.Hosting;`.
Future single-view/mobile lifetimes would add sibling packages on the same common contract.

```
dotnet add package CCSWE.Avalonia.Hosting.Desktop
```

## Repository layout

```
src/
  CCSWE.Avalonia.Hosting/                  common contract (IServiceProviderAccessor)
  CCSWE.Avalonia.Hosting.Desktop/          classic-desktop host (DesktopApplication, …)
  CCSWE.Avalonia.Hosting.Sample/           minimal bootstrap app (not published)
  CCSWE.Avalonia.Hosting.Desktop.UnitTests/
  CCSWE.Avalonia.Hosting.slnx              solution + Directory.Build.props + Directory.Packages.props
docs/                                       usage samples
```

## Build & run

```bash
dotnet build src/CCSWE.Avalonia.Hosting.slnx -c Release   # build
dotnet test  src/CCSWE.Avalonia.Hosting.slnx              # tests
dotnet run --project src/CCSWE.Avalonia.Hosting.Sample    # the sample app
dotnet pack  src/CCSWE.Avalonia.Hosting.slnx -c Release -o artifacts   # both NuGet packages
```

The major version tracks the Avalonia major (`12.x` → Avalonia 12.x), via
[Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning).

## License

[MIT](LICENSE.md) © Cory Charlton / CCSWE.
