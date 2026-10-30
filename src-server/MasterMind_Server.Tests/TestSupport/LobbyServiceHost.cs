using MasterMind_Server.Contracts;
using MasterMind_Server.Services;
using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace MasterMind_Server.Tests.TestSupport
{
    internal class LobbyServiceHost : IDisposable
    {
        private readonly ServiceHost host;
        private readonly NetTcpBinding binding;
        private readonly string baseAddress;

        public LobbyServiceHost()
        {
            int port = GetFreeTcpPort();
            baseAddress = $"net.tcp://localhost:{port}/LobbyService";
            binding = new NetTcpBinding(SecurityMode.None)
            {
                OpenTimeout = TimeSpan.FromSeconds(10),
                CloseTimeout = TimeSpan.FromSeconds(10),
                SendTimeout = TimeSpan.FromSeconds(10),
                ReceiveTimeout = TimeSpan.FromSeconds(10)
            };

            host = new ServiceHost(typeof(LobbyService), new Uri(baseAddress));
            host.AddServiceEndpoint(typeof(ILobbyService), binding, string.Empty);
            var debugBehavior = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (debugBehavior != null)
            {
                debugBehavior.IncludeExceptionDetailInFaults = true;
            }
        }

        public void Start()
        {
            host.Open();
        }

        public LobbyServiceClient CreateClient(LobbyCallbackProbe callback)
        {
            var instanceContext = new InstanceContext(callback);
            var factory = new DuplexChannelFactory<ILobbyService>(
                instanceContext,
                binding,
                new EndpointAddress(baseAddress));
            var channel = factory.CreateChannel();
            return new LobbyServiceClient(factory, channel);
        }

        public void Dispose()
        {
            if (host.State == CommunicationState.Opened)
            {
                host.Close();
            }
            else
            {
                host.Abort();
            }
        }

        private static int GetFreeTcpPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            try
            {
                listener.Start();
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            finally
            {
                listener.Stop();
            }
        }
    }

    internal class LobbyServiceClient : IDisposable
    {
        private readonly DuplexChannelFactory<ILobbyService> factory;
        private readonly ILobbyService channel;

        public LobbyServiceClient(DuplexChannelFactory<ILobbyService> factory, ILobbyService channel)
        {
            this.factory = factory;
            this.channel = channel;
            ((IClientChannel)channel).Open();
        }

        public ILobbyService Service
        {
            get { return channel; }
        }

        public void Dispose()
        {
            try
            {
                ((IClientChannel)channel).Close(TimeSpan.FromSeconds(5));
            }
            catch (CommunicationException)
            {
                ((IClientChannel)channel).Abort();
            }
            catch (TimeoutException)
            {
                ((IClientChannel)channel).Abort();
            }
            finally
            {
                factory.Abort();
            }
        }
    }
}
