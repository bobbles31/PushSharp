namespace PushSharp.Apple
{
    using PushSharp.Common;

    public class ApplePushChannelFactory : ChannelFactory
    {
        private readonly ApplePushChannelSettings applePushChannelSettings;

        public ApplePushChannelFactory(ApplePushChannelSettings applePushChannelSettings)
        {
            this.applePushChannelSettings = applePushChannelSettings;
        }

        public PushChannel Create()
        {
            return new ApplePushChannel(this.applePushChannelSettings);
        }
    }
}