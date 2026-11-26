using System.Windows;

namespace MasterMind_Client.Helpers
{
    public static class SoundEffects
    {
        public static readonly DependencyProperty IsSilentProperty = DependencyProperty.RegisterAttached(
            "IsSilent", typeof(bool), typeof(SoundEffects), new PropertyMetadata(false));

        public static bool GetIsSilent(DependencyObject element)
        {
            return (bool)element.GetValue(IsSilentProperty);
        }

        public static void SetIsSilent(DependencyObject element, bool value)
        {
            element.SetValue(IsSilentProperty, value);
        }
    }
}
