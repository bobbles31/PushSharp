namespace PushSharp.Common.UnitTests.QueueLengthLoadBalancerTests
{
    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class GivenAQueueBalancerWithTwoChannels
    {
        private QueueLengthLoadBalancer balancer;

        private ChannelEvents eventsProxy;

        private PushChannel theEmptyChannel;

        private PushChannel theBusyChannel;

        private Notification theNotification;

        [TestFixtureSetUp]
        public void WhenBalancerIsShutdown()
        {
            this.eventsProxy = new ChannelEvents();

            var channelFactory = Substitute.For<ChannelFactory>();

            this.GivenAnEmptyChannel();
            this.GivenABusyChannel();

            channelFactory.Create().Returns(this.theEmptyChannel, theBusyChannel);

            this.balancer = new QueueLengthLoadBalancer(channelFactory, this.eventsProxy, PlatformType.None);

            this.theNotification = Substitute.For<Notification>();

            this.balancer.AddChannels(2);
            this.balancer.Distribute(this.theNotification);
        }

        [Test]
        public void ThenTheNotificationShouldBeQueuedToTheEmptyChannel()
        {
            theEmptyChannel.Received(1).QueueNotification(this.theNotification);
        }

        private void GivenABusyChannel()
        {
            this.theBusyChannel = this.AChannel(2);
        }

        private void GivenAnEmptyChannel()
        {
            this.theEmptyChannel = this.AChannel(0);
        }

        private PushChannel AChannel(int queuedEventCount)
        {
            var channelEvents = new ChannelEvents();
            var theChannel = Substitute.For<PushChannel>();
            theChannel.Events.Returns(channelEvents);
            theChannel.QueuedNotificationCount.Returns(queuedEventCount);
            return theChannel;
        }
    }
}