# MemoryCleaner

Avalonia 桌面内存优化小工具，提供悬浮球、托盘图标、内存占用显示和一键清理。

## 开发运行

```powershell
dotnet run --project .\MemoryCleaner\MemoryCleaner.csproj
```

## 构建验证

```powershell
dotnet build .\MemoryCleaner.slnx
```

## Release 发布

项目目标框架为 `.NET 10`，Release 默认发布为 `win-x64` 自包含单文件主程序。

```powershell
dotnet publish .\MemoryCleaner\MemoryCleaner.csproj -c Release -r win-x64
```

发布目录：

```text
MemoryCleaner\bin\Release\net10.0-windows\win-x64\publish
```

主程序：

```text
MemoryCleaner.exe
```

Avalonia 原生运行库会随发布目录一起生成：

```text
av_libglesv2.dll
libHarfBuzzSharp.dll
libSkiaSharp.dll
```

## MSI 打包

MSI 打包流程参考 `InputBridge`，使用 WiX `5.0.2`。脚本会自动检查并安装 WiX UI 扩展。

```powershell
powershell -ExecutionPolicy Bypass -File .\MemoryCleaner\scripts\build-msi.ps1
```

生成文件：

```text
artifacts\MemoryCleaner-<版本号>-x64.msi
```

如果已经执行过 `dotnet publish`，只想重新打包 MSI：

```powershell
powershell -ExecutionPolicy Bypass -File .\MemoryCleaner\scripts\build-msi.ps1 -SkipPublish
```

## 版本号

MSI 文件名和安装包版本读取自：

```xml
<Version>1.0.0</Version>
```

位置：

```text
MemoryCleaner\MemoryCleaner.csproj
```
