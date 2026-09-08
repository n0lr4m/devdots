using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

class Program
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".sonatype"
    );
    private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.json");

    class Config
    {
        public string? Username { get; set; }
        public string? Token { get; set; }
    }

    static void Main(string[] args)
    {
        Console.WriteLine("=== Sonatype Guide & OSS Index Dotfiles Generator ===");

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
            Console.WriteLine("Sonatype OSS Index is migrating to Sonatype Guide (https://guide.sonatype.com).");
            Console.WriteLine("Opening Sonatype Guide / OSS Index registration & token page in your default browser...");

            OpenUrl("https://guide.sonatype.com/");

            Console.WriteLine("\nPlease enter your Sonatype Guide Username / Email:");
            config.Username = Console.ReadLine()?.Trim();

            Console.WriteLine("Please enter your Sonatype Guide Personal Access Token (PAT) / OSS Index API Token:");
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
        Console.WriteLine("\nAll dotfiles and environment configurations updated successfully for Sonatype Guide & OSS Index!");
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
        var npmrcContent = $"# Sonatype Guide / OSS Index Credentials for npm tools\n" +
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
        // Supporting both legacy OSSINDEX variables and new Sonatype Guide variables / endpoints
        var envScriptPath = Path.Combine(ConfigDir, "set-env.ps1");
        var envScriptContent = $"[System.Environment]::SetEnvironmentVariable('OSSINDEX_USERNAME', '{username}', [System.EnvironmentVariableTarget]::User)\n" +
                               $"[System.Environment]::SetEnvironmentVariable('OSSINDEX_TOKEN', '{token}', [System.EnvironmentVariableTarget]::User)\n" +
                               $"[System.Environment]::SetEnvironmentVariable('SONATYPE_GUIDE_API_URL', 'https://api.guide.sonatype.com', [System.EnvironmentVariableTarget]::User)\n" +
                               $"[System.Environment]::SetEnvironmentVariable('SONATYPE_GUIDE_USERNAME', '{username}', [System.EnvironmentVariableTarget]::User)\n" +
                               $"[System.Environment]::SetEnvironmentVariable('SONATYPE_GUIDE_TOKEN', '{token}', [System.EnvironmentVariableTarget]::User)\n" +
                               $"Write-Host 'Sonatype Guide and OSS Index environment variables set successfully for User scope.'\n";
        File.WriteAllText(envScriptPath, envScriptContent);
        Console.WriteLine($"[Created] PowerShell Environment Setup Script: {envScriptPath}");

        try
        {
            Process.Start(new ProcessStartInfo("powershell", $"-ExecutionPolicy Bypass -File \"{envScriptPath}\"") { CreateNoWindow = true })?.WaitForExit();
            Console.WriteLine("[Success] User environment variables for Sonatype Guide & OSS Index have been set!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not set environment variables automatically: {ex.Message}");
        }
    }
}


