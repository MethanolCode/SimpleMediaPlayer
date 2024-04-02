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
        //После выбора локализации
        private void LanguageChanged(object sender, EventArgs e)
        {
            CultureInfo currLang = App.Language;
            foreach (MenuItem i in menuLanguage.Items)
            {
                CultureInfo ci = (CultureInfo)i.Tag;
                i.IsChecked = ci != null && ci.Equals(currLang);
            }
        }
        //Выбор локализации
        private void ChangeLanguageClick(object sender, EventArgs e)
        {
            MenuItem mi_language = (MenuItem)sender;
            if (mi_language != null)
            {
                CultureInfo lang = (CultureInfo)mi_language.Tag;
                if (lang != null) App.Language = lang;
                footerInfoBarLocale.Text = "Localization: " + lang?.ToString();
            }
        }
    }
}
