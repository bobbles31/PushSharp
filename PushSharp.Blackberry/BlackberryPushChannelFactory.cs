namespace PushSharp.Blackberry
{
    using PushSharp.Common;

    public class BlackberryPushChannelFactory : ChannelFactory
    {
        private readonly BlackberryPushChannelSettings channelSettings;

        public BlackberryPushChannelFactory(BlackberryPushChannelSettings channelSettings)
        {
            this.channelSettings = channelSettings;
        }

        public PushChannel Create()
        {
            return new BlackberryPushChannel(this.channelSettings);
        }
    }
}