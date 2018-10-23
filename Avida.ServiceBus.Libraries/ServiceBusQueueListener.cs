using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.ServiceFabric.Services.Communication.Runtime;

namespace Avida.ServiceBus.Libraries
{
    public class ServiceBusQueueListener : ICommunicationListener
    {
        private readonly Func<Message, CancellationToken, Task> _callBack;
        private IQueueClient _client;
        private readonly int _maxConcurrentCalls;
        private readonly bool _autoComplete;

        /// <summary>
        /// Instanciate a Azure ServiceBus listener
        /// </summary>
        /// <param name="queueClient">Service bus queue client to manage queues</param>
        /// <param name="callBack">The call back action to trigger on new message event</param>
        /// <param name="maxConcurrentCalls">The maximum number of concurrent calls that can be done to the call back function. Default to 1. Set too higher if you want to allow paralell message processing</param>
        /// <param name="autoComplete">Indicate if message should be automatically completed after returning from the call back function. If false, the message complete must be managed by the call back function provided</param>
        public ServiceBusQueueListener(IQueueClient queueClient, Func<Message, CancellationToken, Task> callBack, int maxConcurrentCalls = 1, bool autoComplete = false)
        {
            _client = queueClient;
            _callBack = callBack;
            _maxConcurrentCalls = maxConcurrentCalls;
            _autoComplete = autoComplete;
        }

        public void Abort()
        {
            Stop();
        }

        public Task CloseAsync(CancellationToken cancellationToken)
        {
            Stop();
            return Task.FromResult(true);
        }

        public Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = _maxConcurrentCalls,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = _autoComplete
            };

            _client.RegisterMessageHandler(async (m, c) => await _callBack.Invoke(m, c), messageHandlerOptions);

            return Task.FromResult(_client.QueueName);
        }

        private void Stop()
        {
            if (_client != null && !_client.IsClosedOrClosing)
            {
                _client.CloseAsync().Wait();
                _client = null;
            }
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            //TODO: Remove console output and implement NLogLogger logging

            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }
}
