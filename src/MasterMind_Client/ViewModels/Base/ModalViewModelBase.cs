using System;

namespace MasterMind_Client.ViewModels.Base
{
    public abstract class ModalViewModelBase : ViewModelBase
    {
        public event EventHandler CloseRequested;

        protected void RequestClose()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
