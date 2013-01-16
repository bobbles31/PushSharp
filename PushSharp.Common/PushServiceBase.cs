namespace PushSharp.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class PushServiceBase : IDisposable
    {
        private readonly ChannelFactory channelFactory;

        private readonly ConcurrentQueue<Notification> queuedNotifications = new ConcurrentQueue<Notification>();

        private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        private ChannelLoadBalancer channelLoadBalancer;

        private ChannelScaler channelScaler;

        private Task distributerTask;

        private bool stopping;

        public PushServiceBase(ChannelFactory channelFactory, PushServiceSettings serviceSettings = null)
        {
            this.channelFactory = channelFactory;
            this.Events = new ChannelEvents();
            this.ServiceSettings = serviceSettings ?? new PushServiceSettings();
            
            this.queuedNotifications = new ConcurrentQueue<Notification>();

            ChannelScaler.Start();

            this.BeginSending();
        }

        public abstract PlatformType Platform { get; }

        public PushServiceSettings ServiceSettings { get; private set; }

        public ChannelEvents Events { get; set; }

        public bool IsStopping
        {
            get
            {
                return stopping;
            }
        }

        public ChannelLoadBalancer ChannelLoadBalancer
        {
            get
            {
                return this.channelLoadBalancer
                       ?? (this.channelLoadBalancer = new QueueLengthLoadBalancer(this.channelFactory, this.Events, this.Platform));
            }

            set
            {
                this.channelLoadBalancer = value;
            }
        }

        public ChannelScaler ChannelScaler
        {
            get
            {
                return this.channelScaler
                       ?? (this.ChannelScaler = new ChannelScalerImpl(this.ServiceSettings, this.ChannelLoadBalancer));
            }

            set
            {
                this.channelScaler = value;
            }
        }

        private void BeginSending()
        {
            this.distributerTask = new Task(this.Distributer, TaskCreationOptions.LongRunning);
            this.distributerTask.ContinueWith(ft => { var ex = ft.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            this.distributerTask.Start();

            this.stopping = false;
        }

        public void QueueNotification(Notification notification)
        {
            notification.EnqueuedTimestamp = DateTime.UtcNow;

            queuedNotifications.Enqueue(notification);
        }

        public void Stop(bool waitForQueueToFinish)
        {
            stopping = true;

            ChannelScaler.Stop();

            ChannelLoadBalancer.ShutdownAll(waitForQueueToFinish);

            Thread.Sleep(2000);

            this.cancelTokenSource.Cancel();
        }

        public void Dispose()
        {
            if (!stopping)
            {
                Stop(false);
            }
        }

        private void Distributer()
        {
            while (!this.cancelTokenSource.IsCancellationRequested)
            {
                if (!ChannelLoadBalancer.HasActiveChannels)
                {
                    Thread.Sleep(250);
                    continue;
                }

                Notification notification;

                if (!queuedNotifications.TryDequeue(out notification))
                {
                    Thread.Sleep(250);
                    continue;
                }

                ChannelLoadBalancer.Distribute(notification);
            }
        }
        
        private void newChannelOnQueueTimed(double queueTimeMilliseconds)
        {
            ChannelScaler.RecordQueueTime(queueTimeMilliseconds);
        }

        public PushChannel Create()
        {
            var newChannel = this.channelFactory.Create();
            newChannel.Events.RegisterProxyHandler(this.Events);
            newChannel.OnQueueTimed += this.newChannelOnQueueTimed;
            return newChannel;
        }
    }
}