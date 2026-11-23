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

            var avatar = await LoadAsync(currentPlayer.Id, currentPlayer.AvatarPath, AvatarZone.Own);

            if (avatar != GetDefaultAvatar())
            {
                currentPlayer.SetAvatar(avatar, currentPlayer.AvatarPath);
            }

            return avatar;
        }

        public static Task<ImageSource> GetAvatarAsync(int playerId, string avatarPath)
        {
            return LoadAsync(playerId, avatarPath, AvatarZone.Temp);
        }

        private static async Task<ImageSource> LoadAsync(int playerId, string avatarPath, AvatarZone zone)
        {
            if (string.IsNullOrEmpty(avatarPath))
            {
                return GetDefaultAvatar();
            }

            if (TryCreateImage(AvatarCache.TryRead(zone, avatarPath), out var cachedAvatar))
            {
                return cachedAvatar;
            }

            AvatarCache.Delete(zone, avatarPath);
            var content = await RequestContentAsync(playerId);

            if (!TryCreateImage(content, out var avatar))
            {
                return GetDefaultAvatar();
            }

            AvatarCache.Save(zone, avatarPath, content);

            return avatar;
        }

        private static async Task<byte[]> RequestContentAsync(int playerId)
        {
            var avatarClient = ServiceClientFactory.CreateAvatarClient();
            var content = Array.Empty<byte>();

            try
            {
                var image = await avatarClient.GetAvatarAsync(playerId);
                content = image.Content;
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
            catch (InvalidOperationException ex)
            {
                logger.Warn(ex);
            }
            finally
            {
                CloseClient(avatarClient);
            }

            return content;
        }

        private static bool TryCreateImage(byte[] content, out ImageSource image)
        {
            image = null;

            if (content.Length == 0)
            {
                return false;
            }

            try
            {
                image = CreateImage(content);

                return true;
            }
            catch (NotSupportedException ex)
            {
                logger.Warn(ex);

                return false;
            }
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
