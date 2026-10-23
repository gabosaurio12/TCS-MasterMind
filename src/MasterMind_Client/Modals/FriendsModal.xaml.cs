using System;
using System.Windows;

namespace MasterMind_Client.Modals
{
    /// <summary>
    /// Interaction logic for FriendsModal.xaml.
    /// </summary>
    public partial class FriendsModal : Window
    {
        public FriendsModal()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (Owner != null)
            {
                Owner.Activate();
            }
            else if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Activate();
            }
        }
    }
}
