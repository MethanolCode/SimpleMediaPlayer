using System.Configuration;
using System.Data;
using System.Globalization;
using System.Windows;

namespace SimpleMediaPlayer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static List<CultureInfo> main_Languages = new List<CultureInfo>();
        public static List<CultureInfo> Languages
        {
            get{return main_Languages;}
        }

        public App()
        {
            main_Languages.Clear();
            main_Languages.Add(new CultureInfo("en-US"));
            main_Languages.Add(new CultureInfo("ru-RU"));
        }
        //Евент для оповещения всех окон приложения
        public static event EventHandler LanguageChanged;

        public static CultureInfo Language
        {
            get{return System.Threading.Thread.CurrentThread.CurrentUICulture;}
            set{
                if (value == null) throw new ArgumentNullException("value");
                if (value == System.Threading.Thread.CurrentThread.CurrentUICulture) return;
                //1. Меняем язык приложения:
                System.Threading.Thread.CurrentThread.CurrentUICulture = value;
                //2. Создаём ResourceDictionary для новой культуры
                ResourceDictionary dict = new ResourceDictionary();
                switch (value.Name){
                    case "ru-RU":
                        dict.Source = new Uri(String.Format("Resources/lang.{0}.xaml", value.Name), UriKind.Relative);
                        break;
                    default:
                        dict.Source = new Uri("Resources/lang.xaml", UriKind.Relative);
                        break;
                }
                //3. Находим старую ResourceDictionary и удаляем его и добавляем новую ResourceDictionary
                ResourceDictionary oldDict = (from d in Application.Current.Resources.MergedDictionaries
                                              where d.Source != null && d.Source.OriginalString.StartsWith("Resources/lang.") select d).First();
                if (oldDict != null){
                    int ind = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Application.Current.Resources.MergedDictionaries.Remove(oldDict);
                    Application.Current.Resources.MergedDictionaries.Insert(ind, dict);
                }
                else{
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                }
                //4. Вызываем евент для оповещения всех окон.
                LanguageChanged(Application.Current, new EventArgs());
            }
        }
        private void Application_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
           Language = SimpleMediaPlayer.Properties.Settings.Default.DefaultLanguage;
        }

        private void App_LanguageChanged(Object sender, EventArgs e)
        {
            SimpleMediaPlayer.Properties.Settings.Default.DefaultLanguage = Language;
            SimpleMediaPlayer.Properties.Settings.Default.Save();
            SimpleMediaPlayer.Properties.Settings.Default.DefaultLanguage.Name.ToLower();
        }
    }

}
