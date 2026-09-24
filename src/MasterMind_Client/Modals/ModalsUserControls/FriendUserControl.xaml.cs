using MasterMind_Client.Assets;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MasterMind_Client.Modals.ModalsUserControls
{
    /// <summary>
    /// Lógica de interacción para FriendUserControl.xaml
    /// </summary>
    public partial class FriendUserControl : UserControl
    {
        public FriendUserControl()
        {
            InitializeComponent();
        }

        public FriendUserControl(string username)
        {
            InitializeComponent();
            UsernameTxt.Text = username;
        }

        public void SetOnlineVisibility()
        {
            OnlineVisibility.Fill = UtilsUI.OnlineGreen;
        }

        public void SetOfflineVisibility()
        {
            OnlineVisibility.Fill = UtilsUI.OfflineGray;
        }
    }
}
