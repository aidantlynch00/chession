# chession
A cross-platform desktop application that tracks Lichess games played during a session, displaying wins/losses/draws with a prominent session score.

## Features

- Session tracking with real-time game updates
- Displays username and profile information
- Tracks wins, losses, and draws for the current session
- Shows total games played during the session
- Secure token-based authentication with Lichess
- Cross-platform support (Windows, Linux, macOS)

## Getting Started

### 1. Create a Lichess API Token

To use chession, you need a Lichess API token:

1. Go to [](https://lichess.org/account/oauth/token/create)
2. Enter a description (e.g., "chession")
3. Click "Generate Token"
4. Copy the generated token

### 2. Run the Application

Download a pre-built binary from the releases page, or run from source:

```bash
dotnet run
```

On first launch, you'll be prompted to enter your Lichess API token. The token will be securely stored for future sessions.

## Build From Source

Building from source requires the .NET 10 SDK.

```bash
# Clone the repo
git clone https://github.com/aidantlynch/chession
cd chession

# Windows
dotnet publish -c Release -r win-x64 --self-contained

# Linux
dotnet publish -c Release -r linux-x64 --self-contained

# macOS
dotnet publish -c Release -r osx-x64 --self-contained
```

## Token Storage

Your Lichess API token is stored securely in the platform-specific app data directory:

- **Windows**: `%APPDATA%\chession\token.json`
- **Linux**: `~/.config/chession/token.json`
- **macOS**: `~/Library/Application Support/chession/token.json`

## Tech Stack

- Avalonia UI 11.2.2
- .NET 10
- LichessSharp (API client)
