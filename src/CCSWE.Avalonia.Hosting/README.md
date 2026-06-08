# CCSWE.Avalonia.Hosting

The lifetime-agnostic contract for bootstrapping Avalonia apps on the .NET Generic Host.

It provides `IServiceProviderAccessor` — implement it on your Avalonia `Application` so the host can hand it the
built `IServiceProvider` before `OnFrameworkInitializationCompleted`.

For desktop apps, use **[CCSWE.Avalonia.Hosting.Desktop](https://www.nuget.org/packages/CCSWE.Avalonia.Hosting.Desktop)**,
which provides `DesktopApplication.CreateBuilder<TApp>(args).Build().Run(args)`.

See the [repository](https://github.com/CCSWE-Avalonia/CCSWE.Avalonia.Hosting) for full usage.

MIT © Cory Charlton / CCSWE.
