using log4net;
using MasterMind_Client.AvatarService;
using System;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MasterMind_Client.Helpers
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

            if (string.IsNullOrEmpty(currentPlayer.AvatarPath))
            {
                return GetDefaultAvatar();
            }

            var avatar = await RequestAvatarAsync(currentPlayer.Id);

            if (avatar == null)
            {
                return GetDefaultAvatar();
            }

            currentPlayer.SetAvatar(avatar, currentPlayer.AvatarPath);

            return avatar;
        }

        private static async Task<ImageSource> RequestAvatarAsync(int playerId)
        {
            var avatarClient = new AvatarServiceClient();

            try
            {
                var image = await avatarClient.GetAvatarAsync(playerId);

                return image.Content.Length == 0 ? null : CreateImage(image.Content);
            }
            catch (FaultException ex)
            {
                logger.Warn("The server could not return the avatar.", ex);
            }
            catch (CommunicationException ex)
            {
                logger.Error("Communication failure while loading the avatar.", ex);
            }
            catch (TimeoutException ex)
            {
                logger.Error("Timeout while loading the avatar.", ex);
            }
            catch (NotSupportedException ex)
            {
                logger.Warn("The avatar returned by the server is not a valid image.", ex);
            }
            finally
            {
                CloseClient(avatarClient);
            }

            return null;
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
                logger.Warn("The avatar channel could not be closed cleanly.", ex);
                avatarClient.Abort();
            }
            catch (TimeoutException ex)
            {
                logger.Warn("Timeout while closing the avatar channel.", ex);
                avatarClient.Abort();
            }
        }
    }
}
