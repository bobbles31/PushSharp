namespace PushSharp.Common
{
    public interface ChannelLoadBalancer
    {
        int ChannelCount { get; }

        bool HasActiveChannels { get; }

        void AddChannels(int channelCount);

        void RemoveChannels(int channelCount);

        void Distribute(Notification notification);

        void ShutdownAll(bool allowQueuesToFinish);
    }
}