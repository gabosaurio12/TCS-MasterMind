using MasterMind_Client.ViewModels.Pages.MatchPages;
using System.Windows;
using System.Windows.Controls;

namespace MasterMind_Client.Pages.MatchPages
{
    /// <summary>
    /// Interaction logic for MatchTriesPage.xaml.
    /// </summary>
    public partial class MatchTriesPage : Page
    {
        public MatchTriesPage()
        {
            InitializeComponent();
            DataContext = new MatchTriesViewModel();
        }
    }
}
