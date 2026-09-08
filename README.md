# OSS Index Dotfiles Generator

A lightweight .NET Core CLI utility for Windows x64 designed to generate dotfiles and configure environment variables for popular package managers (`npm`, `NuGet`, `pip`, and `uv`) to integrate with [Sonatype OSS Index](https://ossindex.sonatype.org/).

---

## Features

- **First-Run Detection**: Automatically opens the Sonatype OSS Index registration/token page in your default browser on first launch and securely saves credentials locally.
- **Config & Dotfiles Generation**:
  - **NPM**: Creates/updates `%USERPROFILE%\.npmrc` with authentication tokens.
  - **NuGet**: Ensures `%APPDATA%\NuGet\NuGet.Config` is properly initialized.
  - **Pip / UV**: Configures `%APPDATA%\pip\pip.ini`.
  - **Environment Variables**: Automatically configures user-scoped `OSSINDEX_USERNAME` and `OSSINDEX_TOKEN` environment variables.

---

## Installation & Usage

### 1. Prerequisites
- [.NET SDK 10.0](https://dotnet.microsoft.com/download) (if building from source)
- Windows x64

### 2. Building from Source
Clone the repository and publish a standalone single-file Windows x64 binary named `devdots.exe`:

```bash
git clone <repository-url>
cd devdots
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable will be generated at:
`bin/Release/net10.0/win-x64/publish/devdots.exe`

### 3. Running the Utility
Run the compiled executable or run via dotnet:

```bash
dotnet run
```
Or execute the published binary directly:
```bash
.\bin\Release\net10.0\win-x64\publish\OssIndexDotfiles.exe
```

---

## Contributing

We welcome contributions! Please check out [CONTRIBUTING.md](CONTRIBUTING.md) for instructions on setting up your development environment, testing, and submitting pull requests.

