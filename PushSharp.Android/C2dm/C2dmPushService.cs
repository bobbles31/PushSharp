namespace PushSharp.Android
{
    using System;

    using PushSharp.Common;

    [Obsolete("Google has Deprecated C2DM, and you should now use GCM Instead.")]
    public class C2dmPushService : PushServiceBase
    {
        public C2dmPushService(C2dmPushChannelSettings channelSettings, PushServiceSettings serviceSettings = null)
            : base(new C2dmPushChannelFactory(channelSettings), serviceSettings)
        {
        }

        public override PlatformType Platform
        {
            get
            {
                return PlatformType.AndroidC2dm;
            }
        }
    }
}