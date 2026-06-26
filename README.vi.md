# ThinkTool Add-in Manager

[English](README.md) | [Tiếng Việt](README.vi.md)

ThinkTool Add-in Manager là trình quản lý add-in WPF cho AutoCAD. Công cụ này giúp nạp và chạy các command .NET bên ngoài cũng như các hàm LISP ngay trong AutoCAD, thuận tiện cho quá trình phát triển mà không cần khởi động lại AutoCAD liên tục.

## Tính năng

- Hỗ trợ AutoCAD 2020 đến 2026.
- Nạp add-in .NET và liệt kê các command method.
- Nạp file LISP và lưu các mục đã dùng gần đây.
- Cung cấp command AutoCAD để mở trình quản lý và chạy lại command gần nhất.
- Có thể deploy debug bundle vào thư mục Autodesk ApplicationPlugins.

## Yêu cầu

- Windows.
- Visual Studio 2022 hoặc mới hơn.
- .NET SDK 8.0 cho bản build AutoCAD 2025 và 2026.
- .NET Framework 4.8 Developer Pack cho bản build AutoCAD 2020 đến 2024.
- Cài AutoCAD để kiểm thử runtime.

## Build

Restore package cho phiên bản AutoCAD bạn muốn build:

```powershell
dotnet restore ThinkToolAddinManager.sln --property:Configuration="Release A26"
```

Build phiên bản AutoCAD đó:

```powershell
dotnet build ThinkToolAddinManager.sln --configuration "Release A26"
```

Các configuration hiện có:

- `Debug A20` / `Release A20` cho AutoCAD 2020
- `Debug A21` / `Release A21` cho AutoCAD 2021
- `Debug A22` / `Release A22` cho AutoCAD 2022
- `Debug A23` / `Release A23` cho AutoCAD 2023
- `Debug A24` / `Release A24` cho AutoCAD 2024
- `Debug A25` / `Release A25` cho AutoCAD 2025
- `Debug A26` / `Release A26` cho AutoCAD 2026

## Cài đặt cho AutoCAD

Build configuration khớp với phiên bản AutoCAD của bạn, sau đó tạo thư mục bundle:

```text
%APPDATA%\Autodesk\ApplicationPlugins\ThinkToolAddinManager.bundle
```

Copy `src\ThinkToolAddinManager\PackageContents.xml` vào thư mục gốc của bundle. Copy output build vào thư mục phiên bản tương ứng trong bundle:

```text
ThinkToolAddinManager.bundle\
  PackageContents.xml
  26\
    ThinkToolAddinManager.dll
    ...
```

Mở AutoCAD và chạy:

```text
ThinkToolManager
```

Các command khác:

- `ThinkToolManagerRunLast`
- `InitThinkToolManager`
- `ThinkToolManagerDockPanel`

Để deploy debug trên máy local, build bằng lệnh:

```powershell
dotnet build ThinkToolAddinManager.sln --configuration "Debug A26" -p:DeployToAutoCAD=true
```

## Cấu trúc dự án

```text
src/ThinkToolAddinManager/
  Command/       Điểm vào command AutoCAD
  Model/         Nạp add-in, thiết lập, manifest, tiện ích
  View/          Giao diện WPF và control
  ViewModel/     Trạng thái UI và command
```

## Đóng góp

Xem [CONTRIBUTING.vi.md](CONTRIBUTING.vi.md). Bản tiếng Anh nằm ở [CONTRIBUTING.md](CONTRIBUTING.md).

## License

Dự án sử dụng MIT License. Xem [License.md](License.md).
