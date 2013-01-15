namespace PushSharp.Common.UnitTests
{
    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class PushServiceBaseTests
    {
        private PushChannelBase pushChannel;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
            var channelSettings = Substitute.For<PushChannelSettings>();
            var serviceSettings = new PushServiceSettings();
            this.pushChannel = Substitute.For<PushChannelBase>(channelSettings, serviceSettings);
            
            var notification = Substitute.For<Notification>();
            pushChannel.QueueNotification(notification);
        }

        [Test]
        public void Test()
        {
            
        }
    }
}
