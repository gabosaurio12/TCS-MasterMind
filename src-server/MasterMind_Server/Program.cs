using log4net;
using MasterMind_Server.Services.Network;
using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel;

namespace MasterMind_Server
{
    internal class Program
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(Program));

        private Program()
        {
        }

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            IPAddress serverAddress = ServerAddressProvider.Resolve();
            List<ServiceHost> hosts = CreateHosts(serverAddress);

            foreach (var host in hosts)
            {
                host.Open();
            }

            string runningMessage = $"Server is running on {serverAddress}";
            Console.WriteLine(runningMessage);
            logger.Info(runningMessage);
            Console.ReadLine();

            foreach (var host in hosts)
            {
                host.Close();
            }

            logger.Info("Server is closed");
        }

        private static List<ServiceHost> CreateHosts(IPAddress serverAddress)
        {
            return new List<ServiceHost>
            {
                CreateHost(typeof(Services.PlayerService), serverAddress, ServicePorts.Player),
                CreateHost(typeof(Services.CodeService), serverAddress, ServicePorts.Code),
                CreateHost(typeof(Services.RoomService), serverAddress, ServicePorts.Room),
                CreateHost(typeof(Services.FriendsService), serverAddress, ServicePorts.Friends),
                CreateHost(typeof(Services.LobbyService), serverAddress, ServicePorts.Lobby),
                CreateHost(typeof(Services.AvatarService), serverAddress, ServicePorts.Avatar),
                CreateHost(typeof(Services.MatchService), serverAddress, ServicePorts.Match)
            };
        }

        private static ServiceHost CreateHost(Type serviceType, IPAddress serverAddress, int port)
        {
            return new ServiceHost(serviceType, new Uri($"net.tcp://{serverAddress}:{port}"));
        }
    }
}
