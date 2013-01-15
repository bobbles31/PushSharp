namespace PushSharp.Common.UnitTests
{
    public class TestablePushService : PushChannelBase
    {
        public TestablePushService(PushChannelSettings channelSettings, PushServiceSettings serviceSettings = null)
            : base(channelSettings, serviceSettings)
        {
        }

        public override PlatformType PlatformType
        {
            get
            {
                return PlatformType.None;
            }
        }

        protected override void SendNotification(Notification notification)
        {
            throw new System.NotImplementedException();
        }
    }
}
