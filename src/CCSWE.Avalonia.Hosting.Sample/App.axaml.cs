using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace CCSWE.Avalonia.Hosting.Sample;

public partial class App : Application, IServiceProviderAccessor
{
    public IServiceProvider? Services { get; private set; }

    IServiceProvider IServiceProviderAccessor.Services
    {
        set => Services = value;
    }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
