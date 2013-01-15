namespace PushSharp.Windows
{
    using PushSharp.Common;

    public class WindowsPushService : PushServiceBase
    {
        public WindowsPushService(
            WindowsPushChannelSettings channelSettings, PushServiceSettings serviceSettings)
            : base(new WindowsPushChannelFactory(channelSettings), serviceSettings)
        {
        }

        public override PlatformType Platform
        {
            get
            {
                return PlatformType.Windows;
            }
        }
    }
}