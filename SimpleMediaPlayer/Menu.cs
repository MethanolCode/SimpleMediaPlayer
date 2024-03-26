using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleMediaPlayer
{
    public partial class MainWindow : Window
    {
        //Меню - основное и контекстное
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
            if (MessageBox.Show("Are you sure?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes) { ExitVoid(); }
        }
        //Закрыть приложение
        private void ExitVoid()
        {
            Application.Current.Shutdown();
        }

        private void Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
            content_Grid.RowDefinitions[0].Height = new GridLength(0);
            Mover.Width = 1600;
        }

        private void Drop_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void Normalized_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Normal;
            content_Grid.RowDefinitions[0].Height = gridRowNull;
            Mover.Width = 600;
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
                SMP_Pause.IsEnabled = false;
                SMP_Play.IsEnabled = true;
                SMP_Stop.IsEnabled = false;
                mediaPlayStopButton.Visibility = Visibility.Visible;
                OpenedFilePath = open.FileName;
                mainMedia.Source = new Uri(OpenedFilePath);
            }
        }
        //Показать/скрыть шапку программы
        private void menu_header_view_Click(object sender, RoutedEventArgs e)
        {
            if (WindowStyle == WindowStyle.ToolWindow)
            {
                WindowStyle = WindowStyle.None;
                main_Close.Visibility = Visibility.Visible;
            }
            else if (WindowStyle == WindowStyle.None)
            {
                WindowStyle = WindowStyle.ToolWindow;
                main_Close.Visibility = Visibility.Collapsed;
            }
        }
        //Перемещение окна
        private void menu_Mover_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                Mover.Width = 600;
                DragMove();
            }
            else DragMove();
        }
        //Переключение скорости воспроизведения
        private void MenuItem_Speed_Click(object sender, RoutedEventArgs e)
        {
            mainMedia.SpeedRatio = Convert.ToDouble(((MenuItem)sender).Header, new CultureInfo("en-us"));
            footerInfoBarSpeed.Text = "Media speed: " + mainMedia.SpeedRatio;
        }
        //Заполнение медиаэлемента контентом
        private void menu_view_stretch_fill_Click(object sender, RoutedEventArgs e)
        {
            if (mainMedia.Stretch != Stretch.Fill)
            {
                mainMedia.Stretch = Stretch.Fill;
            }
        }
        private void menu_view_stretch_uniform_Click(object sender, RoutedEventArgs e)
        {
            if (mainMedia.Stretch != Stretch.Uniform)
            {
                mainMedia.Stretch = Stretch.Uniform;
            }
        }

        private void menu_view_stretch_uniformtofill_Click(object sender, RoutedEventArgs e)
        {
            if (mainMedia.Stretch != Stretch.UniformToFill)
            {
                mainMedia.Stretch = Stretch.UniformToFill;
            }
        }

        private void menu_view_stretch_none_Click(object sender, RoutedEventArgs e)
        {
            if (mainMedia.Stretch != Stretch.None)
            {
                mainMedia.Stretch = Stretch.None;
            }
            }
    }
}
