namespace PushSharp.Apple
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    public class AppleNotification : Common.Notification
    {
        public static readonly DateTime DoNotStore = DateTime.MinValue;

        private static readonly object NextIdentifierLock = new object();

        private static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static int nextIdentifier = 1;

        public AppleNotification()
        {
            this.Platform = Common.PlatformType.Apple;

            Payload = new AppleNotificationPayload();

            Identifier = GetNextIdentifier();
        }

        public AppleNotification(DeviceToken deviceToken)
        {
            DeviceToken = deviceToken;

            Payload = new AppleNotificationPayload();

            Identifier = GetNextIdentifier();
        }

        public AppleNotification(string deviceToken)
        {
            DeviceToken = new DeviceToken(deviceToken);
            Payload = new AppleNotificationPayload();

            Identifier = GetNextIdentifier();
        }

        public AppleNotification(string deviceToken, AppleNotificationPayload payload)
        {
            DeviceToken = new DeviceToken(deviceToken);
            Payload = payload;

            Identifier = GetNextIdentifier();
        }

        public DeviceToken DeviceToken { get; set; }

        public int Identifier { get; private set; }

        public AppleNotificationPayload Payload { get; set; }

        /// <summary>
        /// Gets or sets the expiration date after which Apple will no longer store and forward this push notification.
        /// If no value is provided, an assumed value of one year from now is used.  If you do not wish
        /// for Apple to store and forward, set this value to Notification.DoNotStore.
        /// </summary>
        public DateTime? Expiration { get; set; }

        public override bool IsValidDeviceRegistrationId()
        {
            var r = new System.Text.RegularExpressions.Regex(
                @"^[0-9A-F]+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return r.Match(this.DeviceToken.AsString).Success;
        }

        public override string ToString()
        {
            return Payload.ToJson();
        }

        public byte[] ToBytes()
        {
            // Without reading the response which would make any identifier useful, it seems silly to
            // expose the value in the object model, although that would be easy enough to do. For
            // now we'll just use zero.
            byte[] identifierBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Identifier));

            // APNS will not store-and-forward a notification with no expiry, so set it one year in the future
            // if the client does not provide it.
            int expiryTimeStamp = -1;
            if (Expiration != DoNotStore)
            {
                DateTime concreteExpireDateUtc = (Expiration ?? DateTime.UtcNow.AddMonths(1)).ToUniversalTime();
                TimeSpan epochTimeSpan = concreteExpireDateUtc - UNIX_EPOCH;
                expiryTimeStamp = (int)epochTimeSpan.TotalSeconds;
            }

            byte[] expiry = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(expiryTimeStamp));

            byte[] payload = Payload.AsByteArray();

            byte[] payloadSize = Payload.SizeInBytes();

            var notificationParts = new List<byte[]>();

            notificationParts.Add(new byte[] { 0x01 }); // Enhanced notification format command
            notificationParts.Add(identifierBytes);
            notificationParts.Add(expiry);
            notificationParts.Add(DeviceToken.TokenSizeAsBytes);
            notificationParts.Add(DeviceToken.AsByteArray);
            notificationParts.Add(payloadSize);
            notificationParts.Add(payload);

            return BuildBufferFrom(notificationParts);
        }

        private byte[] BuildBufferFrom(IList<byte[]> bufferParts)
        {
            int bufferSize = 0;
            for (int i = 0; i < bufferParts.Count; i++)
            {
                bufferSize += bufferParts[i].Length;
            }

            var buffer = new byte[bufferSize];

            int position = 0;

            for (int i = 0; i < bufferParts.Count; i++)
            {
                byte[] part = bufferParts[i];
                Buffer.BlockCopy(bufferParts[i], 0, buffer, position, part.Length);
                position += part.Length;
            }

            return buffer;
        }

        private static int GetNextIdentifier()
        {
            lock (NextIdentifierLock)
            {
                if (nextIdentifier >= int.MaxValue - 10)
                {
                    nextIdentifier = 1;
                }

                return nextIdentifier++;
            }
        }
    }
}
