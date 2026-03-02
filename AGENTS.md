# AGENTS.md - chession Development Guide

## Overview

chession is a cross-platform Avalonia desktop application that tracks Lichess games played during a session, displaying wins/losses/draws with a prominent session score.

---

## Tech Stack

- **Framework**: Avalonia UI 11.2.2 (.NET 10)
- **Pattern**: MVVM (manual INotifyPropertyChanged, no CommunityToolkit.Mvvm)
- **API Client**: LichessSharp 0.3.1 (NuGet)
- **Target Platforms**: Windows, Linux, macOS

---

## Build Commands

```bash
# Build the project
dotnet build

# Run in development
dotnet run

# Build for specific platforms
dotnet publish -c Release -r win-x64 -o ./publish/win-x64
dotnet publish -c Release -r linux-x64 -o ./publish/linux-x64
dotnet publish -c Release -r osx-x64 -o ./publish/osx-x64

# Clean and rebuild
dotnet clean && dotnet build
```

---

## Project Structure

```
chession/
├── chession.csproj               # Project file
├── src/
│   ├── App.axaml / App.axaml.cs  # Application root, startup orchestration
│   ├── Program.cs                 # Entry point
│   ├── ViewModels/
│   │   ├── ViewModelBase.cs       # Base class with INotifyPropertyChanged
│   │   ├── MainViewModel.cs       # Main dashboard logic
│   │   ├── AuthViewModel.cs       # Token input flow
│   │   ├── GameSlot.cs            # Individual game slot in UI
│   │   └── GameTracker.cs         # Game streaming and result tracking
│   ├── Views/
│   │   ├── MainWindow.axaml(.cs)  # Window orchestrator, navigation
│   │   ├── MainView.axaml(.cs)   # Dashboard display
│   │   └── AuthView.axaml(.cs)   # Token input screen
│   ├── Services/
│   │   ├── ILichessService.cs     # API abstraction interface
│   │   ├── LichessService.cs      # LichessSharp implementation
│   │   ├── ITokenStorage.cs       # Token persistence interface
│   │   └── TokenStorage.cs        # Platform-specific storage
│   ├── Models/
│   │   ├── TokenData.cs           # Token record type
│   │   ├── GameResult.cs          # Win/loss/draw enum
│   │   ├── CurrentGame.cs         # Ongoing game data
│   │   └── ChessionJsonContext.cs # System.Text.Json context
│   └── Converters/
│       └── ResultToColorConverter.cs
```
---

## Code Style Guidelines

### General Conventions

- **Namespaces**: Use file-scoped namespaces (`namespace chession.ViewModels;`)
- **Access Modifiers**: Explicitly specify all access modifiers
- **Regions**: Do NOT use regions
- **Comments**: Avoid unless explaining complex logic; never comment obvious code

### Naming Conventions

- **Classes/Interfaces**: PascalCase (`MainViewModel`, `ILichessService`)
- **Methods/Properties**: PascalCase (`InitializeAsync`, `ErrorMessage`)
- **Private Fields**: Underscore prefix (`_lichessService`, `_profile`)
- **Parameters**: camelCase (`cancellationToken`, `tokenStorage`)
- **Files**: Match class name (`MainViewModel.cs`)

### Types and Imports

- **Nullable**: Enable in csproj; use `?` for reference types that can be null
- **Records**: Use for simple immutable data models (`TokenData`)
- **Interfaces**: Prefix with `I` (`ILichessService`)
- **Collections**: Use concrete types in ViewModels (`ObservableCollection<T>`)
- **Imports**: Sort alphabetically, group by namespace

### Property Change Notification

Follow the pattern in `ViewModelBase.cs`:

```csharp
private string? _errorMessage;
public string? ErrorMessage
{
    get => _errorMessage;
    set => SetProperty(ref _errorMessage, value);
}
```

Use `SetProperty<T>()` for all observable properties. Call `SetProperty` in setters and use the return value when needed.

### Commands

Implement `ICommand` manually or use simple RelayCommand pattern (see `AuthViewModel.cs`). Prefer `Func<bool>` for can-execute predicates.

### Error Handling

- Use try-catch with specific exception types first
- Catch `LichessAuthenticationException` for auth failures
- Store error messages in ViewModel properties for UI binding
- Avoid silently swallowing exceptions

---

## Architecture Patterns

### Service Layer

Services should define interfaces in `Services/` with implementations in the same folder. Use dependency injection via constructor:

```csharp
public class MainWindow : Window
{
    public async Task InitializeAsync(ITokenStorage tokenStorage)
    {
        var service = new LichessService(token);
    }
}
```

### View-ViewModel Binding

Views bind to ViewModel properties via `x:DataType` and compiled bindings:

```xml
<UserControl x:DataType="vm:MainViewModel">
    <TextBlock Text="{Binding Profile.Username}" />
</UserControl>
```

### Navigation Flow

`MainWindow` orchestrates navigation between `AuthView` and `MainView` by setting `MainContent.Content`. ViewModels communicate via events (e.g., `AuthenticationFailed`, `TokenSubmitted`).

---

## Auth Flow (High-Level)

1. App starts → `MainWindow.InitializeAsync()`
2. Check for stored token via `ITokenStorage`
3. No token → Show `AuthView`
4. Has token → Create `LichessService`, validate via `GetProfileAsync()`
5. Validation fails → Clear token, show `AuthView` with error
6. Validation succeeds → Show `MainView`

Token is stored in platform-specific app data:
- Windows: `%APPDATA%\chession\token.json`
- Linux: `~/.config/chession/token.json`
- macOS: `~/Library/Application Support/chession/token.json`

---

## Session Tracking (High-Level)

The app monitors ongoing games by streaming events from Lichess:
- `gameStart` → Stream the individual game to track its state
- When game completes → Fetch the final result (win/loss/draw)

Each ongoing game is streamed individually. The result is determined when the game finishes and is fetched via the Lichess API.

---

## Future Enhancements (Post-MVP)

- Display chess variant for each game
- List ongoing games
- Session history (persist across app restarts)
- Clear session button
- Configurable "always on top" toggle

---

## Important Notes

- Configure Lichess API client with retries
- Ensure proper `CancellationToken` propagation for all async operations
- Use `async void` only for event handlers; prefer `async Task` otherwise
