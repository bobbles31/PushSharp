namespace PushSharp.WindowsPhone
{
    using PushSharp.Common;

    public class WindowsPhoneChannelFactory : ChannelFactory
    {
        private readonly WindowsPhonePushChannelSettings channelSettings;

        public WindowsPhoneChannelFactory(WindowsPhonePushChannelSettings channelSettings)
        {
            this.channelSettings = channelSettings;
        }

        public PushChannel Create()
        {
            return new WindowsPhonePushChannel(this.channelSettings);
        }
    }
}