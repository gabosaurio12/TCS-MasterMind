using MasterMind_Server.Exceptions;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract]
    interface IPlayerService
    {
        [OperationContract]
        string RegisterPlayer(Player player);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        bool LoginPlayer(string username, string password);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        Player GetPlayerByUsername(string username);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        Player GetPlayerById(int playerId);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        string UpdatePlayer(Player player);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        PlayerRecords GetPlayerRecords(string username);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        string ChangePassword(string username, string password);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        void ReportPlayer(Report report);
    }

    [DataContract]
    public class Player
    {
        [DataMember]
        public int PlayerId { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public int NumberOfReports { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public string AvatarPath { get; set; }
        [DataMember]
        public bool IsOnline { get; set; }
    }

    [DataContract]
    public class PlayerRecords
    {
        [DataMember]
        public string PlayerUsername { get; set; }
        [DataMember]
        public int[] TriesRecords { get; set; }
        [DataMember]
        public int[] TimeTrialRecords { get; set; }
    }

    [DataContract]
    public class Report
    {
        [DataMember]
        public string Reason { get; set; }
        [DataMember]
        public string ReportedPlayerUsername { get; set; }
        [DataMember]
        public string ReportedByPlayerUsername { get; set; }
    }
}
