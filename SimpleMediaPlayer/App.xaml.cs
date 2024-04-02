using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;

namespace SimpleMediaPlayer
{
    public partial class App : Application
    {
        private static List<CultureInfo> main_Languages = new List<CultureInfo>();
        public static List<CultureInfo> Languages
        {
            get { return main_Languages; }
        }

        public App()
        {
            LanguageChanged += App_LanguageChanged;
            main_Languages.Clear();
            main_Languages.Add(new CultureInfo("en-US"));
            main_Languages.Add(new CultureInfo("ru-RU"));
            Language = SimpleMediaPlayer.Properties.Settings.Default.DefaultLanguage;
        }

        public static event EventHandler LanguageChanged;

        public static CultureInfo Language
        {
            get { return Thread.CurrentThread.CurrentUICulture; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if (value == Thread.CurrentThread.CurrentUICulture) return;
                Thread.CurrentThread.CurrentUICulture = value;
                ResourceDictionary dict = new ResourceDictionary();
                switch (value.Name)
                {
                    case "ru-RU":
                        dict.Source = new Uri("Resources/lang.ru-RU.xaml", UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("Resources/lang.xaml", UriKind.Relative);
                        break;
                }
                //Current.Resources.MergedDictionaries.Clear();
                //Current.Resources.MergedDictionaries.Add(dict);
                ResourceDictionary? oldDict = (from d in Current.Resources.MergedDictionaries
                                               where d.Source != null && d.Source.OriginalString.StartsWith("Resources/lang.")
                                               select d).FirstOrDefault();
                if (oldDict != null)
                {
                    int ind = Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Current.Resources.MergedDictionaries.Remove(oldDict);
                    Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else { Current.Resources.MergedDictionaries.Add(dict); }
                LanguageChanged(Current, new EventArgs());
            }
        }
        //При изменении языка записываем в ресурсы текущую локализацию
        private void App_LanguageChanged(object sender, EventArgs e)
        {
            SimpleMediaPlayer.Properties.Settings.Default.DefaultLanguage = Language;
            SimpleMediaPlayer.Properties.Settings.Default.Save();
        }
    }

}
