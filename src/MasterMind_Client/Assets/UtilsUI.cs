using System.Globalization;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MasterMind_Client.Assets
{
    public static class UtilsUI
    {
        public static readonly SolidColorBrush OfflineGray = new SolidColorBrush((Color) ColorConverter.ConvertFromString("#929292"));
        public static readonly SolidColorBrush OnlineGreen = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#67D92A"));
        public static readonly SolidColorBrush NotReadyYellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9C22A"));
        public static readonly SolidColorBrush PressedBlue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2F4CC7"));
        public static readonly SolidColorBrush PressedRed = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A51F22"));
        public static readonly SolidColorBrush PressedYellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A88E18"));
        public static readonly SolidColorBrush PressedOrange = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A85518"));
        public static readonly SolidColorBrush UnpressedBlue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C6EF5"));
        public static readonly SolidColorBrush UnpressedRed = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D92A2D"));
        public static readonly SolidColorBrush UnpressedYellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9C22A"));
        public static readonly SolidColorBrush UnpressedOrange = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D97C2A"));
        public static readonly SolidColorBrush PrivateRed = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D92A2D"));
        public static readonly SolidColorBrush PublicBlue = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A59D9"));
        public static readonly SolidColorBrush RoomDefaultBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#585858"));
        public static readonly SolidColorBrush RoomSelectedBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#58A354"));
        public static readonly SolidColorBrush PlayerSlotDefaultBackground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#313539"));

        public static void ToggleLanguage()
        {
            string usLanguage = "en-US";
            string mxLanguage = "es-MX";
            var currentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;

            CultureInfo newCulture;
            if (currentCulture == mxLanguage)
            {
                newCulture = new CultureInfo(usLanguage);
            }
            else
            {
                newCulture = new CultureInfo(mxLanguage);
            }

            System.Threading.Thread.CurrentThread.CurrentUICulture = newCulture;
            CultureInfo.DefaultThreadCurrentUICulture = newCulture;
        }

        public static void ChangeLanguage(NavigationService navigation)
        {
            ToggleLanguage();
            navigation.Refresh();
        }
    }
}
