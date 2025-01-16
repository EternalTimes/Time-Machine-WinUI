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

            // �������� Acrylic ����
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

                // ���ó�ʼ����
                _backdropConfiguration.IsInputActive = true;
                SetConfigurationSourceTheme();

                // �������� backdrop ����
                _acrylicController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                _acrylicController.SetSystemBackdropConfiguration(_backdropConfiguration);

                // �¼�������̬��������
                if (this.Content is FrameworkElement rootElement)
                {
                    rootElement.ActualThemeChanged += (s, e) => SetConfigurationSourceTheme();
                }
            }
            else
            {
                // ��ǰϵͳ��֧�� Acrylic ����
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
            // �� XAML �ж���� CustomTitleBar ��Ϊ������
            this.SetTitleBar(CustomTitleBar);
        }

        private void ApplyAcrylicToTitleBar()
        {
            // ���� Acrylic ����ˢ
            var acrylicBrush = new AcrylicBrush
            {
                TintColor = Microsoft.UI.Colors.Gray, // �Զ�����ɫ
                TintOpacity = 0.8, // ����͸����
                FallbackColor = Microsoft.UI.Colors.LightGray // ������ɫ
            };

            // Ӧ�õ��Զ��������
            CustomTitleBar.Background = acrylicBrush;
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            // ������Դ
            _acrylicController?.Dispose();
            _acrylicController = null;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // �ڱ�������ť���ʱִ�е��߼�
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
