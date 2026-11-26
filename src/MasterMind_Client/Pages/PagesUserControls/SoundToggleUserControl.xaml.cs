using MasterMind_Client.ViewModels.UserControls;
using System.ComponentModel;
using System.Windows.Controls;

namespace MasterMind_Client.Pages.PagesUserControls
{
    public partial class SoundToggleUserControl : UserControl
    {
        public SoundToggleUserControl()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new SoundToggleViewModel(AppServices.SoundEffectService);
            }
        }
    }
}
