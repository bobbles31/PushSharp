namespace PushSharp.Common.UnitTests.QueueLengthLoadBalancerTests
{
    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class GivenAQueueBalancerWithThreeChannels
    {
        private QueueLengthLoadBalancer balancer;

        private ChannelEvents eventsProxy;

        private int eventProxyChannelDestroyedCallCount;

        private int eventProxyChannelDestroyedLastChannelCount;

        [TestFixtureSetUp]
        public void WhenTwoChannelsAreRemoved()
        {
            var channelEvents = new ChannelEvents();
            this.eventsProxy = new ChannelEvents();
            this.eventsProxy.OnChannelDestroyed += this.EventsProxyOnChannelDestroyed;

            var channelFactory = Substitute.For<ChannelFactory>();
            var channel = Substitute.For<PushChannel>();
            channel.Events.Returns(channelEvents);
            channelFactory.Create().Returns(channel);

            this.balancer = new QueueLengthLoadBalancer(channelFactory, this.eventsProxy, PlatformType.None);

            this.balancer.AddChannels(3);
            this.balancer.RemoveChannels(5);
        }
        
        [Test]
        public void ThenThereShouldBeNoChannelsInTheBalancer()
        {
            Assert.AreEqual(0, this.balancer.ChannelCount);
        }

        [Test]
        public void ThenTheChannelDestroyedEventShouldBeRaised()
        {
            Assert.AreEqual(1, this.eventProxyChannelDestroyedCallCount);
        }

        [Test]
        public void ThenTheChannelCreatedDestroyedEventShouldShowNoChannelsRemaining()
        {
            Assert.AreEqual(0, this.eventProxyChannelDestroyedLastChannelCount);
        }
        
        private void EventsProxyOnChannelDestroyed(PlatformType platformType, int newChannelCount)
        {
            this.eventProxyChannelDestroyedCallCount++;
            this.eventProxyChannelDestroyedLastChannelCount = newChannelCount;
        }
    }
}