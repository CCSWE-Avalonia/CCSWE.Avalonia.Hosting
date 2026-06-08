using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using NUnit.Framework;

namespace CCSWE.Avalonia.Hosting.Desktop.UnitTests;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class DesktopApplicationOptionsTests
{
    public class When_Constructed : DesktopApplicationOptionsTests
    {
        [Test]
        public void It_has_desktop_defaults()
        {
            var options = new DesktopApplicationOptions();

            Assert.Multiple(() =>
            {
                Assert.That(options.ClearLoggingProviders, Is.False);
                Assert.That(options.LogToTrace, Is.True);
                Assert.That(options.UsePlatformDetect, Is.True);
                Assert.That(options.ShutdownMode, Is.EqualTo(ShutdownMode.OnLastWindowClose));
                Assert.That(options.Win32PlatformOptions, Is.Null);
            });
        }
    }
}
