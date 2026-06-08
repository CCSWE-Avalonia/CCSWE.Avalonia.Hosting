using System;
using Avalonia;
using Avalonia.Controls;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CCSWE.Avalonia.Hosting;

/// <summary>
/// Builds a <see cref="DesktopApplication"/> that marries the .NET Generic Host with an Avalonia classic-desktop
/// application. Wraps a <see cref="HostApplicationBuilder"/> for service / configuration / logging setup and an
/// Avalonia <see cref="AppBuilder"/> for windowing configuration. Obtain one from
/// <see cref="DesktopApplication.CreateBuilder{TApp}(string[], Action{DesktopApplicationOptions})"/>.
/// </summary>
[PublicAPI]
public sealed class DesktopApplicationBuilder
{
    private readonly Func<AppBuilder> _appBuilderFactory;
    private Func<AppBuilder, AppBuilder>? _configureAppBuilder;
    private readonly HostApplicationBuilder _hostBuilder;
    private readonly ShutdownMode _shutdownMode;

    internal DesktopApplicationBuilder(HostApplicationBuilder hostBuilder, Func<AppBuilder> appBuilderFactory, ShutdownMode shutdownMode)
    {
        _hostBuilder = hostBuilder;
        _appBuilderFactory = appBuilderFactory;
        _shutdownMode = shutdownMode;
    }

    /// <summary>The host's configuration; add sources or read values during setup.</summary>
    public IConfigurationManager Configuration => _hostBuilder.Configuration;

    /// <summary>The underlying host builder, for advanced scenarios (service-provider factory, options, metrics).</summary>
    public IHostApplicationBuilder Host => _hostBuilder;

    /// <summary>The host's logging builder. Providers are cleared by default (a GUI process has no console);
    /// call <c>Logging.AddXxx()</c> to opt back in.</summary>
    public ILoggingBuilder Logging => _hostBuilder.Logging;

    /// <summary>The application's service collection. Register dependency injection here.</summary>
    public IServiceCollection Services => _hostBuilder.Services;

    /// <summary>Builds the host and returns a runnable <see cref="DesktopApplication"/>.</summary>
    public DesktopApplication Build() => new(_hostBuilder.Build(), BuildAppBuilder, _shutdownMode);

    /// <summary>
    /// Adds a customization of the Avalonia <see cref="AppBuilder"/>, applied after the
    /// <see cref="DesktopApplicationOptions"/> defaults. Composable — multiple calls chain in order. Use it for
    /// fonts, rendering options, developer tools, or anything not exposed on the options.
    /// </summary>
    public DesktopApplicationBuilder ConfigureAppBuilder(Func<AppBuilder, AppBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var previous = _configureAppBuilder;
        _configureAppBuilder = previous is null ? configure : builder => configure(previous(builder));
        return this;
    }

    // The single source of truth for the configured AppBuilder (options defaults + ConfigureAppBuilder chain),
    // reused by Build() at runtime.
    private AppBuilder BuildAppBuilder()
    {
        var appBuilder = _appBuilderFactory();
        return _configureAppBuilder is null ? appBuilder : _configureAppBuilder(appBuilder);
    }
}
