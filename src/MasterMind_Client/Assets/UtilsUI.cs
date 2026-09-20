using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MasterMind_Client.Assets
{
    public static class UtilsUI
    {
        public static readonly SolidColorBrush OfflineGray = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#929292"));
        public static readonly SolidColorBrush OnlineGreen = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#67D92A"));

        public static void ChangeLanguage(NavigationService navigation)
        {
            string usLanguage = "en-US";
            string mxLanguage = "es-MX";
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;

            if (currentCulture == mxLanguage)
            {
                var newCulture = new System.Globalization.CultureInfo(usLanguage);
                System.Threading.Thread.CurrentThread.CurrentUICulture = newCulture;
                System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = newCulture;
            }
            else
            {
                var newCulture = new System.Globalization.CultureInfo(mxLanguage);
                System.Threading.Thread.CurrentThread.CurrentUICulture = newCulture;
                System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = newCulture;
            }

            navigation.Refresh();
        }
    }
}
