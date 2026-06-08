using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CCSWE.Avalonia.Hosting;

/// <summary>
/// A runnable desktop application that owns a Generic <see cref="IHost"/> and an Avalonia classic-desktop
/// lifetime. <see cref="Run"/> starts the hosted services, runs the Avalonia main loop until the application
/// exits, then stops the host. Create one with
/// <see cref="CreateBuilder{TApp}(string[], Action{DesktopApplicationOptions})"/> and
/// <see cref="DesktopApplicationBuilder.Build"/>.
/// </summary>
[PublicAPI]
public sealed partial class DesktopApplication
{
    private readonly Func<AppBuilder> _appBuilderFactory;
    private readonly IHost _host;
    private readonly ShutdownMode _shutdownMode;
    private int _started;

    internal DesktopApplication(IHost host, Func<AppBuilder> appBuilderFactory, ShutdownMode shutdownMode)
    {
        _host = host;
        _appBuilderFactory = appBuilderFactory;
        _shutdownMode = shutdownMode;
    }

    /// <summary>The underlying Generic Host, for advanced lifetime coordination
    /// (<see cref="IHostApplicationLifetime"/>, manual start/stop).</summary>
    public IHost Host => _host;

    /// <summary>The application's service provider (the host's root provider).</summary>
    public IServiceProvider Services => _host.Services;

    /// <summary>
    /// Produces the Avalonia <see cref="AppBuilder"/> for <typeparamref name="TApp"/> with the same defaults
    /// <see cref="CreateBuilder{TApp}(string[], Action{DesktopApplicationOptions})"/> applies, but with no Generic
    /// Host. Intended for the design-time XAML previewer, whose <c>BuildAvaloniaApp()</c> entry point needs a
    /// parameterless <see cref="AppBuilder"/>-returning method. The provider is never set at design time, so the
    /// application must tolerate a null provider.
    /// </summary>
    public static AppBuilder ConfigureAppBuilder<TApp>(Action<DesktopApplicationOptions>? configureOptions = null)
        where TApp : Application, new()
    {
        var options = new DesktopApplicationOptions();
        configureOptions?.Invoke(options);
        return options.Apply(AppBuilder.Configure<TApp>());
    }

    /// <summary>
    /// Creates a <see cref="DesktopApplicationBuilder"/> for the Avalonia application <typeparamref name="TApp"/>.
    /// Initializes the Generic Host and prepares an Avalonia <see cref="AppBuilder"/> with the defaults from
    /// <see cref="DesktopApplicationOptions"/> (which <paramref name="configureOptions"/> may override).
    /// </summary>
    /// <typeparam name="TApp">The Avalonia <c>Application</c> subclass (needs a parameterless constructor).
    /// Implement <see cref="IServiceProviderAccessor"/> on it to receive the built provider.</typeparam>
    public static DesktopApplicationBuilder CreateBuilder<TApp>(string[] args, Action<DesktopApplicationOptions>? configureOptions = null)
        where TApp : Application, new()
    {
        var options = new DesktopApplicationOptions();
        configureOptions?.Invoke(options);

        var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);
        if (options.ClearLoggingProviders)
        {
            hostBuilder.Logging.ClearProviders();
        }

        return new DesktopApplicationBuilder(hostBuilder, () => options.Apply(AppBuilder.Configure<TApp>()), options.ShutdownMode);
    }

    // Hands the built provider to the application instance. A soft contract: an app that doesn't implement
    // IServiceProviderAccessor simply doesn't receive it (it likely has nothing to compose from DI).
    private void InjectServices(Application? application)
    {
        if (application is IServiceProviderAccessor accessor)
        {
            accessor.Services = _host.Services;
            return;
        }

        if (_host.Services.GetService<ILogger<DesktopApplication>>() is { } logger)
        {
            Log.ServiceProviderNotInjected(logger, application?.GetType().Name ?? nameof(Application), nameof(IServiceProviderAccessor));
        }
    }

    /// <summary>
    /// Runs the application: starts the hosted services (non-blocking), runs the Avalonia classic-desktop main
    /// loop (blocking) until the last window closes or shutdown is requested, then stops the host gracefully
    /// (cancelling background services). This is the one call a typical <c>Program.Main</c> needs.
    /// </summary>
    /// <returns>The Avalonia process exit code.</returns>
    public int Run(string[] args)
    {
        if (Interlocked.Exchange(ref _started, 1) != 0)
        {
            throw new InvalidOperationException($"{nameof(DesktopApplication)} has already been run.");
        }

        try
        {
            // Non-blocking: runs each IHostedService.StartAsync and kicks off BackgroundService.ExecuteAsync.
            _host.Start();

            // AfterSetup runs after TApp is constructed + Initialize(), before OnFrameworkInitializationCompleted,
            // so the provider is set in time. StartWithClassicDesktopLifetime then blocks for the UI session.
            return _appBuilderFactory()
                .AfterSetup(builder => InjectServices(builder.Instance))
                .StartWithClassicDesktopLifetime(args, _shutdownMode);
        }
        finally
        {
            // Always stop + dispose, even if startup or the UI loop threw, so background services are cancelled
            // and nothing leaks. StopAsync tolerates a host that never fully started.
            _host.StopAsync().GetAwaiter().GetResult();
            _host.Dispose();
        }
    }

    [ExcludeFromCodeCoverage]
    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Warning, "{Application} does not implement {Interface}; the service provider was not injected", EventName = "ServiceProviderNotInjected")]
        public static partial void ServiceProviderNotInjected(ILogger logger, string application, string @interface);
    }
}
