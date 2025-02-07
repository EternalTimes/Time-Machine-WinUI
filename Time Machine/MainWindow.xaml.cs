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

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            // �����ı���
            var textBox = new TextBox
            {
                PlaceholderText = "�¼�����", // ��ɫ��ʾ����
                Margin = new Thickness(0, 0, 0, 10) // ��ӵײ����
            };

            // ��������ѡ����
            var datePicker = new CalendarDatePicker
            {
                Date = DateTimeOffset.Now, // Ĭ������Ϊ��ǰ����
                Margin = new Thickness(0, 0, 0, 10) // ���ı��򱣳�һ�µ���ʽ
            };

            // ���ؼ���ӵ� StackPanel
            var contentStack = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Spacing = 24
            };
            contentStack.Children.Add(textBox);
            contentStack.Children.Add(datePicker);
            
            // �����Ի���
            var dialog = new ContentDialog
            {
                Title = "������¼�",
                Content = contentStack,
                PrimaryButtonText = "���� \\(^o^)/~",
                CloseButtonText = "ȡ�� ���أ�",
                XamlRoot = this.Content.XamlRoot,
                DefaultButton = ContentDialogButton.Primary
            };

            // ���ñ��水ť��ʼ״̬
            dialog.IsPrimaryButtonEnabled = false;

            // ��̬�����ı���������ݱ仯
            textBox.TextChanged += (s, args) =>
            {
                dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(textBox.Text);
            };

            // ��ʾ�Ի��򲢵ȴ��û�����
            var result = await dialog.ShowAsync();

            // �����û�����
            if (result == ContentDialogResult.Primary)
            {
                string eventName = textBox.Text;
                DateTimeOffset? selectedDate = datePicker.Date;

                // �������߼�
                var message = $"�¼�����: {eventName}\n����: {selectedDate?.ToString("yyyy-MM-dd") ?? "δѡ��"}";
                var confirmationDialog = new ContentDialog
                {
                    Title = "�ѱ���",
                    Content = message,
                    CloseButtonText = "ȷ��",
                    XamlRoot = this.Content.XamlRoot
                };
                await confirmationDialog.ShowAsync();
            }
        }

        private async void DebugButton_Click(object sender, RoutedEventArgs e)
        {
            // ��ȡϵͳ��Ϣ
            string osVersion = Environment.OSVersion.ToString();
            string appVersion = GetAppVersion();
            string databasePath = GetDatabasePath();

            // ��ϵ�����Ϣ
            string debugInfo = $"ϵͳ��Ϣ: {osVersion}\n" +
                               $"����汾: {appVersion}\n" +
                               $"���ݿ�·��: {databasePath}";

            // ��ʾ����
            var dialog = new ContentDialog
            {
                Title = "������Ϣ",
                Content = debugInfo,
                CloseButtonText = "�ر�",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        // ��ȡ����汾
        private string GetAppVersion()
        {
            var version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        // ��ȡ���ݿ�·�����������ݿ�·���洢���ļ��
        private string GetDatabasePath()
        {
            string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string logFilePath = Path.Combine(downloadsFolder, "database_path.txt");

            if (File.Exists(logFilePath))
            {
                return File.ReadAllText(logFilePath);
            }
            return "���ݿ�·��δ�ҵ�";
        }
    }
}
