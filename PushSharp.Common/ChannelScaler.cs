namespace PushSharp.Common
{
    public interface ChannelScaler
    {
        ChannelLoadBalancer ChannelLoadBalancer { get; }

        PushServiceSettings ServiceSettings { get; }

        void RecordQueueTime(double queueTimeMilliseconds);

        void Start();

        void Stop();
    }
}