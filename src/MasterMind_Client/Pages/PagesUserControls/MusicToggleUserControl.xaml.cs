using MasterMind_Client.ViewModels.UserControls;
using System.ComponentModel;
using System.Windows.Controls;

namespace MasterMind_Client.Pages.PagesUserControls
{
    public partial class MusicToggleUserControl : UserControl
    {
        public MusicToggleUserControl()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new MusicToggleViewModel(AppServices.MusicService);
            }
        }
    }
}
