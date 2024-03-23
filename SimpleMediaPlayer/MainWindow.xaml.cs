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
    // Create a class that implements ICommand and accepts a delegate.
    public class SimpleDelegateCommand : ICommand
    {
        // Specify the keys and mouse actions that invoke the command. 
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
        GridLength gridRowOne;
        GridLength gridRowTwo;
        GridLength gridRowThree;
        string OpenedFilePath;
        public MainWindow()
        {
            InitializeComponent();
            Uri iconUri = new Uri("icons/icon.ico", UriKind.Relative);
            Icon = BitmapFrame.Create(iconUri);
            gridRowOne = content_Grid.RowDefinitions[1].Height;
            gridRowTwo = content_Grid.RowDefinitions[2].Height;
            gridRowThree = content_Grid.RowDefinitions[3].Height;
            
            //Timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.Tick += new EventHandler(timer_Tick);
            App.LanguageChanged += LanguageChanged;
            CultureInfo currLang = App.Language;

            //Footer
            footerInfoBarLocale.Text = "Localization: " + currLang.Name;
            footerInfoBarSpeed.Text = "Media speed: " + mainMedia.SpeedRatio.ToString();
            footerInfoBarVolume.Text = "Media volume: " + mainMedia.Volume.ToString();
            footerInfoBarName.Text = "Media file: None";

            //Заполняем меню смены языка:
            menuLanguage.Items.Clear();
            foreach (var lang in App.Languages)
            {
                MenuItem menuLang = new MenuItem();
                menuLang.Header = lang.DisplayName;
                menuLang.Tag = lang;
                menuLang.IsChecked = lang.Equals(currLang);
                var brush = new SolidColorBrush(Color.FromRgb(72,61,139));
                menuLang.Background = brush;
                menuLang.Click += ChangeLanguageClick;
                menuLanguage.Items.Add(menuLang);
            }
        }
        //После выбора локализации
        private void LanguageChanged(Object sender, EventArgs e)
        {
            CultureInfo currLang = App.Language;
            //Отмечаем нужный пункт смены языка как выбранный язык
            foreach (MenuItem i in menuLanguage.Items)
            {
                CultureInfo ci = (CultureInfo)i.Tag;
                i.IsChecked = ci != null && ci.Equals(currLang);
            }
        }
        //Выбор локализации
        private void ChangeLanguageClick(Object sender, EventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            if (mi != null)
            {
                CultureInfo lang = (CultureInfo)mi.Tag;
                if (lang != null) App.Language = lang;
                footerInfoBarLocale.Text = "Localization: " + lang?.ToString();
            }
        }
        //Открытие файла через диалоговое окно
        private void main_Menu_File_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "All Files|*.*|" +
                          "Based Video formats|*.mkv;*.webm;*.avi;*.ogv;*.gif;*.gifv;*.wmv;" +
                          "*.amv;*.mpg;*.mpeg;*.mp4;*.mpe;*.mpv;*.m2v;*.svi;*.mxf|" +
                          "Based Audio formats|*.aac;*.flac;*.dvf;*.m4a;*.m4b;*.mp3;*.mpc;*.ogg;" +
                          "*.oga;*.raw;*.voc;*.wav;*.wv;*.webm;*.mp4";
            if (open.ShowDialog() == true)
            {
                OpenedFilePath = open.FileName;
                mainMedia.Source = new Uri(OpenedFilePath);
            }
        }
        //Контекстное меню приложения
        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ContextMenu? cont = (ContextMenu)FindResource("contextMenu");
            cont.PlacementTarget = this; ;
            cont.IsOpen = true;

        }
        //Закрыть программу
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) {ExitVoid();}
        }
        //Закрыть приложение
        private void ExitVoid()
        {
            Application.Current.Shutdown();
        }

        private void Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            Mover.Width = 1600;
        }

        private void Drop_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void Normalized_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            Mover.Width = 600;
        }
        
        private void Header_Viev_Click(object sender, RoutedEventArgs e)
        {
            if (WindowStyle == WindowStyle.ToolWindow) {
                WindowStyle = WindowStyle.None;
                main_Close.Visibility = Visibility.Visible;
            }
            else if (WindowStyle == WindowStyle.None)
            {
                WindowStyle = WindowStyle.ToolWindow;
                main_Close.Visibility = Visibility.Collapsed;
            }
        }
        private void main_Menu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if(WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                Mover.Width = 600;
                DragMove();
            }
            else DragMove();
        }

        //-----------------------------------------------------------
        //Функции и методы медиа элемента и связанных с ним контролов
        //-----------------------------------------------------------

        //Ошибка при загрузке файла
        private void MediaElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show("...", "MediaFailed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
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
            mainMedia.Stop();
        }
        //Воспроизведение
        private void SMP_Play_Click(object sender, RoutedEventArgs e)
        {
            mainMedia.Play();
            EnableButtons(true);
            MinimizeControls();
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
        }
        //Переключение скорости воспроизведения
        private void MenuItem_Speed_Click(object sender, RoutedEventArgs e)
        {
            mainMedia.SpeedRatio = Convert.ToDouble(((MenuItem)sender).Header, new CultureInfo("en-us"));
            footerInfoBarSpeed.Text = "Media speed: " + mainMedia.SpeedRatio;
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
            if (mainMedia.Volume >= 0 && footerInfoBarVolume != null) { footerInfoBarVolume.Text = "Media volume: " + (Math.Round(mainMedia.Volume, 3) * 100).ToString() + "%"; }
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
                SMP_Stop.IsEnabled = false;
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
                    footerInfoBarVolume.Text = "Media volume: " + (Math.Round(mainMedia.Volume, 3) * 100).ToString() + "%";
                    break;
                case Key.Down:
                    volumeSlider.Value -= 0.05;
                    mainMedia.Volume = (double)volumeSlider.Value;
                    footerInfoBarVolume.Text = "Media volume: " + (Math.Round(mainMedia.Volume, 3) * 100).ToString() +"%";
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
                    if (mainMedia.GetMediaState() == MediaState.Pause) {
                        mainMedia.Play();
                        MinimizeControls();
                        EnableButtons(true);
                    }
                    else if(mainMedia.GetMediaState() == MediaState.Play) {
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
                default:
                    break;
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if(mainMedia.GetMediaState() == MediaState.Pause && (e.Key == Key.Left || e.Key == Key.Right))
            {
                mainMedia.Play();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            content_Grid.RowDefinitions[1].Height = new GridLength(0);
            content_Grid.RowDefinitions[2].Height = new GridLength(0);
            content_Grid.RowDefinitions[3].Height = new GridLength(0);
        }

        private void ShowControls()
        {
            if (content_Grid.RowDefinitions[1].Height == new GridLength(0))
            {
                content_Grid.RowDefinitions[1].Height = gridRowOne;
                content_Grid.RowDefinitions[2].Height = gridRowTwo;
                content_Grid.RowDefinitions[3].Height = gridRowThree;

            }
        }

        private void MinimizeControls()
        {
            if (content_Grid.RowDefinitions[1].Height != new GridLength(0))
            {
                content_Grid.RowDefinitions[1].Height = new GridLength(0);
                content_Grid.RowDefinitions[2].Height = new GridLength(0);
                content_Grid.RowDefinitions[3].Height = new GridLength(0);
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

        private void mediaPlayStopButton_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show("Error: image failed.", "Image Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void mediaPlayStopButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mediaPlayStopButton.Visibility = Visibility.Collapsed;
            mainMedia.Play();
            MinimizeControls();
        }
    }
}