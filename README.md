# Chainguard Libraries Dotfiles Generator

A lightweight .NET Core CLI utility for Windows x64 designed to integrate with **Chainguard Libraries** across **Java, JavaScript, and Python** ecosystems. It automates `chainctl` installation, authentication, pull token generation, and dotfile creation.

---

## Features

- **First-Run Setup**: Automatically opens the Chainguard Console (`https://console.chainguard.dev/`) in your default browser to create an account/organization.
- **chainctl Automation**: Automatically downloads the latest version of `chainctl` from Chainguard's release metadata, adds it to your User `PATH`, and performs `chainctl auth login`.
- **Pull Token Generation**: Interacts with `chainctl` to generate authenticated pull tokens for **Java**, **JavaScript**, and **Python** repositories.
- **Ecosystem Configs & Dotfiles**:
  - **JavaScript (npm)**: Configures `%USERPROFILE%\.npmrc` with Chainguard Libraries repository auth.
  - **Python (pip/uv)**: Configures `%APPDATA%\pip\pip.ini` with custom index URLs.
  - **Java (Maven)**: Configures `%USERPROFILE%\.m2\settings.xml` with Chainguard server credentials and repository profiles.

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
.\bin\Release\net10.0\win-x64\publish\devdots.exe
```

---

## Contributing

We welcome contributions! Please check out [CONTRIBUTING.md](CONTRIBUTING.md) for instructions on setting up your development environment, testing, and submitting pull requests.

