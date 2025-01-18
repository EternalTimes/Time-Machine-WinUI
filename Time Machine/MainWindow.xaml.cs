using DataLayer;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using WinRT;
using WinRT.Interop;
using System.Diagnostics;

namespace Time_Machine
{
    public sealed partial class MainWindow : Window
    {
        private DesktopAcrylicController _acrylicController;
        private SystemBackdropConfiguration _backdropConfiguration;
        private Microsoft.UI.Windowing.AppWindow m_AppWindow;
        private readonly IDataService _dataService;

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

            // 初始化数据服务
            var (key, iv) = KeyManagementService.GetOrGenerateKeyAndIVAsync().Result;
            string databasePath = "SecureData.db";
            _dataService = new DataService(databasePath, key, iv);
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
            switch (result)
            {
                // 用户确认保存
                case ContentDialogResult.Primary:
                    {
                        string eventName = textBox.Text; // 获取事件名称
                        DateTimeOffset? selectedDate = datePicker.Date; // 获取日期

                        // 检查输入是否有效
                        if (string.IsNullOrWhiteSpace(eventName) || !selectedDate.HasValue)
                        {
                            // 如果输入无效，显示错误提示
                            var errorDialog = new ContentDialog
                            {
                                Title = "错误",
                                Content = "请填写完整的事件名称和选择日期。",
                                CloseButtonText = "确定",
                                XamlRoot = this.Content.XamlRoot
                            };
                            await errorDialog.ShowAsync();
                            break;
                        }

                        // 提示保存确认
                        var confirmationDialog = new ContentDialog
                        {
                            Title = "确认保存",
                            Content = $"事件名称: {eventName}\n日期: {selectedDate.Value:yyyy-MM-dd}",
                            PrimaryButtonText = "确认",
                            CloseButtonText = "取消",
                            XamlRoot = this.Content.XamlRoot
                        };

                        var confirmationResult = await confirmationDialog.ShowAsync();

                        if (confirmationResult == ContentDialogResult.Primary)
                        {
                            // 用户确认保存，执行保存逻辑
                            _dataService.SaveData($"事件名称: {eventName}, 日期: {selectedDate.Value:yyyy-MM-dd}");

                            // 显示保存成功提示
                            var successDialog = new ContentDialog
                            {
                                Title = "保存成功",
                                Content = "事件已成功保存！",
                                CloseButtonText = "确定",
                                XamlRoot = this.Content.XamlRoot
                            };
                            await successDialog.ShowAsync();
                        }
                        else
                        {
                            // 用户取消保存，回到编辑状态
                            var cancelDialog = new ContentDialog
                            {
                                Title = "保存取消",
                                Content = "您已取消保存，可以继续编辑。",
                                CloseButtonText = "确定",
                                XamlRoot = this.Content.XamlRoot
                            };
                            await cancelDialog.ShowAsync();
                        }
                        break;
                    }

                // 用户选择取消操作
                case ContentDialogResult.Secondary:
                    {
                        var cancelDialog = new ContentDialog
                        {
                            Title = "操作已取消",
                            Content = "您已取消操作，未保存任何数据。",
                            CloseButtonText = "确定",
                            XamlRoot = this.Content.XamlRoot
                        };
                        await cancelDialog.ShowAsync();
                        break;
                    }

                // 其他未知结果
                default:
                    {
                        var unknownDialog = new ContentDialog
                        {
                            Title = "未知操作",
                            Content = "发生未知错误，未进行任何操作。",
                            CloseButtonText = "确定",
                            XamlRoot = this.Content.XamlRoot
                        };
                        await unknownDialog.ShowAsync();
                        break;
                    }
            }
        }
    }
}