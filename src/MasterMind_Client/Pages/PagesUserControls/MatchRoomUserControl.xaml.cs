using MasterMind_Client.Assets;
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace MasterMind_Client.Pages.PagesUserControls
{
    /// <summary>
    /// Lógica de interacción para MatchRoomUserControl.xaml
    /// </summary>
    public partial class MatchRoomUserControl : UserControl
    {
        public event EventHandler RoomSelected;


        public MatchRoomUserControl()
        {
            InitializeComponent();
            MouseLeftButtonDown += MatchRoomUserControl_MouseLeftButtonDown;
        }

        public void Select()
        {
            RoomBorder.Background = UtilsUI.RoomSelectedBackground;
        }

        public void Deselect()
        {
            RoomBorder.Background = UtilsUI.RoomDefaultBackground;
        }

        public void SetAsPrivate()
        {
            PrivacyFlag.Fill = UtilsUI.PrivateRed;
        }

        public void SetAsPublic()
        {
            PrivacyFlag.Fill = UtilsUI.PrivateRed;
        }

        private void MatchRoomUserControl_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            RoomSelected?.Invoke(this, EventArgs.Empty);
        }
    }
}
