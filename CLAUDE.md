# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

# Project

`CCSWE.Avalonia.Hosting` is a small library suite that bootstraps an Avalonia desktop app on the .NET **Generic
Host** (DI, hosted-service lifecycle, configuration, logging) so an app's `Program.Main` is just *create a
builder, register services, build, run*. See `README.md` for usage. Targets `net10.0` / Avalonia 12.

## Architecture — two packages on one namespace

- **`CCSWE.Avalonia.Hosting`** (common): the lifetime-agnostic contract `IServiceProviderAccessor` — the seam an
  Avalonia `Application` implements to receive the built `IServiceProvider`. No package dependencies.
- **`CCSWE.Avalonia.Hosting.Desktop`**: `DesktopApplication` (the `CreateBuilder<TApp>` factory + `Run()`),
  `DesktopApplicationBuilder` (wraps `HostApplicationBuilder`), `DesktopApplicationOptions`. Depends on
  `Avalonia.Desktop` + `Microsoft.Extensions.Hosting`. **`<RootNamespace>` is `CCSWE.Avalonia.Hosting`** — the
  `.Desktop` suffix is package/assembly identity only, mirroring `Avalonia.Desktop` (types in `Avalonia`) and
  `Microsoft.Extensions.Hosting.Abstractions` (types in `Microsoft.Extensions.Hosting`). Consumers use one `using`.

The classic-desktop lifetime (`StartWithClassicDesktopLifetime`) is cross-platform desktop — **Windows, Linux,
and macOS from a single build**; no per-OS targeting. Single-view (iOS/WASM) and activity (Android) lifecycles
differ fundamentally (the OS owns the loop), so they are **not** generalized into the common package — a future
`.SingleView`/`.Android` package would build on the same `IServiceProviderAccessor` contract.

## Lifecycle design (don't regress)

`DesktopApplication.Run()`: `host.Start()` (non-blocking; starts hosted services) → configured `AppBuilder`
`.AfterSetup(inject provider)` → `StartWithClassicDesktopLifetime` (blocks) → `finally` `host.StopAsync()` +
`Dispose()`. The `AfterSetup` callback runs after the app is constructed + `Initialize()` but before
`OnFrameworkInitializationCompleted`, so the provider is set in time. `Run()` is one-shot (Interlocked guard);
advanced lifetime coordination uses `app.Host`. The previewer entry point is `DesktopApplication.ConfigureAppBuilder<TApp>()`
(no host). Dev-tools (`WithDeveloperTools()`) stay in the **consuming app's** `#if DEBUG ConfigureAppBuilder(...)` —
the library must not reference `Avalonia.Diagnostics`.

## Build / pack / publish

```bash
dotnet build src/CCSWE.Avalonia.Hosting.slnx -c Release
dotnet test  src/CCSWE.Avalonia.Hosting.slnx
dotnet pack  src/CCSWE.Avalonia.Hosting.slnx -c Release -o artifacts
```

Central Package Management (`src/Directory.Packages.props`); never put `Version=` on a `<PackageReference>`.
Versioning via Nerdbank.GitVersioning (`version.json`); the major tracks the Avalonia major. CI
(`.github/workflows/dotnet-build-publish-library.yml`) builds/tests/packs and publishes both packages to
NuGet.org on `master`.

## Coding standards

Standard C# conventions: 4-space indent, Allman braces, explicit access modifiers, file-scoped namespaces,
`using`s outside the namespace (System first, then third-party, then project). `LangVersion=default`,
`ImplicitUsings`/`Nullable` enabled solution-wide. `[PublicAPI]` (JetBrains.Annotations) on the public surface;
`GenerateDocumentationFile` is on, so document public/internal members (use `<inheritdoc />` for interface
implementations where the base doc suffices). `[ExcludeFromCodeCoverage]` on composition-only/log-only types.

- **Formatting:** always brace control flow (never omit for single-line bodies); one statement/declaration per
  line; one blank line between members, no consecutive blanks.
- **Naming:** `PascalCase` types/methods/properties/constants; `camelCase` locals/parameters; `_camelCase`
  private fields; `I`-prefixed interfaces. Two-letter acronyms upper-case (`IO`, `UI`), longer ones PascalCase
  (`Http`, `Json`). Use `nameof()` over string literals for member names.
- **Language style:** `var` when the type is inferred; keyword types (`string`, not `String`); string
  interpolation over concatenation; `&&`/`||` not `&`/`|`; `async`/`await` (never `.Result`/`.Wait()`);
  expression-bodied members for single-line getters/methods.
- **Member order:** group by kind (constants/`static readonly`, fields, ctors, properties, methods) and
  alphabetize within each group regardless of access; nested types go last in the file.
- **Frozen collections:** any never-mutated `static readonly` `HashSet<T>`/`Dictionary<TKey,TValue>` should be a
  `FrozenSet<T>`/`FrozenDictionary<TKey,TValue>` (`System.Collections.Frozen`), built via
  `.ToFrozenSet(comparer)`/`.ToFrozenDictionary()` — faster lookups, and the type signals "immutable lookup".

## Testing

NUnit 4, Moq for mocking (mock `ILogger` with a logger fake rather than raw setups).
`<ClassUnderTest>Tests` (not sealed, `[SuppressMessage("ReSharper", "InconsistentNaming")]`) with nested
`When_<Method>_Is_Called` classes inheriting the outer and `It_<behavior>` methods; AAA with blank-line separation
(no `// Arrange` comments). Test projects sit **physically under `src/`**
(no on-disk `tests/` folder) and appear under a `/tests/` solution folder; named `<ProjectUnderTest>.UnitTests`.
Tests stay off the UI thread (no `Run()` — it starts the desktop lifetime); cover options/builder/host wiring.
