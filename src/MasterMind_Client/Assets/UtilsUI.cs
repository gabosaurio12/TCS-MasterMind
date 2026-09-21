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
        public static readonly SolidColorBrush PressedBlue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F4CC7"));
        public static readonly SolidColorBrush PressedRed = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A51F22"));
        public static readonly SolidColorBrush PressedYellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A88E18"));
        public static readonly SolidColorBrush PressedOrange = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A85518"));
        public static readonly SolidColorBrush UnpressedBlue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C6EF5"));
        public static readonly SolidColorBrush UnpressedRed = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D92A2D"));
        public static readonly SolidColorBrush UnpressedYellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9C22A"));
        public static readonly SolidColorBrush UnpressedOrange = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D97C2A"));

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
