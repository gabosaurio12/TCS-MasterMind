using MasterMind_Client.ViewModels.Pages.MatchPages;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MasterMind_Client.Helpers
{
    public class ClueKindToImageConverter : IValueConverter
    {
        private const string ClueImageSuffix = "CluePinImage";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ClueKind kind && kind != ClueKind.None)
            {
                return Application.Current.TryFindResource(kind + ClueImageSuffix);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
