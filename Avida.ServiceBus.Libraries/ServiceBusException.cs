using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Avida.ServiceBus.Libraries
{
    [Serializable]
    public class ServiceBusException : Exception
    {
        public ServiceBusException()
        {
        }

        public ServiceBusException(string message) : base(message)
        {
        }

        public ServiceBusException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ServiceBusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
