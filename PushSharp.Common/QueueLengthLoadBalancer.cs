namespace PushSharp.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class QueueLengthLoadBalancer : ChannelLoadBalancer
    {
        private readonly ChannelFactory channelFactory;

        private readonly ChannelEvents eventsProxy;

        private readonly List<PushChannel> channels = new List<PushChannel>();

        public QueueLengthLoadBalancer(ChannelFactory channelFactory, ChannelEvents eventsProxy, PlatformType platform)
        {
            this.Platform = platform;
            this.channelFactory = channelFactory;
            this.eventsProxy = eventsProxy;
        }

        protected PlatformType Platform { get; private set; }

        public int ChannelCount
        {
            get
            {
                return channels.Count;
            }
        }

        public bool HasActiveChannels
        {
            get
            {
                return channels.Count > 0;
            }
        }

        public void AddChannels(int channelCount)
        {
            lock (this.channels)
            {
                for (int i = 0; i < channelCount; i++)
                {
                    var newChannel = this.channelFactory.Create();
                    newChannel.Events.RegisterProxyHandler(this.eventsProxy);
                    this.channels.Add(newChannel);
                }
            }

            this.eventsProxy.RaiseChannelCreated(this.Platform, this.channels.Count);
        }

        public void RemoveChannels(int channelCount)
        {
            lock (this.channels)
            {
                for (int i = 0; i < channelCount; i++)
                {
                    var selectedForRemoval = this.TheLeastBusyChannel();
                    if (null == selectedForRemoval)
                    {
                        break;
                    }

                    this.channels.Remove(selectedForRemoval);

                    selectedForRemoval.Stop(true);

                    selectedForRemoval.Events.UnRegisterProxyHandler(this.eventsProxy);
                }

                this.eventsProxy.RaiseChannelDestroyed(this.Platform, this.channels.Count);
            }
        }

        public void Distribute(Notification notification)
        {
            var selectedChannel = this.TheLeastBusyChannel();

            if (selectedChannel != null)
            {
                notification.EnqueuedTimestamp = DateTime.UtcNow;
                selectedChannel.QueueNotification(notification);
            }
        }

        public void ShutdownAll(bool allowQueuesToFinish)
        {
            Parallel.ForEach(
                channels,
                (channel) =>
                    {
                        channel.Stop(allowQueuesToFinish);
                        channel.Events.UnRegisterProxyHandler(this.eventsProxy);
                    });

            this.channels.Clear();
        }

        private PushChannel TheLeastBusyChannel()
        {
            return this.channels.OrderBy(c => c.QueuedNotificationCount).FirstOrDefault();
        }
    }
}