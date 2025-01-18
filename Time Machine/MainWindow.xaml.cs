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

            // �������� Acrylic ����
            TrySetAcrylicBackdrop();

            // ��ʼ�����ݷ���
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
            switch (result)
            {
                // �û�ȷ�ϱ���
                case ContentDialogResult.Primary:
                    {
                        string eventName = textBox.Text; // ��ȡ�¼�����
                        DateTimeOffset? selectedDate = datePicker.Date; // ��ȡ����

                        // ��������Ƿ���Ч
                        if (string.IsNullOrWhiteSpace(eventName) || !selectedDate.HasValue)
                        {
                            // ���������Ч����ʾ������ʾ
                            var errorDialog = new ContentDialog
                            {
                                Title = "����",
                                Content = "����д�������¼����ƺ�ѡ�����ڡ�",
                                CloseButtonText = "ȷ��",
                                XamlRoot = this.Content.XamlRoot
                            };
                            await errorDialog.ShowAsync();
                            break;
                        }

                        // ��ʾ����ȷ��
                        var confirmationDialog = new ContentDialog
                        {
                            Title = "ȷ�ϱ���",
                            Content = $"�¼�����: {eventName}\n����: {selectedDate.Value:yyyy-MM-dd}",
                            PrimaryButtonText = "ȷ��",
                            CloseButtonText = "ȡ��",
                            XamlRoot = this.Content.XamlRoot
                        };

                        var confirmationResult = await confirmationDialog.ShowAsync();

                        if (confirmationResult == ContentDialogResult.Primary)
                        {
                            // �û�ȷ�ϱ��棬ִ�б����߼�
                            _dataService.SaveData($"�¼�����: {eventName}, ����: {selectedDate.Value:yyyy-MM-dd}");

                            // ��ʾ����ɹ���ʾ
                            var successDialog = new ContentDialog
                            {
                                Title = "����ɹ�",
                                Content = "�¼��ѳɹ����棡",
                                CloseButtonText = "ȷ��",
                                XamlRoot = this.Content.XamlRoot
                            };
                            await successDialog.ShowAsync();
                        }
                        else
                        {
                            // �û�ȡ�����棬�ص��༭״̬
                            var cancelDialog = new ContentDialog
                            {
                                Title = "����ȡ��",
                                Content = "����ȡ�����棬���Լ����༭��",
                                CloseButtonText = "ȷ��",
                                XamlRoot = this.Content.XamlRoot
                            };
                            await cancelDialog.ShowAsync();
                        }
                        break;
                    }

                // �û�ѡ��ȡ������
                case ContentDialogResult.Secondary:
                    {
                        var cancelDialog = new ContentDialog
                        {
                            Title = "������ȡ��",
                            Content = "����ȡ��������δ�����κ����ݡ�",
                            CloseButtonText = "ȷ��",
                            XamlRoot = this.Content.XamlRoot
                        };
                        await cancelDialog.ShowAsync();
                        break;
                    }

                // ����δ֪���
                default:
                    {
                        var unknownDialog = new ContentDialog
                        {
                            Title = "δ֪����",
                            Content = "����δ֪����δ�����κβ�����",
                            CloseButtonText = "ȷ��",
                            XamlRoot = this.Content.XamlRoot
                        };
                        await unknownDialog.ShowAsync();
                        break;
                    }
            }
        }
    }
}