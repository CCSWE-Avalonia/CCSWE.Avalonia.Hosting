using JetBrains.Annotations;

namespace CCSWE.Avalonia.Hosting;

/// <summary>
/// Implemented by an Avalonia <c>Application</c> so the hosting infrastructure can hand it the built
/// <see cref="IServiceProvider"/>. The host sets <see cref="Services"/> from an <c>AfterSetup</c> callback —
/// after the application is constructed and <c>Initialize()</c> has run, but before
/// <c>OnFrameworkInitializationCompleted()</c> — so the provider is available when the app composes its UI.
/// </summary>
[PublicAPI]
public interface IServiceProviderAccessor
{
    /// <summary>The application's service provider, set once by the host before framework initialization completes.</summary>
    IServiceProvider Services { set; }
}
