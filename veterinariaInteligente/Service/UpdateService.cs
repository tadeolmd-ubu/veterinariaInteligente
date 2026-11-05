using System.Diagnostics;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace veterinariaInteligente.Service;

internal sealed record ActualizacionDisponible(Version Version, string UrlDescarga);

internal static class UpdateService
{
    private static readonly HttpClient Client = CrearCliente();

    private static HttpClient CrearCliente()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("VeterinariaInteligente-Updater");
        return client;
    }

    public static async Task<ActualizacionDisponible?> BuscarAsync()
    {
        if (!OperatingSystem.IsWindows()) return null;
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var response = await Client.GetAsync(
            "https://api.github.com/repos/tadeolmd-ubu/veterinariaInteligente/releases/latest",
            timeout.Token);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(timeout.Token);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: timeout.Token);
        var root = document.RootElement;
        var tag = root.GetProperty("tag_name").GetString();
        if (!Version.TryParse(tag?.TrimStart('v'), out var version)) return null;
        var normalized = new Version(version.Major, version.Minor,
            Math.Max(version.Build, 0), Math.Max(version.Revision, 0));
        var current = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
        if (normalized <= current) return null;
        foreach (var asset in root.GetProperty("assets").EnumerateArray())
        {
            if (asset.GetProperty("name").GetString() != $"veterinariaInteligente-{version}-win-x64.zip")
                continue;
            var url = asset.GetProperty("browser_download_url").GetString();
            if (url is not null) return new ActualizacionDisponible(normalized, url);
        }
        return null;
    }

    // Returns only after the helper has started; the caller then shuts down Avalonia.
    public static async Task InstalarAsync(ActualizacionDisponible actualizacion, IProgress<string> progress)
    {
        if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
        var destination = AppContext.BaseDirectory;
        var temp = Path.Combine(Path.GetTempPath(), "VeterinariaUpdate-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        var helperStarted = false;
        try
        {
            // Check write access before closing the running application.
            var probe = Path.Combine(destination, Guid.NewGuid() + ".tmp");
            await File.WriteAllTextAsync(probe, "");
            File.Delete(probe);
            progress.Report("Descargando actualización…");
            var zip = Path.Combine(temp, "update.zip");
            using (var response = await Client.GetAsync(actualizacion.UrlDescarga, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                await using var input = await response.Content.ReadAsStreamAsync();
                await using var output = File.Create(zip);
                await input.CopyToAsync(output);
            }
            progress.Report("Preparando instalación…");
            var source = Path.Combine(temp, "files");
            await Task.Run(() => ZipFile.ExtractToDirectory(zip, source));
            if (!File.Exists(Path.Combine(source, "veterinariaInteligente.exe")))
                throw new InvalidDataException("El paquete no contiene el ejecutable.");

            var script = Path.Combine(temp, "update.ps1");
            await File.WriteAllTextAsync(script, """
                param([int]$AppProcessId, [string]$Source, [string]$Destination)
                $ErrorActionPreference = 'Stop'
                $robocopyExitCode = 'No ejecutado'
                $robocopyLog = Join-Path $PSScriptRoot 'robocopy.log'
                try {
                    Wait-Process -Id $AppProcessId -ErrorAction SilentlyContinue
                    & robocopy $Source $Destination /E /IS /R:10 /W:2 /XF *.db *.db-wal *.db-shm "/LOG:$robocopyLog"
                    $robocopyExitCode = $LASTEXITCODE
                    if ($robocopyExitCode -ge 8) {
                        throw "Robocopy falló con código $robocopyExitCode."
                    }
                    Start-Process -FilePath (Join-Path $Destination 'veterinariaInteligente.exe') -WorkingDirectory $Destination
                    Remove-Item $PSScriptRoot -Recurse -Force -ErrorAction SilentlyContinue
                } catch {
                    @(
                        "Origen: $Source"
                        "Destino: $Destination"
                        "Código de Robocopy: $robocopyExitCode"
                        ''
                        ($_ | Out-String)
                    ) | Set-Content -Path (Join-Path $PSScriptRoot 'error.log')
                    Add-Type -AssemblyName PresentationFramework
                    [System.Windows.MessageBox]::Show("No se pudo instalar la actualización. Revisa error.log y robocopy.log en:`n$PSScriptRoot", 'Error de actualización')
                }
                """);
            var start = new ProcessStartInfo
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),
                    "WindowsPowerShell", "v1.0", "powershell.exe"),
                UseShellExecute = false,
                CreateNoWindow = true
            };
            foreach (var arg in new[] { "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", script,
                         "-AppProcessId", Environment.ProcessId.ToString(), "-Source", source, "-Destination", destination })
                start.ArgumentList.Add(arg);
            using var helper = Process.Start(start) ?? throw new IOException("No se pudo iniciar el actualizador.");
            helperStarted = true;
        }
        finally
        {
            if (!helperStarted) Directory.Delete(temp, true);
        }
    }
}
