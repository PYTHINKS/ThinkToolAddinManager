param(
  [string]$Version = "dev"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$artifactsDir = Join-Path $root "artifacts"
$zipPath = Join-Path $artifactsDir "ThinkToolAddinManager-$Version-bundle.zip"
$installerSrc = Join-Path $artifactsDir "installer-src"
$installerOut = Join-Path $artifactsDir "installer"
$embeddedZip = Join-Path $installerSrc "ThinkToolAddinManager.bundle.zip"
$exePath = Join-Path $artifactsDir "ThinkToolAddinManager-$Version-setup.exe"

if (-not (Test-Path -LiteralPath $zipPath)) {
  & (Join-Path $PSScriptRoot "package-release.ps1") -Version $Version
}

Remove-Item -LiteralPath $installerSrc -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $installerOut -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $exePath -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $installerSrc | Out-Null

Copy-Item -LiteralPath $zipPath -Destination $embeddedZip

@'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>ThinkToolAddinManagerSetup</AssemblyName>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>
  </PropertyGroup>
  <ItemGroup>
    <EmbeddedResource Include="ThinkToolAddinManager.bundle.zip" LogicalName="ThinkToolAddinManager.bundle.zip" />
  </ItemGroup>
</Project>
'@ | Set-Content -LiteralPath (Join-Path $installerSrc "ThinkToolAddinManagerSetup.csproj") -Encoding UTF8

@'
using System.IO.Compression;
using System.Reflection;

const string bundleName = "ThinkToolAddinManager.bundle";

Console.Title = "ThinkTool Add-in Manager Setup";
Console.WriteLine("ThinkTool Add-in Manager Setup");
Console.WriteLine();

var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
if (string.IsNullOrWhiteSpace(appData))
{
    Fail("Could not find the current user's AppData folder.");
    return;
}

var pluginsDir = Path.Combine(appData, "Autodesk", "ApplicationPlugins");
var targetBundleDir = Path.Combine(pluginsDir, bundleName);
var tempRoot = Path.Combine(Path.GetTempPath(), "ThinkToolAddinManagerSetup-" + Guid.NewGuid().ToString("N"));
var zipPath = Path.Combine(tempRoot, "bundle.zip");
var extractDir = Path.Combine(tempRoot, "extract");

try
{
    Directory.CreateDirectory(tempRoot);
    Directory.CreateDirectory(extractDir);
    Directory.CreateDirectory(pluginsDir);

    using (var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream("ThinkToolAddinManager.bundle.zip"))
    {
        if (resource is null)
        {
            Fail("Installer payload was not found.");
            return;
        }

        using var file = File.Create(zipPath);
        resource.CopyTo(file);
    }

    ZipFile.ExtractToDirectory(zipPath, extractDir);
    var sourceBundleDir = Path.Combine(extractDir, bundleName);
    if (!Directory.Exists(sourceBundleDir))
    {
        Fail("The extracted bundle folder was not found.");
        return;
    }

    if (Directory.Exists(targetBundleDir))
    {
        Directory.Delete(targetBundleDir, recursive: true);
    }

    CopyDirectory(sourceBundleDir, targetBundleDir);

    Console.WriteLine("Installed successfully.");
    Console.WriteLine();
    Console.WriteLine("Installed to:");
    Console.WriteLine(targetBundleDir);
    Console.WriteLine();
    Console.WriteLine("Restart AutoCAD, then run command: ThinkToolManager");
    Console.WriteLine();
}
catch (Exception ex)
{
    Fail(ex.Message);
}
finally
{
    try
    {
        if (Directory.Exists(tempRoot))
        {
            Directory.Delete(tempRoot, recursive: true);
        }
    }
    catch
    {
    }
}

Console.WriteLine("Press Enter to close.");
Console.ReadLine();

static void CopyDirectory(string sourceDir, string targetDir)
{
    Directory.CreateDirectory(targetDir);

    foreach (var file in Directory.GetFiles(sourceDir))
    {
        var targetFile = Path.Combine(targetDir, Path.GetFileName(file));
        File.Copy(file, targetFile, overwrite: true);
    }

    foreach (var directory in Directory.GetDirectories(sourceDir))
    {
        var targetSubDir = Path.Combine(targetDir, Path.GetFileName(directory));
        CopyDirectory(directory, targetSubDir);
    }
}

static void Fail(string message)
{
    Console.Error.WriteLine("Install failed.");
    Console.Error.WriteLine(message);
    Console.WriteLine();
    Console.WriteLine("Press Enter to close.");
    Console.ReadLine();
}
'@ | Set-Content -LiteralPath (Join-Path $installerSrc "Program.cs") -Encoding UTF8

dotnet publish (Join-Path $installerSrc "ThinkToolAddinManagerSetup.csproj") `
  --configuration Release `
  --output $installerOut `
  /p:DebugType=None `
  /p:DebugSymbols=false

Copy-Item -LiteralPath (Join-Path $installerOut "ThinkToolAddinManagerSetup.exe") -Destination $exePath

Write-Host "Created $exePath"
