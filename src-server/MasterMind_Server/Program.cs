using log4net;
using MasterMind_Server.Services;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace MasterMind_Server
{
    internal class Program
    {
        private readonly static ILog logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            var hosts = new List<ServiceHost>();
            hosts.Add(new ServiceHost(typeof(Services.PlayerService)));
            hosts.Add(new ServiceHost(typeof(Services.CodeService)));

            foreach (var host in hosts)
            {
                host.Open();
            }

            Console.WriteLine("Server is running");
            logger.Info("Server is running");
            Console.ReadLine();

            foreach (var host in hosts)
            {
                host.Close();
            }

            logger.Info("Server is closed");
        }
    }
}
