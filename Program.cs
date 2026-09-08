using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

class Program
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".ossindex"
    );
    private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.json");

    class Config
    {
        public string? Username { get; set; }
        public string? Token { get; set; }
    }

    static void Main(string[] args)
    {
        Console.WriteLine("=== OSS Index Dotfiles Generator ===");

        Directory.CreateDirectory(ConfigDir);

        Config config;
        if (File.Exists(ConfigFile))
        {
            try
            {
                var json = File.ReadAllText(ConfigFile);
                config = JsonSerializer.Deserialize<Config>(json) ?? new Config();
            }
            catch
            {
                config = new Config();
            }
        }
        else
        {
            config = new Config();
        }

        if (string.IsNullOrWhiteSpace(config.Username) || string.IsNullOrWhiteSpace(config.Token))
        {
            Console.WriteLine("\nIt looks like this is your first time running this utility or credentials are missing.");
            Console.WriteLine("To use OSS Index, you need an account and an API token.");
            Console.WriteLine("Opening OSS Index registration / token page in your default browser...");

            OpenUrl("https://ossindex.sonatype.org/");

            Console.WriteLine("\nPlease enter your OSS Index Username (Email):");
            config.Username = Console.ReadLine()?.Trim();

            Console.WriteLine("Please enter your OSS Index API Token:");
            config.Token = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(config.Username) || string.IsNullOrWhiteSpace(config.Token))
            {
                Console.WriteLine("Error: Username and Token cannot be empty.");
                return;
            }

            File.WriteAllText(ConfigFile, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Saved configuration to {ConfigFile}");
        }

        GenerateDotfiles(config.Username, config.Token);
        Console.WriteLine("\nAll dotfiles generated successfully for Windows!");
    }

    static void OpenUrl(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not open browser automatically: {ex.Message}");
            Console.WriteLine($"Please visit: {url}");
        }
    }

    static void GenerateDotfiles(string username, string token)
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        // 1. NPM (.npmrc)
        var npmrcPath = Path.Combine(userProfile, ".npmrc");
        var npmrcContent = $"# OSS Index Credentials for npm audit / tools\n" +
                           $"registry=https://registry.npmjs.org/\n" +
                           $"//registry.npmjs.org/:_authToken={token}\n";
        File.WriteAllText(npmrcPath, npmrcContent);
        Console.WriteLine($"[Created] NPM config: {npmrcPath}");

        // 2. NuGet (NuGet.Config)
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var nugetDir = Path.Combine(appData, "NuGet");
        Directory.CreateDirectory(nugetDir);
        var nugetConfigPath = Path.Combine(nugetDir, "NuGet.Config");
        
        var nugetContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""nuget.org"" value=""https://api.nuget.org/v3/index.json"" protocolVersion=""3"" />
  </packageSources>
</configuration>
";
        File.WriteAllText(nugetConfigPath, nugetContent);
        Console.WriteLine($"[Created] NuGet config: {nugetConfigPath}");

        // 3. Pip / UV (pip.conf)
        var pipDir = Path.Combine(appData, "pip");
        Directory.CreateDirectory(pipDir);
        var pipIniPath = Path.Combine(pipDir, "pip.ini");
        var pipContent = "[global]\n";
        File.WriteAllText(pipIniPath, pipContent);
        Console.WriteLine($"[Created] Pip config: {pipIniPath}");

        // 4. Environment Variables helper script (set-env.ps1)
        var envScriptPath = Path.Combine(ConfigDir, "set-env.ps1");
        var envScriptContent = $"[System.Environment]::SetEnvironmentVariable('OSSINDEX_USERNAME', '{username}', [System.EnvironmentVariableTarget]::User)\n" +
                               $"[System.Environment]::SetEnvironmentVariable('OSSINDEX_TOKEN', '{token}', [System.EnvironmentVariableTarget]::User)\n" +
                               $"Write-Host 'OSS Index environment variables set successfully for User scope.'\n";
        File.WriteAllText(envScriptPath, envScriptContent);
        Console.WriteLine($"[Created] PowerShell Environment Setup Script: {envScriptPath}");

        try
        {
            Process.Start(new ProcessStartInfo("powershell", $"-ExecutionPolicy Bypass -File \"{envScriptPath}\"") { CreateNoWindow = true })?.WaitForExit();
            Console.WriteLine("[Success] User environment variables OSSINDEX_USERNAME and OSSINDEX_TOKEN have been set!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not set environment variables automatically: {ex.Message}");
        }
    }
}

