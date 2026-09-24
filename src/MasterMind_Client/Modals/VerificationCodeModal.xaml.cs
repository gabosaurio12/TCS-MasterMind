using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace MasterMind_Client.Modals
{
    /// <summary>
    /// Lógica de interacción para VerificationCodeModal.xaml
    /// </summary>
    public partial class VerificationCodeModal : Window
    {
        private readonly string username;
        private readonly NavigationService navigationService;

        public VerificationCodeModal()
        {
            InitializeComponent();
        }

        public VerificationCodeModal(string username, NavigationService navigationService)
        {
            InitializeComponent();
            this.username = username;
            this.navigationService = navigationService;
        }

        private void MoveFocusToNext(TextBox currentTxt)
        {
            var request = new TraversalRequest(FocusNavigationDirection.Next);
            currentTxt.MoveFocus(request);
        }

        private string GetCodeFromBoxes()
        {
            return string.Concat(
                CodeI0Txt.Text,
                CodeI1Txt.Text,
                CodeI2Txt.Text,
                CodeI3Txt.Text,
                CodeI4Txt.Text
            );
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Windows.OfType<VerificationCodeModal>().FirstOrDefault()?.Close();
        }

        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            string code = GetCodeFromBoxes();
            var result = TempData.TempAuthService.AuthVerificationCode(username, code);
            if (result.Item1)
            {
                CurrentPlayer.Instance.SetCurrentPlayer(result.Item2);
                navigationService.Navigate(new Uri("Pages/MainPage.xaml", UriKind.Relative));
                Application.Current.Windows.OfType<VerificationCodeModal>().FirstOrDefault()?.Close();
            }
            else
            {
                new ErrorNotificationModal(Properties.Resources.ErrorNotification_WrongCode).Show();
            }
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
