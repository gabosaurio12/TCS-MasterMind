using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MasterMind_Client.Modals
{
    /// <summary>
    /// Lógica de interacción para VerificationCodeModal.xaml
    /// </summary>
    public partial class VerificationCodeModal : Window
    {
        public VerificationCodeModal()
        {
            InitializeComponent();
        }

        private void MoveFocusToNext(TextBox currentTxt)
        {
            var request = new TraversalRequest(FocusNavigationDirection.Next);
            currentTxt.MoveFocus(request);
        }

        private void CodeTxt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var currentTxt = (TextBox)sender;

            if (currentTxt.Text.Length == 1)
            {
                MoveFocusToNext(currentTxt);
            }
        }

        private void CodeTxt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var currentBox = (TextBox)sender;

            if (e.Key == Key.Back && string.IsNullOrEmpty(currentBox.Text))
            {
                var request = new TraversalRequest(FocusNavigationDirection.Previous);
                currentBox.MoveFocus(request);
            }
        }
    }
}
