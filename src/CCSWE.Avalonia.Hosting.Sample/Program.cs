using System;
using Avalonia;

namespace CCSWE.Avalonia.Hosting.Sample;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = DesktopApplication.CreateBuilder<App>(args);

        // Register services here, e.g. builder.Services.AddSingleton<IMyService, MyService>();

#if DEBUG
        builder.ConfigureAppBuilder(appBuilder => appBuilder.WithDeveloperTools());
#endif

        builder.Build().Run(args);
    }

    public static AppBuilder BuildAvaloniaApp() => DesktopApplication.ConfigureAppBuilder<App>();
}
