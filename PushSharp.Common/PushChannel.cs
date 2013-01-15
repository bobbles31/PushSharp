namespace PushSharp.Common
{
    using System;

    public interface PushChannel : IDisposable
    {
        event Action<double> OnQueueTimed;

        PushChannelSettings ChannelSettings { get; }

        PushServiceSettings ServiceSettings { get; }

        PlatformType PlatformType { get; }

        int QueuedNotificationCount { get; }

        ChannelEvents Events { get; }

        void Stop(bool waitForQueueToDrain);

        void QueueNotification(Notification notification, bool countsAsRequeue = true);
    }
}