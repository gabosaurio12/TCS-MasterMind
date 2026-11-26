using MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MasterMind_Client.Helpers
{
    public class ClueKindToImageConverter : IValueConverter
    {
        private const string ClueImageSuffix = "CluePinImage";
        private const string EmptyClueKey = "CluePinEmptyImage";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ClueColour kind && kind != ClueColour.None)
            {
                return Application.Current.TryFindResource(kind + ClueImageSuffix);
            }

            return Application.Current.TryFindResource(EmptyClueKey);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
