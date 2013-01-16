namespace PushSharp.Apple
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class AppleNotificationToPayLoadConversionException : Exception
    {
        public AppleNotificationToPayLoadConversionException()
        {
        }

        public AppleNotificationToPayLoadConversionException(string message)
            : base(message)
        {
        }

        public AppleNotificationToPayLoadConversionException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected AppleNotificationToPayLoadConversionException(
            SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}