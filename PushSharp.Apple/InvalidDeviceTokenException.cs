namespace PushSharp.Apple
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidDeviceTokenException : Exception
    {
        public InvalidDeviceTokenException()
        {
        }

        public InvalidDeviceTokenException(string message)
            : base(message)
        {
        }

        public InvalidDeviceTokenException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidDeviceTokenException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}