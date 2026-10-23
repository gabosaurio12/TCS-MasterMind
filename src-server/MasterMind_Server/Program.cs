using log4net;
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
            var hosts = new List<ServiceHost>
            {
                new ServiceHost(typeof(Services.PlayerService)),
                new ServiceHost(typeof(Services.CodeService)),
                new ServiceHost(typeof(Services.FriendsService))
            };

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
