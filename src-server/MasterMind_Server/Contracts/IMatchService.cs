using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract]
    interface IMatchService
    {
        [OperationContract]
        int SetTimer(string difficulty);

        [OperationContract]
        TryResult ValidateTry(string username, CodeTry codeTry);
    }

    public class TryResult
    {

    }

    public class CodeTry
    {

    }
}
