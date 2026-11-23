using MasterMind_Client.ServerEndpoints;
using log4net;
using MasterMind_Client.AvatarService;
using System;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MasterMind_Client.AvatarManagement
{
    public static class AvatarLoader
    {
        private const string DefaultAvatarResourceKey = "AvatarImage";

        private readonly static ILog logger = LogManager.GetLogger(typeof(AvatarLoader));

        public static ImageSource GetDefaultAvatar()
        {
            return (ImageSource)Application.Current.Resources[DefaultAvatarResourceKey];
        }

        public static async Task<ImageSource> GetCurrentPlayerAvatarAsync()
        {
            var currentPlayer = CurrentPlayer.Instance;

            if (currentPlayer.Avatar != null)
            {
                return currentPlayer.Avatar;
            }

            var avatar = await GetAvatarAsync(currentPlayer.Id, currentPlayer.AvatarPath);

            if (avatar != GetDefaultAvatar())
            {
                currentPlayer.SetAvatar(avatar, currentPlayer.AvatarPath);
            }

            return avatar;
        }

        public static async Task<ImageSource> GetAvatarAsync(int playerId, string avatarPath)
        {
            if (string.IsNullOrEmpty(avatarPath))
            {
                return GetDefaultAvatar();
            }

            return await RequestAvatarAsync(playerId) ?? GetDefaultAvatar();
        }

        private static async Task<ImageSource> RequestAvatarAsync(int playerId)
        {
            var avatarClient = ServiceClientFactory.CreateAvatarClient();
            var avatarImage = GetDefaultAvatar();

            try
            {
                var image = await avatarClient.GetAvatarAsync(playerId);

                if (image.Content.Length > 0)
                {
                    avatarImage = CreateImage(image.Content);
                }

            }
            catch (FaultException ex)
            {
                logger.Error(ex);
            }
            catch (CommunicationException ex)
            {
                logger.Error(ex);
            }
            catch (TimeoutException ex)
            {
                logger.Warn(ex);
            }
            catch (NotSupportedException ex)
            {
                logger.Warn(ex);
            }
            finally
            {
                CloseClient(avatarClient);
            }

            return avatarImage;
        }

        private static BitmapImage CreateImage(byte[] content)
        {
            var image = new BitmapImage();

            using (var stream = new MemoryStream(content))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }

            image.Freeze();

            return image;
        }

        private static void CloseClient(AvatarServiceClient avatarClient)
        {
            try
            {
                avatarClient.Close();
            }
            catch (CommunicationException ex)
            {
                logger.Error(ex);
                avatarClient.Abort();
            }
            catch (TimeoutException ex)
            {
                logger.Warn(ex);
                avatarClient.Abort();
            }
        }
    }
}
