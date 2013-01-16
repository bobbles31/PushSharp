namespace PushSharp.Common.UnitTests.ChannelScalerImplTests
{
    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class GivenAScalerThatIsRunningWell
    {
        [Test]
        public void TheTheScalerShouldNotBeScaled()
        {
            var channelLoadBalancer = Substitute.For<ChannelLoadBalancer>();
            channelLoadBalancer.HasActiveChannels.Returns(true);
            channelLoadBalancer.ChannelCount.Returns(1);
            
            var channelScalerImp = new ChannelScalerImpl(new PushServiceSettings(), channelLoadBalancer);
            channelScalerImp.Start();

            channelScalerImp.CheckScale();

            Assert.AreEqual(1, channelLoadBalancer.ChannelCount);
        }
    }
}