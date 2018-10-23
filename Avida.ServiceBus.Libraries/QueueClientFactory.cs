using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Azure.ServiceBus.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Avida.ServiceBus.Libraries
{
    public class QueueClientFactory
    {
        private readonly string _connectionString;
        private readonly TransportType _transportType;

        public QueueClientFactory(string connectionString, TransportType transportType)
        {
            _connectionString = connectionString;
            _transportType = transportType;
        }

        /// <summary>
        /// Creates queue client for a specified queue
        /// </summary>
        /// <param name="queueName">The queue name</param>
        /// <param name="recieveMode">The recieve mode to use indicating when the message should be removed from the queue</param>
        /// <param name="minimumBackoff">Minimum time in seconds to wait on retry. Defaults to 5 seconds</param>
        /// <param name="maximumBackoff">Maximum time in seconds to wait on retry. Defaults to 30 seconds</param>
        /// <param name="maximumRetryCount">Max number of retrys. Defaults to 10 trys</param>
        /// <returns></returns>
        /// <exception cref="ServiceBusException"></exception>
        public IQueueClient GetQueueClient(string queueName, ReceiveMode recieveMode, double minimumBackoff = 5, double maximumBackoff = 30, int maximumRetryCount = 10)
        {
            var client = new QueueClient(
                     GetConnectionStringKey("Endpoint"),
                     queueName,
                     GetTokenProvider(),
                     TransportType.AmqpWebSockets,
                     recieveMode,
                     GetRetryPolicy(minimumBackoff, maximumBackoff, maximumRetryCount));
            return client;
        }

        /// <summary>
        /// create and return the instance of MessageReceiver
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="recieveMode"></param>
        /// <returns></returns>
        public MessageReceiver GetMessageReceiver(string queueName, ReceiveMode recieveMode)
        {
            return new MessageReceiver(_connectionString, queueName, recieveMode);
        }

        /// <summary>
        /// Creates token provider from connection string
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ServiceBusException"></exception>
        private TokenProvider GetTokenProvider()
        {
            return TokenProvider.CreateSharedAccessSignatureTokenProvider(GetConnectionStringKey("SharedAccessKeyName"), GetConnectionStringKey("SharedAccessKey"));
        }

        /// <summary>
        /// Parses the connection string to get given key value
        /// </summary>
        /// <param name="keyName">The key name to parse out from connection string</param>
        /// <returns></returns>
        /// <exception cref="ServiceBusException"></exception>
        private string GetConnectionStringKey(string keyName)
        {
            try
            {
                string pattern = $"{keyName}=([^;]*)";
                var matches = Regex.Matches(_connectionString, pattern);
                if (matches.Count != 1)
                    throw new ServiceBusException($"Could not parse key {keyName} from service bus connection string. Expected 1 match but got {matches.Count} matches.");
                if (matches[0].Groups.Count != 2)
                    throw new ServiceBusException($"Could not parse key {keyName} from service bus connection string. Expected 1 group but got {matches[0].Groups.Count} groups.");
                return matches[0].Groups[1].Value;
            }
            catch (Exception exception)
            {
                throw new ServiceBusException($"Could not parse key { keyName } from service bus connection string.", exception);
            }
        }

        /// <summary>
        /// Creates a retry policy with given parameters
        /// </summary>
        /// <param name="minimumBackoff">Minimum time in seconds to wait on retry</param>
        /// <param name="maximumBackoff">Maximum time in seconds to wait on retry</param>
        /// <param name="maximumRetryCount">Max number of retrys</param>
        /// <returns></returns>
        private static RetryPolicy GetRetryPolicy(double minimumBackoff, double maximumBackoff, int maximumRetryCount)
        {
            return new RetryExponential(TimeSpan.FromSeconds(minimumBackoff), TimeSpan.FromSeconds(maximumBackoff), maximumRetryCount);
        }

    }
}
