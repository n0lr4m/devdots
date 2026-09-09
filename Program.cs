using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;

class Program
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".chainguard"
    );
    private static readonly string ConfigFile = Path.Combine(ConfigDir, "config.json");
    private static readonly string ProgramDataDir = @"C:\ProgramData\Chainguard";

    class Config
    {
        public string? Parent { get; set; }
        public string? Email { get; set; }
    }

    static void Main(string[] args)
    {
        if (args.Contains("--install-chainctl", StringComparer.OrdinalIgnoreCase))
        {
            InstallChainctl();
            return;
        }

        Console.WriteLine("=== Chainguard Libraries Dotfiles & chainctl Generator ===");
        Console.WriteLine("Tip: Run with '--install-chainctl' to install chainctl into C:\\ProgramData and update your user PATH.");

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

        if (string.IsNullOrWhiteSpace(config.Parent) || string.IsNullOrWhiteSpace(config.Email))
        {
            Console.WriteLine("\nFirst-run setup: Chainguard Libraries & account configuration.");
            Console.WriteLine("Opening Chainguard Console in your default browser to create an account/organization...");

            OpenUrl("https://console.chainguard.dev/");

            Console.WriteLine("\nPlease enter your Chainguard Organization / Parent identifier (e.g., your-org or example.com):");
            config.Parent = Console.ReadLine()?.Trim();

            Console.WriteLine("Please enter your Chainguard account Email:");
            config.Email = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(config.Parent) || string.IsNullOrWhiteSpace(config.Email))
            {
                Console.WriteLine("Error: Parent organization and Email cannot be empty.");
                return;
            }

            File.WriteAllText(ConfigFile, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Saved configuration to {ConfigFile}");
        }

        EnsureAuthenticatedWithChainctl();
        GenerateChainguardDotfiles(config.Parent);
        Console.WriteLine("\nAll Chainguard dotfiles and package manager configurations created successfully for Java, JavaScript, and Python!");
    }

    static void InstallChainctl()
    {
        Console.WriteLine("=== Installing chainctl to C:\\ProgramData ==-");
        try
        {
            Directory.CreateDirectory(ProgramDataDir);
            var chainctlPath = Path.Combine(ProgramDataDir, "chainctl.exe");

            var psScript = 
                $"`$ErrorActionPreference = 'Stop';\n" +
                $"Write-Host 'Fetching latest chainctl version metadata...';\n" +
                $"(`$versionInfo = (Invoke-RestMethod -Uri 'https://dl.enforce.dev/chainctl/latest/metadata.json'));\n" +
                $"`$version = `$versionInfo.version;\n" +
                $"Write-Host \"Downloading chainctl version `$version...\";\n" +
                $"Invoke-WebRequest -Uri \"https://dl.enforce.dev/chainctl/`$version/chainctl_windows_x86_64.exe\" -OutFile \"{chainctlPath}\";\n" +
                $"Write-Host 'chainctl downloaded successfully to {ProgramDataDir}!';";

            var startInfo = new ProcessStartInfo("powershell", $"-NoProfile -ExecutionPolicy Bypass -Command \"{psScript}\"")
            {
                CreateNoWindow = false,
                UseShellExecute = false
            };
            var proc = Process.Start(startInfo);
            proc?.WaitForExit();

            if (proc?.ExitCode == 0 && File.Exists(chainctlPath))
            {
                // Add C:\ProgramData to User PATH
                var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
                if (!userPath.Contains(ProgramDataDir, StringComparison.OrdinalIgnoreCase))
                {
                    var newPath = string.IsNullOrEmpty(userPath) ? ProgramDataDir : $"{userPath};{ProgramDataDir}";
                    Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.User);
                    Console.WriteLine($"[Success] Added {ProgramDataDir} to user PATH.");
                }
                else
                {
                    Console.WriteLine("[Notice] {ProgramDataDir} is already present in user PATH.");
                }
            }
            else
            {
                Console.WriteLine("Error: Failed to download chainctl.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during chainctl installation (administrative permissions may be required for C:\\ProgramData): {ex.Message}");
        }
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

    static void EnsureAuthenticatedWithChainctl()
    {
        var chainctlPath = Path.Combine(ProgramDataDir, "chainctl.exe");
        if (!File.Exists(chainctlPath))
        {
            Console.WriteLine("[Notice] chainctl not found in C:\\ProgramData. Run with '--install-chainctl' to install it.");
            return;
        }

        Console.WriteLine("\nAuthenticating with Chainguard via chainctl...");
        try
        {
            var startInfo = new ProcessStartInfo(chainctlPath, "auth login")
            {
                CreateNoWindow = false,
                UseShellExecute = false
            };
            var proc = Process.Start(startInfo);
            proc?.WaitForExit();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"chainctl login notice: {ex.Message}");
        }
    }

    static void GenerateChainguardDotfiles(string parent)
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var chainctlPath = Path.Combine(ProgramDataDir, "chainctl.exe");

        string[] ecosystems = { "java", "javascript", "python" };

        foreach (var eco in ecosystems)
        {
            Console.WriteLine($"\nGenerating pull token for ecosystem: {eco} (Parent: {parent})...");
            string username = "";
            string password = "";

            if (File.Exists(chainctlPath))
            {
                try
                {
                    var startInfo = new ProcessStartInfo(chainctlPath, $"auth pull-token --repository={eco} --parent={parent} --output=json")
                    {
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };
                    var proc = Process.Start(startInfo);
                    string output = proc?.StandardOutput.ReadToEnd() ?? "";
                    proc?.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        using var doc = JsonDocument.Parse(output);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("username", out var u)) username = u.GetString() ?? "";
                        if (root.TryGetProperty("password", out var p)) password = p.GetString() ?? "";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not automatically generate pull token via chainctl for {eco}: {ex.Message}");
                }
            }

            if (string.IsNullOrEmpty(username)) username = "chainguard-user";
            if (string.IsNullOrEmpty(password)) password = "placeholder-token";

            if (eco == "javascript")
            {
                var npmrcPath = Path.Combine(userProfile, ".npmrc");
                var npmrcContent = $"# Chainguard Libraries JavaScript Repository\n" +
                                   $"registry=https://libraries.chainguard.dev/javascript/\n" +
                                   $"//libraries.chainguard.dev/javascript/:username={username}\n" +
                                   $"//libraries.chainguard.dev/javascript/:_password={password}\n" +
                                   $"//libraries.chainguard.dev/javascript/:email=admin@chainguard.dev\n" +
                                   $"//libraries.chainguard.dev/javascript/:always-auth=true\n";
                File.WriteAllText(npmrcPath, npmrcContent);
                Console.WriteLine($"[Created] JavaScript (.npmrc): {npmrcPath}");
            }
            else if (eco == "python")
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var pipDir = Path.Combine(appData, "pip");
                Directory.CreateDirectory(pipDir);
                var pipIniPath = Path.Combine(pipDir, "pip.ini");
                var pipContent = "[global]\n" +
                                 "index-url = https://libraries.chainguard.dev/python/simple/\n" +
                                 $"extra-index-url = https://{username}:{password}@libraries.chainguard.dev/python/simple/\n";
                File.WriteAllText(pipIniPath, pipIniPath == "" ? "" : pipContent);
                File.WriteAllText(pipIniPath, pipContent);
                Console.WriteLine($"[Created] Python (pip.ini): {pipIniPath}");
            }
            else if (eco == "java")
            {
                var m2Dir = Path.Combine(userProfile, ".m2");
                Directory.CreateDirectory(m2Dir);
                var settingsPath = Path.Combine(m2Dir, "settings.xml");
                var settingsContent = $@"<settings>
  <servers>
    <server>
      <id>chainguard-java</id>
      <username>{username}</username>
      <password>{password}</password>
    </server>
  </servers>
  <profiles>
    <profile>
      <id>chainguard-java</id>
      <repositories>
        <repository>
          <id>chainguard-java</id>
          <url>https://libraries.chainguard.dev/java</url>
          <releases><enabled>true</enabled></releases>
          <snapshots><enabled>false</enabled></snapshots>
        </repository>
      </repositories>
    </profile>
  </profiles>
  <activeProfiles>
    <activeProfile>chainguard-java</activeProfile>
  </activeProfiles>
</settings>
";
                File.WriteAllText(settingsPath, settingsContent);
                Console.WriteLine($"[Created] Java Maven settings.xml: {settingsPath}");
            }
        }

        var envScriptPath = Path.Combine(ConfigDir, "set-env.ps1");
        var envScriptContent = $"[System.Environment]::SetEnvironmentVariable('CHAINGUARD_PARENT', '{parent}', [System.EnvironmentVariableTarget]::User)\n" +
                               $"Write-Host 'Chainguard environment variables and dotfiles successfully configured.'\n";
        File.WriteAllText(envScriptPath, envScriptContent);

        try
        {
            Process.Start(new ProcessStartInfo("powershell", $"-ExecutionPolicy Bypass -File \"{envScriptPath}\"") { CreateNoWindow = true })?.WaitForExit();
            Console.WriteLine("[Success] Chainguard user environment variables updated!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Notice setting env vars: {ex.Message}");
        }
    }
}



