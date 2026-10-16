using MasterMind_Server.Exceptions;
using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract]
    interface ICodeService
    {

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        void SendVerificationCode(int playerId);

        [OperationContract]
        [FaultContract(typeof(ErrorMessage))]
        bool AuthVerificationCode(string username, string code);
    }
}
