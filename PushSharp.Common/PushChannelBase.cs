namespace PushSharp.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class PushChannelBase : PushChannel
    {
        private readonly ConcurrentQueue<Notification> queuedNotifications;

        private readonly ManualResetEventSlim waitQueuedNotification;

        public event Action<double> OnQueueTimed;

        public PushChannelBase(PushChannelSettings channelSettings, PushServiceSettings serviceSettings = null)
		{
            this.Events = new ChannelEvents();
			this.Stopping = false;
			this.CancelTokenSource = new CancellationTokenSource();
			this.CancelToken = CancelTokenSource.Token;

			this.queuedNotifications = new ConcurrentQueue<Notification>();
		
			this.ChannelSettings = channelSettings;
			this.ServiceSettings = serviceSettings ?? new PushServiceSettings();
			this.waitQueuedNotification = new ManualResetEventSlim();

			// Start our sending task
			TaskSender = new Task(this.Sender, TaskCreationOptions.LongRunning);
			TaskSender.ContinueWith((t) => { var ex = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
			TaskSender.Start();
		}

    protected bool Stopping { get; set; }

    protected Task TaskSender { get; set; }

    protected CancellationTokenSource CancelTokenSource { get; set; }

    protected CancellationToken CancelToken { get; set; }

    public ChannelEvents Events { get; private set; }

    public PushChannelSettings ChannelSettings { get; private set; }
		
        public PushServiceSettings ServiceSettings { get; private set; }
        
		protected abstract void SendNotification(Notification notification);

        public abstract PlatformType PlatformType { get; }

		public virtual void Stop(bool waitForQueueToDrain)
		{
			Stopping = true;

			if (waitQueuedNotification != null)
				waitQueuedNotification.Set();

			//See if we want to wait for the queue to drain before stopping
			if (waitForQueueToDrain)
			{
				while (QueuedNotificationCount > 0)
					Thread.Sleep(50);
			}

			//Sleep a bit to prevent any race conditions
			Thread.Sleep(2000);

			if (!CancelTokenSource.IsCancellationRequested)
				CancelTokenSource.Cancel();

			//Wait on our tasks for a maximum of 30 seconds
			Task.WaitAll(new Task[] { TaskSender }, 30000);
		}

		public virtual void Dispose()
		{
			//Stop without waiting
			if (!Stopping)
				Stop(false);
		}

		public int QueuedNotificationCount
		{
			get { return queuedNotifications.Count; }
		}

		public void QueueNotification(Notification notification, bool countsAsRequeue = true)
		{
            if (this.CancelToken.IsCancellationRequested)
            {
                Events.RaiseChannelException(new ObjectDisposedException("Channel", "Channel has already been signaled to stop"), this.PlatformType, notification);
                return;
            }

			//If the count is -1, it can be queued infinitely, otherwise check that it's less than the max
			if (this.ServiceSettings.MaxNotificationRequeues < 0 || notification.QueuedCount <= this.ServiceSettings.MaxNotificationRequeues)
			{
				//Reset the Enqueued time in case this is a requeue
				notification.EnqueuedTimestamp = DateTime.UtcNow;

				//Increase the queue counter
				if (countsAsRequeue)
					notification.QueuedCount++;

				queuedNotifications.Enqueue(notification);

				//Signal a possibly wait-stated Sender loop that there's work to do
				waitQueuedNotification.Set();
			}
			else
				Events.RaiseNotificationSendFailure(notification, new MaxSendAttemptsReachedException());
		}

		void Sender()
		{
			while (!this.CancelToken.IsCancellationRequested || QueuedNotificationCount > 0)
			{
				Notification notification = null;

				if (!queuedNotifications.TryDequeue(out notification))
				{
					//No notifications in queue, go into wait state
					waitQueuedNotification.Reset();
					try { waitQueuedNotification.Wait(5000, this.CancelToken); }
					catch { }
					continue;
				}

				//Report back the time in queue
				var timeInQueue = DateTime.UtcNow - notification.EnqueuedTimestamp;
				if (OnQueueTimed != null)
					OnQueueTimed(timeInQueue.TotalMilliseconds);

				//Send it
				this.SendNotification(notification);
			}
		}

	}
}
