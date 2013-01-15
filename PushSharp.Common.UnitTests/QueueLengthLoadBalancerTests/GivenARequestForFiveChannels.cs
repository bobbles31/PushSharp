namespace PushSharp.Common.UnitTests.QueueLengthLoadBalancerTests
{
    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class GivenARequestForFiveChannels
    {
        private QueueLengthLoadBalancer balancer;

        private ChannelEvents eventsProxy;

        private int eventProxyChannelCreatedCallCount;

        private int eventProxyChannelCreatedLastChannelCount;

        [TestFixtureSetUp]
        public void GivenAQueueBalancer()
        {
            var channelEvents = new ChannelEvents();
            this.eventsProxy = new ChannelEvents();
            this.eventsProxy.OnChannelCreated += this.eventsProxyOnChannelCreated;

            var channelFactory = Substitute.For<ChannelFactory>();
            var channel = Substitute.For<PushChannel>();
            channel.Events.Returns(channelEvents);
            channelFactory.Create().Returns(channel);

            this.balancer = new QueueLengthLoadBalancer(channelFactory, this.eventsProxy, PlatformType.None);

            this.balancer.AddChannels(5);
        }
        
        [Test]
        public void ThenThereShouldBeFiveChannelsInTheBalancer()
        {
            Assert.AreEqual(5, this.balancer.ChannelCount);
        }

        [Test]
        public void ThenTheChannelCreatedEventShouldBeRaised()
        {
            Assert.AreEqual(1, this.eventProxyChannelCreatedCallCount);
        }

        [Test]
        public void ThenTheChannelCreatedEventShouldShowFiveChannelsCreated()
        {
            Assert.AreEqual(5, this.eventProxyChannelCreatedLastChannelCount);
        }

        private void eventsProxyOnChannelCreated(PlatformType platformType, int newChannelCount)
        {
            this.eventProxyChannelCreatedCallCount++;
            this.eventProxyChannelCreatedLastChannelCount = newChannelCount;
        }
    }
}