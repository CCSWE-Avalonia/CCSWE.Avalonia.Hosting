// A complete Program.cs for an Avalonia desktop app hosted on the .NET Generic Host.
// Add the CCSWE.Avalonia.Hosting.Desktop package, then:

using System;
using Avalonia;
using CCSWE.Avalonia.Hosting;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = DesktopApplication.CreateBuilder<App>(args, options =>
        {
            options.Win32PlatformOptions = new Win32PlatformOptions { DpiAwareness = Win32DpiAwareness.Unaware };
        });

        builder.Services.AddSingleton<IMyService, MyService>();
        builder.Services.AddHostedService<MyBackgroundWorker>();

#if DEBUG
        builder.ConfigureAppBuilder(appBuilder => appBuilder.WithDeveloperTools());
#endif

        builder.Build().Run(args);
    }

    // Avalonia's XAML previewer calls this parameterless AppBuilder-returning method.
    public static AppBuilder BuildAvaloniaApp() => DesktopApplication.ConfigureAppBuilder<App>();
}

// App receives the built provider by implementing IServiceProviderAccessor:
//
//   public partial class App : Application, IServiceProviderAccessor
//   {
//       public IServiceProvider? Services { get; private set; }
//       IServiceProvider IServiceProviderAccessor.Services { set => Services = value; }
//
//       public override void OnFrameworkInitializationCompleted()
//       {
//           // Services is set before this runs — resolve view models, register data templates,
//           // and set the main window from DI here.
//           base.OnFrameworkInitializationCompleted();
//       }
//   }
