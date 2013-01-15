namespace PushSharp.Common.UnitTests.ChannelScalerImplTests
{
    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class GivenALoadBalancerWithTooManyChannels
    {
        [Test]
        public void ThenTheScalerShouldAddAChannelToTheLoadBalancer()
        {
            var channelLoadBalancer = Substitute.For<ChannelLoadBalancer>();
            channelLoadBalancer.HasActiveChannels.Returns(true);
            channelLoadBalancer.ChannelCount.Returns(10, 9, 8, 7, 6, 5);

            var pushServiceSettings = new PushServiceSettings { AutoScaleChannels = false, Channels = 5 };
            var channelScalerImp = new ChannelScalerImpl(pushServiceSettings, channelLoadBalancer);
            channelScalerImp.Start();
            
            int measurementCounter = 0;
            while (measurementCounter < 1000)
            {
                channelScalerImp.RecordQueueTime(pushServiceSettings.MinAvgTimeToScaleChannels + 1);
                measurementCounter++;
            }

            channelScalerImp.CheckScale();

            channelLoadBalancer.Received(5).RemoveChannels(1);
        }
    }
}
