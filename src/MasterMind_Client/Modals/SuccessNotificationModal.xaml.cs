using System;
using System.Linq;
using System.Windows;

namespace MasterMind_Client.Modals
{
    /// <summary>
    /// Lógica de interacción para SuccessNotificationModal.xaml
    /// </summary>
    public partial class SuccessNotificationModal : Window
    {
        public event EventHandler ModalClosed;
        private readonly bool isRegisterModal;

        public SuccessNotificationModal()
        {
            InitializeComponent();
        }

        public SuccessNotificationModal(string message, bool isRegisterModal)
        {
            InitializeComponent();
            NotificationMessageTxt.Text = message;
            this.isRegisterModal = isRegisterModal;
        }

        public SuccessNotificationModal(string message)
        {
            InitializeComponent();
            NotificationMessageTxt.Text = message;
        }

        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isRegisterModal)
            {
                ModalClosed?.Invoke(this, EventArgs.Empty);
            }
            Application.Current.Windows.OfType<SuccessNotificationModal>().FirstOrDefault()?.Close();
        }
    }
}
