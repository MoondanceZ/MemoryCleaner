using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using MemoryCleaner.Services;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MemoryCleaner;

public partial class MainWindow : Window
{
    private const int BallSize = 72;
    private const int GwlStyle = -16;
    private const int GwlExStyle = -20;
    private const int SwpNoMove = 0x0002;
    private const int SwpNoSize = 0x0001;
    private const int SwpNoZOrder = 0x0004;
    private const int SwpFrameChanged = 0x0020;
    private const int WsPopup = unchecked((int)0x80000000);
    private const int WsCaption = 0x00C00000;
    private const int WsThickFrame = 0x00040000;
    private const int WsMinimizeBox = 0x00020000;
    private const int WsMaximizeBox = 0x00010000;
    private const int WsSysMenu = 0x00080000;
    private const int WsBorder = 0x00800000;
    private const int WsDlgFrame = 0x00400000;
    private const int WsExDlgModalFrame = 0x00000001;
    private const int WsExClientEdge = 0x00000200;
    private const int WsExStaticEdge = 0x00020000;
    private const int WsExToolWindow = 0x00000080;
    private const int DwmwaNcRenderingPolicy = 2;
    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwaBorderColor = 34;
    private const int DwmwaSystemBackdropType = 38;
    private const int DwmncrpDisabled = 1;
    private const int DwmwcpDoNotRound = 1;
    private const int DwmwaColorNone = unchecked((int)0xFFFFFFFE);
    private const int DwmSystemBackdropNone = 1;
    private readonly DispatcherTimer _timer;
    private readonly DispatcherTimer _animationTimer;
    private readonly MemoryService _memoryService = new();
    private bool _isCleaning;
    private double _wavePhase;
    private double _rocketAnimationProgress = 1;

    public MainWindow()
    {
        InitializeComponent();

        Opened += (_, _) =>
        {
            PlaceNearBottomRight();
            RemoveWindowFrame();
        };

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (_, _) => RefreshMemoryUsage();
        _timer.Start();

        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(45)
        };
        _animationTimer.Tick += (_, _) =>
        {
            _wavePhase += 0.12;
            WaveBall.Phase = _wavePhase;
            UpdateRocketLaunchAnimation();
        };
        _animationTimer.Start();

        RefreshMemoryUsage();
    }

    public void ToggleVisibility()
    {
        if (IsVisible)
        {
            Hide();
        }
        else
        {
            Show();
            Activate();
        }
    }

    public async Task CleanAsync()
    {
        if (_isCleaning)
        {
            return;
        }

        _isCleaning = true;
        CleanButton.IsEnabled = false;
        RocketButtonIcon.Opacity = 0.38;
        StartRocketLaunchAnimation();

        try
        {
            var cleanedCount = await Task.Run(_memoryService.Clean);
            RefreshMemoryUsage();
            RocketButtonIcon.Opacity = cleanedCount > 0 ? 0.78 : 0.62;
            await Task.Delay(900);
        }
        finally
        {
            RocketButtonIcon.Opacity = 1;
            RocketButtonIcon.FlameProgress = 0;
            CleanButton.IsEnabled = true;
            _isCleaning = false;
        }
    }

    private void RefreshMemoryUsage()
    {
        try
        {
            var snapshot = _memoryService.GetSnapshot();
            var usedPercent = Math.Round(snapshot.UsedPercent);
            UsageText.Text = $"{usedPercent:0}%";
            WaveBall.Percent = usedPercent;
        }
        catch
        {
            UsageText.Text = "--%";
            WaveBall.Percent = 0;
        }
    }

    private async void CleanButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        await CleanAsync();
    }

    private void FloatingBall_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }

    private void PlaceNearBottomRight()
    {
        var screen = Screens.ScreenFromWindow(this) ?? Screens.Primary;
        if (screen is null)
        {
            return;
        }

        var area = screen.WorkingArea;
        Position = new Avalonia.PixelPoint(
            area.X + area.Width - BallSize - 24,
            area.Y + area.Height - BallSize - 48);
    }

    private void StartRocketLaunchAnimation()
    {
        _rocketAnimationProgress = 0;
        RocketLaunchIcon.Opacity = 1;
        RocketLaunchIcon.FlameProgress = 1;
        RocketButtonIcon.FlameProgress = 0.55;
        RocketLaunchIcon.RenderTransform = new TranslateTransform(0, 0);
    }

    private void UpdateRocketLaunchAnimation()
    {
        if (_rocketAnimationProgress >= 1)
        {
            return;
        }

        _rocketAnimationProgress = Math.Min(1, _rocketAnimationProgress + 0.08);
        var eased = 1 - Math.Pow(1 - _rocketAnimationProgress, 3);
        if (RocketLaunchIcon.RenderTransform is TranslateTransform transform)
        {
            transform.Y = -32 * eased;
        }
        RocketLaunchIcon.Opacity = 1 - eased;
        RocketLaunchIcon.FlameProgress = 1 - eased * 0.7;
        RocketButtonIcon.FlameProgress = Math.Max(0, 0.55 * (1 - eased));
        if (_rocketAnimationProgress >= 1)
        {
            RocketLaunchIcon.Opacity = 0;
            RocketLaunchIcon.FlameProgress = 0;
        }
    }

    private void RemoveWindowFrame()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var platformHandle = TryGetPlatformHandle();
        if (platformHandle?.Handle is not { } handle || handle == IntPtr.Zero)
        {
            return;
        }

        var style = GetWindowLongPtr(handle, GwlStyle).ToInt64();
        style &= ~(WsCaption | WsThickFrame | WsMinimizeBox | WsMaximizeBox | WsSysMenu | WsBorder | WsDlgFrame);
        style |= WsPopup;
        SetWindowLongPtr(handle, GwlStyle, new IntPtr(style));

        var exStyle = GetWindowLongPtr(handle, GwlExStyle).ToInt64();
        exStyle &= ~(WsExDlgModalFrame | WsExClientEdge | WsExStaticEdge);
        exStyle |= WsExToolWindow;
        SetWindowLongPtr(handle, GwlExStyle, new IntPtr(exStyle));

        var policy = DwmncrpDisabled;
        DwmSetWindowAttribute(handle, DwmwaNcRenderingPolicy, ref policy, sizeof(int));
        var cornerPreference = DwmwcpDoNotRound;
        DwmSetWindowAttribute(handle, DwmwaWindowCornerPreference, ref cornerPreference, sizeof(int));
        var borderColor = DwmwaColorNone;
        DwmSetWindowAttribute(handle, DwmwaBorderColor, ref borderColor, sizeof(int));
        var backdropType = DwmSystemBackdropNone;
        DwmSetWindowAttribute(handle, DwmwaSystemBackdropType, ref backdropType, sizeof(int));
        SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpNoZOrder | SwpFrameChanged);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hWnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
}
