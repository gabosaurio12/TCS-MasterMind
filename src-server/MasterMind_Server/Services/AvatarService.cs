using log4net;
using MasterMind_Server.Contracts;
using MasterMind_Server.Exceptions;
using MasterMind_Server.Repositories;
using MasterMind_Server.Repositories.Enums;
using MasterMind_Server.Repositories.Interfaces;
using MasterMind_Server.Services.ClassHelpers;
using System.Data.Entity.Core;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using PlayerEntity = MasterMind_Server.Data.Player;

namespace MasterMind_Server.Services
{
    internal class AvatarService : IAvatarService
    {
        private const int MaximumInputBytes = 10485760;

        private readonly static ILog logger = LogManager.GetLogger(typeof(AvatarService));
        private readonly static SemaphoreSlim uploadSemaphore = new SemaphoreSlim(1, 1);

        private readonly IPlayerRepository playerRepository;
        private readonly AvatarImageProcessor imageProcessor;
        private readonly AvatarStorage avatarStorage;

        public AvatarService() : this(new PlayerRepository(), new AvatarImageProcessor(), new AvatarStorage())
        {
        }

        public AvatarService(IPlayerRepository playerRepository, AvatarImageProcessor imageProcessor, AvatarStorage avatarStorage)
        {
            this.playerRepository = playerRepository;
            this.imageProcessor = imageProcessor;
            this.avatarStorage = avatarStorage;
        }

        public async Task<AvatarImage> GetAvatar(int playerId)
        {
            try
            {
                PlayerEntity player = GetPlayer(playerId);

                return await Task.Run(() => BuildAvatarImage(player.avatar_path));
            }
            catch (AvatarOperationException ex)
            {
                logger.Warn(ex);
                throw BuildFault(ex.Reason);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(ErrorMessages.DataBaseError);
            }
        }

        public async Task<string> SaveAvatar(int playerId, byte[] imageContent)
        {
            try
            {
                ValidateInput(imageContent);

                PlayerEntity player = GetPlayer(playerId);

                await uploadSemaphore.WaitAsync();

                try
                {
                    byte[] compressed = await Task.Run(() => imageProcessor.Compress(imageContent));

                    return StoreAvatar(player, compressed);
                }
                finally
                {
                    uploadSemaphore.Release();
                }
            }
            catch (AvatarOperationException ex)
            {
                logger.Warn(ex);
                throw BuildFault(ex.Reason);
            }
            catch (EntityException ex)
            {
                logger.Error(ex);
                throw BuildFault(ErrorMessages.DataBaseError);
            }
        }

        private PlayerEntity GetPlayer(int playerId)
        {
            PlayerEntity player = playerRepository.GetPlayerById(playerId);

            if (player == null)
            {
                throw new AvatarOperationException(ErrorMessages.PlayerNotFound);
            }

            return player;
        }

        private void ValidateInput(byte[] imageContent)
        {
            if (imageContent == null || imageContent.Length == 0)
            {
                throw new AvatarOperationException(ErrorMessages.InvalidImage);
            }

            if (imageContent.Length > MaximumInputBytes)
            {
                throw new AvatarOperationException(ErrorMessages.ImageTooLarge);
            }
        }

        private AvatarImage BuildAvatarImage(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return new AvatarImage { FileName = string.Empty, Content = new byte[0] };
            }

            byte[] content = avatarStorage.Read(fileName);

            if (content == null)
            {
                return new AvatarImage { FileName = fileName, Content = new byte[0] };
            }

            return new AvatarImage { FileName = fileName, Content = content };
        }

        private string StoreAvatar(PlayerEntity player, byte[] content)
        {
            string previousFileName = player.avatar_path;
            string fileName = avatarStorage.BuildFileName(player.player_id, player.username);

            avatarStorage.Save(content, fileName);

            string result = playerRepository.UpdateAvatarPath(player.player_id, fileName);

            if (!result.Equals(RepositoriesMessages.Success.ToString()))
            {
                avatarStorage.Delete(fileName);
                throw new AvatarOperationException(ErrorMessages.DataBaseError);
            }

            avatarStorage.Delete(previousFileName);

            return fileName;
        }

        private FaultException<ErrorMessage> BuildFault(ErrorMessages reason)
        {
            var clientMessage = new ErrorMessage { Message = reason.ToString() };

            return new FaultException<ErrorMessage>(clientMessage);
        }
    }
}
