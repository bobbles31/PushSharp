namespace PushSharp.WindowsPhone
{
    using PushSharp.Common;

    public class WindowsPhonePushService : PushServiceBase
    {
        public WindowsPhonePushService(
            WindowsPhonePushChannelSettings channelSettings, PushServiceSettings serviceSettings = null)
            : base(new WindowsPhoneChannelFactory(channelSettings), serviceSettings)
        {
        }

        public override PlatformType Platform
        {
            get
            {
                return PlatformType.WindowsPhone;
            }
        }
    }
}