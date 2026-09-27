# StreamDeckSimHub - Agent Guide

This document provides coding agents with essential information about building, testing, and maintaining this codebase.

## Project Overview

**StreamDeckSimHub** is a Stream Deck plugin for SimHub racing simulation hub, built with .NET 8.0 (C#), WPF, and a React-based Property Inspector UI. The plugin enables Stream Deck buttons to dynamically update based on SimHub properties.

- **Language**: C# with nullable reference types enabled
- **Framework**: .NET 8.0-windows (WPF + Windows Forms)
- **Architecture**: Plugin-based event-driven with MVVM pattern
- **Testing**: NUnit + Moq
- **License**: LGPL-3.0-or-later

## Build Commands

### Prerequisites
- .NET 8.0 SDK
- Windows (required for WPF)
- PowerShell (for bundling)

### Restore Dependencies
```bash
dotnet restore StreamDeckSimHub.Plugin/StreamDeckSimHub.Plugin.csproj
dotnet restore StreamDeckSimHub.PluginTests/StreamDeckSimHub.PluginTests.csproj
```

### Build
```bash
# Build main plugin
dotnet build StreamDeckSimHub.Plugin/StreamDeckSimHub.Plugin.csproj

# Build tests
dotnet build StreamDeckSimHub.PluginTests/StreamDeckSimHub.PluginTests.csproj

# Build with no restore
dotnet build --no-restore StreamDeckSimHub.Plugin/StreamDeckSimHub.Plugin.csproj
```

### Publish
```bash
# Publish plugin (triggers versioning and bundling via PowerShell scripts)
dotnet publish StreamDeckSimHub.Plugin/StreamDeckSimHub.Plugin.csproj
```

### Solution

`StreamDeckSimHub.sln` contains all three projects: the plugin, the plugin tests, and `StreamDeckSimHub.Installer` (a .NET Framework 4.8 WPF installer, but still an SDK-style project, see `StreamDeckSimHub.Installer/AGENTS.md`). Building/testing the solution as a whole (e.g. `dotnet build StreamDeckSimHub.sln`, `dotnet test`) builds all three projects.

### Test Commands
```bash
# Run all tests
dotnet test StreamDeckSimHub.PluginTests/StreamDeckSimHub.PluginTests.csproj

# Run tests with no build
dotnet test --no-build --verbosity normal StreamDeckSimHub.PluginTests/StreamDeckSimHub.PluginTests.csproj

# Run single test by full name
dotnet test StreamDeckSimHub.PluginTests/StreamDeckSimHub.PluginTests.csproj --filter "FullyQualifiedName=StreamDeckSimHub.PluginTests.Actions.HotkeyActionTests.TestOldComparisonInteger"

# Run all tests in a class
dotnet test StreamDeckSimHub.PluginTests/StreamDeckSimHub.PluginTests.csproj --filter "FullyQualifiedName~StreamDeckSimHub.PluginTests.Actions.HotkeyActionTests"

# Run tests matching name pattern
dotnet test StreamDeckSimHub.PluginTests/StreamDeckSimHub.PluginTests.csproj --filter "Name~OldComparison"
```

## Code Structure

```
StreamDeckSimHub.Plugin/
├── Actions/              # Stream Deck action implementations
│   ├── GenericButton/   # Complex configurable button (main feature)
│   │   ├── Model/       # Business logic, MVVM models
│   │   ├── JsonSettings/# DTO classes for serialization
│   │   └── Renderer/    # Button rendering (ImageSharp)
│   ├── HotkeyAction.cs  # 2-state hotkey
│   ├── Hotkey4Action.cs # 4-state hotkey
│   ├── DialAction.cs    # Encoder/dial
│   ├── FlagsAction.cs   # Racing flags
│   └── InputAction.cs   # Input trigger
├── ActionEditor/        # WPF visual editor
│   ├── Views/          # XAML views
│   ├── ViewModels/     # CommunityToolkit.Mvvm view models
│   └── Behaviors/      # WPF behaviors
├── SimHub/             # SimHub TCP connection & integration
├── PropertyLogic/      # NCalc expression evaluation and other property handling
├── Tools/              # Utilities (ImageManager, StateManager, etc.)
└── pi/                 # Property Inspector (HTML/JS/React)
```

## Code Style Guidelines

### General
- Always write the simplest, cleanest code possible.
- Code must always be consistent with the rest of the application.
- Never write unnecessary code.
- Remove unused imports.

### File Headers
- All C# files must include:
  ```csharp
  // Copyright (C) YEAR Martin Renner
  // LGPL-3.0-or-later (see file COPYING and COPYING.LESSER)
  ```
  whereas "YEAR" should be replaced with the current year.

- When modifying an existing file, update the year in the header to the current year.

### Imports
- **Implicit usings are DISABLED** for main plugin project
- Use `GlobalUsings.cs` for global imports (see `StreamDeckSimHub.Plugin/GlobalUsings.cs`)
- Default global usings: `System`, `System.Collections.Generic`, `System.Linq`, `System.Threading`, `System.Threading.Tasks`
- Always explicitly import other namespaces at file top
- Order: System namespaces, then third-party, then project namespaces

### Naming Conventions
- **Classes/Interfaces**: PascalCase (e.g., `GenericButtonAction`, `ISimHubConnection`)
- **Methods/Properties**: PascalCase (e.g., `PropertyChanged`, `DisplayItems`)
- **Private fields**: `_camelCase` with underscore prefix (e.g., `_settingsConverter`, `_imageManager`)
- **Parameters/Locals**: camelCase (e.g., `propertyName`, `propValueTrue`)
- **Constants**: PascalCase (e.g., `NewActionKeySize`)

### Types & Nullability
- **Nullable reference types enabled** - use `?` for nullable types
- Use `required` keyword for required properties: `public required Size KeySize { get; set; }`
- Prefer explicit types over `var` when type is not obvious
- Use `var` for obvious types: `var logger = Mock.Of<ILogger<PropertyComparer>>();`

### Patterns & Practices
- **Dependency Injection**: Register services in `Program.cs::ConfigureServices()`
  - Use constructor injection
  - Prefer `Singleton` for managers and connections
- **MVVM**: Use `CommunityToolkit.Mvvm` base classes
  - `ObservableObject` for view models
  - `[ObservableProperty]` attribute for bindable properties
  - `RelayCommand` for commands
- **Action Classes**: Inherit from `StreamDeckAction<TSettings>`
  - Decorate with `[StreamDeckAction("action.id")]`
  - Implement lifecycle methods: `OnWillAppear`, `OnKeyDown`, etc.
- **Error Handling**:
  - Use try-catch for async operations
  - Log errors with NLog via injected `ILogger<T>`
  - Propagate exceptions appropriately

### Async/Await
- Use `async`/`await` for I/O operations
- Async methods should have `Async` suffix (except event handlers)
- Use `ConfigureAwait(false)` where appropriate
- Return `Task` for async void methods (except event handlers)

### Collections
- Use `HashSet<T>` for unique collections with case-insensitive keys: `new(StringComparer.OrdinalIgnoreCase)`
- Use `ObservableCollection<T>` for bindable collections in MVVM
- Use `SortedDictionary<K, V>` for ordered key-value pairs
- Prefer collection initializers: `new List<string> { "item1", "item2" }`

### Comments & Documentation
- XML doc comments for public APIs: `/// <summary>`, `/// <param>`, `/// <returns>`
- Inline comments for complex logic
- Use `<c>code</c>` for inline code in XML comments
- Use `<remarks>` for additional context

### Testing
- **Framework**: NUnit
- **Mocking**: Moq
- Use `[SetUp]` for test initialization
- Test method naming: `Test<Scenario>` (e.g., `TestOldComparisonInteger`)
- Use `Assert.That` with constraint model: `Assert.That(actual, Is.EqualTo(expected))`
- Mock dependencies: `var mock = Mock.Of<IInterface>();`

## Project-Specific Patterns

### Settings & DTOs
- **DTO classes**: End with `Dto` suffix (e.g., `SettingsDto`, `DisplayItemDto`)
- **Model classes**: No suffix (e.g., `Settings`, `DisplayItem`)
- **Conversion**: Use `SettingsConverter` to convert between DTOs and models
- **Serialization**: Use `Newtonsoft.Json` with `[JsonProperty]` attributes

### Property Subscription Pattern
1. Subscribe to SimHub properties via `ISimHubConnection`
2. Implement `IPropertyChangedReceiver` interface
3. Handle property changes in `PropertyChanged` method
4. Return fast - use `Task` for long operations

### Image Handling
- Use `ImageManager` for image caching and retrieval
- SVG support via `Svg.Skia`
- Bitmap support via `SixLabors.ImageSharp`
- Images from: file system, `@flags`, `@core` resources

### Expression Evaluation
- Use `NCalcHandler` for property expressions
- Expressions support SimHub properties: `[PropertyName]`
- Use `PropertyComparer` for condition evaluation

## Common Pitfalls

1. **ImplicitUsings**: Disabled in main project - always explicitly import
2. **Windows Forms conflict**: `ColorDialog` requires Windows Forms, causes type conflicts with ImageSharp
3. **Nullable warnings**: All projects have nullable enabled - handle nulls properly
4. **StreamDeck SDK**: Custom `SharpDeck.dll` in `/lib/` - not a NuGet package
5. **React build**: React components are pre-built production files in `pi/react/`

## Important Files

- `Program.cs` - DI container configuration
- `GlobalUsings.cs` - Global using directives
- `appsettings.json` - NLog and SimHub connection settings
- `Directory.Build.props` - MSBuild properties (Nerdbank.GitVersioning)
- `version.json` - Version configuration
- `manifest-streamdeck.json` - Stream Deck plugin manifest

## CI / Release

- This project uses the Gitflow workflow (`develop` / `release/vX.Y` / `main`) together with Nerdbank.GitVersioning (`nbgv`), same as the sibling project `SimHubPropertyServer`.
- GitHub Actions workflows (`.github/workflows/`):
  - `test.yml` - builds and tests the solution on every push.
  - `prepare-release.yaml` - `workflow_dispatch` to cut a `release/vX.Y` branch from `develop`.
  - `create-release.yaml` - `workflow_dispatch` to merge a release branch into `main`, tag it via `nbgv tag`, and merge back into `develop`.
  - `publish-release.yaml` - triggers on pushed `vX.Y.Z` tags, builds the plugin and the installer, and creates a draft GitHub release with the Stream Deck plugin, the Stream Dock plugin, and the installer attached.
- Full process: see `doc/Release.adoc`.

## Resources

- **Repository**: https://github.com/pre-martin/StreamDeckSimHubPlugin
- **Documentation**: See `/doc/` folder (AsciiDoc format)
- **ADR**: Architecture Decision Records in `/doc/ADR/`
