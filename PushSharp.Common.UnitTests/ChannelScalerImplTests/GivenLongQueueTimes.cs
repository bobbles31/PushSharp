namespace PushSharp.Common.UnitTests.ChannelScalerImplTests
{
    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class GivenLongQueueTimes
    {
        [Test]
        public void ThenTheScalerShouldAdd5ChannelsToTheLoadBalancer()
        {
            var channelLoadBalancer = Substitute.For<ChannelLoadBalancer>();
            channelLoadBalancer.HasActiveChannels.Returns(true);
            channelLoadBalancer.ChannelCount.Returns(5);

            var channelScalerImp = new ChannelScalerImpl(new PushServiceSettings(), channelLoadBalancer);
            channelScalerImp.Start();
            
            int measurementCounter = 0;
            while (measurementCounter < 1000)
            {
                channelScalerImp.RecordQueueTime(6000);
                measurementCounter++;
            }

            channelScalerImp.CheckScale();

            channelLoadBalancer.Received(1).AddChannels(5);
        }
    }
}
