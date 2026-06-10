using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MemoryCleaner.Services;
using System;
using System.IO;
using System.Reflection;

namespace MemoryCleaner;

public partial class App : Application
{
    private MainWindow? _mainWindow;
    private TrayIcon? _trayIcon;
    private readonly StartupService _startupService = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _mainWindow = new MainWindow();
            desktop.MainWindow = _mainWindow;
            _trayIcon = CreateTrayIcon(desktop);
            desktop.Exit += (_, _) => _trayIcon?.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private TrayIcon CreateTrayIcon(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var showItem = new NativeMenuItem("显示/隐藏悬浮球");
        showItem.Click += (_, _) => _mainWindow?.ToggleVisibility();

        var cleanItem = new NativeMenuItem("一键清理内存");
        cleanItem.Click += async (_, _) =>
        {
            if (_mainWindow is null)
            {
                return;
            }

            if (!_mainWindow.IsVisible)
            {
                _mainWindow.Show();
            }

            await _mainWindow.CleanAsync();
        };

        var startupItem = new NativeMenuItem("开机启动")
        {
            ToggleType = MenuItemToggleType.CheckBox,
            IsChecked = _startupService.IsEnabled()
        };
        startupItem.Click += (_, _) =>
        {
            var enabled = !_startupService.IsEnabled();
            _startupService.SetEnabled(enabled);
            startupItem.IsChecked = enabled;
        };

        var exitItem = new NativeMenuItem("退出");
        exitItem.Click += (_, _) =>
        {
            if (_trayIcon is not null)
            {
                _trayIcon.IsVisible = false;
            }

            desktop.Shutdown();
        };

        var menu = new NativeMenu
        {
            Items =
            {
                showItem,
                cleanItem,
                startupItem,
                new NativeMenuItemSeparator(),
                exitItem
            }
        };

        var trayIcon = new TrayIcon
        {
            Icon = LoadTrayIcon(),
            ToolTipText = "MemoryCleaner",
            Menu = menu,
            IsVisible = true
        };

        trayIcon.Clicked += (_, _) => _mainWindow?.ToggleVisibility();
        return trayIcon;
    }

    private static WindowIcon LoadTrayIcon()
    {
        const string resourceName = "MemoryCleaner.Assets.tray.ico";
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream is not null)
        {
            return new WindowIcon(stream);
        }

        return new WindowIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "tray.ico"));
    }
}
