using DotNetEnv;
using MasterMind_Client.AvatarService;
using MasterMind_Client.CodeService;
using MasterMind_Client.FriendsService;
using MasterMind_Client.LobbyService;
using MasterMind_Client.MatchService;
using MasterMind_Client.PlayerService;
using MasterMind_Client.RoomService;
using System;
using System.ServiceModel;

namespace MasterMind_Client.ServerEndpoints
{
    /// <summary>
    /// Creates the WCF proxies pointing at the host configured in SERVER_HOST (default: loopback).
    /// </summary>
    public static class ServiceClientFactory
    {
        private const string ServerHostKey = "SERVER_HOST";
        private const string DefaultHost = "127.0.0.1";
        private const string ServicePath = "MasterMindService";
        private const int PlayerPort = 8081;
        private const int CodePort = 8083;
        private const int FriendsPort = 8085;
        private const int RoomPort = 8087;
        private const int LobbyPort = 8089;
        private const int AvatarPort = 8093;
        private const int MatchPort = 8095;

        private static readonly string serverHost = ReadServerHost();

        public static PlayerServiceClient CreatePlayerClient()
        {
            return new PlayerServiceClient("NetTcpBinding_IPlayerService", BuildAddress(PlayerPort));
        }

        public static CodeServiceClient CreateCodeClient()
        {
            return new CodeServiceClient("NetTcpBinding_ICodeService", BuildAddress(CodePort));
        }

        public static RoomServiceClient CreateRoomClient()
        {
            return new RoomServiceClient("NetTcpBinding_IRoomService", BuildAddress(RoomPort));
        }

        public static AvatarServiceClient CreateAvatarClient()
        {
            return new AvatarServiceClient("NetTcpBinding_IAvatarService", BuildAddress(AvatarPort));
        }

        public static FriendsServiceClient CreateFriendsClient(InstanceContext callbackContext)
        {
            return new FriendsServiceClient(callbackContext, "NetTcpBinding_IFriendsService",
                BuildAddress(FriendsPort));
        }

        public static LobbyServiceClient CreateLobbyClient(InstanceContext callbackContext)
        {
            return new LobbyServiceClient(callbackContext, "NetTcpBinding_ILobbyService",
                BuildAddress(LobbyPort));
        }

        public static MatchServiceClient CreateMatchClient(InstanceContext callbackContext)
        {
            return new MatchServiceClient(callbackContext, "NetTcpBinding_IMatchService",
                BuildAddress(MatchPort));
        }

        private static string ReadServerHost()
        {
            Env.Load();
            string configuredHost = Environment.GetEnvironmentVariable(ServerHostKey);

            return string.IsNullOrWhiteSpace(configuredHost) ? DefaultHost : configuredHost.Trim();
        }

        private static string BuildAddress(int port)
        {
            return $"net.tcp://{serverHost}:{port}/{ServicePath}";
        }
    }
}
