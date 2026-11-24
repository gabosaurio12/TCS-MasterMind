using System.Runtime.Serialization;

namespace MasterMind_Server.Repositories.Dtos
{
    [DataContract]
    public class ChatMessageResult
    {
        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}
