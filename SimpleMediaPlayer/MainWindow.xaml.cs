using Microsoft.Win32;
using SimpleMediaPlayer.Extensions;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SimpleMediaPlayer
{
    public class SimpleDelegateCommand : ICommand
    {
        public Key GestureKey { get; set; }
        public ModifierKeys GestureModifier { get; set; }
        public MouseAction MouseGesture { get; set; }

        Action<object> _executeDelegate;

        public SimpleDelegateCommand(Action<object> executeDelegate)
        {
            _executeDelegate = executeDelegate;
        }

        public void Execute(object parameter)
        {
            _executeDelegate(parameter);
        }

        public bool CanExecute(object parameter) { return true; }
        public event EventHandler CanExecuteChanged;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer;
        GridLength gridNullLength = new GridLength(0);
        GridLength gridRowNull;
        GridLength gridRowOne;
        GridLength gridRowTwo;
        GridLength gridRowThree;
        string OpenedFilePath;

        public MainWindow()
        {
            InitializeComponent();
            Uri iconUri = new Uri("icons/icon.ico", UriKind.Relative);
            Icon = BitmapFrame.Create(iconUri);
            //Сохраняем и задаем изначальные данные
            gridRowNull = content_Grid.RowDefinitions[0].Height;
            gridRowOne = content_Grid.RowDefinitions[2].Height;
            gridRowTwo = content_Grid.RowDefinitions[3].Height;
            gridRowThree = content_Grid.RowDefinitions[4].Height;
            mediaPlayStopButton.Visibility = Visibility.Collapsed;
            SMP_Pause.IsEnabled = false;
            SMP_Play.IsEnabled = true;
            SMP_Stop.IsEnabled = false;
            //Создаем таймер
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += new EventHandler(timer_Tick);
            //Работа с локализацией
            App.LanguageChanged += LanguageChanged;
            CultureInfo currLang = App.Language;
            menuLanguage.Items.Clear();
            foreach (var lang in App.Languages)
            {
                MenuItem menuLang = new MenuItem();
                menuLang.Header = lang.DisplayName;
                menuLang.Tag = lang;
                menuLang.IsChecked = lang.Equals(currLang);
                var brush = new SolidColorBrush(Color.FromRgb(72, 61, 139));
                menuLang.Background = brush;
                menuLang.Click += ChangeLanguageClick;
                menuLanguage.Items.Add(menuLang);
            }
            //Задаем информацию в подвале
            footerInfoBarLocale.Text = "Localization: " + currLang.Name;
            footerInfoBarSpeed.Text = "Media speed: " + mainMedia.SpeedRatio.ToString();
            footerInfoBarVolume.Text = "Media volume: " + mainMedia.Volume.ToString();
            footerInfoBarName.Text = "Media file: None";
        }

        private void mediaPlayStopButton_ImageFailed(object sender, ExceptionRoutedEventArgs e) => MessageBox.Show("Error: image failed.", "Image Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e) => MessageBox.Show("...", "MediaFailed", MessageBoxButton.OK, MessageBoxImage.Error);
        //При открытии медиа
        private void MediaElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            footerInfoBarName.Text = "Media file: " + OpenedFilePath;
            TimeSpan ts = mainMedia.NaturalDuration.TimeSpan;
            timelineSlider.Minimum = 0;
            timelineSlider.Maximum = ts.TotalSeconds;
            timeEnd.Text = ts.ToString();
            timer.Start();
        }
        //При завершении медиа
        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            ShowControls();
            mainMedia.Stop();
            EnableButtons(false);

        }
        //Воспроизведение
        private void SMP_Play_Click(object sender, RoutedEventArgs e)
        {
            mainMedia.Play();
            EnableButtons(true);
            MinimizeControls();
            if (mediaPlayStopButton.Visibility == Visibility.Visible) mediaPlayStopButton.Visibility = Visibility.Collapsed;
        }
        // Пауза
        private void SMP_Pause_Click(object sender, RoutedEventArgs e)
        {
            mainMedia.Pause();
            ShowControls();
            EnableButtons(false);
        }
        //Остановить медиа
        private void SMP_Stop_Click(object sender, RoutedEventArgs e)
        {
            mainMedia.Stop();
            EnableButtons(false);
            timelineSlider.Value = 0;
        }
        //Таймлайн
        private void timelineSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mainMedia.Position = TimeSpan.FromSeconds(timelineSlider.Value);
            timeLost.Text = mainMedia.Position.ToString(@"hh\:mm\:ss");
        }
        //Таймер 
        void timer_Tick(object sender, EventArgs e)
        {
            timelineSlider.Value = mainMedia.Position.TotalSeconds;
            timeLost.Text = mainMedia.Position.ToString(@"hh\:mm\:ss");
        }
        //Настройка громкости
        private void ChangeMediaVolume(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            mainMedia.Volume = (double)volumeSlider.Value;
            if (mainMedia.Volume >= 0 && footerInfoBarVolume != null) { footerInfoBarVolume.Text = "Media volume: " + (Convert.ToInt32(mainMedia.Volume * 100)).ToString() + "%"; }
        }
        //Управление контролами воспроизведения
        private void EnableButtons(bool is_playing)
        {
            if (is_playing)
            {
                SMP_Play.IsEnabled = false;
                SMP_Pause.IsEnabled = true;
                SMP_Stop.IsEnabled = true;
                SMP_Play.Opacity = 0.5;
                SMP_Pause.Opacity = 1.0;
            }
            else
            {
                SMP_Play.IsEnabled = true;
                SMP_Pause.IsEnabled = false;
                SMP_Stop.IsEnabled = true;
                SMP_Play.Opacity = 1.0;
                SMP_Pause.Opacity = 0.5;
            }
            timer.IsEnabled = is_playing;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (mainMedia.GetMediaState() == MediaState.Play && (e.Key == Key.Left || e.Key == Key.Right))
            {
                mainMedia.Pause();
            }
            switch (e.Key)
            {
                case Key.Up:
                    volumeSlider.Value += 0.05;
                    mainMedia.Volume = (double)volumeSlider.Value;
                    footerInfoBarVolume.Text = "Media volume: " + Convert.ToInt32(mainMedia.Volume * 100).ToString() + "%";
                    Volume_Vizor.Text = mainMedia.Volume + "%";
                    Volume_Vizor.Visibility = Visibility.Visible;
                    //Task.Delay(15000);
                    //Volume_Vizor.Visibility = Visibility.Collapsed;
                    break;
                case Key.Down:
                    volumeSlider.Value -= 0.05;
                    mainMedia.Volume = (double)volumeSlider.Value;
                    footerInfoBarVolume.Text = "Media volume: " + (Convert.ToInt32(mainMedia.Volume * 100)).ToString() + "%";
                    Volume_Vizor.Text = mainMedia.Volume + "%";
                    Volume_Vizor.Visibility = Visibility.Visible;
                    //Task.Delay(15000);
                    //Volume_Vizor.Visibility = Visibility.Collapsed;
                    break;
                case Key.Left:
                    timelineSlider.Value -= 0.5;
                    mainMedia.Position = TimeSpan.FromSeconds(timelineSlider.Value);
                    break;
                case Key.Right:
                    timelineSlider.Value += 0.5;
                    mainMedia.Position = TimeSpan.FromSeconds(timelineSlider.Value);
                    break;
                case Key.Space:
                    if (mainMedia.GetMediaState() == MediaState.Pause)
                    {
                        mainMedia.Play();
                        MinimizeControls();
                        EnableButtons(true);
                    }
                    else if (mainMedia.GetMediaState() == MediaState.Play)
                    {
                        mainMedia.Pause();
                        ShowControls();
                        EnableButtons(false);
                    }
                    else if (mainMedia.GetMediaState() == MediaState.Manual)
                    {
                        mainMedia.Play();
                        EnableButtons(true);
                    }
                    break;
                case Key.Escape:
                    if (WindowState == WindowState.Maximized)
                    {
                        WindowState = WindowState.Normal;
                        content_Grid.RowDefinitions[0].Height = gridRowNull; //исправить развертку строк грида
                    }
                    break;
                default:
                    break;
            }
        }
        //Не помню что, но пусть будет
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (mainMedia.GetMediaState() == MediaState.Pause && (e.Key == Key.Left || e.Key == Key.Right))
            {
                mainMedia.Play();
                EnableButtons(true);
            }
        }
        //Показать контролы
        private void ShowControls()
        {
            if (content_Grid.RowDefinitions[2].Height == new GridLength(0))
            {
                content_Grid.RowDefinitions[2].Height = gridRowOne;
                content_Grid.RowDefinitions[3].Height = gridRowTwo;
                content_Grid.RowDefinitions[4].Height = gridRowThree;
            }
        }
        //Скрыть контролы
        private void MinimizeControls()
        {
            if (content_Grid.RowDefinitions[2].Height != gridNullLength)
            {
                content_Grid.RowDefinitions[2].Height = gridNullLength;
                content_Grid.RowDefinitions[3].Height = gridNullLength;
                content_Grid.RowDefinitions[4].Height = gridNullLength;
            }
        }

        private void RowDefinition_MouseMove(object sender, MouseEventArgs e)
        {
            /*            if (mainMedia.GetMediaState() == MediaState.Play)
                        {
                            ShowControls();
                            Task.Delay(3000);
                            Point position = e.GetPosition(this);
                            if (Math.Abs(position.X - startPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                Math.Abs(position.Y - startPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                            {
                                ShowControls();
                                Task.Delay(3000);
                            }
                        }*/
        }
        //Кнопка воспроизведения на медиа элементе

        private void mediaPlayStopButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mediaPlayStopButton.Visibility = Visibility.Collapsed;
            mainMedia.Play();
            EnableButtons(true);
            MinimizeControls();
        }
        //показ первого фрейма видео
        private void mainMedia_Loaded(object sender, RoutedEventArgs e)
        {
            mainMedia.Volume = 0;
            mainMedia.Play();
            mainMedia.Pause();
            mainMedia.Volume = 0.45;
        }
    }
}