using Avalonia;
using Avalonia.Controls;
using JetBrains.Annotations;

namespace CCSWE.Avalonia.Hosting;

/// <summary>
/// Declarative configuration for the Avalonia <see cref="AppBuilder"/> and host defaults used by
/// <see cref="DesktopApplication.CreateBuilder{TApp}(string[], System.Action{DesktopApplicationOptions})"/>. The
/// defaults match a typical single-window desktop app; turn a knob to opt out.
/// </summary>
[PublicAPI]
public sealed class DesktopApplicationOptions
{
    /// <summary>Clear the Generic Host's default logging providers (Console/Debug/EventSource/EventLog), which a
    /// windowless GUI process cannot use. The <c>ILogger&lt;T&gt;</c> infrastructure stays registered. Default
    /// <see langword="false"/>.</summary>
    public bool ClearLoggingProviders { get; set; } = false;

    /// <summary>Call <c>LogToTrace()</c> so Avalonia diagnostics flow to <see cref="System.Diagnostics.Trace"/>.
    /// Default <see langword="true"/>.</summary>
    public bool LogToTrace { get; set; } = true;

    /// <summary>How the classic desktop lifetime decides to shut down. Default
    /// <see cref="ShutdownMode"/>.<c>OnLastWindowClose</c>.</summary>
    public ShutdownMode ShutdownMode { get; set; } = ShutdownMode.OnLastWindowClose;

    /// <summary>Call <c>UsePlatformDetect()</c> on the app builder. Default <see langword="true"/>.</summary>
    public bool UsePlatformDetect { get; set; } = true;

    /// <summary>Optional Win32 platform options (e.g. DPI awareness), applied via <c>.With(...)</c> when set.</summary>
    public Win32PlatformOptions? Win32PlatformOptions { get; set; }

    internal AppBuilder Apply(AppBuilder builder)
    {
        if (UsePlatformDetect)
        {
            builder = builder.UsePlatformDetect();
        }

        if (Win32PlatformOptions is { } win32Options)
        {
            builder = builder.With(win32Options);
        }

        if (LogToTrace)
        {
            builder = builder.LogToTrace();
        }

        return builder;
    }
}
