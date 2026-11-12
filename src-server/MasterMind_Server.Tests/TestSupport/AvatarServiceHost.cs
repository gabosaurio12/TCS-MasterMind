using MasterMind_Server.Contracts;
using MasterMind_Server.Services;
using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace MasterMind_Server.Tests.TestSupport
{
    internal class AvatarServiceHost : IDisposable
    {
        private const int MaximumMessageSize = 12582912;
        private const int MaximumStringContentLength = 16777216;

        private readonly ServiceHost host;
        private readonly NetTcpBinding binding;
        private readonly string baseAddress;

        public AvatarServiceHost()
        {
            int port = GetFreeTcpPort();
            baseAddress = $"net.tcp://localhost:{port}/AvatarService";
            binding = BuildBinding();

            host = new ServiceHost(typeof(AvatarService), new Uri(baseAddress));
            host.AddServiceEndpoint(typeof(IAvatarService), binding, string.Empty);
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

        public AvatarServiceClient CreateClient()
        {
            var factory = new ChannelFactory<IAvatarService>(binding, new EndpointAddress(baseAddress));
            var channel = factory.CreateChannel();

            return new AvatarServiceClient(factory, channel);
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

        private static NetTcpBinding BuildBinding()
        {
            var binding = new NetTcpBinding(SecurityMode.None)
            {
                OpenTimeout = TimeSpan.FromSeconds(10),
                CloseTimeout = TimeSpan.FromSeconds(10),
                SendTimeout = TimeSpan.FromSeconds(30),
                ReceiveTimeout = TimeSpan.FromSeconds(30),
                MaxReceivedMessageSize = MaximumMessageSize,
                MaxBufferSize = MaximumMessageSize
            };

            binding.ReaderQuotas.MaxArrayLength = MaximumMessageSize;
            binding.ReaderQuotas.MaxStringContentLength = MaximumStringContentLength;
            binding.ReaderQuotas.MaxBytesPerRead = MaximumStringContentLength;

            return binding;
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

    internal class AvatarServiceClient : IDisposable
    {
        private readonly ChannelFactory<IAvatarService> factory;
        private readonly IAvatarService channel;

        public AvatarServiceClient(ChannelFactory<IAvatarService> factory, IAvatarService channel)
        {
            this.factory = factory;
            this.channel = channel;
            ((IClientChannel)channel).Open();
        }

        public IAvatarService Service
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
