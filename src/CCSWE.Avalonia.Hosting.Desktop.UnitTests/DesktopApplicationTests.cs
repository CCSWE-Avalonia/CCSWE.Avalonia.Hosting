using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CCSWE.Avalonia.Hosting.Desktop.UnitTests;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DesktopApplicationTests
{
    public class When_CreateBuilder_Is_Called : DesktopApplicationTests
    {
        [Test]
        public void It_exposes_the_service_collection()
        {
            var builder = DesktopApplication.CreateBuilder<TestApp>([]);

            Assert.That(builder.Services, Is.Not.Null);
        }

        [Test]
        public void It_returns_the_same_builder_from_ConfigureAppBuilder()
        {
            var builder = DesktopApplication.CreateBuilder<TestApp>([]);

            var result = builder.ConfigureAppBuilder(appBuilder => appBuilder);

            Assert.That(result, Is.SameAs(builder));
        }
    }

    public class When_Build_Is_Called : DesktopApplicationTests
    {
        [Test]
        public void It_builds_an_application_whose_provider_resolves_registered_services()
        {
            var builder = DesktopApplication.CreateBuilder<TestApp>([]);
            builder.Services.AddSingleton<IExampleService, ExampleService>();

            var application = builder.Build();

            Assert.That(application.Services.GetService<IExampleService>(), Is.InstanceOf<ExampleService>());
        }
    }

    private interface IExampleService;

    private sealed class ExampleService : IExampleService;

    private sealed class TestApp : Application;
}
