namespace PushSharp.Android
{
    using PushSharp.Common;

    public class GcmPushService : PushServiceBase
    {
        public GcmPushService(GcmPushChannelSettings channelSettings, PushServiceSettings serviceSettings = null)
            : base(new GcmPushChannelFactory(channelSettings), serviceSettings)
        {
        }

        public override PlatformType Platform
        {
            get
            {
                return PlatformType.AndroidGcm;
            }
        }
    }
}