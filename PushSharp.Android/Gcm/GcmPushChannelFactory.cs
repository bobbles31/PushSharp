namespace PushSharp.Android
{
    using PushSharp.Common;

    public class GcmPushChannelFactory : ChannelFactory
    {
        private readonly GcmPushChannelSettings channelSettings;

        public GcmPushChannelFactory(GcmPushChannelSettings channelSettings)
        {
            this.channelSettings = channelSettings;
        }

        public PushChannel Create()
        {
            return new GcmPushChannel(this.channelSettings);
        }
    }
}