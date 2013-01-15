namespace PushSharp.Windows
{
    using PushSharp.Common;

    public class WindowsPushChannelFactory : ChannelFactory
    {
        private readonly WindowsPushChannelSettings channelSettings;

        public WindowsPushChannelFactory(WindowsPushChannelSettings channelSettings)
        {
            this.channelSettings = channelSettings;
        }

        public PushChannel Create()
        {
            return new WindowsPushChannel(this.channelSettings);
        }
    }
}