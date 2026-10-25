using System.ServiceModel;

namespace MasterMind_Server.Contracts
{
    [ServiceContract(CallbackContract = typeof(IFriendsCallbackService))]
    interface IFriendsCallbackService
    {
        [OperationContract(IsOneWay = true)]
        void OnFriendshipStatusChanged(Friendship friendship);
    }
}
