using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avida.ServiceBus.Libraries.Interface
{
    public interface IQueueClientFactory
    {
        IQueueClient GetQueueClient(string queueName, ReceiveMode recieveMode, double minimumBackoff = 5, double maximumBackoff = 30, int maximumRetryCount = 10);
        MessageReceiver GetMessageReceiver(string queueName, ReceiveMode recieveMode);
    }
}
