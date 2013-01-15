namespace PushSharp.Common
{
    using System;
    using System.Linq;
    using System.Threading;

    public class ChannelScalerImpl : ChannelScaler
    {
        private readonly double[] messageQueueTimes = new double[1000];

        private int lastMeasurementIndex;
         
        private Timer timerCheckScale;

        private bool isRunning;

        public ChannelScalerImpl(PushServiceSettings settings, ChannelLoadBalancer loadBalancer)
        {
            this.ServiceSettings = settings;
            this.ChannelLoadBalancer = loadBalancer;
        }

        public ChannelLoadBalancer ChannelLoadBalancer { get; private set; }

        public PushServiceSettings ServiceSettings { get; private set; }

        public void RecordQueueTime(double queueTimeMilliseconds)
        {
            lock (this.messageQueueTimes)
            {
                this.messageQueueTimes[this.lastMeasurementIndex] = queueTimeMilliseconds;
                this.SetNextMeasurementIndex();
            }
        }

        private void SetNextMeasurementIndex()
        {
            this.lastMeasurementIndex++;
            if (this.lastMeasurementIndex == this.messageQueueTimes.Length)
            {
                this.lastMeasurementIndex = 0;
            }
        }

        public void Start()
        {
            this.timerCheckScale = new Timer(
                state => this.CheckScale(),
                null,
                TimeSpan.FromSeconds(15),
                TimeSpan.FromSeconds(15));
            this.isRunning = true;
            this.CheckScale();
        }

        public void Stop()
        {
            this.timerCheckScale.Change(Timeout.Infinite, Timeout.Infinite);
            this.isRunning = false;
        }

        public void CheckScale()
        {
            if (!this.isRunning)
            {
                return;
            }

            if (this.ServiceSettings.AutoScaleChannels)
            {
                this.ApplyAutoScalePolicy();
            }
            else
            {
                this.ApplySettingsBasedScalePolicy();
            }
        }

        private void ApplySettingsBasedScalePolicy()
        {
            while (this.ChannelLoadBalancer.ChannelCount > this.ServiceSettings.Channels)
            {
                this.ScaleChannels(ChannelScaleAction.Destroy);
            }

            while (this.ChannelLoadBalancer.ChannelCount < this.ServiceSettings.Channels)
            {
                this.ScaleChannels(ChannelScaleAction.Create);
            }
        }

        private void ApplyAutoScalePolicy()
        {
            if (!this.ChannelLoadBalancer.HasActiveChannels)
            {
                this.ScaleChannels(ChannelScaleAction.Create);
                return;
            }

            var avgTime = this.GetAverageQueueWait();

            if (avgTime < this.ServiceSettings.MinAvgTimeToScaleChannels && this.ChannelLoadBalancer.ChannelCount > 1)
            {
                this.ScaleChannels(ChannelScaleAction.Destroy);
            }
            else if (this.ChannelLoadBalancer.ChannelCount < this.ServiceSettings.MaxAutoScaleChannels)
            {
                var numChannelsToSpinUp = 0;

                if (avgTime > 5000)
                {
                    numChannelsToSpinUp = 5;
                }
                else if (avgTime > 1000)
                {
                    numChannelsToSpinUp = 2;
                }
                else if (avgTime > this.ServiceSettings.MinAvgTimeToScaleChannels)
                {
                    numChannelsToSpinUp = 1;
                }

                this.ScaleChannels(ChannelScaleAction.Create, numChannelsToSpinUp);
            }
        }

        private void ScaleChannels(ChannelScaleAction action, int count = 1)
        {
            if (action == ChannelScaleAction.Create)
            {
                this.ChannelLoadBalancer.AddChannels(count);
            }
            else if (action == ChannelScaleAction.Destroy)
            {
                this.ChannelLoadBalancer.RemoveChannels(count);
            }
        }

        private double GetAverageQueueWait()
        {
            lock (this.messageQueueTimes)
            {
                return this.messageQueueTimes.Average();
            }
        }
    }
}