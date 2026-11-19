using MasterMind_Server.Exceptions;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace MasterMind_Server.Contracts
{
    [ServiceContract]
    interface IAvatarService
    {
        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        Task<AvatarImage> GetAvatar(int playerId);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        Task<string> SaveAvatar(int playerId, byte[] imageContent);
    }

    [DataContract]
    public class AvatarImage
    {
        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public byte[] Content { get; set; }
    }
}
