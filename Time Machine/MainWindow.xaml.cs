using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using WinRT;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Time_Machine
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class MainWindow : Window
    {
        private DesktopAcrylicController _acrylicController;
        private SystemBackdropConfiguration _backdropConfiguration;
        private Microsoft.UI.Windowing.AppWindow m_AppWindow;

        public MainWindow()
        {
            this.InitializeComponent();

            m_AppWindow = GetAppWindowForCurrentWindow();
            var titleBar = m_AppWindow.TitleBar;

            // Hide system title bar.
            titleBar.ExtendsContentIntoTitleBar = true;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            // 尝试设置 Acrylic 背景
            TrySetAcrylicBackdrop();

        }

        private Microsoft.UI.Windowing.AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return Microsoft.UI.Windowing.AppWindow.GetFromWindowId(wndId);
        }

        private void TrySetAcrylicBackdrop()
        {
            if (DesktopAcrylicController.IsSupported())
            {
                _acrylicController = new DesktopAcrylicController();
                _backdropConfiguration = new SystemBackdropConfiguration();

                // 设置初始配置
                _backdropConfiguration.IsInputActive = true;
                SetConfigurationSourceTheme();

                // 将窗口与 backdrop 关联
                _acrylicController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                _acrylicController.SetSystemBackdropConfiguration(_backdropConfiguration);

                // 事件处理，动态更新主题
                if (this.Content is FrameworkElement rootElement)
                {
                    rootElement.ActualThemeChanged += (s, e) => SetConfigurationSourceTheme();
                }
            }
            else
            {
                // 当前系统不支持 Acrylic 背景
                throw new NotSupportedException("Desktop Acrylic is not supported on this system.");
            }
        }

        private void SetConfigurationSourceTheme()
        {
            if (this.Content is FrameworkElement rootElement)
            {
                _backdropConfiguration.Theme = rootElement.ActualTheme switch
                {
                    ElementTheme.Dark => SystemBackdropTheme.Dark,
                    ElementTheme.Light => SystemBackdropTheme.Light,
                    _ => SystemBackdropTheme.Default,
                };
            }
        }

        private void SetCustomTitleBar()
        {
            // 将 XAML 中定义的 CustomTitleBar 作为标题栏
            this.SetTitleBar(CustomTitleBar);
        }

        private void ApplyAcrylicToTitleBar()
        {
            // 创建 Acrylic 背景刷
            var acrylicBrush = new AcrylicBrush
            {
                TintColor = Microsoft.UI.Colors.Gray, // 自定义颜色
                TintOpacity = 0.8, // 调整透明度
                FallbackColor = Microsoft.UI.Colors.LightGray // 回退颜色
            };

            // 应用到自定义标题栏
            CustomTitleBar.Background = acrylicBrush;
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            // 清理资源
            _acrylicController?.Dispose();
            _acrylicController = null;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // 在标题栏按钮点击时执行的逻辑
            ContentDialog dialog = new ContentDialog
            {
                Title = "Add Button Clicked",
                Content = "You clicked the Add button!",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };
            dialog.ShowAsync();
        }
    }
}
