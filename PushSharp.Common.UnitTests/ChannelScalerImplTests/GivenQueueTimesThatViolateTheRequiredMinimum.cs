namespace PushSharp.Common.UnitTests.ChannelScalerImplTests
{
    using NSubstitute;

    using NUnit.Framework;

    [TestFixture]
    public class GivenQueueTimesThatViolateTheRequiredMinimum
    {
        [Test]
        public void ThenTheScalerShouldAddAChannelToTheLoadBalancer()
        {
            
            var channelLoadBalancer = Substitute.For<ChannelLoadBalancer>();
            channelLoadBalancer.HasActiveChannels.Returns(true);

            var pushServiceSettings = new PushServiceSettings();
            var channelScalerImp = new ChannelScalerImpl(pushServiceSettings, channelLoadBalancer);
            channelScalerImp.Start();
            
            int measurementCounter = 0;
            while (measurementCounter < 1000)
            {
                channelScalerImp.RecordQueueTime(pushServiceSettings.MinAvgTimeToScaleChannels + 1);
                measurementCounter++;
            }

            channelScalerImp.CheckScale();

            channelLoadBalancer.Received(1).AddChannels(1);
        }
    }
}
