using MasterMind_Server.Contracts;
using MasterMind_Server.Services;
using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace MasterMind_Server.Tests.TestSupport
{
    internal class FriendsServiceHost : IDisposable
    {
        private readonly ServiceHost host;
        private readonly NetTcpBinding binding;
        private readonly string baseAddress;

        public FriendsServiceHost()
        {
            int port = GetFreeTcpPort();
            baseAddress = $"net.tcp://localhost:{port}/FriendsService";
            binding = new NetTcpBinding(SecurityMode.None)
            {
                OpenTimeout = TimeSpan.FromSeconds(10),
                CloseTimeout = TimeSpan.FromSeconds(10),
                SendTimeout = TimeSpan.FromSeconds(10),
                ReceiveTimeout = TimeSpan.FromSeconds(10)
            };

            host = new ServiceHost(typeof(FriendsService), new Uri(baseAddress));
            host.AddServiceEndpoint(typeof(IFriendsService), binding, string.Empty);
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

        public FriendsServiceClient CreateClient(FriendsCallbackProbe callback)
        {
            var instanceContext = new InstanceContext(callback);
            var factory = new DuplexChannelFactory<IFriendsService>(
                instanceContext,
                binding,
                new EndpointAddress(baseAddress));
            var channel = factory.CreateChannel();
            return new FriendsServiceClient(factory, channel);
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

    internal class FriendsServiceClient : IDisposable
    {
        private readonly DuplexChannelFactory<IFriendsService> factory;
        private readonly IFriendsService channel;

        public FriendsServiceClient(DuplexChannelFactory<IFriendsService> factory, IFriendsService channel)
        {
            this.factory = factory;
            this.channel = channel;
            ((IClientChannel)channel).Open();
        }

        public IFriendsService Service
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
