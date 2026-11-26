using MasterMind_Server.Contracts;
using MasterMind_Server.Services;
using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace MasterMind_Server.Tests.TestSupport
{
    internal class MatchServiceHost : IDisposable
    {
        private static readonly TimeSpan BindingTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan IdleTimeout = TimeSpan.FromMinutes(1);

        private readonly ServiceHost host;
        private readonly NetTcpBinding binding;
        private readonly string baseAddress;

        public MatchServiceHost()
        {
            int port = GetFreeTcpPort();
            baseAddress = $"net.tcp://localhost:{port}/MatchService";
            binding = new NetTcpBinding(SecurityMode.None)
            {
                OpenTimeout = BindingTimeout,
                CloseTimeout = BindingTimeout,
                SendTimeout = BindingTimeout,
                ReceiveTimeout = IdleTimeout
            };

            host = new ServiceHost(typeof(MatchService), new Uri(baseAddress));
            host.AddServiceEndpoint(typeof(IMatchService), binding, string.Empty);
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

        public MatchServiceClient CreateClient(MatchCallbackProbe callback)
        {
            var factory = new DuplexChannelFactory<IMatchService>(
                new InstanceContext(callback),
                binding,
                new EndpointAddress(baseAddress));
            return new MatchServiceClient(factory, factory.CreateChannel());
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

    internal class MatchServiceClient : IDisposable
    {
        private static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(5);

        private readonly DuplexChannelFactory<IMatchService> factory;
        private readonly IMatchService channel;

        public MatchServiceClient(DuplexChannelFactory<IMatchService> factory, IMatchService channel)
        {
            this.factory = factory;
            this.channel = channel;
            ((IClientChannel)channel).Open();
        }

        public IMatchService Service
        {
            get { return channel; }
        }

        public void Dispose()
        {
            try
            {
                ((IClientChannel)channel).Close(CloseTimeout);
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
