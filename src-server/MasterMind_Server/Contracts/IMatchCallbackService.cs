using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract(CallbackContract = typeof(IMatchCallbackService))]
    interface IMatchCallbackService
    {
        [OperationContract(IsOneWay = true)]
        void OnSumAPointCallback(string username);

        [OperationContract(IsOneWay = true)]
        void OnSetWinnerCallback(string username);
    }
}
