using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleMediaPlayer
{
    public partial class MainWindow : Window
    {
        //Функции локализации
        //После выбора локализации
        private void LanguageChanged(Object sender, EventArgs e)
        {
            CultureInfo currLang = App.Language;
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
    }
}
