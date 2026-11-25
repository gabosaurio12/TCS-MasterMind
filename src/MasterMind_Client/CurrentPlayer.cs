using log4net;
using MasterMind_Client.PlayerService;
using MasterMind_Client.ServerEndpoints;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MasterMind_Client
{
    public sealed class CurrentPlayer
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(CurrentPlayer));
        private static readonly TimeSpan OnlineStatusTimeout = TimeSpan.FromSeconds(3);
        private static CurrentPlayer instance;
        public static CurrentPlayer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new CurrentPlayer();
                }
                return instance;
            }
        }

        public int Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public ImageSource Avatar { get; private set; }
        public string AvatarPath { get; private set; }
        public bool IsOnlineRegistered { get; private set; }
        public bool IsGuest { get; private set; }
        public PlayerRecords CachedRecords { get; private set; }

        private CurrentPlayer()
        {
        }

        public void SetCurrentPlayer(Player player)
        {
            Id = player.PlayerId;
            Username = player.Username;
            Email = player.Email;
            if (AvatarPath != player.AvatarPath)
            {
                Avatar = null;
            }

            AvatarPath = player.AvatarPath;
            IsGuest = false;
        }

        public void SetGuest(Player guest)
        {
            SetCurrentPlayer(guest);
            IsGuest = true;
        }

        public async Task RegisterOnlineAsync()
        {
            if (IsOnlineRegistered)
            {
                return;
            }

            IsOnlineRegistered = true;

            try
            {
                await CreateOnlineStatusClient().SetOnlineStatusAsync(Id, true);
            }
            catch (Exception ex) when (IsServiceException(ex))
            {
                IsOnlineRegistered = false;
                logger.Error("Could not register the player as online.", ex);
            }
        }

        public void RegisterOffline()
        {
            if (IsGuest)
            {
                DeleteGuest();
                return;
            }

            if (!IsOnlineRegistered)
            {
                return;
            }

            IsOnlineRegistered = false;

            try
            {
                CreateOnlineStatusClient().SetOnlineStatus(Id, false);
            }
            catch (Exception ex) when (IsServiceException(ex))
            {
                logger.Error("Could not register the player as offline.", ex);
            }
        }

        private void DeleteGuest()
        {
            IsGuest = false;
            IsOnlineRegistered = false;

            try
            {
                CreateOnlineStatusClient().DeleteGuest(Id);
            }
            catch (Exception ex) when (IsServiceException(ex))
            {
                logger.Error("Could not delete the guest player.", ex);
            }
        }

        private static PlayerServiceClient CreateOnlineStatusClient()
        {
            var client = ServiceClientFactory.CreatePlayerClient();
            client.InnerChannel.OperationTimeout = OnlineStatusTimeout;

            return client;
        }

        private static bool IsServiceException(Exception ex)
        {
            return ex is FaultException || ex is CommunicationException || ex is TimeoutException;
        }

        public void SetAvatar(ImageSource avatar, string avatarPath)
        {
            Avatar = avatar;
            AvatarPath = avatarPath;
        }

        public void SetCachedRecords(PlayerRecords records)
        {
            CachedRecords = records;
        }
    }
}
