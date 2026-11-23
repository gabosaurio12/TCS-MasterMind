using DotNetEnv;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace MasterMind_Server.Services.Network
{
    internal static class ServerAddressProvider
    {
        private const string ServerIpKey = "SERVER_IP";
        private const byte ClassAFirstOctet = 10;
        private const byte ClassBFirstOctet = 172;
        private const byte ClassBSecondOctetMinimum = 16;
        private const byte ClassBSecondOctetMaximum = 31;
        private const byte ClassCFirstOctet = 192;
        private const byte ClassCSecondOctet = 168;
        private const int FirstOctetIndex = 0;
        private const int SecondOctetIndex = 1;

        private static readonly ILog logger = LogManager.GetLogger(typeof(ServerAddressProvider));

        public static IPAddress Resolve()
        {
            Env.Load();
            string configuredIp = Environment.GetEnvironmentVariable(ServerIpKey);

            if (!string.IsNullOrWhiteSpace(configuredIp))
            {
                return ParseConfiguredAddress(configuredIp);
            }

            IPAddress privateAddress = FindPrivateAddress();

            if (privateAddress != null)
            {
                logger.InfoFormat("Using private address {0}.", privateAddress);
                return privateAddress;
            }

            logger.Warn("No private IPv4 address found, falling back to the loopback address.");
            return IPAddress.Loopback;
        }

        private static IPAddress ParseConfiguredAddress(string configuredIp)
        {
            if (!IPAddress.TryParse(configuredIp.Trim(), out IPAddress address) ||
                address.AddressFamily != AddressFamily.InterNetwork)
            {
                var exception = new ArgumentException("SERVER_IP is not a valid IPv4 address.");
                logger.Error(exception);
                throw exception;
            }

            return address;
        }

        private static IPAddress FindPrivateAddress()
        {
            IPAddress address = GetInterfaceAddresses().FirstOrDefault(IsPrivate);

            return address ?? GetDnsAddresses().FirstOrDefault(IsPrivate);
        }

        private static IEnumerable<IPAddress> GetInterfaceAddresses()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(IsPhysicalConnectedInterface)
                    .SelectMany(networkInterface => networkInterface.GetIPProperties().UnicastAddresses)
                    .Select(unicast => unicast.Address)
                    .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
                    .ToList();
            }
            catch (NetworkInformationException networkException)
            {
                logger.Warn("Network interfaces could not be read.", networkException);
                return Enumerable.Empty<IPAddress>();
            }
        }

        private static bool IsPhysicalConnectedInterface(NetworkInterface networkInterface)
        {
            bool isSupportedType = networkInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet ||
                networkInterface.NetworkInterfaceType == NetworkInterfaceType.GigabitEthernet ||
                networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211;

            return isSupportedType &&
                networkInterface.OperationalStatus == OperationalStatus.Up &&
                networkInterface.GetIPProperties().GatewayAddresses
                    .Any(gateway => gateway.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !gateway.Address.Equals(IPAddress.Any));
        }

        private static IEnumerable<IPAddress> GetDnsAddresses()
        {
            try
            {
                return Dns.GetHostEntry(Dns.GetHostName()).AddressList
                    .Where(address => address.AddressFamily == AddressFamily.InterNetwork)
                    .ToList();
            }
            catch (SocketException socketException)
            {
                logger.Warn("The host name could not be resolved.", socketException);
                return Enumerable.Empty<IPAddress>();
            }
        }

        private static bool IsPrivate(IPAddress address)
        {
            byte[] octets = address.GetAddressBytes();
            byte first = octets[FirstOctetIndex];
            byte second = octets[SecondOctetIndex];

            return first == ClassAFirstOctet ||
                (first == ClassBFirstOctet && second >= ClassBSecondOctetMinimum &&
                    second <= ClassBSecondOctetMaximum) ||
                (first == ClassCFirstOctet && second == ClassCSecondOctet);
        }
    }
}
