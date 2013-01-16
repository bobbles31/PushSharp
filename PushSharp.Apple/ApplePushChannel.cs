namespace PushSharp.Apple
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;

    using PushSharp.Common;

    public class ApplePushChannel : PushChannelBase
	{
		#region Constants
		
        private const int InitialReconnectDelay = 3000;
        private const float ReconnectBackoffMultiplier = 1.5f;

		#endregion

        private readonly ApplePushChannelSettings appleSettings;

        private readonly object streamWriteLock = new object();
        
        private readonly X509Certificate certificate;
        
        private readonly X509CertificateCollection certificates;

        private readonly byte[] readBuffer = new byte[6];

        private readonly Task taskCleanup;

        private readonly PendingItemsQueue pendingItems = new PendingItemsQueue();

        private DateTime lastConnectionDateTime = DateTime.MaxValue;

        private int reconnectDelay = 3000;

        private bool connected;
        
        private TcpClient client;
        private SslStream stream;
        private System.IO.Stream networkStream;

		public ApplePushChannel(ApplePushChannelSettings channelSettings, PushServiceSettings serviceSettings = null) 
            : base(channelSettings, serviceSettings)
		{
			this.appleSettings = channelSettings;

			certificate = this.appleSettings.Certificate;

            certificates = new X509CertificateCollection();

            if (appleSettings.AddLocalAndMachineCertificateStores)
            {
                var store = new X509Store(StoreLocation.LocalMachine);
                certificates.AddRange(store.Certificates);

                store = new X509Store(StoreLocation.CurrentUser);
                certificates.AddRange(store.Certificates);
            }

			certificates.Add(certificate);

		    if (this.appleSettings.AdditionalCertificates != null)
		    {
		        foreach (var addlCert in this.appleSettings.AdditionalCertificates)
		        {
		            certificates.Add(addlCert);
		        }
		    }

		    // Start our cleanup task
			taskCleanup = new Task(this.Cleanup, TaskCreationOptions.LongRunning);
			taskCleanup.ContinueWith(t => { var ex = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
			taskCleanup.Start();
		}

        public delegate void ConnectingDelegate(string host, int port);

        public delegate void ConnectionFailureDelegate(ConnectionFailureException exception);

        public delegate void WaitBeforeReconnectDelegate(int millisecondsToWait);

        public delegate void ConnectedDelegate(string host, int port);

        public event ConnectedDelegate OnConnected;

        public event ConnectingDelegate OnConnecting;

        public event ConnectionFailureDelegate OnConnectionFailure;

        public event WaitBeforeReconnectDelegate OnWaitBeforeReconnect;

        public override PlatformType PlatformType
        {
            get { return PlatformType.Apple; }
        }

        private bool ShouldCleanQueue
        {
            get
            {
                return pendingItems.ItemsPending
                       && (connected || CancelToken.IsCancellationRequested)
                       && lastConnectionDateTime < DateTime.UtcNow.AddMilliseconds(-1 * appleSettings.MillisecondsToWaitBeforeMessageDeclaredSuccess);
            }
        }
		
        protected override void SendNotification(Notification notification)
		{
			var appleNotification = notification as AppleNotification;

		    bool isOkToSend = true;
		    var notificationData = new byte[] { };

		    try
			{
				notificationData = appleNotification.ToBytes();
			}
			catch (NotificationFailureException nfex)
			{
				// Bad notification format already
				isOkToSend = false;

				this.Events.RaiseNotificationSendFailure(notification, nfex);
			}

			if (isOkToSend)
			{
				Connect();
				
				try 
				{
				    lock (streamWriteLock)
				    {
				        networkStream.Write(notificationData, 0, notificationData.Length);
				        pendingItems.QueueItem(new SentNotification(appleNotification));
				    }
				}
				catch (Exception)
				{ 
					this.QueueNotification(notification); 
				} // If this failed, we probably had a networking error, so let's requeue the notification
			}
		}

		public override void Stop(bool waitForQueueToDrain)
		{
			Stopping = true;

			// See if we want to wait for the queue to drain before stopping
			if (waitForQueueToDrain)
			{
				while (QueuedNotificationCount > 0 || pendingItems.Count >0 /* sentNotifications.Count > 0*/)
				{
				    Thread.Sleep(50);
				}
			}

			// Sleep a bit to prevent any race conditions
			// especially since our cleanup method may need 3 seconds
			Thread.Sleep(5000);

			if (!CancelTokenSource.IsCancellationRequested)
			{
			    CancelTokenSource.Cancel();
			}

			// Wait on our tasks for a maximum of 30 seconds
			Task.WaitAll(new[] { this.TaskSender, taskCleanup }, 30000);
		}
		
		private void ListenForErrorPacket()
		{
			try
			{
			    networkStream.BeginRead(readBuffer, 0, 6, this.ReadStream, null);
			}
			catch
			{
				this.setDisconnected();
			}
		}

        private void ReadStream(IAsyncResult asyncResult)
        {
            try
            {
                var bytesRead = this.networkStream.EndRead(asyncResult);

                if (bytesRead > 0)
                {
                    // We now expect apple to close the connection on us anyway, so let's try and close things
                    // up here as well to get a head start
                    // Hopefully this way we have less messages written to the stream that we have to requeue
                    try
                    {
                        this.stream.Close();
                        this.stream.Dispose();
                    }
                    catch
                    {
                    }

                    try
                    {
                        this.client.Close();
                        this.stream.Dispose();
                    }
                    catch
                    {
                    }

                    // Get the enhanced format response
                    // byte 0 is always '1', byte 1 is the status, bytes 2,3,4,5 are the identifier of the notification
                    var status = this.readBuffer[1];
                    var identifier = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(this.readBuffer, 2));

                    var failedQueueState = pendingItems.FailureAt(identifier);

                    RaiseSuccessEventForSuceededItems(failedQueueState.CompletedItems);

                    this.Events.RaiseNotificationSendFailure(
                        failedQueueState.FailedNotification.Notification,
                        new NotificationFailureException(status, failedQueueState.FailedNotification.Notification));

                    RequeueDiscardedNotifications(failedQueueState.ItemsIgnoredByApple);

                    this.ListenForErrorPacket();
                }
                else
                {
                    this.setDisconnected();
                }
            }
            catch
            {
                this.setDisconnected();
            }
        }

        private void RequeueDiscardedNotifications(IEnumerable<SentNotification> itemsIgnoredByApple)
        {
            foreach (var item in itemsIgnoredByApple)
            {
                this.QueueNotification(item.Notification, false);
            }
        }

        private void RaiseSuccessEventForSuceededItems(IEnumerable<SentNotification> completedItems)
        {
            foreach (var item in completedItems)
            {
                this.Events.RaiseNotificationSent(item.Notification);
            }
        }

        void Cleanup()
		{
			while (true)
			{
				bool wasRemoved = false;

			    if (pendingItems.Count > 0)
			    {
			        if (ShouldCleanQueue)
			        {
			            var succesfulItems =
			                pendingItems.ExtractItemsSentBefore(
			                    DateTime.UtcNow.AddMilliseconds(
			                        -1 * appleSettings.MillisecondsToWaitBeforeMessageDeclaredSuccess)).ToList();

			            this.RaiseSuccessEventForSuceededItems(succesfulItems);
			        }

			    }

			    if (this.CancelToken.IsCancellationRequested)
					break;
				else if (!wasRemoved)
					Thread.Sleep(250);
			}
		}

		void Connect()
		{
			while (this.CanConnect)
			{
				try
				{
					connect();
					this.setConnected();
				}
				catch (ConnectionFailureException ex)
				{
					this.setDisconnected();

					//Report the error
					var cf = this.OnConnectionFailure;

					if (cf != null)
						cf(ex);

					this.Events.RaiseChannelException(ex, PlatformType.Apple);
				}

				if (!connected)
				{
					//Notify we are waiting before reconnecting
					var eowbrd = this.OnWaitBeforeReconnect;
					if (eowbrd != null)
						eowbrd(reconnectDelay);

					int slept = 0;
					while (slept <= reconnectDelay && !this.CancelToken.IsCancellationRequested)
					{
						Thread.Sleep(250);
						slept += 250;
					}

					this.IncrementReconnectDelay();
				}
				else
				{
				    this.ResetReconnectDelay();

				    this.RaiseConnectedEvent();
				}
			}
		}

        private void RaiseConnectedEvent()
        {
            //Notify we are connected
            var eoc = this.OnConnected;
            if (eoc != null)
            {
                eoc(this.appleSettings.Host, this.appleSettings.Port);
            }
        }

        private void ResetReconnectDelay()
        {
            //Reset connect delay
            this.reconnectDelay = InitialReconnectDelay;
        }

        private void IncrementReconnectDelay()
        {
            this.reconnectDelay = (int)(ReconnectBackoffMultiplier * this.reconnectDelay);
        }

        private bool CanConnect
        {
            get
            {
                return !this.connected && !this.CancelToken.IsCancellationRequested;
            }
        }

        void connect()
		{
			client = new TcpClient();

			//Notify we are connecting
			var eoc = this.OnConnecting;
			if (eoc != null)
				eoc(this.appleSettings.Host, this.appleSettings.Port);

			try
			{
				client.Connect(this.appleSettings.Host, this.appleSettings.Port);
			}
			catch (Exception ex)
			{
				throw new ConnectionFailureException("Connection to Host Failed", ex);
			}

			if (appleSettings.SkipSsl)
			{
				networkStream = client.GetStream();
			}
			else
			{
				stream = new SslStream(client.GetStream(), false,
					new RemoteCertificateValidationCallback((sender, cert, chain, sslPolicyErrors) => { return true; }),
					new LocalCertificateSelectionCallback((sender, targetHost, localCerts, remoteCert, acceptableIssuers) =>
					{
						return certificate;
					}));

				try
				{
					stream.AuthenticateAsClient(this.appleSettings.Host, this.certificates, System.Security.Authentication.SslProtocols.Ssl3, false);
					//stream.AuthenticateAsClient(this.appleSettings.Host);
				}
				catch (System.Security.Authentication.AuthenticationException ex)
				{
					throw new ConnectionFailureException("SSL Stream Failed to Authenticate as Client", ex);
				}

				if (!stream.IsMutuallyAuthenticated)
					throw new ConnectionFailureException("SSL Stream Failed to Authenticate", null);

				if (!stream.CanWrite)
					throw new ConnectionFailureException("SSL Stream is not Writable", null);

				networkStream = stream;
			}
			
			ListenForErrorPacket();
		}
		
        private void setConnected()
        {
            lastConnectionDateTime = DateTime.Now;
            connected = true;
        }

        private void setDisconnected()
        {
            lastConnectionDateTime = DateTime.MaxValue;
            this.connected = true;
        }
	}
}