using System.Runtime.Serialization;

namespace MasterMind_Server.Exceptions
{
    [DataContract]
    public class ErrorMessage
    {
        [DataMember]
        public string Message { get; set; }
    }
}
