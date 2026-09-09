# Contributing to Chainguard Libraries Dotfiles Generator

Thank you for your interest in contributing to the Chainguard Libraries Dotfiles Generator utility! This guide will help you get started with setting up your development environment, making changes, testing, and submitting your contributions.

## Prerequisites

To work on this project, ensure you have the following installed on your machine:
- [.NET SDK 10.0](https://dotnet.microsoft.com/download) (or compatible .NET 10 SDK)
- Git
- PowerShell (for `chainctl` installation and environment configuration on Windows)

---

## Getting Started

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd devdots
   ```

2. **Switch to the feature branch or create a new one:**
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

---

## Project Structure

- `OssIndexDotfiles.csproj`: The project file targeting .NET 10 (`net10.0`) configured as a console executable named `devdots`.
- `Program.cs`: The core entry point containing:
  - First-run detection & Chainguard Console browser launch (`https://console.chainguard.dev/`).
  - Automatic download and PATH configuration of `chainctl` under `%USERPROFILE%\.chainguard\bin`.
  - Authentication automation via `chainctl auth login`.
  - Pull token generation across Chainguard ecosystems (**Java**, **JavaScript**, and **Python**).
  - Dotfile and configuration writers for `.npmrc`, `pip.ini`, and Maven `settings.xml`.

---

## Making Changes & Testing

1. **Run the application locally:**
   ```bash
   dotnet run
   ```
   *Note: On first run, it will open your browser to Chainguard Console and prompt for your parent organization and email.*

2. **Build and test the project:**
   ```bash
   dotnet build -c Debug
   ```

3. **Publish as a Standalone Windows X64 Binary:**
   To build the self-contained single-file executable named `devdots.exe` for Windows x64:
   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
   ```
   The compiled binary will be available at:
   `bin/Release/net10.0/win-x64/publish/devdots.exe`

---

## Submitting Your Contribution

1. **Commit your changes:**
   Follow clear and descriptive commit messages:
   ```bash
   git add .
   git commit -m "Add feature: <description of changes>"
   ```

2. **Push to the branch:**
   ```bash
   git push origin feature/your-feature-name
   ```

3. **Open a Pull Request:**
   Submit a Pull Request against the repository main branch with a clear description of your changes and why they are needed.

