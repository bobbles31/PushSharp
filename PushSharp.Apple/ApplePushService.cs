namespace PushSharp.Apple
{
    using System;
    using System.Threading;

    using PushSharp.Common;

    public class ApplePushService : PushServiceBase
    {
        private readonly FeedbackService feedbackService;

        private readonly CancellationTokenSource cancelTokenSource;

        private Timer timerFeedback;

        public ApplePushService(ApplePushChannelSettings channelSettings, PushServiceSettings serviceSettings = null)
            : base(new ApplePushChannelFactory(channelSettings), serviceSettings)
        {
            var appleChannelSettings = channelSettings;
            cancelTokenSource = new CancellationTokenSource();
            feedbackService = new FeedbackService();
            feedbackService.OnFeedbackReceived +=
                this.feedbackService_OnFeedbackReceived;

            // allow control over feedback call interval, if set to zero, don't make feedback calls automatically
            if (appleChannelSettings.FeedbackIntervalMinutes > 0)
            {
                timerFeedback = new Timer(
                    state =>
                        {
                            try
                            {
                                this.feedbackService.Run(
                                    channelSettings, this.cancelTokenSource.Token);
                            }
                            catch (Exception ex)
                            {
                                this.Events.RaiseChannelException(ex, PlatformType.Apple);
                            }

                            // Timer will run first after 10 seconds, then every 10 minutes to get feedback!
                        },
                    null,
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromMinutes(appleChannelSettings.FeedbackIntervalMinutes));

            }
        }

        private void feedbackService_OnFeedbackReceived(string deviceToken, DateTime timestamp)
        {
            this.Events.RaiseDeviceSubscriptionExpired(PlatformType.Apple, deviceToken);
        }

        public override PlatformType Platform
        {
            get
            {
                return PlatformType.Apple;
            }
        }
    }
}