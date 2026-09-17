using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MasterMind_Client.Modals
{
    /// <summary>
    /// Lógica de interacción para SuccessNotificationModal.xaml
    /// </summary>
    public partial class SuccessNotificationModal : Window
    {
        public SuccessNotificationModal()
        {
            InitializeComponent();
        }

        public SuccessNotificationModal(string message)
        {
            InitializeComponent();
            NotificationMessageTxt.Text = message;
        }

        private void ContinueBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Windows.OfType<SuccessNotificationModal>().FirstOrDefault()?.Close();
        }
    }
}
