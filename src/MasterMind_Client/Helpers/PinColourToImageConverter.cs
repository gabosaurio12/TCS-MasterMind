using MasterMind_Client.ViewModels.Pages.MatchPages.MatchTries.Helpers;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MasterMind_Client.Helpers
{
    public class PinColourToImageConverter : IValueConverter
    {
        private const string PinImageSuffix = "PinImage";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PinColour colour)
            {
                return Application.Current.TryFindResource(colour + PinImageSuffix);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
