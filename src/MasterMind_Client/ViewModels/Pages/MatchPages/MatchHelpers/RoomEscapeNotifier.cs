using log4net;
using MasterMind_Client.LobbyService;
using MasterMind_Client.ServerEndpoints;
using MasterMind_Client.ViewModels.Services;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers
{
    /// <summary>
    /// Tells the lobby server that the current player left a room while a match was running.
    /// </summary>
    public class RoomEscapeNotifier
    {
        private const string EscapedStatus = "Escaped";

        private static readonly ILog logger = LogManager.GetLogger(typeof(RoomEscapeNotifier));

        private readonly IDialogService dialogService;

        public RoomEscapeNotifier(IDialogService dialogService)
        {
            this.dialogService = dialogService;
        }

        public async Task NotifyAsync(int roomId)
        {
            var client = ServiceClientFactory.CreateLobbyClient(new InstanceContext(new NoOpLobbyCallback()));
            var status = new PlayerStatus
            {
                RoomId = roomId,
                Username = CurrentPlayer.Instance.Username,
                Status = EscapedStatus
            };

            try
            {
                await client.ChangePlayerStatusAsync(status);
            }
            catch (FaultException<ErrorMessage> faultException)
            {
                logger.WarnFormat(faultException.Detail.Message);
                dialogService.ShowServerError(faultException.Detail.Message);
            }
            catch (FaultException faultException)
            {
                logger.Error(faultException);
                dialogService.ShowServerError(faultException.Message);
            }
            catch (CommunicationException communicationException)
            {
                logger.Error(communicationException);
                dialogService.ShowServerError(string.Empty);
            }
            finally
            {
                CloseClient(client);
            }
        }

        private static void CloseClient(ICommunicationObject client)
        {
            try
            {
                client.Close();
            }
            catch (Exception exception) when (exception is CommunicationException
                || exception is TimeoutException)
            {
                logger.Warn(exception);
                client.Abort();
            }
        }

        [CallbackBehavior(UseSynchronizationContext = false)]
        private class NoOpLobbyCallback : ILobbyServiceCallback
        {
            public void OnMessageReceivedCallback(ChatMessage message)
            {
            }

            public void OnChangePlayerStatusCallback(PlayerStatus status)
            {
            }
        }
    }
}
