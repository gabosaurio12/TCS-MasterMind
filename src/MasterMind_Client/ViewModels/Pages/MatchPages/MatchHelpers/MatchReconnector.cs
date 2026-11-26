using log4net;
using MasterMind_Client.MatchService;
using MasterMind_Client.ServerEndpoints;
using System;
using System.ServiceModel;
using System.Threading.Tasks;

namespace MasterMind_Client.ViewModels.Pages.MatchPages.MatchHelpers
{
    public class MatchReconnector
    {
        private const int RetryDelayMilliseconds = 3000;
        private const int MaxAttempts = 20;

        private static readonly ILog logger = LogManager.GetLogger(typeof(MatchReconnector));

        private readonly MatchCallbackHandler callbackHandler;

        public MatchReconnector(MatchCallbackHandler callbackHandler)
        {
            this.callbackHandler = callbackHandler;
        }

        public async Task<MatchSession> ReconnectAsync(string username)
        {
            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                await Task.Delay(RetryDelayMilliseconds);
                var session = await TryConnectAsync(username);
                if (session != null)
                {
                    return session;
                }

                logger.WarnFormat("Reconnection attempt {0} of {1} failed.", attempt, MaxAttempts);
            }

            return null;
        }

        private async Task<MatchSession> TryConnectAsync(string username)
        {
            var client = ServiceClientFactory.CreateMatchClient(new InstanceContext(callbackHandler));
            try
            {
                await client.SubscribeAsync(username);
                var snapshot = await client.GetMatchSnapshotAsync(username);
                return new MatchSession(client, snapshot);
            }
            catch (FaultException faultException)
            {
                logger.Warn(faultException);
            }
            catch (CommunicationException communicationException)
            {
                logger.Warn(communicationException);
            }
            catch (TimeoutException timeoutException)
            {
                logger.Warn(timeoutException);
            }

            client.Abort();
            return null;
        }
    }
}
