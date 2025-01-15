using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;

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

        public MainWindow()
        {
            this.InitializeComponent();

            // 尝试设置 Acrylic 背景
            TrySetAcrylicBackdrop();
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

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            // 清理资源
            _acrylicController?.Dispose();
            _acrylicController = null;
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
        }
    }
}
