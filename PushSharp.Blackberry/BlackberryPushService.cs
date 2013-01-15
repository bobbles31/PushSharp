namespace PushSharp.Blackberry
{
    using PushSharp.Common;

    public class BlackberryPushService : PushServiceBase
    {
        public BlackberryPushService(
            BlackberryPushChannelSettings channelSettings, PushServiceSettings serviceSettings)
            : base(new BlackberryPushChannelFactory(channelSettings), serviceSettings)
        {
        }
        
        public override PlatformType Platform
        {
            get
            {
                return PlatformType.Blackberry;
            }
        }
    }
}