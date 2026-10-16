using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace MasterMind_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var hosts = new List<ServiceHost>();
            hosts.Add(new ServiceHost(typeof(Services.PlayerService)));
            hosts.Add(new ServiceHost(typeof(Services.CodeService)));

            foreach (var host in hosts)
            {
                host.Open();
            }

            Console.WriteLine("Server is running");
            Console.ReadLine();

            foreach (var host in hosts)
            {
                host.Close();
            }
        }
    }
}
