namespace PushSharp.Apple
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class DeviceTokenStringToByteArrayParseException : Exception
    {
        public DeviceTokenStringToByteArrayParseException()
        {
        }

        public DeviceTokenStringToByteArrayParseException(string message)
            : base(message)
        {
        }

        public DeviceTokenStringToByteArrayParseException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected DeviceTokenStringToByteArrayParseException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}