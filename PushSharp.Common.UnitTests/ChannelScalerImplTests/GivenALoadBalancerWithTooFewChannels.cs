namespace PushSharp.Common.UnitTests.ChannelScalerImplTests
{
    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class GivenALoadBalancerWithTooFewChannels
    {
        [Test]
        public void ThenTheScalerShouldAddAChannelToTheLoadBalancer()
        {
            var channelLoadBalancer = Substitute.For<ChannelLoadBalancer>();
            channelLoadBalancer.HasActiveChannels.Returns(true);
            channelLoadBalancer.ChannelCount.Returns(1, 2, 3, 4, 5);

            var pushServiceSettings = new PushServiceSettings { AutoScaleChannels = false, Channels = 5 };
            var channelScalerImp = new ChannelScalerImpl(pushServiceSettings, channelLoadBalancer);
            channelScalerImp.Start();

            channelScalerImp.CheckScale();

            channelLoadBalancer.Received(3).AddChannels(1);
        }
    }
}
