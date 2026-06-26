# ThinkTool Add-in Manager

[English](README.md) | [Tiếng Việt](README.vi.md)

ThinkTool Add-in Manager is a WPF add-in manager for AutoCAD. It helps load and run external .NET commands and LISP functions from inside AutoCAD without restarting the host application during day-to-day development.

## Features

- Supports AutoCAD 2020 through 2026.
- Loads .NET add-ins and lists command methods.
- Loads LISP files and keeps recently used entries.
- Provides AutoCAD commands for opening the manager and rerunning the last command.
- Can deploy a debug bundle to the Autodesk ApplicationPlugins folder.

## Requirements

- Windows.
- Visual Studio 2022 or newer.
- .NET SDK 8.0 for AutoCAD 2025 and 2026 builds.
- .NET Framework 4.8 Developer Pack for AutoCAD 2020 through 2024 builds.
- AutoCAD installed for runtime testing.

## Build

Restore packages for the AutoCAD version you want to build:

```powershell
dotnet restore ThinkToolAddinManager.sln --property:Configuration="Release A26"
```

Build that AutoCAD version:

```powershell
dotnet build ThinkToolAddinManager.sln --configuration "Release A26"
```

Available configurations:

- `Debug A20` / `Release A20` for AutoCAD 2020
- `Debug A21` / `Release A21` for AutoCAD 2021
- `Debug A22` / `Release A22` for AutoCAD 2022
- `Debug A23` / `Release A23` for AutoCAD 2023
- `Debug A24` / `Release A24` for AutoCAD 2024
- `Debug A25` / `Release A25` for AutoCAD 2025
- `Debug A26` / `Release A26` for AutoCAD 2026

## Install for AutoCAD

For the fastest install, download the latest `ThinkToolAddinManager-*-setup.exe` from [Releases](https://github.com/PYTHINKS/ThinkToolAddinManager/releases), then run it.

The installer copies `ThinkToolAddinManager.bundle` to:

```text
%APPDATA%\Autodesk\ApplicationPlugins
```

Then restart AutoCAD and run:

```text
ThinkToolManager
```

Manual zip install:

Download `ThinkToolAddinManager-*-bundle.zip`, extract it, and copy `ThinkToolAddinManager.bundle` to `%APPDATA%\Autodesk\ApplicationPlugins`.

Manual build/install:

Build the configuration that matches your AutoCAD version, then create this bundle folder:

```text
%APPDATA%\Autodesk\ApplicationPlugins\ThinkToolAddinManager.bundle
```

Copy `src\ThinkToolAddinManager\PackageContents.xml` into the bundle root. Copy the build output into the matching version folder inside the bundle:

```text
ThinkToolAddinManager.bundle\
  PackageContents.xml
  26\
    ThinkToolAddinManager.dll
    ...
```

Start AutoCAD and run:

```text
ThinkToolManager
```

Other commands:

- `ThinkToolManagerRunLast`
- `InitThinkToolManager`
- `ThinkToolManagerDockPanel`

For local debug deployment, build with:

```powershell
dotnet build ThinkToolAddinManager.sln --configuration "Debug A26" -p:DeployToAutoCAD=true
```

## Project Layout

```text
src/ThinkToolAddinManager/
  Command/       AutoCAD command entry points
  Model/         Add-in loading, settings, manifests, utilities
  View/          WPF views and controls
  ViewModel/     UI state and commands
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Vietnamese contributors can read [CONTRIBUTING.vi.md](CONTRIBUTING.vi.md).

## Release Packaging

Create a local bundle zip:

```powershell
.\tools\package-release.ps1 -Version v1.0.0
```

Create a local one-click installer:

```powershell
.\tools\build-installer.ps1 -Version v1.0.0
```

The zip is written to `artifacts/`. GitHub Releases are created automatically when a `v*` tag is pushed.

## License

This project is licensed under the MIT License. See [License.md](License.md).
