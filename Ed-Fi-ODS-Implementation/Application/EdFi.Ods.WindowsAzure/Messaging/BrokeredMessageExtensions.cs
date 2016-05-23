using System;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.ServiceBus.Messaging;

namespace EdFi.Ods.WindowsAzure.Messaging
{
    public static class BrokeredMessageExtensions
    {
        public static object GetMessage(this BrokeredMessage receivedMessage)
        {
            var typeNameAsObject = receivedMessage.Properties["TypeName"];

            // No type name specified?  We can't deserialize...
            if (typeNameAsObject == null)
                throw new Exception(string.Format((string) "'TypeName' not found in the properties of BrokeredMessage '{0}'.  Unable to deserialize message body without type information.", (object) receivedMessage.MessageId));

            var messageTypeName = typeNameAsObject.ToString();
            var messageType = Type.GetType(messageTypeName);

            var getBodyMethod = GetClosedGenericMethod(messageType);

            var messageBody = getBodyMethod.Invoke(receivedMessage, null);

            return messageBody;
        }

        private static MethodInfo getBodyOpenGenericMethod;

        private static ConcurrentDictionary<Type, MethodInfo> closedGenericMethodByMessageType
            = new ConcurrentDictionary<Type, MethodInfo>();

        private static MethodInfo GetClosedGenericMethod(Type messageType)
        {
            // Initialize the open generic method if it hasn't already been initialized
            if (getBodyOpenGenericMethod == null)
                getBodyOpenGenericMethod = typeof(BrokeredMessage).GetMethod("GetBody", new Type[0]);

            return closedGenericMethodByMessageType
                .GetOrAdd(messageType, t =>
                    getBodyOpenGenericMethod.MakeGenericMethod(messageType)); // Note: Switching 'messageType' to 't' caused unit test failures
        }
    }
}