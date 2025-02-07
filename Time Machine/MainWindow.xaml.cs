using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using Windows.ApplicationModel;
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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // 创建文本框
            var textBox = new TextBox
            {
                PlaceholderText = "事件名称", // 灰色提示文字
                Margin = new Thickness(0, 0, 0, 10) // 添加底部间距
            };

            // 创建日期选择器
            var datePicker = new CalendarDatePicker
            {
                Date = DateTimeOffset.Now, // 默认日期为当前日期
                Margin = new Thickness(0, 0, 0, 10) // 与文本框保持一致的样式
            };

            // 将控件添加到 StackPanel
            var contentStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 24
            };
            contentStack.Children.Add(textBox);
            contentStack.Children.Add(datePicker);
            
            // 创建对话框
            var dialog = new ContentDialog
            {
                Title = "添加新事件",
                Content = contentStack,
                PrimaryButtonText = "保存 \\(^o^)/~",
                CloseButtonText = "取消 ￣へ￣",
                XamlRoot = this.Content.XamlRoot,
                DefaultButton = ContentDialogButton.Primary
            };

            // 禁用保存按钮初始状态
            dialog.IsPrimaryButtonEnabled = false;

            // 动态监听文本输入框内容变化
            textBox.TextChanged += (s, args) =>
            {
                dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(textBox.Text);
            };

            // 显示对话框并等待用户操作
            var result = await dialog.ShowAsync();

            // 处理用户操作
            if (result == ContentDialogResult.Primary)
            {
                string eventName = textBox.Text;
                DateTimeOffset? selectedDate = datePicker.Date;

                // 处理保存逻辑
                var message = $"事件名称: {eventName}\n日期: {selectedDate?.ToString("yyyy-MM-dd") ?? "未选择"}";
                var confirmationDialog = new ContentDialog
                {
                    Title = "已保存",
                    Content = message,
                    CloseButtonText = "确定",
                    XamlRoot = this.Content.XamlRoot
                };
                await confirmationDialog.ShowAsync();
            }
        }

        private async void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取系统信息
            string osVersion = Environment.OSVersion.ToString();
            string appVersion = GetAppVersion();
            string databasePath = GetDatabasePath();

            // 组合调试信息
            string debugInfo = $"系统信息: {osVersion}\n" +
                               $"软件版本: {appVersion}\n" +
                               $"数据库路径: {databasePath}";

            // 显示弹窗
            var dialog = new ContentDialog
            {
                Title = "调试信息",
                Content = debugInfo,
                CloseButtonText = "关闭",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        // 获取软件版本
        private string GetAppVersion()
        {
            var version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        // 获取数据库路径（假设数据库路径存储在文件里）
        private string GetDatabasePath()
        {
            string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string logFilePath = Path.Combine(downloadsFolder, "database_path.txt");

            if (File.Exists(logFilePath))
            {
                return File.ReadAllText(logFilePath);
            }
            return "数据库路径未找到";
        }
    }
}
