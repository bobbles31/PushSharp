namespace PushSharp.Android
{
    using PushSharp.Common;

    public class C2dmPushChannelFactory : ChannelFactory
    {
        private readonly C2dmPushChannelSettings channelSettings;

        public C2dmPushChannelFactory(C2dmPushChannelSettings channelSettings)
        {
            this.channelSettings = channelSettings;
        }

        public PushChannel Create()
        {
            return new C2dmPushChannel(this.channelSettings);
        }
    }
}