using MasterMind_Client.Assets;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MasterMind_Client.Pages
{
    /// <summary>
    /// Lógica de interacción para CreateRoomPage.xaml
    /// </summary>
    public partial class CreateRoomPage : Page
    {
        private bool timeTrialFlag = false;
        private bool triesFlag = false;
        private bool privateFlag = false;
        private bool publicFlag = false;
        private bool easyFlag = false;
        private bool normalFlag = false;
        private bool hardFlag = false;
        private bool enigmaFlag = false;

        public CreateRoomPage()
        {
            InitializeComponent();
        }

        private void TimeTrialBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!timeTrialFlag)
            {
                timeTrialFlag = true;
                TimeTrialBtn.Background = UtilsUI.PressedBlue;
            }
            else
            {
                timeTrialFlag = false;
                TimeTrialBtn.Background = UtilsUI.UnpressedBlue;
            }
        }

        private void TriesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!triesFlag)
            {
                triesFlag = true;
                TriesBtn.Background = UtilsUI.PressedBlue;
            }
            else
            {
                triesFlag = false;
                TriesBtn.Background = UtilsUI.UnpressedBlue;
            }
            
        }

        private void PrivateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!privateFlag)
            {
                privateFlag = true;
                PrivateBtn.Background = UtilsUI.PressedRed;
            }
            else
            {
                privateFlag = false;
                PrivateBtn.Background = UtilsUI.UnpressedRed;
            }
        }

        private void PublicBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!publicFlag)
            {
                publicFlag = true;
                PublicBtn.Background = UtilsUI.PressedBlue;
            }
            else
            {
                publicFlag = false;
                PublicBtn.Background = UtilsUI.UnpressedBlue;
            }
        }

        private void EasyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!easyFlag)
            {
                easyFlag = true;
                EasyBtn.Background = UtilsUI.PressedBlue;
            }
            else
            {
                easyFlag = false;
                EasyBtn.Background = UtilsUI.UnpressedBlue;
            }
        }

        private void NormalBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!normalFlag)
            {
                normalFlag = true;
                NormalBtn.Background = UtilsUI.PressedYellow;
            }
            else
            {
                normalFlag = false;
                NormalBtn.Background = UtilsUI.UnpressedYellow;
            }
        }

        private void HardBtn_Click(object sender, RoutedEventArgs e)
        {
            if (!hardFlag)
            {
                hardFlag = true;
                HardBtn.Background = UtilsUI.PressedOrange;
            }
            else
            {
                hardFlag = false;
                HardBtn.Background = UtilsUI.UnpressedOrange;
            }
        }

        private void Enigmabtn_Click(object sender, RoutedEventArgs e)
        {
            if (!enigmaFlag)
            {
                enigmaFlag = true;
                Enigmabtn.Background = UtilsUI.PressedRed;
            }
            else
            {
                enigmaFlag = false;
                Enigmabtn.Background = UtilsUI.UnpressedRed;
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("Pages/RoomsPage.xaml", UriKind.Relative));
        }

        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
