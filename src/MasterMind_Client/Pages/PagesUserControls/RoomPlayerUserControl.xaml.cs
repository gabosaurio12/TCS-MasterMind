using MasterMind_Client.Assets;
using System.Windows.Controls;

namespace MasterMind_Client.Pages.PagesUserControls
{
    /// <summary>
    /// Lógica de interacción para RoomPlayerUserControl.xaml
    /// </summary>
    public partial class RoomPlayerUserControl : UserControl
    {
        public RoomPlayerUserControl()
        {
            InitializeComponent();
        }

        public RoomPlayerUserControl(string username)
        {
            InitializeComponent();
            UsernameTxt.Text = username;
        }

        public void SetAsReady()
        {
            Grid.Background = UtilsUI.OnlineGreen;
        }

        public void SetAsNotReady()
        {
            Grid.Background = UtilsUI.NotReadyYellow;
        }
    }
}
