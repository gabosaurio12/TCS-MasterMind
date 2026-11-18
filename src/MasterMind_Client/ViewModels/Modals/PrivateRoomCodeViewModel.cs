using MasterMind_Client.RoomService;
using MasterMind_Client.ViewModels.Base;
using MasterMind_Client.ViewModels.Services;
using log4net;
using System;
using System.Linq;
using System.ServiceModel;
using System.Windows.Input;

namespace MasterMind_Client.ViewModels.Modals
{
    public class PrivateRoomCodeViewModel : ModalViewModelBase, IDisposable
    {
        private const int RoomCodeLength = 6;
        private const string SuccessResult = "Success";
        private const string RoomIsFullResult = "RoomIsFull";
        private const string WrongCodeResult = "WrongCode";

        private readonly static ILog logger = LogManager.GetLogger(typeof(PrivateRoomCodeViewModel));

        private readonly IDialogService dialogService;
        private readonly RoomServiceClient roomService;
        private readonly string username;

        private string codeDigit0;
        private string codeDigit1;
        private string codeDigit2;
        private string codeDigit3;
        private string codeDigit4;
        private string codeDigit5;
        private bool isJoining;
        private bool isDisposed;

        public event EventHandler<Room> RoomJoined;

        public RelayCommand JoinCommand { get; }

        public RelayCommand CancelCommand { get; }

        public string CodeDigit0
        {
            get => codeDigit0;
            set
            {
                SetProperty(ref codeDigit0, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string CodeDigit1
        {
            get => codeDigit1;
            set
            {
                SetProperty(ref codeDigit1, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string CodeDigit2
        {
            get => codeDigit2;
            set
            {
                SetProperty(ref codeDigit2, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string CodeDigit3
        {
            get => codeDigit3;
            set
            {
                SetProperty(ref codeDigit3, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string CodeDigit4
        {
            get => codeDigit4;
            set
            {
                SetProperty(ref codeDigit4, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string CodeDigit5
        {
            get => codeDigit5;
            set
            {
                SetProperty(ref codeDigit5, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public PrivateRoomCodeViewModel(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            roomService = new RoomServiceClient();
            username = CurrentPlayer.Instance.Username;
            JoinCommand = new RelayCommand(_ => Join(), _ => CanJoin());
            CancelCommand = new RelayCommand(_ => RequestClose());
        }

        private void Join()
        {
            if (!isJoining)
            {
                var code = BuildCode();
                if (!IsCodeComplete(code))
                {
                    dialogService.ShowError(Properties.Resources.ErrorNotification_WrongCode);
                    return;
                }

                isJoining = true;
                CommandManager.InvalidateRequerySuggested();

                try
                {
                    JoinRoomWithCode(code);
                }
                finally
                {
                    isJoining = false;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private void JoinRoomWithCode(string code)
        {
            try
            {
                var room = roomService.GetRoomByCode(code);
                if (room != null && room.RoomId > 0)
                {
                    var result = roomService.JoinRoomWithCode(room, username, code);
                    HandleJoinResult(result, room);
                }
            }
            catch (FaultException<ErrorMessage> faultException)
            {
                logger.WarnFormat(faultException.Detail.Message);
                ShowServerError(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error(faultException);
                ShowServerError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error(communicationException);
                dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
            }
        }

        private void HandleJoinResult(string result, Room room)
        {
            if (string.Equals(result, SuccessResult))
            {
                RoomJoined?.Invoke(this, room);
                return;
            }

            if (string.Equals(result, RoomIsFullResult))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_RoomFull);
                return;
            }

            if (string.Equals(result, WrongCodeResult))
            {
                dialogService.ShowError(Properties.Resources.ErrorNotification_WrongCode);
                return;
            }

            logger.WarnFormat(result);
            dialogService.ShowError(Properties.Resources.ErrorNotification_ErrorJoiningMatchRoom);
        }

        private void ShowServerError(string errorDetail)
        {
            switch (errorDetail)
            {
                case "DataBaseError":
                    dialogService.ShowError(Properties.Resources.ErrorNotification_DatabaseError);
                    break;
                case "HashError":
                    dialogService.ShowError(Properties.Resources.ErrorNotification_Timeout);
                    break;
                default:
                    logger.WarnFormat(errorDetail);
                    dialogService.ShowError(Properties.Resources.ErrorNotification_ServerDisconnected);
                    break;
            }
        }

        private string BuildCode()
        {
            return string.Concat(codeDigit0, codeDigit1, codeDigit2, codeDigit3, codeDigit4, codeDigit5);
        }

        private static bool IsCodeComplete(string code)
        {
            return !string.IsNullOrEmpty(code)
                && code.Length == RoomCodeLength
                && code.All(char.IsDigit);
        }

        private bool CanJoin()
        {
            return !isJoining && IsCodeComplete(BuildCode());
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            try
            {
                roomService.Close();
            }
            catch (CommunicationException)
            {
                roomService.Abort();
            }
            catch (TimeoutException)
            {
                roomService.Abort();
            }
        }
    }
}
