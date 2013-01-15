namespace PushSharp.Apple
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidDeviceTokenBinarySizeException : Exception
    {
        public InvalidDeviceTokenBinarySizeException()
        {
        }

        public InvalidDeviceTokenBinarySizeException(string message)
            : base(message)
        {
        }

        public InvalidDeviceTokenBinarySizeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected InvalidDeviceTokenBinarySizeException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}