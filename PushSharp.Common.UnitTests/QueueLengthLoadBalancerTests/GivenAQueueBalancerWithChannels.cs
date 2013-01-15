namespace PushSharp.Common.UnitTests.QueueLengthLoadBalancerTests
{
    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class GivenAQueueBalancerAChannels
    {
        private QueueLengthLoadBalancer balancer;

        private ChannelEvents eventsProxy;

        private PushChannel theChannel;

        [TestFixtureSetUp]
        public void WhenBalancerIsShutdown()
        {
            var channelEvents = new ChannelEvents();
            this.eventsProxy = new ChannelEvents();

            var channelFactory = Substitute.For<ChannelFactory>();
            this.theChannel = Substitute.For<PushChannel>();
            this.theChannel.Events.Returns(channelEvents);
            channelFactory.Create().Returns(this.theChannel);

            this.balancer = new QueueLengthLoadBalancer(channelFactory, this.eventsProxy, PlatformType.None);

            this.balancer.AddChannels(1);
            this.balancer.ShutdownAll(true);
        }
        
        [Test]
        public void ThenThereShouldBeNoChannelsInTheBalancer()
        {
            Assert.AreEqual(0, this.balancer.ChannelCount);
        }

        [Test]
        public void ThenTheChannelsShouldBeShutDown()
        {
            theChannel.Received().Stop(true);
        }
    }
}